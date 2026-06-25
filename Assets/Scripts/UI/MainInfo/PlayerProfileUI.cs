using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

#if REVIEW
// GPM WebView 네임스페이스 (설치 여부에 따라 확인 필요)
using Gpm.WebView;
#endif

using Newtonsoft.Json.Linq;
using UnityEngine.Networking;



public class PlayerProfileUI : UIBase
{
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private GameObject nickNameObject;
    [SerializeField] private GameObject thumbnailObject;
    [SerializeField] private TMP_Text _uidTxt;

    [SerializeField] private GameObject selectServerObject;
    
    // ClipBoard
    public TMP_Text sourceText;

    // Input PlayerName
    [Header("Input Field (User Input)")]
    public TMP_InputField inputField;

    [Header("NickName Display Text")]
    public TMP_Text nicknameText;

    [Header("Profile Main Thumbnail (Image)")]
    public Image avatarImage;
    public Image frameImage;

    [SerializeField] private AvatarThumbnailPanel avatarPanel;
    private List<int> avatarNames = new List<int>();
    private List<int> frameNames = new List<int>();

    [Header("BattlePower")]
    [SerializeField] private TMP_Text _powerValueTxt;

    [Header("Guild")]
    [SerializeField] private Image _guildIcon;
    [SerializeField] private TMP_Text _guildNameTxt;

    [Header("Level")]
    [SerializeField] private TMP_Text _levelTxt;

    [Header("QnA")] 
    [SerializeField] private GameObject qnaPanel;

    [Header("WebShop")]
    [SerializeField] private GameObject webShop;
    
    // --- Super Reward 관련 변수 추가 ---
    private bool isWebOpening = false;
    private string webURL;
    // --------------------------------

    [Header("Social Login (인게임 연동용)")]
    [SerializeField] private SocialLogin socialLogin; // 에디터에서 연결 필요!
    private bool _isLinkingAccount = false;
    [SerializeField] private GameObject btnGoogleLink; 
    // [SerializeField] private GameObject btnAppleLink;
    
    private Action<Sprite> _onSelectedOnce;
    public void SetOnAvatarSelectedOnce(Action<Sprite> cb) => _onSelectedOnce = cb;

    private Action<Sprite> _onFrameSelectedOnce;
    public void SetOnFrameSelectedOnce(Action<Sprite> cb) => _onFrameSelectedOnce = cb;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Utils.BindText(UserInfoData.userNickName, nicknameText, this);
        Utils.BindText(UserInfoData.AllBattlePower, _powerValueTxt, this);
        Utils.BindText(UserInfoData.userLevel, _levelTxt, this);

