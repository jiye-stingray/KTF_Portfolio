// using Assets.SimpleSignIn.Google.Scripts;

using CHAT;
using System;
// using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using UnityEngine.Networking;

#if ANALYTICS
using AnalyticsLogEventHelp;
#endif

public class TitleManager : MonoBehaviour
{
    public enum ELoginProvider { Guest = 0, Google = 1, Apple = 2 }

    
    bool exitProcess = false;
    bool isDoneSocialId = false;
    bool isBGTouch = false;
 
    [SerializeField] private SocialLogin socialLogin;
    
    [Header("UI")] 
    [SerializeField] private GameObject goGoogleLogin;
    [SerializeField] private GameObject goAppleLogin;
    [SerializeField] private GameObject goGuestLogin = null;
    [SerializeField] private GameObject logoutBtn;
    
    [Header("Account Provider Icon UI")]
    [SerializeField] private Image currentProviderIcon = null;
    
    // [SerializeField] private Button switchAccountBtn;
    
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI copyrightText = null;
    [SerializeField] private TextMeshProUGUI versionText = null;
    [SerializeField] private TextMeshProUGUI serverText = null;
    
    [SerializeField] GameObject backTouchImg = null;
    [SerializeField] GameObject goTouchScreen = null;
    
    // Permission Check 
    private PermissionsManager permissionsManager;
    
    [SerializeField] PolicyUIManager policyUIManager;
    [SerializeField] private CreateNicknameUI _nicknameUI;
    [SerializeField] private GuestLogoutUI guestLogoutHandler;
    
    // 비동기 흐름 제어용 TCS
    private UniTaskCompletionSource<bool> _loginTcs;
    private UniTaskCompletionSource<bool> _touchTcs;

    //--
    private UniTaskCompletionSource<bool> _serverLoginTcs;
    private bool _isCreatingUser;
    
    private bool _allowLoginButtonVisible = false;

    // 객체 파괴 시 자동 취소되는 토큰 (Start의 모든 await에 전파)
    private CancellationToken _ct;

#if LOGIN_TEST
    // =====================================================================
    // [검수 전용] LOGIN_TEST 강제 로그인
    // ---------------------------------------------------------------------
    // Player Settings > Other Settings > Scripting Define Symbols 에
    // "LOGIN_TEST" 를 추가하면, 소셜 인증을 건너뛰고 아래 값으로 강제 로그인합니다.
    // 심볼을 다시 제거하면 모든 코드가 원래의 정상 로그인 흐름으로 그대로 복귀합니다.
    // ※ 라이브 배포 전 반드시 LOGIN_TEST 심볼을 제거하세요!
    // =====================================================================
    private const int    LOGIN_TEST_PLATFORM  = 0;   // 0: Guest, 1: Google, 2: Apple
    private const string LOGIN_TEST_SOCIAL_ID = "guest_여기에_대상유저_social_id"; // platform 0/2 용
    private const string LOGIN_TEST_JWT       = "";  // platform 1(Google) 용 JWT 문자열

    // 검수용 계정 정보를 GameDefineData 에 주입한다.
    // 성공 시 true 를 반환하며, 이후 기존 서버 로그인 흐름이 이 값을 그대로 사용한다.
    private bool TryApplyLoginTestAccount()
    {
        int platform    = LOGIN_TEST_PLATFORM;
        string socialId = LOGIN_TEST_SOCIAL_ID;
        string jwt      = LOGIN_TEST_JWT;

        // 플랫폼별 필수 값 검증 (Google 은 JWT, 그 외는 social id)
        if (platform == 1)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                MyLogger.LogError("[LOGIN_TEST] platform=1(Google) 인데 LOGIN_TEST_JWT 가 비어 있습니다.");
                return false;
            }
        }
        else
        {
            if (string.IsNullOrEmpty(socialId))
            {
                MyLogger.LogError("[LOGIN_TEST] LOGIN_TEST_SOCIAL_ID 가 비어 있습니다.");
                return false;
            }
        }

        // 기존 저장 정보와 충돌하지 않도록 초기화 후 주입
        // (encrypt:false 로 저장 → 서버 로그인부의 decrypt:false 읽기와 짝을 맞춤)
        GameDefineData.DeleteData(Define.KEY_SERVER_JWT);
        GameDefineData.SetInt(Define.KEY_PLATFORM, platform);

        if (!string.IsNullOrEmpty(socialId))
            GameDefineData.SetString(Define.KEY_USER_SOCIAL_ID, socialId, encrypt: false);

        if (platform == 1)
            GameDefineData.SetString(Define.KEY_SERVER_JWT, jwt, encrypt: false);

        GameDefineData.DeleteData("KEY_PLATFORM_PENDING");
        GameDefineData.Save();

        isDoneSocialId = true;

        MyLogger.LogWarning($"[LOGIN_TEST] 검수용 강제 로그인 적용됨 (platform={platform}). " +
                            "※ 라이브 배포 전 LOGIN_TEST 심볼을 제거하세요!");
        return true;
    }
