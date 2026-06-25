using CHAT;
using Cysharp.Threading.Tasks;
using SentryToolkit;
using System;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

#if ANALYTICS
using AnalyticsLogEventHelp;
#endif

public class Managers : Singleton<Managers>
{
#region Manager class
    private UserInfoData _userInfoData;
    private ResourceObjectManager _resMgr;
    private EconomySystem _economySystem;
    private BestHttp_GameManager _serverManager;
    private UIManager _uiMgr = new UIManager();
    private ObjectUnitManager _objectUnitMgr = new ObjectUnitManager();
    private MapManager _mapMgr = new MapManager();
    private ClientLocalDB_Simple _simpleDB = new ClientLocalDB_Simple();
    private CameraManager _cameraManager = new CameraManager();
    private SaveManager _saveManager = new SaveManager();
    private SoundManager _soundManager = new SoundManager();
    private AtlasManager _atlasManager = new AtlasManager();
#if IAP
    private IapManager _iapManager;
    public IapManager IAP => _iapManager;
    private ShopManager _shopManager;
    public ShopManager Shop => _shopManager;

#endif
#if CHAT
    private UnityChatClient _chatClient;
    public UnityChatClient Chat => _chatClient;

    private ChatHistory _chatHistory;
    public ChatHistory ChatHistory => _chatHistory;
#endif
    
#if SINGULAR
    private SingularManager _singularManager;
    public SingularManager Singular => _singularManager;
    
#endif
    
#if CRASHLYTICS && (UNITY_ANDROID || UNITY_IOS)
    private FirebaseCrashlyticsManager _crashlytics;
    public FirebaseCrashlyticsManager Crashlytics => _crashlytics;
#endif
    
    
    [SerializeField] private LoadingUI _loadingUI;
    public SoundManager Sound => _soundManager;
    public DungeonFieldBase _dungeonFieldBase;
    private SyncCurrencyManager _syncCurrencyManager;
    private TreasureBoxManager _treasureBoxManager;
    private UITutorialManager _tutorialManager;
    
#endregion

    private JoystickController joystickController = new JoystickController();

    protected override void Awake()
    {
        // 1. 최우선 실행: 라이브 환경(심볼 없음)에서 로그 끄기
        #if DEV_SERVER_SET && !ONE
                // 개발 환경
                // UnityEngine.Debug.unityLogger.logEnabled = true;

//                if (SRDebug.Instance != null)
                {
                    // 필요하면 패널 강제로 열 수도 있음
                    // SRDebug.Instance.ShowDebugPanel();
                }
                // SRDebug.Instance.ShowDebugPanel();
                MyLogger.Log("[Managers] 개발 모드: 로그 활성화");

        #else
            // // 라이브 환경
            // SRDebug.Instance.ShowDebugPanel();
            UnityEngine.Debug.unityLogger.logEnabled = false;
        #endif
        
        // [중요] 예외 핸들러 등록
        // 로그는 꺼져도 Exception
        Application.logMessageReceived -= HandleException;
        Application.logMessageReceived += HandleException;
        
        DontDestroyOnLoad(this.gameObject);
    }
    
    void HandleException(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Assert || type == LogType.Error)
        {
            #if CRASHLYTICS && (UNITY_ANDROID || UNITY_IOS)
                    if (_crashlytics != null)
                    {
                        _crashlytics.Log($"[UnityError] {logString}"); // 커스텀 로그 기록
                        _crashlytics.SendException(logString, stackTrace);
                    }
            #endif
        }
    }
    
    public void Init()
    {
        MyLogger.Log("Managers Init");
        _resMgr = transform.GetComponentInChildren<ResourceObjectManager>();
        _serverManager = transform.GetComponentInChildren<BestHttp_GameManager>();
        _economySystem = transform.GetComponentInChildren<EconomySystem>();
        _economySystem?.SetPause(true);
        _syncCurrencyManager = transform.GetComponentInChildren<SyncCurrencyManager>();
        _treasureBoxManager = transform.GetComponentInChildren<TreasureBoxManager>();
        _tutorialManager = transform.GetComponentInChildren<UITutorialManager>();
        
#if CHAT
        InitChatClient();
#endif

#if SINGULAR
        EnsureSingularManager();
#endif
        
#if IAP
        EnsureIapManager();
        EnsureShopManager();   
#endif
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        _soundManager.Init();
    }
    
