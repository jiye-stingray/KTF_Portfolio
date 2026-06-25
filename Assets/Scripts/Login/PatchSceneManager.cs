
#if ANALYTICS
using AnalyticsLogEventHelp;
#endif

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Balaso;
using Newtonsoft.Json;
using System;
using UnityEngine.Networking;

public class ServerConfig
{
    public string aos_version;
    public string ios_version;
    public bool is_aos_review_active;
    public bool is_ios_review_active;
}
public class PatchSceneManager : MonoBehaviour
{
    // 실제 라이브 CDN에 업로드된 server_config.json의 URL 주소
    private const string CDN_CONFIG_URL = "d2115kwiy2o8n.cloudfront.net/server_config.json";
    
    [SerializeField] private DownloadWindow downloadWindow;
    [SerializeField] private AppTrackingTransparencyObject attObj;

    private async void Start()
    {
        MyLogger.Log("[PatchScene] Start 루틴 시작");

        // 1) iOS ATT 요청만 수행 (기다리지는 않음)
        RequestATTIfNeeded();

        // 2) SDK 초기화
        InitializeSingular();
        Managers.Instance.InitAdmob();
        await InitFirebaseSequenceInit();
        
        // Get AD Info
        await AdvertisingIdHelper.RefreshAsync();
        
        // 1. [필수] 다른 어떤 통신보다 먼저 CDN에서 서버 라우팅 상태를 받아옵니다.
#if !DEV_SERVER_SET        
        await CheckServerConfigAndRouteAsync();
#endif
        // 3) 리소스 다운로드 및 데이터 로드
#if ADDRESSABLES_ENABLED
        MyLogger.Log("[PatchScene] 어드레서블 다운로드 시작");
        await downloadWindow.StartDownloadRoutine();
        MyLogger.Log("[PatchScene] 게임 데이터 로드 시작");
        await downloadWindow.StartBundleLoadRoutine();
#else
        MyLogger.Log("[PatchScene] 게임 데이터 로드 시작");
        Managers.Instance.GetSimpleDBManager().LoadAll();
#endif
// #if IAP
//         InitIapAfterDbLoadAll();
// #endif
        await UniTask.Yield();
        SceneManager.LoadScene("TitleScene");
    }

    private void RequestATTIfNeeded()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (attObj != null)
        {
            MyLogger.Log("[PatchScene] Request ATT");
            //attObj.RequestTrackingAuthorization();
        }
        else
        {
            MyLogger.LogWarning("[PatchScene] ATT Object is null.");
        }
#endif
    }

    private void InitializeSingular()
    {
#if SINGULAR
        var sdk = FindAnyObjectByType<Singular.SingularSDK>(FindObjectsInactive.Include);

        MyLogger.Log($"[PatchScene] SingularManager exists = {Managers.Instance.Singular != null}");
        MyLogger.Log($"[PatchScene] SingularSDK exists = {sdk != null}");

        if (sdk != null)
        {
            MyLogger.Log($"[PatchScene] SingularSDK activeInHierarchy = {sdk.gameObject.activeInHierarchy}");
            MyLogger.Log($"[PatchScene] SingularSDK enabled = {sdk.enabled}");
            MyLogger.Log($"[PatchScene] SingularSDK object name = {sdk.gameObject.name}");
        }

        if (Managers.Instance.Singular != null)
        {
            Managers.Instance.Singular.Initialize();
            MyLogger.Log("[PatchScene] Singular Initialize requested.");
        }
        else
        {
            MyLogger.LogWarning("[PatchScene] SingularManager is null.");
        }
#endif
    }

