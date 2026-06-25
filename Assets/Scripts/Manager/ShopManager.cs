using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopFailReason
{
    None,
    InvalidItem,
    SoldOut,
    NotEnoughCurrency,
    IapManagerNull,
    IapNotReady,
    IapInProgress,
    MissingIapProductId,
    IapFailed,
    Unknown,
}


/// <summary>
/// 구매 규칙/처리 단일 진입점
/// - UI는 TryPurchase(item)만 호출
/// - IAP 성공/실패는 IapManager 이벤트를 구독해 처리
/// - 보상/저장/구매횟수/검증(추후) 위치를 한 곳으로 모음
/// </summary>

#if IAP

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public event Action<ProductShopData> OnPurchaseStarted;
    public event Action<ProductShopData> OnPurchaseCompleted;
    public event Action<ProductShopData, ShopFailReason, string> OnPurchaseFailed;


    public event Action<string , string, string, Action<string>> OnPurchaseAction;

    private const float IAP_SERVER_TIMEOUT = 15f;
    
    // IAP pending
    private ProductShopData _pendingItem;
    private string _pendingIapProductId;

    private UserInfoData User => Managers.Instance.UserInfo();

    // IAP 상품 매칭용 캐시
    private readonly Dictionary<string, ProductShopData> _iapItemMap = new();
    
    private readonly HashSet<string> _serverInFlight = new();

    public enum EShopKind { MidCash, Pass, Limit }
    // item에서 타입 판별
    private EShopKind GetShopKind(ProductShopData item)
    {
        if (item is PassGroup) return EShopKind.Pass;
        if (item is LimitedShop) return EShopKind.Limit;
        return EShopKind.MidCash; // MidCashShop
    }
    
    // ShopManager - productId가 특정 상점 종류인지 판별 (UI가 호출)
    public bool IsProductOfKind(string productId, EShopKind kind)
    {
        if (_iapItemMap.TryGetValue(productId, out var item) && item != null)
            return GetShopKind(item) == kind;
        return false;
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SubscribeIap(true);
    }

    private void OnDisable()
    {
        SubscribeIap(false);
    }

    private void SubscribeIap(bool sub)
    {
        var iap = Managers.Instance?.IAP;
        if (iap == null) return;

        // 중복 구독 방지
        iap.OnIapPurchaseSucceeded -= HandleIapSuccess;
        iap.OnIapPurchaseFailed -= HandleIapFail;

        if (!sub) return;

        iap.OnIapPurchaseSucceeded += HandleIapSuccess;
        iap.OnIapPurchaseFailed += HandleIapFail;
        
        
        // 구독 직후, 구독 전에 발화돼 놓친 미완료 pending 주문 복구
        iap.RepublishPendingOrders();
        
    }
    
    // IAP 준비 + 아이템 등록 완료 후 Managers에서 호출.
    // 구독을 보장하고, 구독 전에 발화돼 놓친 pending 주문을 다시 통지받는다.
    public void OnReadyForRecovery()
    {
        SubscribeIap(true); // 멱등: 내부에서 -= 후 += 하므로 중복 구독 안 됨

        var iap = Managers.Instance?.IAP;
        if (iap == null) return;

        MyLogger.Log("[ShopManager] OnReadyForRecovery - republish pending orders");
    }

    /// <summary>
    /// UI는 이 함수만 호출하면 됨
    /// </summary>
    public void TryPurchase(ProductShopData item)
    {
        if (item == null)
        {
            FailPurchase(null, ShopFailReason.InvalidItem, "ItemNull");
            return;
        }

        // 진행 중 IAP가 있으면 추가 구매 막기
        if (HasIapInProgress())
        {
            FailPurchase(item, ShopFailReason.IapInProgress, "IapAlreadyInProgress");
            return;
        }

        if (IsIapItem(item))
        {
            PurchaseByIap(item);
        }
        else
        {

        }
    }

    /// <summary>
    /// 현재 DB 변경안 기준: CostItemType == IAP이면 CostItemDBData는 null, IapProductId는 세팅
    /// </summary>
    private bool IsIapItem(ProductShopData item)
    {
        // return item.CostItemDBData == null && !string.IsNullOrEmpty(item.IapProductId);
        return !string.IsNullOrEmpty(item.ProductID);
    }

    private void PurchaseByIap(ProductShopData item)
    {
        var iap = Managers.Instance?.IAP;
        if (iap == null)
        {
            FailPurchase(item, ShopFailReason.IapManagerNull, "Managers.IAP is null");
            return;
        }

        if (!iap.IsReady)
        {
            FailPurchase(item, ShopFailReason.IapNotReady, "IAP NotReady");
            return;
        }

        if (iap.InProgress)
        {
            FailPurchase(item, ShopFailReason.IapInProgress, "IAP InProgress");
            return;
        }

        if (string.IsNullOrEmpty(item.ProductID))
        {
            FailPurchase(item, ShopFailReason.MissingIapProductId, "IapProductId empty");
            return;
        }

        // pending 저장(성공/실패 매칭)
        _pendingItem = item;
        _pendingIapProductId = item.ProductID;
        
        ShowPurchaseLoading();
        
        OnPurchaseStarted?.Invoke(item);
        
        iap.Purchase(item.ProductID);
    }

    private void HandleIapSuccess(string productId, string receipt)
    {
        ProductShopData item = null;
        bool isRecovery = false;

        // 1) 일반 진행 중 구매
        if (!string.IsNullOrEmpty(_pendingIapProductId) &&
            string.Equals(productId, _pendingIapProductId, StringComparison.Ordinal))
        {
            item = _pendingItem;
        }
        else
        {
            // 2) 앱 재실행 후 복구 구매
            item = FindShopItemByProductId(productId);

            if (item == null)
            {
                MyLogger.LogWarning($"[ShopManager] Recovered IAP but item not found - productId:{productId}");
                return;
            }

            isRecovery = true;
        }

        // 서버 통신 핸들러가 없으면 보류 (상태 오염 방지: pending 세팅하지 않음)
        if (OnPurchaseAction == null)
        {
            MyLogger.LogWarning($"[ShopManager] OnPurchaseAction no subscriber. Defer - productId:{productId}");
            if (!isRecovery)
                ClearPendingIap(); // 진행 중이던 건도 다음 기회에 복구 경로로 재진입하도록 정리
            return;
        }

        // 여기서부터 실제 처리 확정 → 복구 건도 진행 중으로 세팅
        if (isRecovery)
        {
            _pendingItem = item;
            _pendingIapProductId = productId;
            MyLogger.Log($"[ShopManager] Recovered pending IAP - productId:{productId}");
        }

        MyLogger.Log(
            $"[ShopManager] HandleIapSuccess - productId:{productId}, hasReceipt:{!string.IsNullOrEmpty(receipt)}, receiptLength:{(string.IsNullOrEmpty(receipt) ? 0 : receipt.Length)}");

        ProcessIapPurchaseAfterStoreSuccess(item, productId, receipt);
    }
    
    private void ProcessIapPurchaseAfterStoreSuccess(ProductShopData item, string productId, string receipt)
    {
        if (item == null)
        {
            FailPurchase(null, ShopFailReason.Unknown, "PendingItemNull", true);
            return;
        }
        
        if (OnPurchaseAction == null)
        {
            // 서버 통신 핸들러(상점 UI 등)가 아직 구독되지 않음.
            // pending과 _iapItemMap은 그대로 유지되므로, 구독자가 붙은 뒤
            // 다시 RepublishPendingOrders로 복구 가능.
            MyLogger.LogWarning($"[ShopManager] OnPurchaseAction has no subscriber. Defer recovery - productId:{productId}");
            return;
        }
        
        if (!_serverInFlight.Add(productId))
        {
            MyLogger.LogWarning($"[ShopManager] Server request already in-flight: {productId}");
            return;
        }
        

    #if UNITY_IOS
        string shop = "apple";
    #else
        string shop = "google";
    #endif

        //string goodsId = productId; // 서버 규칙 확인 필요 [google / apple]
        string goodsId = item.ID.ToString(); // 서버 규칙 확인 필요 [google / apple]

        MyLogger.Log(
            $"[ShopManager] IAP server request - goodsId:{goodsId}, shop:{shop}, hasReceipt:{!string.IsNullOrEmpty(receipt)}, receiptLength:{(string.IsNullOrEmpty(receipt) ? 0 : receipt.Length)}");


        OnPurchaseAction.Invoke(goodsId,
            receipt,
            shop, response =>
            {
                _serverInFlight.Remove(productId);
                
                MyLogger.Log(response);
                MyLogger.Log($"[ShopManager] IAP purchase completed - productId:{productId}");
                Managers.Instance.IAP.ConfirmPendingPurchase(item.ProductID);
                CompletePurchase(item, true);
            });
    }
    
    
    private void ClearPendingIap()
    {
        _pendingItem = null;
        _pendingIapProductId = null;
    }

    private void HandleIapFail(string productId, string reason)
    {
        ProductShopData item = null;

        if (!string.IsNullOrEmpty(_pendingIapProductId) &&
            string.Equals(productId, _pendingIapProductId, StringComparison.Ordinal))
        {
            item = _pendingItem;
        }
        else
        {
            item = FindShopItemByProductId(productId);
            MyLogger.LogWarning($"[ShopManager] Recovered/Unexpected IAP fail - productId:{productId}, reason:{reason}");
        }

        FailPurchase(item, ShopFailReason.IapFailed, reason, true);
    }
    
    public void RegisterShopItems(IEnumerable<ProductShopData> items)
    {
        if (items == null)
            return;

        foreach (var item in items)
        {
            if (item == null)
                continue;

            if (string.IsNullOrEmpty(item.ProductID))
                continue;

            if (_iapItemMap.ContainsKey(item.ProductID))
            {
                MyLogger.LogWarning($"[ShopManager] Duplicate IapProductId detected: {item.ProductID}");
            }

            _iapItemMap[item.ProductID] = item;
        }
        MyLogger.Log($"[ShopManager] IAP items registered: {_iapItemMap.Count}");

    }
    
    private ProductShopData FindShopItemByProductId(string productId)
    {
        if (string.IsNullOrEmpty(productId))
            return null;

        if (_iapItemMap.TryGetValue(productId, out var item))
            return item;

        MyLogger.LogWarning($"[ShopManager] FindShopItemByProductId failed - productId:{productId}");
        return null;
    }
    
    private void FailPurchase(ProductShopData item, ShopFailReason reason, string message, bool clearPending = false)
    {
        HidePurchaseLoading();
        
        if (clearPending)
            ClearPendingIap();

        OnPurchaseFailed?.Invoke(item, reason, message);
    }

    private void CompletePurchase(ProductShopData item, bool clearPending = false)
    {
        HidePurchaseLoading();
        
        if (clearPending)
            ClearPendingIap();

        OnPurchaseCompleted?.Invoke(item);
    }
    
    private bool HasIapInProgress()
    {
        var iap = Managers.Instance?.IAP;
        MyLogger.Log($"_pendingIapProductId {_pendingIapProductId}");
        MyLogger.Log($"iap.InProgress {iap.InProgress}");
        return !string.IsNullOrEmpty(_pendingIapProductId) || (iap != null && iap.InProgress);
    }
    
    private void ShowPurchaseLoading()
    {
        Managers.Instance?.GetLoadingUI()?.Show();
    }

    private void HidePurchaseLoading()
    {
        Managers.Instance?.GetLoadingUI()?.Hide();
    }
    
}
#endif