#if IAP
    
    private void EnsureShopManager()
    {
        if (_shopManager != null) return;

        _shopManager = GetComponentInChildren<ShopManager>(true);
        if (_shopManager == null)
        {
            var go = new GameObject("ShopManager");
            go.transform.SetParent(this.transform, worldPositionStays: false);
            _shopManager = go.AddComponent<ShopManager>();
        }
    }

    private void EnsureIapManager()
    {
        if (_iapManager != null) return;

        _iapManager = GetComponentInChildren<IapManager>(true);
        if (_iapManager == null)
        {
            var go = new GameObject("IapManager");
            go.transform.SetParent(this.transform, worldPositionStays: false);
            _iapManager = go.AddComponent<IapManager>();
        }

        _iapManager.OnInitializedSuccess -= OnIapInitializedSuccess;
        _iapManager.OnInitializedFailed -= OnIapInitializedFailed;

        _iapManager.OnInitializedSuccess += OnIapInitializedSuccess;
        _iapManager.OnInitializedFailed += OnIapInitializedFailed;
    }
    
    private void OnIapInitializedSuccess()
    {
        MyLogger.Log("[Managers] IAP Initialized Success");
        // IAP 준비 완료 → 상점 아이템 맵 채운 뒤 미완료 거래 복구
        EnsureShopManager();
        RegisterShopItemsFromDB();

        if (_shopManager != null && _iapManager != null)
        {
            _shopManager.OnReadyForRecovery(); // 구독 보장 + republish 트리거 (아래 ShopManager에 추가)
        }
    }

    private void OnIapInitializedFailed(string msg)
    {
        Debug.LogError("[Managers] IAP Initialized Failed: " + msg);
    }

    private bool _ugsInitialized;
    private bool _ugsInitializing;

    public async void InitIapWithCatalog(List<IapProductDef> catalog)
    {
        EnsureIapManager();

        if (_iapManager == null) return;
        if (_iapManager.IsReady) return;

        if (catalog == null || catalog.Count == 0)
        {
            MyLogger.LogWarning("[Managers] IAP catalog empty. Skip init.");
            return;
        }

        if (!_ugsInitialized)
        {
            if (_ugsInitializing)
            {
                MyLogger.Log("[Managers] UGS is already initializing.");
                return;
            }

            _ugsInitializing = true;

            try
            {
                await UnityServices.InitializeAsync();
                _ugsInitialized = true;
                MyLogger.Log("[Managers] UGS Initialized Success");
            }
            catch (Exception e)
            {
                Debug.LogError("[Managers] UGS Initialized Failed: " + e.Message);
                Debug.LogException(e);
                _ugsInitializing = false;
                return;
            }

            _ugsInitializing = false;
        }
        
        _iapManager.Init(catalog);
        
    }
#endif
    
    #if CHAT
    private void InitChatClient()
    {
        // 1) Client
        if (_chatClient == null)
        {
            _chatClient = GetComponentInChildren<UnityChatClient>(true);
            if (_chatClient == null)
            {
                var go = new GameObject("UnityChatClient");
                go.transform.SetParent(transform, false);
                _chatClient = go.AddComponent<UnityChatClient>();
            }
        }

        _chatClient.OnSystemMessage -= OnChatSystemMessage;
        _chatClient.OnSystemMessage += OnChatSystemMessage;

        // 2) History
        if (_chatHistory == null)
        {
            _chatHistory = GetComponentInChildren<ChatHistory>(true);
            if (_chatHistory == null)
            {
                var go = new GameObject("ChatHistory");
                go.transform.SetParent(transform, false);
                _chatHistory = go.AddComponent<ChatHistory>();
            }
        }

        // 3) 연결
        _chatHistory.Initialize(_chatClient);
    }
    
    private void OnChatSystemMessage(string msg)
    {
        MyLogger.Log($"[Chat] {msg}");
    }
    
    #endif
    
    #if SINGULAR
    private void EnsureSingularManager()
    {
        if (_singularManager != null) return;

        _singularManager = GetComponentInChildren<SingularManager>(true);
        if (_singularManager == null)
        {
            var go = new GameObject("SingularManager");
            go.transform.SetParent(this.transform, worldPositionStays: false);
            _singularManager = go.AddComponent<SingularManager>();
        }
    }
    #endif
    
    #if CRASHLYTICS && (UNITY_ANDROID || UNITY_IOS)
        public async UniTask InitFirebaseCrashlytics()
        {
            if (_crashlytics == null)
            {
                _crashlytics = GetComponentInChildren<FirebaseCrashlyticsManager>(true);
                if (_crashlytics == null)
                {
                    var go = new GameObject("FirebaseCrashlyticsManager");
                    go.transform.SetParent(this.transform, false);
                    _crashlytics = go.AddComponent<FirebaseCrashlyticsManager>();
                }
            }
            await _crashlytics.InitializeAsync();
        }
    #else
        public UniTask InitFirebaseCrashlytics() => UniTask.CompletedTask;
    #endif
        
    private void Update()
    {
#if UNITY_ANDROID
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _uiMgr.HandleAndroidBackButton();
        }