// #if IAP
    // private void InitIapAfterDbLoadAll()
    // {
    //     var catalog = new List<IapProductDef>();
    //     var unique = new HashSet<string>(System.StringComparer.Ordinal);
    //
    //     var midCashDb = ClientLocalDB_Simple.GetDB<MidCashShop>(DBKey.MidCashShop);
    //     if (midCashDb != null)
    //     {
    //         foreach (var item in midCashDb.Values)
    //         {
    //             if (item == null || string.IsNullOrEmpty(item.ProductID)) continue;
    //             if (!unique.Add(item.ProductID)) continue;
    //
    //             catalog.Add(new IapProductDef { ProductId = item.ProductID });
    //         }
    //     }
    //
    //     var passDb = ClientLocalDB_Simple.GetDB<PassGroup>(DBKey.PassGroup);
    //     if (passDb != null)
    //     {
    //         foreach (var item in passDb.Values)
    //         {
    //             if (item == null || string.IsNullOrEmpty(item.ProductID)) continue;
    //             if (!unique.Add(item.ProductID)) continue;
    //
    //             catalog.Add(new IapProductDef { ProductId = item.ProductID });
    //         }
    //     }
    //
    //     var limitDb = ClientLocalDB_Simple.GetDB<LimitedShop>(DBKey.LimitedShop);
    //     if (limitDb != null)
    //     {
    //         foreach (var item in limitDb.Values)
    //         {
    //             if (item == null || string.IsNullOrEmpty(item.ProductID)) continue;
    //             if (!unique.Add(item.ProductID)) continue;
    //
    //             catalog.Add(new IapProductDef { ProductId = item.ProductID });
    //         }
    //     }
    //
    //     if (catalog.Count > 0)
    //     {
    //         Managers.Instance.InitIapWithCatalog(catalog);
    //         MyLogger.Log($"[Patch] IAP Init requested. Count={catalog.Count}");
    //     }
    // }
// #endif

    private async UniTask InitFirebaseSequenceInit()
    {
        if (!await FirebaseReady.EnsureAsync()) return;
#if CRASHLYTICS
        await Managers.Instance.InitFirebaseCrashlytics();
#endif
        
#if ANALYTICS
        await Managers.Instance.InitFirebaseAnalytics();
        AnalyticsLogEventHelper.LogEvent(AnalyticsEventNames.StartApp);
        AnalyticsLogEventHelper.LogEvent(AnalyticsEventNames.StartLogo);
#endif

#if MESSAGING
        await Managers.Instance.InitFirebaseMessaging();
#endif
        
    }
    
    /// <summary>
    /// 게임 시작 시 가장 먼저 호출되어 CDN 설정을 기반으로 서버 주소를 라우팅합니다.
    /// </summary>
    public async UniTask CheckServerConfigAndRouteAsync()
    {
        // CDN 특유의 주소 캐싱을 방지하기 위해 뒤에 타임스탬프(?t=)를 쿼리스트링으로 붙여서 요청합니다.
        string requestUrl = $"{CDN_CONFIG_URL}?t={DateTime.UtcNow.Ticks}";

        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            try
            {
                await request.SendWebRequest().ToUniTask();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonText = request.downloadHandler.text;
                    ServerConfig config = JsonConvert.DeserializeObject<ServerConfig>(jsonText);
                    Managers.Instance.GetServerManager()._serverConfig = config; 
                    
                    // 현재 빌드된 앱의 버전 (예: 1.0.1)
                    var appVersion = new Version(Application.version);
                    var serverVersion = new Version();
                    bool isReviewVersion = false;
                    bool reviewActive; 
#if UNITY_ANDROID
                    serverVersion = new System.Version(config.aos_version);
                    reviewActive = config.is_aos_review_active;
#elif UNITY_IOS
                    serverVersion = new System.Version(config.ios_version);
                    reviewActive = config.is_ios_review_active;
#endif
                    isReviewVersion = appVersion > serverVersion && reviewActive;
                    // [일치 여부에 따라 주소 및 플래그 스위칭 실행]
                    RestAPIURL.SetReviewMode(isReviewVersion);
                    DownloadController.SetReviewMode(isReviewVersion);
                }
                else
                {
                    MyLogger.LogError($"[Config] CDN 로드 실패 ({request.error}). 안전을 위해 기본 라이브 서버로 진행합니다.");
                    RestAPIURL.SetReviewMode(false); // 통신 실패 시 방어 코드로 라이브 서버 강제 지정
                }
            }
            catch (Exception ex)
            {
                MyLogger.LogError($"[Config] CDN 연동 중 예외 발생: {ex.Message}. 안전을 위해 기본 라이브 서버로 진행합니다.");
                RestAPIURL.SetReviewMode(false); // 예외 발생 시 방어 코드로 라이브 서버 강제 지정
            }
        }
    }
}
