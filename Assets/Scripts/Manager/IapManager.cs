#if IAP
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

using System.Linq;
using Unity.Services.Core;
using UnityEngine.Networking;
using UnityEngine.Purchasing.Extension; // iOS JWS 정보 접근용

#if ANALYTICS
using AnalyticsLogEventHelp;
#endif

public class IapManager : MonoBehaviour
{
    public static IapManager Instance { get; private set; }

    public bool IsReady => _storeController != null && _productsFetched;
    public bool IsInitialized => _storeController != null;

    public bool InProgress => _inProgress;
    public string ProcessingId => _processingId;

    private bool _productsFetched;
    private bool _inProgress;
    private string _processingId;

    /// <summary>
    /// 구매 승인 완료 시 호출
    /// receipt는 아직 Confirm 전 상태일 수 있음
    /// </summary>
    public event Action<string, string> OnIapPurchaseSucceeded; // productId, receipt

    /// <summary>
    /// 스토어 구매 실패 시 호출
    /// </summary>
    public event Action<string, string> OnIapPurchaseFailed; // productId, reason

    public event Action OnInitializedSuccess;
    public event Action<string> OnInitializedFailed;

    [Header("Confirm Mode")]
    [Tooltip("true면 구매 승인 후 즉시 Confirm하지 않고, 외부 검증/보상 완료 후 ConfirmPendingPurchase를 호출함.")]
    [SerializeField] private bool _useDeferredConfirm = true;

    private StoreController _storeController;

    // Confirm 전 pending 주문 캐시
    private readonly Dictionary<string, PendingOrder> _pendingOrders = new();

    // 중복 상품 등록 방지
    private readonly HashSet<string> _registered = new();