#endif

    async void Start()
    {
        // 객체 파괴 시 자동 취소되는 토큰을 진입 직후 1회 캐싱 (OnDestroy 이후 호출 금지)
        _ct = this.GetCancellationTokenOnDestroy();

        try
        {
            HideLoginButtons();

            InitializeUI();

#if !DEV_SERVER_SET
            await CheckServerVersionUpdateAsync(_ct);
#endif
            HideLoginButtons();

            // CheckFirstLaunchAndCleanUp();

            await policyUIManager.WaitForAcceptAsync().AttachExternalCancellation(_ct);

            #if UNITY_ANDROID
                    // Android 푸시 권한 확인 및 요청
                    // 2026.05.21 (사용 안해서 주석처리)
/*                    if (permissionsManager != null)
                        await permissionsManager.CheckAndRequestAll();*/
            #endif

        #if ANALYTICS && (UNITY_ANDROID || UNITY_IOS)
            AnalyticsLogEventHelper.LogEvent(AnalyticsEventNames.AppTitleMainStart);
        #endif

            // #if ANALYTICS
            // AnalyticsLogEventHelper.LogEvent();
            // #endif


            DoResetGameData();

            socialLogin.OnLoginSucceeded += HandleSocialLoginSuccess;
            socialLogin.OnLoginFailed += HandleSocialLoginFailure;

            await UniTask.Yield(_ct);

            // BGM Play
            int bgmSetting = PlayerPrefs.GetInt(ToggleSettingType.BGM.ToString(), 1);
            if (bgmSetting == 1) // 1을 켜짐으로 약속했다면 명확하게 비교
            {
                await Managers.Instance.Sound.PlayBGMAsync($"Title_OST2").AttachExternalCancellation(_ct);
            }
            else
            {
                Managers.Instance.Sound.StopBGM();
            }

            HideLoginButtons();

            // 소셜 로그인 처리
            bool isSocialLoginSuccess = await HandleSocialLogin();
            if (!isSocialLoginSuccess) return;

            // 이후 전체 플로우 (서버 로그인 → 닉네임 → 터치 → 씬 전환)
            await HandleLoginFlowAfterSuccess();
        }
        catch (OperationCanceledException)
        {
            // 씬 이탈/객체 파괴로 인한 정상 취소 — 조용히 무시
            MyLogger.Log("[TitleManager] Start 취소됨 (객체 파괴/씬 전환).");
        }
        catch (Exception e)
        {
            // 예상치 못한 예외는 삼키지 말고 반드시 노출
            Debug.LogError($"[TitleManager] Start 흐름 예외: {e}");

            // 필요 시 로그인 화면으로 복구 (객체가 살아있을 때만 의미)
            if (this != null)
            {
                SetLoginButtonVisibility(true);
                SetMainUIVisibility(false);
            }
        }
    }
    
    private void OnDestroy()
    {
        if (socialLogin != null)
        {
            socialLogin.OnLoginSucceeded -= HandleSocialLoginSuccess;
            socialLogin.OnLoginFailed -= HandleSocialLoginFailure;
        }

    }
    
    private void ShowLoginButtons()
    {
        _allowLoginButtonVisible = true;
        SetLoginButtonVisibility(true);
    }
    
    public async UniTask CheckServerVersionUpdateAsync(CancellationToken ct = default)
    {
        ServerConfig config = Managers.Instance.GetServerManager()._serverConfig;
                    
        // 현재 빌드된 앱의 버전 (예: 1.0.1)
        var appVersion = new Version(Application.version);
        var serverVersion = new Version();
#if UNITY_ANDROID
        serverVersion = new System.Version(config.aos_version);
#elif UNITY_IOS
        serverVersion = new System.Version(config.ios_version);
#endif
        if (appVersion < serverVersion)
        {
            var tcs = new UniTaskCompletionSource();
            Managers.Instance.GetUIManager().ShowErrorPopUp(EErrorCloseType.Confirm, "업데이트가 필요합니다.", "확인을 누르시면 스토어로 이동합니다.",
                () =>
                {
#if REVIEW
                    ReviewHandler.Instance.OpenMarketPageForUpdate();
#endif
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
	                Application.Quit();
#endif
                    tcs.TrySetResult();
                });
            await tcs.Task.AttachExternalCancellation(ct);
        }
    }
    
    private void HideLoginButtons()
    {
        _allowLoginButtonVisible = false;

        if (goGuestLogin != null) goGuestLogin.SetActive(false);
        if (goGoogleLogin != null) goGoogleLogin.SetActive(false);
        if (goAppleLogin != null) goAppleLogin.SetActive(false);
    }
    
    #region Function
    
    void InitializeUI()
    {
        #if DEV_SERVER_SET
            serverText.text = "DEV";
            MyLogger.Log("<color=yellow>SERVER MODE: DEV</color>");
        #elif ONE
            serverText.text = "ONE";
            MyLogger.Log("<color=red>SERVER MODE: DEV</color>");
        #else
            serverText.text = "LIVE";
            MyLogger.Log("<color=cyan>SERVER MODE: LIVE</color>");
        #endif
        
    
        if (versionText != null)
        {
            versionText.text = string.Format("ver.{0}", Application.version);
        }
 
        
        if (goTouchScreen == null)
            Debug.LogError("goTouchScreen is not assigned in the Inspector!");

        if (backTouchImg == null)
            Debug.LogError("backTouchImg is not assigned in the Inspector!");
                
        goTouchScreen.SetActive(false);
        backTouchImg.SetActive(false);

        if (goGuestLogin != null)
        {
            Button guestBtn = goGuestLogin.GetComponent<Button>();
            if (guestBtn != null)
            {
                guestBtn.onClick.RemoveAllListeners(); // 중복 방지
                guestBtn.onClick.AddListener(OnClickGuestLogin);
            }
        }
        
        SetLoginButtonVisibility(false);
        SetMainUIVisibility(false);
        
        if (currentProviderIcon != null)
        {
            currentProviderIcon.gameObject.SetActive(false);
        }
        
        MyLogger.Log("InitializeUI");
    }

    void DoResetGameData()
    {
        // 사운드 및 기타 게임 리소스 로드 로직 
        MyLogger.Log("DoResetGameData"); 
    }
    
    private void DoGameResourceLoad()
    {
        MyLogger.Log("DoGameResourceLoad"); 
        // 게임 리소스 로드
    }
    
    // 게스트 보여지는 
    private void SetLoginButtonVisibility(bool isActive)
    {
        if (isActive && !_allowLoginButtonVisible)
        {
            isActive = false;
        }
    
        #if UNITY_ANDROID 
                if (goGuestLogin != null) goGuestLogin.SetActive(isActive);
    
        #if ONE
            if (goGoogleLogin != null) goGoogleLogin.SetActive(false);
        #else
                if (goGoogleLogin != null) goGoogleLogin.SetActive(isActive);
        #endif
    
                if (goAppleLogin != null) goAppleLogin.SetActive(false);
    
        #elif UNITY_IOS
            if (goGuestLogin != null) goGuestLogin.SetActive(isActive);
            if (goGoogleLogin != null) goGoogleLogin.SetActive(isActive);
            if (goAppleLogin != null) goAppleLogin.SetActive(isActive);
    
        #else
            if (goGuestLogin != null) goGuestLogin.SetActive(isActive);
            if (goGoogleLogin != null) goGoogleLogin.SetActive(false);
            if (goAppleLogin != null) goAppleLogin.SetActive(false);
        #endif
    }
    
    // private void SetLoginButtonVisibility(bool isActive)
    // {
    //     if (isActive && !_allowLoginButtonVisible)
    //     {
    //         isActive = false;
    //     }
    //
    //     // 에디터 환경에서는 테스트를 위해 활성화, 그 외 빌드(실기기)에서는 무조건 숨김
    //     #if UNITY_EDITOR
    //             if (goGuestLogin != null) goGuestLogin.SetActive(isActive);
    //     #else
    //         if (goGuestLogin != null) goGuestLogin.SetActive(false);
    //     #endif
    //
    //     #if UNITY_ANDROID 
    //     #if ONE
    //                 if (goGoogleLogin != null) goGoogleLogin.SetActive(false);
    //     #else
    //             if (goGoogleLogin != null) goGoogleLogin.SetActive(isActive);
    //     #endif
    //
    //             if (goAppleLogin != null) goAppleLogin.SetActive(false);
    //
    //     #elif UNITY_IOS
    //                 if (goGoogleLogin != null) goGoogleLogin.SetActive(isActive);
    //                 if (goAppleLogin != null) goAppleLogin.SetActive(isActive);
    //
    //     #else
    //                 if (goGoogleLogin != null) goGoogleLogin.SetActive(false);
    //                 if (goAppleLogin != null) goAppleLogin.SetActive(false);
    //     #endif
    // }
    
    // private void SetLoginButtonVisibility(bool isActive)
    // {
    //     if (isActive && !_allowLoginButtonVisible)
    //     {
    //         isActive = false;
    //     }
    //
    //     // 기획/정책 변경으로 게스트 로그인 버튼은 플랫폼 무관 무조건 숨김 처리
    //     if (goGuestLogin != null) goGuestLogin.SetActive(false);
    //
    //     #if UNITY_ANDROID 
    //     #if ONE
    //             if (goGoogleLogin != null) goGoogleLogin.SetActive(false);
    //     #else
    //             if (goGoogleLogin != null) goGoogleLogin.SetActive(isActive);
    //     #endif
    //
    //             if (goAppleLogin != null) goAppleLogin.SetActive(false);
    //
    //     #elif UNITY_IOS
    //                 if (goGoogleLogin != null) goGoogleLogin.SetActive(isActive);
    //                 if (goAppleLogin != null) goAppleLogin.SetActive(isActive);
    //
    //     #else
    //                 if (goGoogleLogin != null) goGoogleLogin.SetActive(false);
    //                 if (goAppleLogin != null) goAppleLogin.SetActive(false);
    //     #endif
    // }
    
    // private void SetLoginButtonVisibility(bool isActive)
    // {
    //     if (isActive && !_allowLoginButtonVisible)
    //     {
    //         isActive = false;
    //     }
    //
    //     if (goGuestLogin != null) goGuestLogin.SetActive(isActive);
    //
    //     #if UNITY_ANDROID 
    //     #if ONE
    //                 if (goGoogleLogin != null) goGoogleLogin.SetActive(false);
    //     #else
    //             if (goGoogleLogin != null) goGoogleLogin.SetActive(isActive);
    //     #endif
    //
    //             if (goAppleLogin != null) goAppleLogin.SetActive(false);
    //
    //     #elif UNITY_IOS
    //             if (goGoogleLogin != null) goGoogleLogin.SetActive(isActive);
    //             if (goAppleLogin != null) goAppleLogin.SetActive(isActive);
    //
    //     #else
    //             // 에디터나 기타 플랫폼에서도 게스트 로그인은 정상적으로 작동할 수 있도록 활성화
    //             if (goGoogleLogin != null) goGoogleLogin.SetActive(false);
    //             if (goAppleLogin != null) goAppleLogin.SetActive(false);
    //     #endif
    // }
    
    private void SetMainUIVisibility(bool isVisible)
    {
        if (logoutBtn != null)
            logoutBtn.SetActive(isVisible);
    
        if (goTouchScreen != null)
            goTouchScreen.SetActive(isVisible);
        
        if (currentProviderIcon != null)
        {
            // 데이터 저장소에 기록된 플랫폼 번호 확인 (0: 게스트, 1: 구글, 2: 애플)
            int platform = GameDefineData.GetInt(Define.KEY_PLATFORM, -1);

            if (isVisible && platform >= 0)
            {
                // 여기서 로드 및 표시 함수 호출!
                UpdateProviderIcon(platform);
            }
            else
            {
                currentProviderIcon.gameObject.SetActive(false);
            }
        }
        
    }
    
    private void UpdateProviderIcon(int platform)
    {
        if (currentProviderIcon == null) return;

        string spriteName = string.Empty;
        switch ((ELoginProvider)platform)
        {
            case ELoginProvider.Guest:
                spriteName = "icon_guest";
                break;
            case ELoginProvider.Google:
                spriteName = "icon_google";
                break;
            case ELoginProvider.Apple:
                spriteName = "icon_apple";
                break;
            default:
                currentProviderIcon.gameObject.SetActive(false);
                return;
        }

        // 경로 명시: Resources/Sprite/UI/Common/스프라이트명 (Resources 폴더 내부는 확장자 생략)
        string fullPath = $"Sprite/UI/Common/{spriteName}";
        Sprite loadedSprite = Resources.Load<Sprite>(fullPath);

        if (loadedSprite != null)
        {
            currentProviderIcon.sprite = loadedSprite;
            currentProviderIcon.gameObject.SetActive(true);
            MyLogger.Log($"[AccountIcon] 계정 아이콘 표시 성공: {fullPath}");
        }
        else
        {
            currentProviderIcon.gameObject.SetActive(false);
            MyLogger.LogError($"[AccountIcon] 아이콘 로드 실패 (경로를 확인하세요): {fullPath}");
        }
    }
    
    public void Logout()
    {
        MyLogger.Log("로그아웃 시작");

        socialLogin.SignOut();
        
        // 저장된 로그인 정보 삭제
        GameDefineData.DeleteAll();
        GameDefineData.Save();
        
        
        // 로그인 UI 활성화
        SetLoginButtonVisibility(true);
        SetMainUIVisibility(false);
        // goTouchScreen.SetActive(false);
    
        // 타이틀 씬으로 이동
        SceneManager.LoadScene("TitleScene");
    }

    public async void SwitchAccount()
    {
        MyLogger.Log("계정 전환 시작");
        
        SetLoginButtonVisibility(true);
        SetMainUIVisibility(false);

        // 기존 로그인 정보 삭제
        GameDefineData.DeleteAll();
        GameDefineData.Save();
        
        MyLogger.Log("새로운 계정으로 로그인 진행 대기");
        
        // 새로운 로그인 시도
        bool loginSuccess = await HandleSocialLogin();

        if (loginSuccess)
        {
            MyLogger.Log("새로운 계정 로그인 성공!");
            
            // UI 업데이트: 로그인 화면 숨기기, 메인 UI 표시
            SetLoginButtonVisibility(false);
            SetMainUIVisibility(true);
        }
        else
        {
            MyLogger.LogWarning("새로운 계정 로그인 실패! 다시 로그인 UI 활성화");
            SetLoginButtonVisibility(true);
        }
    }

    //by rainful 2025-08-08 계정 생성
    public void OnClickCreate()
    {
#if UNITY_EDITOR || DEV_SERVER_SET
        if (_isCreatingUser)
        {
            MyLogger.LogWarning("[Server] CreateUser already running. Skip manual create.");
            return;
        }

        Managers.Instance.GetServerManager().OnRequestCreateUser();
#else
    MyLogger.LogWarning("[Server] Manual create is disabled in release build.");
#endif
    }

    public void OnClickLogout()
    {
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuClose");
        
        int platform = GameDefineData.GetInt(Define.KEY_PLATFORM, -1);
        // MyLogger.Log("OnClickLogout = " + platform);
        
        if (platform == 0)
        {
            guestLogoutHandler?.ShowGuestLogoutPopup(this);
        }
        else
        {
            Logout(); // 일반 계정용
        }
    }
    
    public async void OnClickSwitchAccount()
    {
        MyLogger.Log("OnClickSwitchAccount");
        
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");
        
        SwitchAccount();
        MyLogger.Log("계정 전환 시작");
    }
    #endregion


    #region async Function
    async UniTask<bool> HandleSocialLogin()
    {
        exitProcess = false;
        bool result = await SocialLoginProcAsync();
        if (exitProcess) return false;
        return result;
    }

    async UniTask<bool> SocialLoginProcAsync()
    {
        MyLogger.Log("SocialLoginProcAsync() 시작됨");

        // SocialLoginProcAsync 진입 시점에는 일단 로그인 버튼을 확실히 숨김
        HideLoginButtons();

#if LOGIN_TEST
        // [검수 전용] 소셜 인증을 건너뛰고 지정한 계정 정보로 강제 로그인한다.
        // 주입 성공 시, 아래의 저장소 기반 자동 로그인 흐름을 그대로 이어받는다.
        if (TryApplyLoginTestAccount())
        {
            MyLogger.LogWarning("[LOGIN_TEST] 강제 로그인 흐름 진입 (소셜 인증 생략)");

            HideLoginButtons();

            if (goTouchScreen != null)
                goTouchScreen.SetActive(true);

            return true;
        }

        MyLogger.LogError("[LOGIN_TEST] 강제 로그인 값이 유효하지 않습니다. 일반 로그인 흐름으로 진행합니다.");
#endif

        // 저장된 소셜 ID 및 플랫폼 확인
        string socialId = GameDefineData.GetString(Define.KEY_USER_SOCIAL_ID, null);
        int platform = GameDefineData.GetInt(Define.KEY_PLATFORM, -1);

        MyLogger.Log($"저장된 로그인 정보 확인: socialId={socialId}, platform={platform}");

        // 이전 로그인 정보가 있으면 자동 로그인 시도
        if (!string.IsNullOrEmpty(socialId) && platform >= 0)
        {
            MyLogger.Log("기존 로그인 정보 확인됨. 자동 로그인 진행...");

            bool loginSuccess = await ProcessSocialLoginAsync(socialId, platform);

            if (loginSuccess)
            {
                MyLogger.Log("자동 로그인 성공! TouchToScreen 활성화");

                HideLoginButtons();

                if (goTouchScreen != null)
                    goTouchScreen.SetActive(true);

                return true;
            }

            MyLogger.LogWarning("자동 로그인 실패! 기존 로그인 정보 삭제 후 재로그인");

            GameDefineData.DeleteAll();
            GameDefineData.Save();

            HideLoginButtons();
        }

        // 여기까지 왔다는 것은 실제로 유저가 로그인 버튼을 눌러야 하는 상태
        MyLogger.Log("로그인 정보 없음 또는 자동 로그인 실패 → 로그인 버튼 활성화");

        isDoneSocialId = false;

        ShowLoginButtons();

        // 로그인 버튼 클릭 후 로그인 완료 대기
        bool newLoginSuccess = await WaitForUserLogin();

        if (newLoginSuccess)
        {
            MyLogger.Log("신규 로그인 성공! TouchToScreen 활성화");

            HideLoginButtons();

            // if (goTouchScreen != null)
            //     goTouchScreen.SetActive(true);

            return true;
        }

        MyLogger.LogWarning("로그인 실패! 다시 로그인 버튼 표시");

        // if (goTouchScreen != null)
        //     goTouchScreen.SetActive(false);

        // 실패한 경우에만 다시 로그인 선택 버튼 표시
        ShowLoginButtons();

        return false;
    }
    
    private async UniTask<bool> WaitForUserLogin()
    {
        MyLogger.Log("로그인 대기 중...");

        isDoneSocialId = false;  // 로그인 여부 초기화

        // 로그인 완료될 때까지 대기 (객체 파괴 시 취소)
        while (!isDoneSocialId)
        {
            await UniTask.Delay(100, cancellationToken: _ct);
        }

        return true;
    }
    
    private async UniTask WaitForUserTouch()
    {
        // [핵심 방어 코드] 이전 클릭의 잔재가 남아있을 수 있으므로 진입 시 무조건 false로 초기화
        isBGTouch = false; 

        // HandleLoginFlowAfterSuccess에서 이미 SetMainUIVisibility(true)를 호출했지만,
        // 명시적으로 한 번 더 보장해줘도 좋습니다.
        if (goTouchScreen != null)
            goTouchScreen.SetActive(true);
    
        if (backTouchImg != null)
            backTouchImg.SetActive(true);
        
        MyLogger.Log("Waiting for user touch...");

        while (!isBGTouch)
        {
            await UniTask.Delay(100, cancellationToken: _ct);
        }

        MyLogger.Log("User touched! Moving to next scene...");
        isBGTouch = false; // 씬 전환 직전에도 안전하게 false 처리
    }
     
    async UniTask TransitionToNextScene()
    {
        Loading.Load(Loading.Field);
        
        await WaitForSceneLoad("LoadingScene");
        await WaitForSceneLoad("FieldScene");

        MyLogger.Log("FieldScene 로드 완료");
    }
    
    async UniTask WaitForSceneLoad(string sceneName)
    {
        MyLogger.Log($"[{sceneName}] 씬 로딩 대기 중...");
        while (SceneManager.GetActiveScene().name != sceneName)
        {
            await UniTask.Delay(100, cancellationToken: _ct);
        }

        MyLogger.Log($"[{sceneName}] 씬 로드 완료");
    }
    
    private async UniTask<bool> ProcessSocialLoginAsync(string socialId = null, int platform = -1)
    {
        if (string.IsNullOrEmpty(socialId) || platform < 0)
        {
            var (success, newSocialId, newPlatform) = await SocialLogin.LoginAsync();

            if (!success)
                return false;

            socialId = newSocialId;
            platform = newPlatform;

            GameDefineData.SetString(Define.KEY_USER_SOCIAL_ID, socialId);
            GameDefineData.SetInt(Define.KEY_PLATFORM, platform);
            GameDefineData.Save();
        }
        else
        {
            // 🌟 2. 기존 유저인 경우 무조건 토큰이 유효한지 검사(갱신)하고 넘어가야 함!
            if (platform == 1) // 구글인 경우
            {
                // 🚨 수정된 부분: 대문자 SocialLogin 이 아니라 소문자 socialLogin 인스턴스로 호출!
                bool isTokenValid = await socialLogin.EnsureValidGoogleTokenAsync();
                
                if (!isTokenValid)
                {
                    MyLogger.LogError("구글 토큰 갱신 실패로 자동 로그인 불가");
                    return false; // 실패 처리하여 다시 로그인 버튼 띄우게 유도
                }
            }
            else if (platform == 2) // ★ 추가된 애플(Apple)인 경우
            {
                MyLogger.Log("애플 인증 상태(Credential) 유효성 검사 시작");
                // 애플은 검증을 위해 기존에 저장된 socialId (Apple UserId)가 필요합니다.
                bool isTokenValid = await socialLogin.EnsureValidAppleTokenAsync(socialId);
                
                if (!isTokenValid)
                {
                    MyLogger.LogError("애플 인증 만료(또는 연동 해제)로 자동 로그인 불가");
                    return false; // 실패 처리하여 다시 로그인 버튼 띄우게 유도
                }
            }
            // (애플인 경우 플랫폼 2에 대한 토큰 갱신 로직이 필요하다면 여기에 추가)
        }
        MyLogger.Log($"소셜 로그인 완료: {socialId}, platform = {platform}");
        return true;
    }

    private void MarkSocialLoginSuccess(string userId, int platform)
    {
        GameDefineData.SetString(Define.KEY_USER_SOCIAL_ID, userId);
        GameDefineData.SetInt(Define.KEY_PLATFORM, platform); // 0: Guest, 1: Google, 2: Apple
        GameDefineData.DeleteData("KEY_PLATFORM_PENDING");  // 성공했으니 PENDING 제거
        GameDefineData.Save();

        // [추가] Crashlytics에 유저 ID 세팅
        #if CRASHLYTICS && (UNITY_ANDROID || UNITY_IOS)
            Managers.Instance.Crashlytics.SetUserId(userId);
        #endif
        
        isDoneSocialId = true;                 // 성공
        HideLoginButtons();
        // goTouchScreen?.SetActive(true);
    }

    
    
    #region NickName
    private async UniTask NicknameAsync()
    {

        string nickname = GameDefineData.GetString("USER_NICKNAME", null);
        if (!string.IsNullOrEmpty(nickname))
            return;

        // (선택) 임시 저장 키가 있다면 먼저 사용
        string temp = GameDefineData.GetString("USER_NICKNAME_TEMP", null);
        if (!string.IsNullOrEmpty(temp))
        {
            GameDefineData.SetString("USER_NICKNAME", temp);
            GameDefineData.DeleteData("USER_NICKNAME_TEMP");
            GameDefineData.Save();
            return;
        }

        // UI 호출하여 닉네임 받기
        string newNick = await _nicknameUI.WaitForNicknameAsync();
        if (string.IsNullOrWhiteSpace(newNick))
        {
            MyLogger.LogWarning("[TitleManager] 빈 닉네임 입력 감지. 재시도 유도.");
            await NicknameAsync(); // 재요청
            return;
        }


        // 저장
        GameDefineData.SetString("USER_NICKNAME", newNick);
        GameDefineData.Save();
    }
    

    #endregion

    #endregion
    
    private async UniTask<bool> HandleServerLogin()
    {
        // 이미 서버 로그인 플로우가 진행 중이면 새 요청을 보내지 않고 기존 결과를 기다림
        if (_serverLoginTcs != null)
        {
            MyLogger.LogWarning("[Server] HandleServerLogin already running. Wait current task.");
            return await _serverLoginTcs.Task;
        }

        var tcs = new UniTaskCompletionSource<bool>();
        _serverLoginTcs = tcs;

        try
        {
            MyLogger.Log("[Server] HandleServerLogin() 시작됨");
            exitProcess = false;

            bool result = await GameServerProc();
            if (!result || exitProcess)
            {
                tcs.TrySetResult(false);
                return await tcs.Task;
            }

            var serverMgr = Managers.Instance.GetServerManager();
            if (serverMgr == null)
            {
                Debug.LogError("[Server] 로그인 요청 실패: serverMgr is null");
                tcs.TrySetResult(false);
                return await tcs.Task;
            }

            // 1. 기존 유저 로그인 성공
            void OnSuccess(string response)
            {
                try
                {
                    MyLogger.Log("[Server] 기존 계정 서버 로그인 성공");

                    serverMgr.OnLoginResponseSuccess(response);

                    tcs.TrySetResult(true);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Server] 로그인 응답 처리 실패: " + e.Message);
                    tcs.TrySetResult(false);
                }
            }

            // 2. 로그인 에러 발생
            void OnError(BestHttp_APIServiceManager.ErrorResponse error)
            {
                if (error == null)
                {
                    Debug.LogError("[Server] 로그인 실패: error response is null");
                    tcs.TrySetResult(false);
                    return;
                }

                // 신규 유저: 서버에 유저가 없어서 401 발생
                if (error.StatusCode == 401)
                {
                    MyLogger.LogWarning("[Server] 401 에러(신규 사용자) → 계정 생성 후 자동 로그인 진행");

                    // 계정 생성 중복 요청 방지
                    if (_isCreatingUser)
                    {
                        MyLogger.LogWarning("[Server] CreateUser already running. Skip duplicate create request.");
                        return;
                    }

                    _isCreatingUser = true;

                    void HandleCreateSuccess(string response)
                    {
                        serverMgr.OnLoginResponseSuccessEvent -= HandleCreateSuccess;
                        serverMgr.OnLoginResponseErrorEvent -= HandleCreateError;

                        _isCreatingUser = false;

                        try
                        {
                            MyLogger.Log("[Server] 신규 계정 생성 완료! 생성 응답으로 로그인 처리");

    #if DEV_SERVER_SET || UNITY_EDITOR
                            MyLogger.Log("[Server] CreateUser response = " + response);
    #endif

                            // 중요:
                            // CreateUser 응답이 로그인 성공 응답과 같은 구조일 때만 이 방식 사용 가능
                            serverMgr.OnLoginResponseSuccess(response);

                            tcs.TrySetResult(true);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("[Server] 신규 계정 생성 응답 처리 실패: " + e.Message);
                            tcs.TrySetResult(false);
                        }
                    }

                    void HandleCreateError(BestHttp_APIServiceManager.ErrorResponse createError)
                    {
                        serverMgr.OnLoginResponseSuccessEvent -= HandleCreateSuccess;
                        serverMgr.OnLoginResponseErrorEvent -= HandleCreateError;

                        _isCreatingUser = false;

                        if (createError != null)
                        {
                            Debug.LogError($"[Server] 신규 계정 생성 실패: {createError.StatusCode} / {createError.ErrorMessage}");
                        }
                        else
                        {
                            Debug.LogError("[Server] 신규 계정 생성 실패: error response is null");
                        }

                        tcs.TrySetResult(false);
                    }

                    // 이벤트 등록 및 계정 생성 요청
                    serverMgr.OnLoginResponseSuccessEvent += HandleCreateSuccess;
                    serverMgr.OnLoginResponseErrorEvent += HandleCreateError;

                    try
                    {
                        serverMgr.OnRequestCreateUser();
                    }
                    catch (Exception e)
                    {
                        serverMgr.OnLoginResponseSuccessEvent -= HandleCreateSuccess;
                        serverMgr.OnLoginResponseErrorEvent -= HandleCreateError;

                        _isCreatingUser = false;

                        Debug.LogError("[Server] 신규 계정 생성 요청 예외: " + e.Message);
                        tcs.TrySetResult(false);
                    }

                    return;
                }

                // 401이 아닌 진짜 통신/서버 에러인 경우
                MyLogger.LogError($"[Server] 로그인 실패: {error.StatusCode} / {error.ErrorMessage}");

                // 특수 코드(점검 -5, 중복접속 -9998, 정지 -9999 등)는 기존 GameManager 핸들러가
                // 전용 팝업을 띄우므로 그쪽으로만 보냅니다.
                bool isSpecialCode =
                    error.ErrorCode == "-5" || error.ErrorCode == "-9998" || error.ErrorCode == "-9999";

                if (isSpecialCode)
                {
                    try { serverMgr.OnLoginResponseError(error); }
                    catch (Exception e) { Debug.LogError("[Server] 에러 응답 처리 실패: " + e.Message); }
                }
                else
                {
                    // 일반 서버/클라이언트 오류는 여기서 한 번만 팝업
                    ShowLoginErrorPopup(error);
                }

                tcs.TrySetResult(false);
            }

    #if USE_SERVER
            // ---------------------------------------------------------
            // 1. 기존에 저장소에서 값을 꺼내오는 원본 코드를 주석 처리합니다.
            // ---------------------------------------------------------
            
            int platform = GameDefineData.GetInt(Define.KEY_PLATFORM, -1);
            string id = null;
            
            switch (platform)
            {
                case 0:
                    id = GameDefineData.GetString(Define.KEY_USER_SOCIAL_ID, null, decrypt: false);
                    break;
            
                case 1:
                    id = GameDefineData.GetString(Define.KEY_SERVER_JWT, null, decrypt: false);
                    break;
            
                case 2:
                    id = GameDefineData.GetString(Define.KEY_USER_SOCIAL_ID, null, decrypt: false);
                    break;
            }
            Debug.LogError("platform = " + platform);
            Debug.LogError("id = " + id);

            
            // ---------------------------------------------------------
            // 2. 🚨 여기에 강제 접속 테스트 코드를 추가합니다. 🚨
            // ---------------------------------------------------------
            // int platform = 1; // 테스트할 플랫폼 입력 (0: 게스트, 1: 구글, 2: 애플)
            // string id = "guest_6d7891f8b396473aa6d0ff2bced929d6";//"여기에_전달받은_유저의_JWT_문자열이나_ID를_넣어주세요";
            //
            if (!string.IsNullOrEmpty(id))
            {
                MyLogger.Log($"[Server] 로그인 요청 발송 (platform={platform})");

                try
                {
                    serverMgr.PostLoginRequest(id, OnSuccess, OnError);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Server] 로그인 요청 예외: " + e.Message);
                    tcs.TrySetResult(false);
                }
            }
            else
            {
                Debug.LogError("[Server] 로그인 요청 실패: id 없음");
                tcs.TrySetResult(false);
            }
    #else
            MyLogger.LogWarning("[Server] USE_SERVER not defined. Skip real server login.");
            tcs.TrySetResult(true);
    #endif

            bool loginResult = await tcs.Task;
            return loginResult;
        }
        finally
        {
            _isCreatingUser = false;

            if (_serverLoginTcs == tcs)
            {
                _serverLoginTcs = null;
            }
        }
    }
        
    private async UniTask<bool> GameServerProc()
    {

        MyLogger.Log("[Mock] GameServerProc() 시작됨");

        await UniTask.Delay(1000);

        try
        {
            MyLogger.Log("[Mock] 게임 서버 로그인 성공!");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"서버 로그인 중 오류 발생: {ex.Message}");
        }
        
        // 실패 시
        Debug.LogError("[Mock] 게임 서버 로그인 실패!");
        exitProcess = true;
        
        return false;
    }

    
    #region Button Event

    public void OnClickBG()
    {
        
        if (isBGTouch) return;  // 중복 실행 방지
        isBGTouch = true;
        
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");

        
        MyLogger.Log("OnClickBG Triggered - Direct Scene Transition");
    }
    
    private bool _isLoginProcessing = false; // 공통 플래그로 변경
    public void OnClickLogin(int providerInt)
    {
        if (_isLoginProcessing) return; // 중복 터치 방어
        _isLoginProcessing = true;
        
        var provider = (ELoginProvider)providerInt;

        // Guest는 반드시 OnClickGuestLogin() 한 곳으로 통일
        if (provider == ELoginProvider.Guest)
        {
            OnClickGuestLogin();
            return;
        }

        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");

        switch (provider)
        {
            case ELoginProvider.Google:
                {
                    // 시도 직후 “PENDING” 기록 (중간 종료 복구용)
                    GameDefineData.SetInt("KEY_PLATFORM_PENDING", (int)ELoginProvider.Google);
                    GameDefineData.Save();

#if UNITY_EDITOR
                    MyLogger.Log("[Editor] Google 시뮬레이션");
                    MarkSocialLoginSuccess("editor_google_id", 1);
#else
            MyLogger.Log("[Login] Google");
            SetLoginButtonVisibility(false);
            socialLogin.SignInGoogle(); // 이벤트 콜백에서 MarkSocialLoginSuccess 호출됨
#endif
                    break;
                }

            case ELoginProvider.Apple:
                {
                    // 시도 직후 “PENDING” 기록
                    GameDefineData.SetInt("KEY_PLATFORM_PENDING", (int)ELoginProvider.Apple);
                    GameDefineData.Save();

#if UNITY_IOS
    #if UNITY_EDITOR
            MyLogger.Log("[Editor] Apple 시뮬레이션");
            MarkSocialLoginSuccess("editor_apple_id", (int)ELoginProvider.Apple);
    #else
            MyLogger.Log("[Login] Apple");
            SetLoginButtonVisibility(false);
            socialLogin.SignInApple(); // 이벤트 콜백에서 MarkSocialLoginSuccess 호출됨
    #endif
#else
                    MyLogger.LogWarning("[Login] Apple은 iOS에서만 지원");
                    _isLoginProcessing = false;   
#endif
                    break;
                }
            default:
                MyLogger.LogWarning($"[Login] 미지원 provider: {provider}");
                _isLoginProcessing = false;
                break;
            
        }
    }
    
    private bool _isGuestLoginProcessing;
    public void OnClickGuestLogin()
    {
        if (_isGuestLoginProcessing)
        {
            MyLogger.LogWarning("[GuestLogin] already processing.");
            return;
        }

        _isGuestLoginProcessing = true;

        MyLogger.Log("[GuestLogin] 게스트 로그인 진입");

        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");

        SetLoginButtonVisibility(false);

        GameDefineData.DeleteData(Define.KEY_SERVER_JWT);
        GameDefineData.Save();

        string guestId = "guest_" + Guid.NewGuid().ToString("N");

        GameDefineData.SetString(Define.KEY_USER_SOCIAL_ID, guestId);
        GameDefineData.SetInt(Define.KEY_PLATFORM, 0);
        GameDefineData.Save();

        MyLogger.Log($"[GuestLogin] 새 게스트 ID 생성: {guestId}");

        MarkSocialLoginSuccess(guestId, 0);
    }

    // public void OnSocialLoginFinished(string idToken, int providerCode)
    // {
    //     if (_isLinkingAccount)
    //     {
    //         MyLogger.Log($"[Link] BestHttp_GameManager로 연동 요청 전달. Provider: {providerCode}");
    //         
    //         // GameManager로 연동 API 호출 명령 전달
    //         Managers.Instance.GetServerManager().RequestAccountLinking(idToken, providerCode);
    //     }
    //     else
    //     {
    //         MyLogger.LogError("_isLinkingAccount false");
    //     }
    // }
    
 
    // private void HandleSocialLoginSuccess(string platform, string userId, string userName)
    // {
    //     // 저장
    //     int pf = platform == "Google" ? 1 : 2;
    //     MyLogger.Log($"[{platform}] 로그인 성공: {userName} ({userId})");
    //     
    //     // 소셜 식별자/플랫폼 저장
    //     GameDefineData.SetString(Define.KEY_USER_SOCIAL_ID, userId);
    //     GameDefineData.SetInt(Define.KEY_PLATFORM, pf);
    //     GameDefineData.Save();
    //     
    //     MarkSocialLoginSuccess(userId, pf);
    //     
    //     // 서버 인증 등 후속 처리
    // }
    
    private async void HandleSocialLoginSuccess(string platformStr, string userId, string userName)
    {
        int providerCode = platformStr == "Google" ? 1 : (platformStr == "Apple" ? 2 : 0);
        MyLogger.Log($"[{platformStr}] 소셜 인증 콜백 수신 완료: {userName} ({userId})");
        MyLogger.Log("[Login] 일반 로그인/계정전환 프로세스 진행");

        // 로컬 데이터에 플랫폼 및 21자리 Social ID 갱신 저장
        GameDefineData.SetString(Define.KEY_USER_SOCIAL_ID, userId);
        GameDefineData.SetInt(Define.KEY_PLATFORM, providerCode);
        GameDefineData.Save();
        
        // 기존 터치 대기 및 서버 통신용 함수 호출
        MarkSocialLoginSuccess(userId, providerCode);
    }
    
    #if REVIEW
    private async UniTask DoSuperServiceUserCheckAsync()
    {
        MyLogger.Log("[SuperService] User Check 시작");

        var userInfo = Managers.Instance.UserInfo();
        if (userInfo == null)
        {
            Debug.LogError("[SuperService] UserInfo is null.");
            return;
        }

#if SINGULAR
        string singularUserId = userInfo.userId.ToString();
        MyLogger.Log($"[SuperService] Singular CustomUserId 설정: {singularUserId}");
        Managers.Instance.Singular?.SetCustomUserIdWithFallback(singularUserId);
#endif

        string url = RestAPIURL.GetSuperServiceFullUrl(RestAPIURL.userCheckAction);
        WWWForm form = RestAPIURL.GetUserCheckForm();

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            try
            {
                await request.SendWebRequest().ToUniTask();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    MyLogger.Log($"[SuperService] User Check 성공: {request.downloadHandler.text}");
                }
                else
                {
                    Debug.LogError(
                        $"[SuperService] User Check 실패: {request.responseCode} / {request.error} / {request.downloadHandler.text}"
                    );
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SuperService] User Check 예외: {e.Message}");
            }
        }
    }
    #endif
    private void HandleSocialLoginFailure(string error)
    {
        Debug.LogError("로그인 실패: " + error);
        
        _isGuestLoginProcessing = false;
        _isLoginProcessing = false;
        
        GameDefineData.DeleteData("KEY_PLATFORM_PENDING");  // 실패했으니 PENDING 제거
        GameDefineData.Save();
        
        isDoneSocialId = false;
        SetLoginButtonVisibility(true);
        goTouchScreen.SetActive(false);
    }
 
    // 서버 오류 / 클라이언트(네트워크) 오류를 StatusCode로 구분해 동일한 ErrorPopup으로 노출
    private void ShowLoginErrorPopup(BestHttp_APIServiceManager.ErrorResponse error)
    {
        int code = error?.StatusCode ?? 0;

        string title;
        string message;

        if (code == 0)
        {
            // StatusCode == 0 : 응답 자체를 못 받음 (타임아웃/연결 끊김) → 클라이언트/네트워크 오류
            title   = "네트워크 오류";
            message = "네트워크 연결이 원활하지 않습니다.\n연결 상태를 확인 후 다시 시도해주세요.";
            MyLogger.LogError("[TitleManager] 클라이언트/네트워크 오류 (StatusCode=0)");
        }
        else if (code >= 500)
        {
            // 5xx : 서버 내부 오류
            title   = "서버 오류";
            message = $"일시적인 서버 오류가 발생했습니다. ({code})\n잠시 후 다시 시도해주세요.";
            MyLogger.LogError($"[TitleManager] 서버 오류 ({code}) - {error?.ErrorMessage}");
        }
        else
        {
            // 4xx (401은 신규 유저 분기에서 이미 처리됨) : 요청/클라이언트 오류
            title   = "요청 오류";
            message = string.IsNullOrEmpty(error?.ErrorMessage)
                ? $"요청을 처리할 수 없습니다. ({code})"
                : error.ErrorMessage;
            MyLogger.LogError($"[TitleManager] 클라이언트 요청 오류 ({code}) - {error?.ErrorMessage}");
        }

        Managers.Instance.GetUIManager().ShowErrorPopUp(EErrorCloseType.Confirm, title, message, null);
    }
    
    private async UniTask HandleLoginFlowAfterSuccess()
    {
        MyLogger.Log("HandleLoginFlowAfterSuccess() 시작");

        // 1) 로그인 성공 직후: 로그인 버튼만 숨기고 메인 UI(터치 스크린)는 아직 켜지 않습니다.
        SetLoginButtonVisibility(false);
    
        // [제거됨] SetMainUIVisibility(true); 
        // [제거됨] if (goTouchScreen != null) goTouchScreen.SetActive(true);

        // 2) 게임 서버 로그인 대기
        bool isServerLoginSuccess = await HandleServerLogin();
        if (!isServerLoginSuccess) 
        {
            MyLogger.LogError("서버 로그인 실패 -> 로그인 화면으로 복귀");
        
            _isGuestLoginProcessing = false;
        
            // 실패 시 다시 로그인할 수 있도록 로그인 UI만 켜고 메인 UI는 확실히 꺼줍니다.
            SetLoginButtonVisibility(true);
            SetMainUIVisibility(false);
            return; 
        }

        // 3) 닉네임 설정 여부 확인
        bool hasNickname = Managers.Instance.GetServerManager().IsSettingUserGameName;
        if (!hasNickname)
        {
            string newNick = await _nicknameUI.WaitForNicknameAsync();
            if (string.IsNullOrWhiteSpace(newNick)) 
            {
                _isGuestLoginProcessing = false;
            
                // 닉네임 입력을 취소/실패했을 때도 동일하게 UI 복구
                SetLoginButtonVisibility(true);
                SetMainUIVisibility(false);
                return; 
            }
    
            GameDefineData.SetString("USER_NICKNAME", newNick);
            GameDefineData.Save();
        }
    
        // 4) 부가 서비스 연동 (모든 무거운 백그라운드 작업 처리)
#if REVIEW && !ONE
        await DoSuperServiceUserCheckAsync();
#endif
    
#if CHAT
        await ConnectChatAfterLogin();
#endif
    
        // 5) 모든 작업이 완료된 후, 비로소 터치 스크린과 메인 UI를 활성화합니다.
        MyLogger.Log("WaitForUserTouch() 호출 직전 - 이제 화면 터치가 가능합니다.");
    
        SetMainUIVisibility(true); 

        await WaitForUserTouch();
        MyLogger.Log("WaitForUserTouch() 실행 완료");

        // 6) 씬 전환
        await TransitionToNextScene();
    }