#endif
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR

#endif
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 모든 씬에 존재하게 
        if(GetUIManager().LoadingCanvas != null)
        {
            GameObject loadingGo = GetUIManager().LoadingCanvas.transform.Find("ServerLoadingUI").gameObject;
            if (loadingGo != null)
            {
                _loadingUI = loadingGo.GetComponent<LoadingUI>();
                if (_loadingUI != null)
                {
                    _loadingUI.Hide();

                }
            }
            else
                _loadingUI = null;
        }
    }

#if ADMOB && (UNITY_ANDROID || UNITY_IOS)
    private AdmobManager _admob;
    // 외부(PatchSceneManager)에서 호출할 수 있도록 public으로 변경합니다.
    public void InitAdmob()
    {
        if (_admob == null)
        {
            _admob = GetComponentInChildren<AdmobManager>(true);
            if (_admob == null)
            {
                var go = new GameObject("AdmobManager");
                go.transform.SetParent(this.transform, worldPositionStays: false); 
                _admob = go.AddComponent<AdmobManager>();
            }
        }

        // 실제 SDK 초기화 및 리워드 광고 사전 로드 수행
        _admob.Initialize();   
    }
#else
    // 다른 플랫폼이거나 ADMOB 심볼이 없을 경우 에러 방지를 위한 빈 함수
    public void InitAdmob() { /* no-op */ }
#endif

#if MESSAGING && (UNITY_ANDROID || UNITY_IOS)
    private FirebaseMessagingManager _messaging;

    public async UniTask InitFirebaseMessaging()
    {
        if (_messaging == null)
        {
            _messaging = GetComponentInChildren<FirebaseMessagingManager>(true);
            if (_messaging == null)
            {
                var go = new GameObject("FirebaseMessagingManager");
                go.transform.SetParent(this.transform, worldPositionStays: false); // Managers 하위로
                _messaging = go.AddComponent<FirebaseMessagingManager>();
            }
        }

        await _messaging.InitializeAsync();
    }
#else
    private UniTask InitFirebaseMessaging() => UniTask.CompletedTask;
#endif

#if ANALYTICS && (UNITY_ANDROID || UNITY_IOS)
    private FirebaseAnalyticsManager _analytics;

    public async UniTask InitFirebaseAnalytics()
    {
        if (_analytics == null)
        {
            _analytics = GetComponentInChildren<FirebaseAnalyticsManager>(true);
            if (_analytics == null)
            {
                var go = new GameObject("FirebaseAnalyticsManager");
                go.transform.SetParent(this.transform, worldPositionStays: false); // Managers 하위
                _analytics = go.AddComponent<FirebaseAnalyticsManager>();
            }
        }

        await _analytics.InitializeAsync();

#if ANALYTICS && (UNITY_ANDROID || UNITY_IOS)      
        AnalyticsLogEventHelper.LogEvent(AnalyticsEventNames.StartApp);
#endif
        
    }
#else
    private UniTask InitFirebaseAnalytics() => UniTask.CompletedTask;
#endif
    