        avatarImage.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.CharacterIconAtlas, $"Thum_SD_Cr_{UserInfoData._thumbnailID.ToString("000")}");
        if (frameImage)
        {
            var frameSprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.FrameAtlas,
                $"FrameImg_{UserInfoData._frameID.ToString("000")}");
            frameImage.sprite = frameSprite;
            frameImage.gameObject.SetActive(frameSprite != null);
        }

        return true;
    }

    public override void Open()
    {
        base.Open();
        Managers.Instance.Sound.PlaySFX("Effect", "SE_Inventory_Open_01");

        uiRoot.SetActive(true);
        uiRoot.transform.localScale = Vector3.zero;
        uiRoot.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        _uidTxt.text = UserInfoData.userId.ToString();


        // 첫 로그인 시에 유저 길드 정보는 받았지만 길드 이름이 없을 때 API 재요청
        if (UserInfoData.ExistGuild && string.IsNullOrEmpty(UserInfoData.guildInfo.name))
        {
            Managers.Instance.GetServerManager().OnRequestMyGuildInfo(() => {
                _guildNameTxt.text = UserInfoData.ExistGuild ? UserInfoData.guildInfo.name : "연합 없음";
                SetGuildIcon();
            });
        }
        else
        {
            _guildNameTxt.text = UserInfoData.ExistGuild ? UserInfoData.guildInfo.name : "연합 없음";
            SetGuildIcon();
        }

        if (webShop != null)
        {
        #if ONE
            webShop.SetActive(false);
        #else
            webShop.SetActive(!RestAPIURL.IsReviewMode);
        #endif
        }
        
        nickNameObject.SetActive(false);
        thumbnailObject.SetActive(false);
        
        UpdateAccountLinkButtonVisibility();
        
        // ★ [추가] 인게임 소셜 연동 이벤트 구독
        if (socialLogin != null)
        {
            socialLogin.OnLoginSucceeded -= HandleSocialLoginSuccess;
            socialLogin.OnLoginSucceeded += HandleSocialLoginSuccess;
            
            socialLogin.OnLoginFailed -= HandleSocialLoginFailure;
            socialLogin.OnLoginFailed += HandleSocialLoginFailure;
        }

        var serverMgr = Managers.Instance.GetServerManager();
        if (serverMgr != null)
        {
            serverMgr.OnAccountLinkSuccessEvent -= HandleAccountLinkSuccess;
            serverMgr.OnAccountLinkSuccessEvent += HandleAccountLinkSuccess;
            
            serverMgr.OnAccountLinkFailedEvent -= HandleAccountLinkFailed;
            serverMgr.OnAccountLinkFailedEvent += HandleAccountLinkFailed;
        }
        
    }

    #region Nickname & Settings Logic
    public void OnClickNickNameUIOpen()
    {
        if (nickNameObject != null)
        {
            nicknameText.text = UserInfoData.userNickName.Value;
            if (inputField != null) inputField.text = UserInfoData.userNickName.Value;

            Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");
            nickNameObject.SetActive(true);
        }
    }

    public void OnClickSettingUIOpen()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");
        UIManager.SettingUIPopup.Init();
    }

    public void OnClickNickNameClose() { Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuClose"); nickNameObject.SetActive(false); }
    public void OnClickNickNameOk() { Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuClose"); nickNameObject.SetActive(false); }
    
    public void OnClickCopyToClipboard()
    {
        if (sourceText == null) return;
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");
        GUIUtility.systemCopyBuffer = sourceText.text;
        Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("클립보드에 복사됨", "ToastMessage");
    }

    public void OnClickNickNameChange()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");
        if (inputField == null || nicknameText == null) return;

        string inputFiedTxt = inputField.text;
        if(inputFiedTxt.Length < 2) { UIManager.ShowUIToast<UIToastBase>("닉네임은 2글자 이상이어야 합니다", "ToastMessage"); return; }
        if (inputFiedTxt.Contains(" ")) { UIManager.ShowUIToast<UIToastBase>("닉네임은 공백을 포함할 수 없습니다", "ToastMessage"); return; }
        if (Utils.CheckBanText(inputFiedTxt)) { UIManager.ShowUIToast<UIToastBase>("닉네임에 사용할 수 없는 단어가 포함되어 있습니다", "ToastMessage"); return; }
        if(!Regex.IsMatch(inputFiedTxt, @"^[\p{L}\p{N}]+$")) { UIManager.ShowUIToast<UIToastBase>("닉네임에는 특수문자나 이모지를 포함할 수 없습니다", "ToastMessage"); return; }

#if USE_SERVER
        BestHttp_GameManager.OnPostChangeGameName(inputFiedTxt);
#else
        nicknameText.text = inputFiedTxt;
        Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>($"닉네임이 변경되었습니다", "ToastMessage");
        nickNameObject.SetActive(false);
#endif
    }
    
    public void OnClickNickNameCancel()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuClose");
        nicknameText.text = UserInfoData.userNickName.Value;
        if (inputField != null) inputField.text = UserInfoData.userNickName.Value;
        nickNameObject.gameObject.SetActive(false);
    }
    #endregion

    #region Thumbnail Logic
    public void OnClickThumbnailUIOpen()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");
        thumbnailObject.SetActive(true);
        var currentName = avatarImage && avatarImage.sprite ? avatarImage.sprite.name : null;
        avatarNames.Clear();
        var cList = Managers.Instance.UserInfo()._dicCharacterItemData
            .Where(c => c.Value.isOpen)
            .Where(c => c.Value._unitData.Live)
            .Select(c => c.Value).ToList();
        avatarNames = cList.Select(c => c.id).ToList();  
        avatarPanel.PopulateFromKeys(Define.EAtlasType.CharacterIconAtlas, avatarNames.ToArray(), currentName);

        // Frame
        var currentFrameName = frameImage && frameImage.sprite ? frameImage.sprite.name : null;
        frameNames = new List<int> { 1, 2 }; // TODO: userinfo 에서 받아오도록 교체
        avatarPanel.PopulateFrameFromKeys(frameNames.ToArray(), currentFrameName);
    }
    
    public void OnClickThumbnailClose() { Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuClose"); thumbnailObject.SetActive(false); }
     
    public void ApplyAvatar(Sprite sprite, Sprite frameSprite)
    {
        if (!avatarImage || !sprite) return;
        avatarImage.overrideSprite = null;
        avatarImage.preserveAspect = true;
        avatarImage.sprite = sprite;

        if (frameImage && frameSprite)
        {
            frameImage.overrideSprite = null;
            frameImage.sprite = frameSprite;
        }

        _onSelectedOnce?.Invoke(sprite);
        _onFrameSelectedOnce?.Invoke(frameSprite);
    }
    #endregion

    #region External Links & Server
    public void OnClickGameNotice() { Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen"); UIManager.ShowPopup<GameNoticePopup>("GameNoticePopup"); }
    public void OnClickTermsAndConditions() { Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen"); Application.OpenURL("https://sites.google.com/view/root3games-terms/%ED%99%88"); }
    public void OnClickPersonalIinformation() { Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen"); Application.OpenURL("https://sites.google.com/view/root3games-privacy/%ED%99%88"); }
    public void OnClickSelectServer() { Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen"); selectServerObject.SetActive(true); }
    public void OnClickSelectServerClose() { Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuClose"); selectServerObject.SetActive(false); }
    public void OnClickFeedBack() { Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen"); Application.OpenURL("https://game.naver.com/lounge/Mini_Yokai_Hunters/board/1"); }
    #endregion

    #region Super Reward (WebView) Logic
    public void OnClickSuperReward()
    {
#if REVIEW
        // 1. 사운드 재생
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");

        // 2. 중복 실행 방지
        if (isWebOpening) return;

        // 3. 플랫폼 조건부 컴파일 처리
    #if !UNITY_EDITOR
    // GPM 웹뷰가 이미 활성화되어 있다면 닫아줌 (명세서 권장 사항)
    if (GpmWebView.IsActive()) 
    {
        GpmWebView.Close();
    }
    
    // 코루틴 시작 (기본값 false로 슈퍼리워드 메인 오픈)
    StartCoroutine(coGetWebToken(false));
    #else
        // 유니티 에디터 환경 대응
        MyLogger.Log("<color=yellow>[SuperReward]</color> WebView는 모바일 환경 전용입니다.");
    
        // 사용자 알림 (토스트 메시지)
        var uiManager = Managers.Instance.GetUIManager();
        if (uiManager != null)
        {
            uiManager.ShowUIToast<UIToastBase>("모바일 기기에서만 이용 가능합니다.", "ToastMessage");
        }
    
        // 에디터 테스트용 로그 (토큰 요청 URL이 정상인지 확인 가능)
        MyLogger.Log($"[Test] Token Request URL: {RestAPIURL.GetTokenRequestUrl()}");
    #endif
#endif
    }

    public void OnClickWebShop()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");
        
        // #if REVIEW
        // StartCoroutine(coGetWebToken(true));
        // #endif
        
#if REVIEW
        // 웹샵은 외부 브라우저로 열어야 하므로 별도의 코루틴을 실행합니다.
        StartCoroutine(coOpenExternalWebShop());
#endif
        
    }
    
    #region Super Reward Network Logic

    private IEnumerator coGetWebToken(bool isWebShop = false)
    {
        if (isWebOpening) yield break;
        isWebOpening = true;

        // 1. 로딩 UI 활성화 (UIManager 등을 통해 접근 권장)
        // var loadingUI = Only1Games.UI.CircularProgressViewerImmediately;
        // if (loadingUI != null) loadingUI.SetActive(true);

        // 2. RestAPIURL 클래스를 사용하여 명세서 규격에 맞는 전체 URL 생성
        string requestUrl = RestAPIURL.GetTokenRequestUrl();
        MyLogger.Log($"[SuperReward] Requesting Token: {requestUrl}");

        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            yield return request.SendWebRequest();

            // 네트워크 에러 처리
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[SuperReward] Network Error: {request.error}");
                isWebOpening = false;
                //if (loadingUI != null) loadingUI.SetActive(false);
                yield break;
            }

            try
            {
                string jsonResult = request.downloadHandler.text;
                JObject responseData = JObject.Parse(jsonResult);
            
                // 명세서 구조: response -> token
                string token = responseData["response"]?["token"]?.ToString();

                if (!string.IsNullOrEmpty(token))
                {
                    // 3. 목적지에 따른 URL 조합 (메인 로그인 vs 웹샵 직접 호출)
                    string finalUrl = isWebShop 
                        ? RestAPIURL.GetWebShopFullUrl(token) 
                        : RestAPIURL.GetWebviewFullUrl(token);

                    MyLogger.Log($"[SuperReward] Opening WebView: {finalUrl}");
                    OpenGpmWebView(finalUrl);
                }
                else
                {
                    // 실패 반환 시 message 확인 (invalid_key, invalid_data 등)
                    string errorMsg = responseData["response"]?["message"]?.ToString();
                    Debug.LogError($"[SuperReward] Token Failed: {errorMsg}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SuperReward] Newtonsoft.Json Parsing Error: {e.Message}");
            }
        }

        isWebOpening = false;
        // if (loadingUI != null) loadingUI.SetActive(false);
    }

    private IEnumerator coOpenExternalWebShop()
    {
        if (isWebOpening) yield break;
        isWebOpening = true;

        // 1. 토큰 발급 URL 가져오기
        
        string requestUrl = RestAPIURL.GetTokenRequestUrl(true); 
        MyLogger.Log($"[WebShop] Requesting Token: {requestUrl}");

        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[WebShop] Network Error: {request.error}");
                isWebOpening = false;
                yield break;
            }

            try
            {
                string jsonResult = request.downloadHandler.text;
                JObject responseData = JObject.Parse(jsonResult);
        
                // 명세서 구조: response -> token
                string token = responseData["response"]?["token"]?.ToString();

                if (!string.IsNullOrEmpty(token))
                {
                    // 2. 발급받은 토큰으로 웹샵 링크 생성 (method="aganim"이 기본값으로 들어감)
                    string webShopUrl = RestAPIURL.GetWebShopFullUrl(token);
                    Debug.Log($"[WebShop] Opening External Browser: {webShopUrl}");
                    
                
                    // 3. (핵심) GpmWebView가 아닌 기기의 기본 외부 브라우저(크롬/사파리) 열기
                    Application.OpenURL(webShopUrl);
                    
                }
                else
                {
                    string errorMsg = responseData["response"]?["message"]?.ToString();
                    Debug.LogError($"[WebShop] Token Failed: {errorMsg}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WebShop] Newtonsoft.Json Parsing Error: {e.Message}");
            }
        }

        isWebOpening = false;
    }
    
    #endregion

    private void OpenGpmWebView(string url)
    {
        #if REVIEW
        GpmWebView.ShowUrl(
            url,
            new GpmWebViewRequest.Configuration()
            {
                style = GpmWebViewStyle.FULLSCREEN,
                orientation = GpmOrientation.PORTRAIT,
                isClearCookie = true,
                isClearCache = true,
                isNavigationBarVisible = true,
                navigationBarColor = "#4B96E6",
                title = "Super Reward",
                isBackButtonVisible = true,
                isForwardButtonVisible = true,
                isCloseButtonVisible = true,
                supportMultipleWindows = true,
#if UNITY_IOS
                contentMode = GpmWebViewContentMode.MOBILE
#endif
            },
            OnCallback,
            new List<string>() { "ssreward://", "USER_CUSTOM_SCHEME" });
        #endif
    }

    #if REVIEW
        private static void OnCallback(GpmWebViewCallback.CallbackType callbackType, string data, GpmWebViewError error)
        {
            MyLogger.Log("GPM Callback: " + callbackType);

            switch (callbackType)
            {
                case GpmWebViewCallback.CallbackType.Scheme:
                    {
                        if (error != null)
                        {
                            Debug.LogError($"[SuperReward] Scheme Error: {error}");
                            break;
                        }

                        if (string.IsNullOrEmpty(data))
                            break;

                        const string scheme = "ssreward://";

                        if (data.StartsWith(scheme))
                        {
                            string encodedUrl = data.Substring(scheme.Length);
                            string targetUrl = UnityWebRequest.UnEscapeURL(encodedUrl);

                            MyLogger.Log($"[SuperReward] Open External URL: {targetUrl}");

                            #if UNITY_IOS
                                    OpenUrlManager.OpenURLInBrowser(targetUrl);
                            #else
                                    Application.OpenURL(targetUrl);
                            #endif
                        }

                        break;
                    }
            }
        }
    #endif
    
    #endregion

    #region QnA Logic
    public void OnClickQnAClose() => qnaPanel.SetActive(false);
    public void OnClickQnAOpen() => qnaPanel.SetActive(true);
    public void Q1() => MailToSend("계정 문의");
    public void Q2() => MailToSend("결제 문의");
    public void Q3() => MailToSend("건의/제안 문의");
    public void Q4() => MailToSend("오류 제보");
    public void Q5() => MailToSend("유저 신고");
    public void Q6() => MailToSend("기타 문의");

    void MailToSend(string kind)
    {
        string mailto = "root3rnd@gmail.com";
        string subject = EscapeURL(kind);
        
        // 주의: 마지막에 있던 .Replace("\\n", "\n")를 제거했습니다.
        // EscapeURL로 변환된 줄바꿈(%0A)을 다시 일반 줄바꿈으로 되돌리면 iOS 메일 앱이 포맷을 인식하지 못할 수 있습니다.
        string body = EscapeURL(
            "\n\n\n\n" +             
            "Device Model : " + SystemInfo.deviceModel + "\n" +
            "Device OS : " + SystemInfo.operatingSystem + "\n" +
            "User Name : " + Managers.Instance.UserInfo().userNickName + "\n" +
            "UID : " + Managers.Instance.UserInfo().userId+ "\n" +
#if UNITY_ANDROID
            "Platform : play_store\n" +
#elif UNITY_IOS
            "Platform : ios\n" +
#endif
            "Version : " + Application.version + "\n" +
            "Language : ko\n"
        );

        // iOS, 안드로이드 구분 없이 모두 시스템 OpenURL을 사용하도록 통일합니다.
        Application.OpenURL("mailto:" + mailto + "?subject=" + subject + "&body=" + body); 
    }

    private string EscapeURL(string url) => UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    #endregion

    public override void Close()
    {
        Debug.Log("[Profile] 프로필 UI 닫힘!");
        // ★ [추가] UI가 닫힐 때 연동 이벤트 해제 (메모리 누수 및 중복 실행 방지)
        if (socialLogin != null)
        {
            socialLogin.OnLoginSucceeded -= HandleSocialLoginSuccess;
            socialLogin.OnLoginFailed -= HandleSocialLoginFailure;
        }

        var serverMgr = Managers.Instance.GetServerManager();
        if (serverMgr != null)
        {
            serverMgr.OnAccountLinkSuccessEvent -= HandleAccountLinkSuccess;
            serverMgr.OnAccountLinkFailedEvent -= HandleAccountLinkFailed;
        }

        _onFrameSelectedOnce = null;
        _onSelectedOnce = null;
        base.Close();
    }
    
    #region In-Game Account Linking (인게임 계정 연동 및 롤백)
    
    // UI에 있는 [구글 연동] 버튼에 연결
    public void OnClickLinkGoogleAccount()
    {
        // 1. 현재 게스트(0)인지 확인
        int currentPlatform = GameDefineData.GetInt(Define.KEY_PLATFORM, -1);
        if (currentPlatform != 0)
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("게스트 계정만 연동할 수 있습니다.", "ToastMessage");
            return;
        }

        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");
        _isLinkingAccount = true;
        
        BackupGuestData(); // ★ 실패 대비 게스트 정보 백업
        if (socialLogin != null) socialLogin.SignInGoogle();
    }

    // UI에 있는 [애플 연동] 버튼에 연결
    public void OnClickLinkAppleAccount()
    {
        int currentPlatform = GameDefineData.GetInt(Define.KEY_PLATFORM, -1);
        if (currentPlatform != 0)
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("게스트 계정만 연동할 수 있습니다.", "ToastMessage");
            return;
        }

        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");
        _isLinkingAccount = true;
        
        BackupGuestData(); // ★ 실패 대비 게스트 정보 백업
        if (socialLogin != null) socialLogin.SignInApple();
    }

    // =========================================
    // 롤백 (백업 / 복구) 시스템
    // =========================================
    private void BackupGuestData()
    {
        string currentId = GameDefineData.GetString(Define.KEY_USER_SOCIAL_ID, "");
        int currentPlatform = GameDefineData.GetInt(Define.KEY_PLATFORM, 0);

        GameDefineData.SetString("BACKUP_GUEST_ID", currentId);
        GameDefineData.SetInt("BACKUP_PLATFORM", currentPlatform);
        GameDefineData.Save();
    }

    private void RollbackGuestData()
    {
        _isLinkingAccount = false;
        string backupId = GameDefineData.GetString("BACKUP_GUEST_ID", "");
        int backupPlatform = GameDefineData.GetInt("BACKUP_PLATFORM", 0);

        if (!string.IsNullOrEmpty(backupId))
        {
            // 백업해둔 진짜 게스트 정보로 원상 복구
            GameDefineData.SetString(Define.KEY_USER_SOCIAL_ID, backupId);
            GameDefineData.SetInt(Define.KEY_PLATFORM, backupPlatform);
            GameDefineData.DeleteData(Define.KEY_SERVER_JWT); // 잘못 들어온 긴 토큰 찌꺼기 삭제
            
            // 자동 로그인 방지를 위해 소셜 SDK 로그아웃
            if (socialLogin != null) socialLogin.SignOut(); 
            
            GameDefineData.Save();
            MyLogger.Log($"[Profile] 게스트 데이터 완벽 롤백 처리 완료: {backupId}");
        }
    }

    // =========================================
    // 연동 콜백 이벤트 처리
    // =========================================
    // (주의: PlayerProfileUI 클래스 상단에 이 두 변수가 선언되어 있어야 합니다)
    // private string _pendingSocialId = "";
    // private int _pendingPlatform = 0;

    private async void HandleSocialLoginSuccess(string platformStr, string userId, string userName)
    {
        if (!_isLinkingAccount) return;

        int providerCode = platformStr == "Google" ? 1 : (platformStr == "Apple" ? 2 : 0);
        string payload = userId;

        if (providerCode == 1) 
        {
            bool isTokenValid = false;
            try
            {
                MyLogger.Log("[Profile] 구글 토큰 갱신 시도 (최대 10초 대기)");
                
                // ★ 타임아웃 방어: 10초 이상 응답이 없으면 강제로 TimeoutException 발생
                isTokenValid = await socialLogin.EnsureValidGoogleTokenAsync().Timeout(TimeSpan.FromSeconds(10));
            }
            catch (TimeoutException)
            {
                MyLogger.LogError("[Profile] 🚨 구글 인증 토큰 갱신 타임아웃 (10초 초과) - 무한 대기 차단");
                isTokenValid = false;
            }
            catch (Exception ex)
            {
                MyLogger.LogError($"[Profile] 🚨 구글 인증 토큰 갱신 중 예외 발생: {ex.Message}");
                isTokenValid = false;
            }

            if (!isTokenValid)
            {
                RollbackGuestData();
                Managers.Instance.GetUIManager().ShowErrorPopUp(EErrorCloseType.Confirm, "연동 지연", "네트워크 불안정으로 인증이 지연되었습니다.\n잠시 후 다시 시도해 주세요.", null);
                return;
            }

            // 구글은 JWT 토큰으로 payload 교체
            payload = GameDefineData.GetString(Define.KEY_SERVER_JWT, null, decrypt: false);
            
            // ★ 추가 안전장치: 토큰이 비어있는 상태로 서버에 쏘는 것 방지
            if (string.IsNullOrEmpty(payload))
            {
                MyLogger.LogError("[Profile] 🚨 로컬에 저장된 JWT 토큰이 없습니다. 연동을 취소합니다.");
                RollbackGuestData();
                Managers.Instance.GetUIManager().ShowErrorPopUp(EErrorCloseType.Confirm, "연동 오류", "인증 정보를 불러오지 못했습니다.", null);
                return;
            }
        }
        else if (providerCode == 2)
        {
            bool isTokenValid = false;
            try
            {
                MyLogger.Log("[Profile] 애플 인증 시도 (최대 10초 대기)");
                isTokenValid = await socialLogin.EnsureValidAppleTokenAsync(userId).Timeout(TimeSpan.FromSeconds(10));
            }
            catch (TimeoutException)
            {
                MyLogger.LogError("[Profile] 🚨 애플 인증 타임아웃 (10초 초과) - 무한 대기 차단");
                isTokenValid = false;
            }
            catch (Exception ex)
            {
                MyLogger.LogError($"[Profile] 🚨 애플 인증 중 예외 발생: {ex.Message}");
                isTokenValid = false;
            }

            if (!isTokenValid)
            {
                RollbackGuestData();
                Managers.Instance.GetUIManager().ShowErrorPopUp(EErrorCloseType.Confirm, "연동 지연", "네트워크 불안정으로 애플 인증이 지연되었습니다.\n잠시 후 다시 시도해 주세요.", null);
                return;
            }
        }

        // ⭕ [핵심 수정] 로컬 저장(Save)을 바로 하지 않고, 서버 승인을 기다리기 위해 임시 변수에만 저장!
        _pendingSocialId = userId;
        _pendingPlatform = providerCode;

        MyLogger.Log($"[Profile] 서버 연동 요청 준비 완료 (아직 로컬 확정 안됨) -> 플랫폼: {providerCode}, ID: {userId}");

        // 서버로 연동 API 통신 쏘기! (이 순간 유저의 기기는 여전히 안전하게 게스트 상태입니다)
        Managers.Instance.GetServerManager().RequestAccountLinking(payload, providerCode);
    }

    private void HandleSocialLoginFailure(string error)
    {
        if (!_isLinkingAccount) return;
        
        RollbackGuestData(); // 취소/실패했으니 롤백
        Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("연동이 취소되었습니다.", "ToastMessage");
    }

    private string _pendingSocialId = "";
    private int _pendingPlatform = 0;
    private void HandleAccountLinkSuccess()
    {
        if (!_isLinkingAccount) return;
        _isLinkingAccount = false;

        // ★ [핵심 추가] 서버도 연동을 승인했으므로, 이제 기기의 로컬 데이터를 진짜 소셜 계정으로 확정!
        GameDefineData.SetString(Define.KEY_USER_SOCIAL_ID, _pendingSocialId);
        GameDefineData.SetInt(Define.KEY_PLATFORM, _pendingPlatform);
    
        // 1. 성공했으니 백업해둔 게스트 데이터 청소
        GameDefineData.DeleteData("BACKUP_GUEST_ID");
        GameDefineData.DeleteData("BACKUP_PLATFORM");
        GameDefineData.Save(); // ★ 여기서 최종 1회만 Save()
    
        // 2. 연동 성공 후 버튼 강제 비활성화 로직
        UpdateAccountLinkButtonVisibility(); 
        if (btnGoogleLink != null) btnGoogleLink.SetActive(false);
    
        // 3. 성공 팝업
        Managers.Instance.GetUIManager().ShowCommonToastMessage(
            "성공적으로 계정이 연동되었습니다."
        );

        MyLogger.Log("[Profile] 연동 성공 처리 완료 및 로컬 데이터 확정 저장 완료");
    }

    private void HandleAccountLinkFailed()
    {
        if (!_isLinkingAccount) return;
        
        MyLogger.Log("[Profile] 서버 연동 실패(-10 등) 감지 -> 게스트 롤백 진행");
        RollbackGuestData();
        // 팝업은 GameManager에서 이미 띄웠으므로(Confirm 타입), 여기서는 백업 데이터로 롤백만 시켜주면 됩니다!
    }
    #endregion
    
    private void SetGuildIcon()
    {
        if (_guildIcon == null) return;

        if (UserInfoData.ExistGuild && UserInfoData.guildInfo.guildPattern > 0)
        {
            string markSpriteName = $"GuildMark{UserInfoData.guildInfo.guildPattern.ToString("00")}";
            _guildIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.GuildAtlas, markSpriteName);
            _guildIcon.gameObject.SetActive(true);
        }
        else
        {
            _guildIcon.gameObject.SetActive(false);
        }
    }

    private void UpdateAccountLinkButtonVisibility()
    {
        // 1. 현재 로컬에 저장된 진짜 플랫폼 확인
        int currentPlatform = GameDefineData.GetInt(Define.KEY_PLATFORM, -1);
        
        // ★ [보강] _isLinkingAccount가 진행 중이 아니고, 확실한 게스트(0)일 때만 true
        bool isGuest = (currentPlatform == 0);

        // 2. 게스트가 아니면 무조건 다 끄고 함수 종료
        if (!isGuest)
        {
            if (btnGoogleLink != null) btnGoogleLink.SetActive(false);
            // if (btnAppleLink != null) btnAppleLink.SetActive(false);
            return;
        }

        // 3. 게스트일 경우에만 플랫폼에 맞춰 활성화
#if UNITY_ANDROID
        // AOS: 구글 연동 버튼만 노출
        if (btnGoogleLink != null) btnGoogleLink.SetActive(true);
        // if (btnAppleLink != null) btnAppleLink.SetActive(false);
#elif UNITY_IOS
        // iOS: 구글, 애플 연동 버튼 모두 노출
        if (btnGoogleLink != null) btnGoogleLink.SetActive(true);
        // if (btnAppleLink != null) btnAppleLink.SetActive(true);
#else
        // Editor 등 환경: 기본 구글만 노출
        if (btnGoogleLink != null) btnGoogleLink.SetActive(true);
        // if (btnAppleLink != null) btnAppleLink.SetActive(false);
#endif
    }
}