#if CHAT
    private async UniTask ConnectChatAfterLogin()
    {
        var chat = Managers.Instance.Chat;
        if (chat == null) return;

        long socialId = Managers.Instance.UserInfo().userId;
        MyLogger.Log("ConnectChatAfterLogin socialId = " + socialId);
        
        string nick = GameDefineData.GetString("USER_NICKNAME", "");
        if (string.IsNullOrWhiteSpace(nick))
        {
            string idString = socialId.ToString();
            
            string shortId = idString.Length > 4 ? idString.Substring(0, 4) : idString;
            nick = "Guest_" + shortId;
            
            GameDefineData.SetString("USER_NICKNAME", nick);
            GameDefineData.Save();
    
            MyLogger.Log($"[Chat/Nickname] 로컬 닉네임이 비어 있어 임시 닉네임을 생성 및 저장했습니다: {nick}");
        }
            
        chat.SetLoginInfo(socialId, nick, Managers.Instance.UserInfo()._thumbnailID);
        
        // =========================================================
        // 접속 직전에 서버 환경(Local / Live) 결정
        // =========================================================
#if DEV_SERVER_SET
        chat.SetEnvironment(UnityChatClient.ServerEnvironment.Local);
        MyLogger.Log("[Chat] 개발 환경 -> 로컬 서버로 세팅");
#else
        chat.SetEnvironment(UnityChatClient.ServerEnvironment.Live);
        MyLogger.Log(
            RestAPIURL.IsReviewMode
                ? "[Chat] 심사 모드(Review)지만 채팅은 HTTPS/WSS 이슈 방지를 위해 라이브 서버로 세팅"
                : "[Chat] 라이브 모드 -> 라이브 서버로 세팅"
        );
#endif
        // =========================================================

        try
        {
            // 💡 핵심: 최대 5초까지만 채팅 서버 연결을 기다리고, 넘어가면 Timeout 예외 발생
            //         (객체 파괴 시에는 _ct 로 즉시 취소)
            bool ok = await chat.ConnectAsync().AttachExternalCancellation(_ct).Timeout(TimeSpan.FromSeconds(5));
            
            if (ok)
            {
                MyLogger.Log("[Chat] 라이브 채팅 서버 접속 성공, 로그인 시도!");
                chat.Login(socialId);
            }
            else
            {
                MyLogger.LogError($"[Chat] 라이브 채팅 서버 접속 실패! (Host: {chat.CurrentHost}, Port: {chat.CurrentPort})");
            }
        }
        catch (TimeoutException)
        {
            // 타임아웃 발생 시
            MyLogger.LogError("[Chat] 채팅 서버 응답 지연 (Timeout). 접속을 포기하고 게임 진입을 우선합니다.");
        }
        catch (Exception ex)
        {
            // TCP/Socket 에러 등 각종 크래시 방어 (여기서 에러를 먹어버려야 메인 흐름이 안 끊깁니다)
            MyLogger.LogError($"[Chat] 채팅 서버 연결 중 예외 발생 (무시하고 게임 진입): {ex.Message}");
        }
    }