#if IAP
    public void InitializeIAP_FromDB()
    {
        if (_iapManager != null && _iapManager.IsReady) 
        {
            MyLogger.Log("[Managers] IAP is already ready. Skip init.");
            return;
        }

        var catalog = new List<IapProductDef>();
        var unique = new HashSet<string>(System.StringComparer.Ordinal);

        // 해결: GetSimpleDBManager()를 빼고 ClientLocalDB_Simple 클래스명으로 직접 호출합니다.
        var midCashDb = ClientLocalDB_Simple.GetDB<MidCashShop>(DBKey.MidCashShop);
        if (midCashDb != null)
        {
            foreach (var item in midCashDb.Values)
            {
                if (item == null || string.IsNullOrEmpty(item.ProductID)) continue;
                if (!unique.Add(item.ProductID)) continue;
                catalog.Add(new IapProductDef { ProductId = item.ProductID });
            }
        }

        var passDb = ClientLocalDB_Simple.GetDB<PassGroup>(DBKey.PassGroup);
        if (passDb != null)
        {
            foreach (var item in passDb.Values)
            {
                if (item == null || string.IsNullOrEmpty(item.ProductID)) continue;
                if (!unique.Add(item.ProductID)) continue;
                catalog.Add(new IapProductDef { ProductId = item.ProductID });
            }
        }

        var limitDb = ClientLocalDB_Simple.GetDB<LimitedShop>(DBKey.LimitedShop);
        if (limitDb != null)
        {
            foreach (var item in limitDb.Values)
            {
                if (item == null || string.IsNullOrEmpty(item.ProductID)) continue;
                if (!unique.Add(item.ProductID)) continue;
                catalog.Add(new IapProductDef { ProductId = item.ProductID });
            }
        }

        if (catalog.Count > 0)
        {
            InitIapWithCatalog(catalog);
            MyLogger.Log($"[Managers] IAP Init requested from DB. Count={catalog.Count}");
        }
    }
    
    private void RegisterShopItemsFromDB()
    {
        if (_shopManager == null) return;

        var items = new List<ProductShopData>();
        var unique = new HashSet<string>(System.StringComparer.Ordinal);

        void Collect<T>(DBKey key) where T : ProductShopData
        {
            var db = ClientLocalDB_Simple.GetDB<T>(key);
            if (db == null) return;
            foreach (var item in db.Values)
            {
                if (item == null || string.IsNullOrEmpty(item.ProductID)) continue;
                if (!unique.Add(item.ProductID)) continue;
                items.Add(item);
            }
        }

        Collect<MidCashShop>(DBKey.MidCashShop);
        Collect<PassGroup>(DBKey.PassGroup);
        Collect<LimitedShop>(DBKey.LimitedShop);

        _shopManager.RegisterShopItems(items);
        MyLogger.Log($"[Managers] RegisterShopItemsFromDB done. Count={items.Count}");
    }
    
#endif
    
    
    #region GET

    public UserInfoData UserInfo()
    {
        return _userInfoData;
    }

    public void CreateUserInfo()
    {
        _userInfoData = new UserInfoData();
        _userInfoData.Init();
    }

    public JoystickController GetJoystick()
    {
        return joystickController;
    }

    public UIManager GetUIManager()
    {
        return _uiMgr;
    }

    public MapManager GetMapManager()
    {
        return _mapMgr;
    }

    public ResourceObjectManager GetResObjectManager()
    {
        return _resMgr;
    }

    public ObjectUnitManager GetObjectUnitManager()
    {
        return _objectUnitMgr;
    }

    public ClientLocalDB_Simple GetSimpleDBManager()
    {
        return _simpleDB;
    }

    public CameraManager GetCameraManager()
    {
        return _cameraManager;
    }

    public SaveManager GetSaveManager()
    {
        return _saveManager;
    }
    
    public BestHttp_GameManager GetServerManager()
    {
        return _serverManager;
    }

    public EconomySystem GetEconomySystem()
    {
        return _economySystem;
    }
    // public PassiveSkillManager GetPassiveSkillFactory()
    // {
    //     return _passiveSkillFactory;
    // }

    public AtlasManager GetAtlasManager()
    {
        return _atlasManager;
    }

    public LoadingUI GetLoadingUI()
    {
        return _loadingUI;
    }

    public SyncCurrencyManager GetSyncCurrencyManager()
    {
        return _syncCurrencyManager;
    }
    
    public TreasureBoxManager GetTreasureBoxManager()
    {
        return _treasureBoxManager;
    }

    public UITutorialManager GetTutorialManager()
    {
        return _tutorialManager;
    }
    #endregion
}