    // 비소모성/구독 중복 보상 방지
    private readonly HashSet<string> _rewardedProducts = new();
    private const string RewardedProductsKey = "IAP_REWARDED_PRODUCTS";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadRewardedProducts();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        UnregisterStoreEvents();
    }

    public async void Init(IReadOnlyList<IapProductDef> products)
    {
        if (IsReady)
        {
            Debug.Log("[IAP] Already initialized.");
            return;
        }

        if (products == null || products.Count == 0)
        {
            Debug.LogError("[IAP] Init failed: Product list is empty.");
            OnInitializedFailed?.Invoke("InitFailed:ProductListEmpty");
            return;
        }

        _productsFetched = false;
        _inProgress = false;
        _processingId = null;
        _pendingOrders.Clear();

        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[IAP] UnityServices.InitializeAsync warning: {ex.Message}");
        }

        _registered.Clear();

        var productDefinitions = new List<ProductDefinition>();

        foreach (var p in products)
        {
            if (string.IsNullOrWhiteSpace(p.ProductId))
                continue;

            if (!_registered.Add(p.ProductId))
                continue;

            productDefinitions.Add(new ProductDefinition(p.ProductId, p.Type));
        }

        if (productDefinitions.Count == 0)
        {
            Debug.LogWarning("[IAP] No IAP products to register.");
            OnInitializedFailed?.Invoke("InitFailed:NoProducts");
            return;
        }

        _storeController = UnityIAPServices.StoreController();
        RegisterStoreEvents();

        try
        {
            await _storeController.Connect();

            // 중요:
            // FetchProducts는 즉시 완료되는 함수가 아니므로
            // 여기서 GetProductById로 바로 검사하면 안 됨.
            _storeController.FetchProducts(productDefinitions);

            Debug.Log("[IAP] FetchProducts requested.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[IAP] Init failed: {ex}");
            OnInitializedFailed?.Invoke($"InitFailed:{ex.Message}");
        }
    }
    
    private void RegisterStoreEvents()
    {
        if (_storeController == null)
            return;

        _storeController.OnProductsFetched += OnProductsFetched;
        _storeController.OnProductsFetchFailed += OnProductsFetchFailed;
        
        _storeController.OnPurchasePending += OnPurchasePending;
        _storeController.OnPurchaseFailed += OnPurchaseFailedV5;
        _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        _storeController.OnPurchasesFetched += OnPurchasesFetched;
        _storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
        _storeController.OnStoreDisconnected += OnStoreDisconnected;
    }

    private void UnregisterStoreEvents()
    {
        if (_storeController == null)
            return;

        _storeController.OnProductsFetched -= OnProductsFetched;
        _storeController.OnProductsFetchFailed -= OnProductsFetchFailed;
        
        _storeController.OnPurchasePending -= OnPurchasePending;
        _storeController.OnPurchaseFailed -= OnPurchaseFailedV5;
        _storeController.OnPurchaseConfirmed -= OnPurchaseConfirmed;
        _storeController.OnPurchasesFetched -= OnPurchasesFetched;
        _storeController.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
        _storeController.OnStoreDisconnected -= OnStoreDisconnected;
    }

    public string GetLocalizedPrice(string productId)
    {
        if (!IsReady)
            return string.Empty;

        var product = _storeController.GetProductById(productId);
        return product?.metadata?.localizedPriceString ?? string.Empty;
    }

    public bool HasProduct(string productId)
    {
        if (!IsReady)
            return false;

        return _storeController.GetProductById(productId) != null;
    }

    // 특정 상품이 pending 상태인지 확인
    public bool HasPending(string productId)
    {
        return !string.IsNullOrWhiteSpace(productId) && _pendingOrders.ContainsKey(productId);
    }

    // pending 주문의 receipt 조회
    public bool TryGetPendingReceipt(string productId, out string receipt)
    {
        receipt = string.Empty;

        if (!HasPending(productId))
            return false;

        var order = _pendingOrders[productId];
        var firstItem = order?.CartOrdered?.Items()?.FirstOrDefault();
        var product = firstItem?.Product;

        receipt = product?.receipt ?? string.Empty;
        return !string.IsNullOrEmpty(receipt);
    }

    // pending 주문을 수동 제거
    // Confirm하지 않으면 같은 주문이 다시 들어올 수 있음
    public void ClearPending(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return;

        _pendingOrders.Remove(productId);
    }

    public void Purchase(string productId)
    {
        if (!IsReady)
        {
            OnIapPurchaseFailed?.Invoke(productId, "NotReady");
            return;
        }

        if (_inProgress)
        {
            OnIapPurchaseFailed?.Invoke(productId, $"AlreadyInProgress:{_processingId}");
            return;
        }

        var product = _storeController.GetProductById(productId);
        if (product == null)
        {
            OnIapPurchaseFailed?.Invoke(productId, "ProductNotFound");
            return;
        }

        if (!product.availableToPurchase)
        {
            OnIapPurchaseFailed?.Invoke(productId, "NotAvailableToPurchase");
            return;
        }

        _inProgress = true;
        _processingId = productId;

        try
        {
            _storeController.PurchaseProduct(productId);
        }
        catch (Exception ex)
        {
            _inProgress = false;
            _processingId = null;
            Debug.LogException(ex);
            OnIapPurchaseFailed?.Invoke(productId, "PurchaseException");
        }
    }

    // 보상 완료 후 pending 주문을 최종 Confirm
    public bool ConfirmPendingPurchase(string productId)
    {
        if (!IsReady)
        {
            Debug.LogWarning($"[IAP] ConfirmPendingPurchase failed: NotReady ({productId})");
            return false;
        }

        if (string.IsNullOrWhiteSpace(productId))
        {
            Debug.LogWarning("[IAP] ConfirmPendingPurchase failed: productId is null or empty");
            return false;
        }

        if (_pendingOrders.TryGetValue(productId, out var order) && order != null)
        {
            // 트래킹 로그 가드 키 정리에 쓸 transactionId를 confirm 전에 확보
            string txId = null;
            var confirmProduct = _storeController.GetProductById(productId);
            if (confirmProduct != null)
                txId = confirmProduct.transactionID;

            try
            {
                // 1. 유니티 IAP 스토어에 최종 구매 확정 처리
                _storeController.ConfirmPurchase(order);
                _pendingOrders.Remove(productId);
                Debug.Log($"[IAP] ConfirmPendingPurchase success: {productId}");

                // 2. 트래킹 중복 방지용 PlayerPrefs 키 정리 (거래 종료 → 더 이상 불필요)
                if (!string.IsNullOrEmpty(txId))
                {
                    string logKey = $"IAP_LOGGED_{txId}";
                    if (PlayerPrefs.HasKey(logKey))
                    {
                        PlayerPrefs.DeleteKey(logKey);
                        PlayerPrefs.Save();
                    }
                }

                // ========================================================
                // iOS: 서버 영수증 검증 및 보상 지급 완료 후 Singular TrackRevenue 전송
                // AOS: OnPurchasePending에서 TrackInAppPurchase(order)를 사용하므로 여기서는 전송하지 않음
                // ========================================================
    #if SINGULAR && UNITY_IOS && !UNITY_EDITOR
                if (Managers.Instance != null && Managers.Instance.Singular != null)
                {
                    var product = _storeController.GetProductById(productId);

                    if (product != null)
                    {
                        string currency = product.metadata.isoCurrencyCode;
                        string priceStr = product.metadata.localizedPrice.ToString();

                        if (double.TryParse(
                                priceStr,
                                System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out double amount))
                        {
                            Managers.Instance.Singular.TrackRevenue(currency, amount);
                            Debug.Log($"[Singular] iOS TrackRevenue success after verification: {amount} {currency} / productId:{productId}");
                        }
                        else
                        {
                            Debug.LogError($"[Singular] iOS TrackRevenue price parse failed: {priceStr} / productId:{productId}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[Singular] iOS TrackRevenue failed: product is null / productId:{productId}");
                    }
                }
    #endif
                // ========================================================

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IAP] ConfirmPendingPurchase exception: {productId} / {ex}");
                return false;
            }
        }

        Debug.LogWarning($"[IAP] ConfirmPendingPurchase failed: PendingNotFound ({productId})");
        return false;
    }

        
    private void OnProductsFetched(List<Product> products)
    {
        if (products == null || products.Count == 0)
        {
            Debug.LogError("[IAP] Products fetched but empty.");
            OnInitializedFailed?.Invoke("InitFailed:ProductsEmpty");
            return;
        }

        _productsFetched = true;

        Debug.Log($"[IAP] Products fetched: {products.Count}");

        _storeController.FetchPurchases();

        OnInitializedSuccess?.Invoke();
        Debug.Log("[IAP] Initialized (v5)");
    }

    private void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        Debug.LogError($"[IAP] Products fetch failed: {failure}");
        OnInitializedFailed?.Invoke("InitFailed:ProductsFetchFailed");
    }

    private void OnPurchasePending(PendingOrder order)
    {
        if (order == null)
        {
            _inProgress = false;
            _processingId = null;
            Debug.LogError("[IAP] OnPurchasePending failed: order is null");
            OnIapPurchaseFailed?.Invoke("Unknown", "PendingOrderNull");
            return;
        }

        var firstItem = order.CartOrdered?.Items()?.FirstOrDefault();
        var product = firstItem?.Product;

        if (product == null)
        {
            _inProgress = false;
            _processingId = null;
            Debug.LogError("[IAP] OnPurchasePending failed: product is null");
            OnIapPurchaseFailed?.Invoke("Unknown", "PendingOrderProductNull");
            return;
        }

        var id = product.definition.id;
        
        var internalReceipt = ""; // 우리 서버용
        var superServiceReceipt = ""; // 슈퍼서비스용 추가
        
        // 플랫폼 공통으로 사용할 트랜잭션 ID 변수
        string finalTransactionId = product.transactionID; 
        
        // 플랫폼별 영수증 페이로드 추출 및 서버 맞춤형 포맷팅 분기
#if UNITY_ANDROID
        internalReceipt = product.receipt;
        superServiceReceipt = product.receipt; // 안드로이드는 기존과 동일
        
#elif UNITY_IOS
        string jwsToken = order.Info.Apple?.jwsRepresentation;
        
        if (string.IsNullOrEmpty(jwsToken))
        {
            jwsToken = product.receipt;
        }

        string tId = product.transactionID;
        
        if (string.IsNullOrEmpty(tId) && !string.IsNullOrEmpty(jwsToken))
        {
            tId = ExtractTransactionIdFromJWS(jwsToken);
        }

        finalTransactionId = tId;

        IosReceiptPayload payloadObj = new IosReceiptPayload
        {
            productId = id,
            transactionId = tId ?? "",
            signedTransaction = jwsToken
        };

        internalReceipt = JsonUtility.ToJson(payloadObj);
        superServiceReceipt = jwsToken;
#else
        internalReceipt = product.receipt;
        superServiceReceipt = product.receipt;
#endif
        
        _inProgress = false;

        if (!string.IsNullOrEmpty(_processingId) && id != _processingId)
            Debug.LogWarning($"[IAP] Mismatch processing={_processingId} purchased={id}");

        _processingId = null;

        if (_useDeferredConfirm)
        {
            if (_pendingOrders.ContainsKey(id))
                Debug.LogWarning($"[IAP] Duplicate pending order received: {id} (already pending)");

            _pendingOrders[id] = order;
        }

        bool preventDuplicateReward =
            product.definition.type == ProductType.NonConsumable ||
            product.definition.type == ProductType.Subscription;

        bool alreadyRewarded = preventDuplicateReward && _rewardedProducts.Contains(id);

        if (!alreadyRewarded)
        {
            string safeTransactionId = string.IsNullOrEmpty(finalTransactionId) 
                ? $"UNKNOWN_TX_{System.Guid.NewGuid()}" 
                : finalTransactionId;
                
            string logKey = $"IAP_LOGGED_{safeTransactionId}";

            if (PlayerPrefs.GetInt(logKey, 0) == 0)
            {

            #if ANALYTICS
                string currency = product.metadata?.isoCurrencyCode;
                double price = 0;
                if (product.metadata != null)
                {
                    price = decimal.ToDouble(product.metadata.localizedPrice);
                }
                AnalyticsLogEventHelper.LogInAppPurchase(id, currency, price);
                
            #endif
                
#if SINGULAR && UNITY_ANDROID
                if (Managers.Instance != null && Managers.Instance.Singular != null)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        { "productId", id },
                        { "deferredConfirm", _useDeferredConfirm }
                    };
            
                    // AOS: 기존 방식 유지
                    // PendingOrder 객체를 그대로 넘겨 Singular InAppPurchase 트래킹
                    Managers.Instance.Singular.TrackInAppPurchase(order, attributes);
                    Debug.Log($"[Singular] AOS TrackInAppPurchase(PendingOrder) called for: {id}");
                }
#endif

#if REVIEW
                SendSuperServicePurchaseLog(product, superServiceReceipt);
#endif

                PlayerPrefs.SetInt(logKey, 1);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log($"[IAP] Tracking logs already sent for transaction: {safeTransactionId}. Skipping duplicate logs.");
            }

            // 아이템 지급 처리는 로그 중복 여부와 무관하게 정상 진행
            OnIapPurchaseSucceeded?.Invoke(id, internalReceipt);

            if (preventDuplicateReward)
                AddRewardedProduct(id);
        }
        else
        {
            Debug.LogWarning($"[IAP] Reward already granted for: {id}");
        }

        if (!_useDeferredConfirm)
        {
            _storeController.ConfirmPurchase(order);
        }
    }

    // JWS 토큰에서 transactionId를 추출하는 안전한 디코더 함수
    private string ExtractTransactionIdFromJWS(string jwsToken)
    {
        try
        {
            string[] parts = jwsToken.Split('.');
            if (parts.Length < 2) return "";

            string payload = parts[1];
            
            // Base64Url 형식을 일반 Base64 형식으로 변환하고 패딩(=)을 맞춰줍니다.
            payload = payload.Replace('-', '+').Replace('_', '/');
            int pad = payload.Length % 4;
            if (pad == 2) payload += "==";
            else if (pad == 3) payload += "=";

            byte[] decodedBytes = System.Convert.FromBase64String(payload);
            string json = System.Text.Encoding.UTF8.GetString(decodedBytes);

            // 디코딩된 JSON에서 transactionId 필드만 빼옵니다.
            JwsPayload parsed = JsonUtility.FromJson<JwsPayload>(json);
            return parsed.transactionId;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[IAP] JWS 디코딩 실패: {ex.Message}");
            return "";
        }
    }

    private void OnPurchaseFailedV5(FailedOrder order)
    {
        _inProgress = false;
        _processingId = null;

        var firstItem = order?.CartOrdered?.Items()?.FirstOrDefault();
        var product = firstItem?.Product;
        var id = product?.definition?.id ?? "Unknown";

        Debug.LogError($"[IAP] Purchase failed: {id}");
        OnIapPurchaseFailed?.Invoke(id, "PurchaseFailed");
    }

    private void OnPurchaseConfirmed(Order order)
    {
        var firstItem = order?.CartOrdered?.Items()?.FirstOrDefault();
        var product = firstItem?.Product;
        var id = product?.definition?.id ?? "Unknown";

        if (id != "Unknown")
            _pendingOrders.Remove(id);

        Debug.Log($"[IAP] Purchase confirmed: {id}");
    }

    private void OnPurchasesFetched(Orders orders)
    {
        if (orders == null)
        {
            Debug.Log("[IAP] Purchases fetched: null");
            return;
        }

        int confirmedCount = orders.ConfirmedOrders?.Count ?? 0;
        int pendingCount = orders.PendingOrders?.Count ?? 0;
        int deferredCount = orders.DeferredOrders?.Count ?? 0;

        Debug.Log($"[IAP] Purchases fetched. Confirmed={confirmedCount}, Pending={pendingCount}, Deferred={deferredCount}");
        
        // 앱 재시작/네트워크 단절 등으로 Confirm되지 못한 미완료 거래 복원
        if (orders.PendingOrders != null)
        {
            foreach (var pending in orders.PendingOrders)
            {
                RestorePendingOrder(pending);
            }
        }
        
    }
    
    // FetchPurchases로 복원된 pending 주문을 정상 보상/Confirm 경로로 라우팅
    private void RestorePendingOrder(PendingOrder order)
    {
        if (order == null)
            return;

        var firstItem = order.CartOrdered?.Items()?.FirstOrDefault();
        var product = firstItem?.Product;
        if (product == null)
            return;

        var id = product.definition.id;

        // 이미 처리 중인 pending이면 중복 발화 방지
        if (_useDeferredConfirm && _pendingOrders.ContainsKey(id))
        {
            Debug.Log($"[IAP] RestorePendingOrder skip (already pending): {id}");
            return;
        }

        // 비소모성/구독은 이미 보상 지급됐다면 Confirm만 처리하고 종료
        bool preventDuplicateReward =
            product.definition.type == ProductType.NonConsumable ||
            product.definition.type == ProductType.Subscription;

        if (preventDuplicateReward && _rewardedProducts.Contains(id))
        {
            Debug.Log($"[IAP] RestorePendingOrder already rewarded, confirming only: {id}");
            try { _storeController.ConfirmPurchase(order); }
            catch (Exception ex) { Debug.LogError($"[IAP] Restore confirm exception: {id} / {ex}"); }
            return;
        }

        Debug.Log($"[IAP] RestorePendingOrder routing to OnPurchasePending: {id}");

        // 보상 지급 + 서버 검증 후 ConfirmPendingPurchase까지 기존 경로 재사용
        OnPurchasePending(order);
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
    {
        Debug.LogWarning($"[IAP] PurchasesFetchFailed: {failure?.message}");
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
    {
        _productsFetched = false;

        var msg = $"StoreDisconnected:{failure?.message}";
        Debug.LogError($"[IAP] {msg}");
    }

    // 외부 구매 진입점
    public void Buy(string productId)
    {
        Purchase(productId);
    }

    public Product GetProduct(string productID)
    {
        if (_storeController == null)
        {
            Debug.LogError($"[IAP] GetProduct failed: StoreController is null ({productID})");
            return null;
        }

        var product = _storeController.GetProductById(productID);
        if (product == null)
            Debug.LogError($"[IAP] Null Product ID: {productID}");

        return product;
    }

    private void LoadRewardedProducts()
    {
        _rewardedProducts.Clear();

        var raw = PlayerPrefs.GetString(RewardedProductsKey, string.Empty);
        if (string.IsNullOrEmpty(raw))
            return;

        var ids = raw.Split('|');
        for (int i = 0; i < ids.Length; i++)
        {
            var id = ids[i];
            if (!string.IsNullOrWhiteSpace(id))
                _rewardedProducts.Add(id);
        }

        Debug.Log($"[IAP] Rewarded products loaded: {_rewardedProducts.Count}");
    }

    private void SaveRewardedProducts()
    {
        var raw = string.Join("|", _rewardedProducts);
        PlayerPrefs.SetString(RewardedProductsKey, raw);
        PlayerPrefs.Save();

        Debug.Log($"[IAP] Rewarded products saved: {_rewardedProducts.Count}");
    }

    private void AddRewardedProduct(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return;

        if (_rewardedProducts.Add(productId))
        {
            SaveRewardedProducts();
            Debug.Log($"[IAP] Rewarded product added: {productId}");
        }
    }

    public void RemoveRewardedProduct(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return;

        if (_rewardedProducts.Remove(productId))
        {
            SaveRewardedProducts();
            Debug.Log($"[IAP] Rewarded product removed: {productId}");
        }
    }

    public bool HasRewardedProduct(string productId)
    {
        return !string.IsNullOrWhiteSpace(productId) && _rewardedProducts.Contains(productId);
    }

    public void ClearRewardedProducts()
    {
        _rewardedProducts.Clear();
        PlayerPrefs.DeleteKey(RewardedProductsKey);
        PlayerPrefs.Save();

        Debug.Log("[IAP] Rewarded products cleared");
    }
    
    public void RepublishPendingOrders(System.Func<string, bool> filter = null)
    {
        if (_pendingOrders.Count == 0)
            return;

        // 순회 중 컬렉션 변경 가능성 대비 복사
        var snapshot = _pendingOrders.ToList();

        foreach (var kv in snapshot)
        {
            var id = kv.Key;
            var order = kv.Value;
            if (order == null)
                continue;

            // 필터 통과 못 하면 발화하지 않음 (상점 종류 불일치 등)
            if (filter != null && !filter(id))
                continue;
            
            var firstItem = order.CartOrdered?.Items()?.FirstOrDefault();
            var product = firstItem?.Product;
            if (product == null)
                continue;

            string internalReceipt;
#if UNITY_ANDROID
            internalReceipt = product.receipt;
#elif UNITY_IOS
        string jwsToken = order.Info.Apple?.jwsRepresentation;
        if (string.IsNullOrEmpty(jwsToken))
            jwsToken = product.receipt;
        string tId = product.transactionID;
        if (string.IsNullOrEmpty(tId) && !string.IsNullOrEmpty(jwsToken))
            tId = ExtractTransactionIdFromJWS(jwsToken);
        var payloadObj = new IosReceiptPayload
        {
            productId = id,
            transactionId = tId ?? "",
            signedTransaction = jwsToken
        };
        internalReceipt = JsonUtility.ToJson(payloadObj);
#else
        internalReceipt = product.receipt;
#endif

            Debug.Log($"[IAP] RepublishPendingOrders -> {id}");
            OnIapPurchaseSucceeded?.Invoke(id, internalReceipt);
        }
    }
    

#if REVIEW
    private string BuildUnityReceipt(Product product, string payload)
    {
        string rawReceipt = payload; 

        if (string.IsNullOrEmpty(rawReceipt))
            return string.Empty;

        // AOS 처리: 이미 Unity 기본 포맷이면 그대로 반환
        if (rawReceipt.Contains("\"Store\"") && rawReceipt.Contains("\"Payload\""))
            return rawReceipt;

#if UNITY_ANDROID
        string store = "GooglePlay";
#elif UNITY_IOS
        string store = "AppleAppStore";
#else
        string store = "Unknown";
#endif

        string transactionId = product.transactionID ?? string.Empty;

        return "{" +
               $"\"Store\":\"{store}\"," +
               $"\"TransactionID\":\"{transactionId}\"," +
               $"\"Payload\":{Newtonsoft.Json.JsonConvert.ToString(rawReceipt)}" +
               "}";
    }
    
    // 파라미터에 payload(영수증 데이터) 추가
    private async void SendSuperServicePurchaseLog(Product product, string payload)
    {
        if (product == null)
            return;

        string receipt = BuildUnityReceipt(product, payload);
        
        if (string.IsNullOrEmpty(receipt))
        {
            Debug.LogError($"[SuperService] Purchase log skip: receipt is empty / productId={product.definition?.id}");
            return;
        }

        string url = RestAPIURL.GetSuperServiceFullUrl(RestAPIURL.purchaseAction);
        WWWForm form = RestAPIURL.GetPurchaseForm(receipt);

        Debug.Log($"[SuperService] Purchase log request productId={product.definition?.id}, receiptLength={receipt.Length}");

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            try
            {
                await request.SendWebRequest().ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"[SuperService] Purchase log exception: {e.Message} / " +
                    $"code={request.responseCode} / body={request.downloadHandler?.text}"
                );
                return;
            }

            Debug.Log($"[SuperService] Purchase log response: {request.responseCode} / {request.downloadHandler.text}");
        }
    }
#endif
}

[Serializable]
public struct IapProductDef
{
    public string ProductId;
    public UnityEngine.Purchasing.ProductType Type; // Consumable/NonConsumable
    public string StoreIdGooglePlay; // null/empty면 ProductId 사용
    public string StoreIdApple;      // null/empty면 ProductId 사용
}

[System.Serializable]
public struct IosReceiptPayload
{
    public string productId;
    public string transactionId;
    public string signedTransaction;
}

[System.Serializable]
public struct JwsPayload
{
    public string transactionId;
}

#endif