#endif
    
    // TitleManager.cs 내부에 기존 변수들과 함께 선언
    private void CheckFirstLaunchAndCleanUp()
    {
        try
        {
            string checkFilePath = Path.Combine(Application.persistentDataPath, "app_installed.dat");

            // 1. 파일이 이미 존재한다면 이미 실행했던 앱이므로 아무것도 하지 않고 종료
            if (File.Exists(checkFilePath))
            {
                return;
            }

            MyLogger.Log("[FirstLaunch] 새 설치(또는 클리어) 감지. 잔존 데이터 검사 시작.");

            // 2. 🌟 [핵심 방어 코드] 파일은 없는데, 기기에 게스트 ID가 남아있다? 
            // -> 100% 앱 삭제 시 '데이터 유지'를 체크해서 좀비처럼 살아남은 게스트 데이터입니다.
            int platform = GameDefineData.GetInt(Define.KEY_PLATFORM, -1);
            string socialId = GameDefineData.GetString(Define.KEY_USER_SOCIAL_ID, null);

            if (platform == 0 && !string.IsNullOrEmpty(socialId))
            {
                MyLogger.LogWarning($"[FirstLaunch] 좀비 게스트 데이터 발견 ({socialId}). 유저 오인을 막기 위해 파기합니다.");
            
                // 기존에 OS가 억지로 남겨둔 옛날 게스트 정보만 저격해서 삭제
                GameDefineData.DeleteData(Define.KEY_USER_SOCIAL_ID);
                GameDefineData.DeleteData(Define.KEY_PLATFORM);
                GameDefineData.Save();
            }
            // 💡 만약 platform이 1(구글)이나 2(애플)라면 업데이트 상황이거나 
            // 안전한 계정이므로 절대 데이터를 밀지 않고 통과시킵니다.

            // 3. 다음 실행 때는 이 루틴을 타지 않도록 추적용 파일을 생성
            File.WriteAllText(checkFilePath, DateTime.UtcNow.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirstLaunch] 체크 중 예외 발생 (안전을 위해 패스): {ex.Message}");
        }
    }
    
    #endregion

}
