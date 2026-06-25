using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Assets.SimpleSignIn.Google.Scripts;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using JWT = Assets.SimpleSignIn.Google.Scripts.JWT;
using TokenResponse = Assets.SimpleSignIn.Google.Scripts.TokenResponse;

#if UNITY_ANDROID || UNITY_EDITOR || UNITY_IOS
using AppleAuth;
using GUserInfo = Assets.SimpleSignIn.Google.Scripts.UserInfo;
#endif

#if UNITY_IOS
#endif

public class SocialLogin : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private TextMeshProUGUI outputText;

    [Header("Login Buttons")]
    [SerializeField] private GameObject googleLoginButton;
    [SerializeField] private GameObject appleLoginButton;

#if UNITY_ANDROID || UNITY_IOS
    private GoogleAuth googleAuth;
#endif

#if UNITY_IOS && !UNITY_EDITOR
    private IAppleAuthManager appleAuthManager;
#endif

    private enum AuthProvider { None, Google, Apple }
    private AuthProvider currentProvider = AuthProvider.None;

    public Action<string /*platform*/, string /*userId*/, string /*userName*/> OnLoginSucceeded;
    public Action<string /*error*/> OnLoginFailed;
    private void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        googleAuth = new GoogleAuth();
#endif
#if UNITY_IOS && !UNITY_EDITOR
        if (AppleAuthManager.IsCurrentPlatformSupported)
            appleAuthManager = new AppleAuthManager(new PayloadDeserializer());
#endif
    }

    private void Start()
    {
        Application.logMessageReceived += OnLogMessageReceived;
        ConfigurePlatformUI();

#if UNITY_ANDROID || UNITY_IOS
        googleAuth.TryResume(OnGoogleSignIn, OnGoogleTokenResponse);
#endif
#if UNITY_IOS
        // appleAuth.TryResume(OnAppleSignIn, OnAppleTokenResponse);
#endif
    }

    private void Update()
    {
#if UNITY_IOS && !UNITY_EDITOR
        appleAuthManager?.Update();
#endif
    }
    
    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
    }

    private void ConfigurePlatformUI()
    {
#if UNITY_ANDROID
        googleLoginButton?.SetActive(false);
        appleLoginButton?.SetActive(false);
#elif UNITY_IOS
        googleLoginButton?.SetActive(false);
        appleLoginButton?.SetActive(false);
#else
        googleLoginButton?.SetActive(false);
        appleLoginButton?.SetActive(false);
#endif
    }

    #region Public API

    public static async UniTask<(bool success, string userId, int platform)> LoginAsync()
    {
        var tcs = new UniTaskCompletionSource<(bool, string, int)>();

        var obj = GameObject.FindObjectOfType<SocialLogin>();
        if (obj == null)
        {
            Debug.LogError("SocialLogin 오브젝트를 찾을 수 없습니다.");
            return (false, null, -1);
        }

        obj.OnLoginSucceeded = (platformStr, userId, userName) =>
        {
            int platformCode = platformStr == "Google" ? 1 : (platformStr == "Apple" ? 2 : 0);
            tcs.TrySetResult((true, userId, platformCode));
        };

        obj.OnLoginFailed = (error) =>
        {
            Debug.LogError("소셜 로그인 실패: " + error);
            tcs.TrySetResult((false, null, -1));
        };

#if UNITY_ANDROID
    obj.SignInGoogle();
#elif UNITY_IOS
        obj.SignInApple();
#else
    MyLogger.LogWarning("지원되지 않는 플랫폼");
    tcs.TrySetResult((false, null, -1));
#endif

        return await tcs.Task;
    }

    
    public void SignInGoogle()
    {
#if UNITY_ANDROID || UNITY_IOS
        currentProvider = AuthProvider.Google;
        googleAuth.SignIn(OnGoogleSignIn, caching: true);
#else
        SetOutput("Google 로그인은 이 플랫폼에서 지원되지 않습니다.");
#endif
    }

    public void SignInApple()
    {
#if UNITY_IOS && !UNITY_EDITOR
        currentProvider = AuthProvider.Apple;
        
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
        appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                if (credential is IAppleIDCredential appleIdCredential)
                {
                    string userId = appleIdCredential.User;
                    string userName = appleIdCredential.FullName?.GivenName ?? "AppleUser";
                    string identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);

                    Debug.LogError($"Apple ID Token (social id): {identityToken}");

                    // if (!string.IsNullOrEmpty(identityToken))
                    // {
                    //     GameDefineData.SetString(Define.KEY_USER_SOCIAL_ID, identityToken, encrypt: false);
                    // }

                    // GameDefineData.DeleteData(Define.KEY_SERVER_JWT);
                    //
                    // // 플랫폼 기록 (Apple = 2)
                    // GameDefineData.SetInt(Define.KEY_PLATFORM, 2);
                    // GameDefineData.Save();

                    if (!string.IsNullOrEmpty(userName))
                    {
                        GameDefineData.SetString(Define.KEY_USER_SOCIAL_ID, userName, encrypt: false);
                    }
                    
                    GameDefineData.DeleteData(Define.KEY_SERVER_JWT);
                    
                    // 플랫폼 기록 (Apple = 2)
                    GameDefineData.SetInt(Define.KEY_PLATFORM, 2);
                    GameDefineData.Save();
                    
                    
                    SetOutput($"Apple 로그인 성공: {userName}");
                    Debug.LogError("userName = " + userName);
                    Debug.LogError("userId = " + userId);
                    
                    OnLoginSucceeded?.Invoke("Apple", userId, userName);
                }
            },
            error =>
            {
                SetOutput($"Apple 로그인 실패: {error.LocalizedDescription}");
                OnLoginFailed?.Invoke(error.LocalizedDescription);
            });
#else
        SetOutput("Apple 로그인은 iOS에서만 지원됩니다.");
#endif
    }

    public void SignOut()
    {
        switch (currentProvider)
        {
#if UNITY_ANDROID || UNITY_IOS
            case AuthProvider.Google:
                googleAuth.SignOut(revokeAccessToken: true);
                break;
#endif
#if UNITY_IOS
            case AuthProvider.Apple:
                break;
#endif
        }

        SetOutput("Not signed in");
        currentProvider = AuthProvider.None;
    }
 
    #endregion

    #region Google Callbacks

#if UNITY_ANDROID || UNITY_IOS
    private void OnGoogleSignIn(bool success, string error, GUserInfo userInfo)
    {
        if (success)
        {
            SetOutput($"Google 로그인 성공: {userInfo.name}");
            currentProvider = AuthProvider.Google;
            
            OnLoginSucceeded?.Invoke("Google", userInfo.sub, userInfo.name);
        }
        else
        {
            SetOutput($"Google 로그인 실패: {error}");
            OnLoginFailed?.Invoke(error);

        }
    }

    private void OnGoogleTokenResponse(bool success, string error, TokenResponse tokenResponse)
    {
        if (!success)
        {
            SetOutput($"토큰 실패: {error}");
            return;
        }

        // (선택) 서명 검증
        var jwt = new JWT(tokenResponse.IdToken);
        jwt.ValidateSignature(googleAuth.ClientId, OnValidateSignature);
        
        // Debug.LogError($"JWT idToken: {tokenResponse.IdToken}");

        
        // SetOutput($"Google AccessToken: {tokenResponse.AccessToken}");
        
        // var jwt = new JWT(tokenResponse.IdToken);
        // Debug.LogError($"JWT Payload: {jwt.Payload}");
        // jwt.ValidateSignature(googleAuth.ClientId, OnValidateSignature);
    }

     
#endif

    #endregion
 
    // public async UniTask<bool> EnsureValidGoogleTokenAsync()
    // {
    //     var tcs = new UniTaskCompletionSource<bool>();
    //     var auth = new GoogleAuth();
    //
    //     if (auth.SavedAuth == null)
    //     {
    //         Debug.LogWarning("[SocialLogin] SavedAuth가 없습니다. 백그라운드 갱신 불가, 수동 로그인 유도.");
    //         tcs.TrySetResult(false);
    //         return await tcs.Task;
    //     }
    //     
    //     // GetTokenResponse는 토큰이 만료되었으면 자동으로 RefreshAccessToken을 타게 해줍니다.
    //     auth.GetTokenResponse((success, error, tokenResponse) =>
    //     {
    //         if (success && tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.IdToken))
    //         {
    //             // (Step 1에서 이미 저장하지만, 확실하게 한번 더 저장)
    //             GameDefineData.SetString(Define.KEY_SERVER_JWT, tokenResponse.IdToken, encrypt: false);
    //             GameDefineData.Save();
    //             tcs.TrySetResult(true);
    //         }
    //         else
    //         {
    //             Debug.LogError("[SocialLogin] 구글 토큰 갱신 실패: " + error);
    //             tcs.TrySetResult(false);
    //         }
    //     });
    //     return await tcs.Task;
    // }
    
    public async UniTask<bool> EnsureValidGoogleTokenAsync() 
    {
        var tcs = new UniTaskCompletionSource<bool>();
    
        // 새로 new GoogleAuth() 하지 않고, 클래스 멤버인 googleAuth를 그대로 사용
        if (googleAuth == null || googleAuth.SavedAuth == null)
        {
            Debug.LogWarning("[SocialLogin] SavedAuth가 없습니다. 백그라운드 갱신 불가, 수동 로그인 유도.");
            tcs.TrySetResult(false);
            return await tcs.Task;
        }
    
        // 기존 인스턴스에서 토큰 갱신 시도
        googleAuth.GetTokenResponse((success, error, tokenResponse) =>
        {
            if (success && tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                GameDefineData.SetString(Define.KEY_SERVER_JWT, tokenResponse.IdToken, encrypt: false);
                GameDefineData.Save();
                tcs.TrySetResult(true);
            }
            else
            {
                Debug.LogError("[SocialLogin] 구글 토큰 갱신 실패: " + error);
                tcs.TrySetResult(false);
            }
        });
    
        return await tcs.Task;
    }
    
    public async UniTask<bool> EnsureValidAppleTokenAsync(string appleUserId)
    {
        var tcs = new UniTaskCompletionSource<bool>();

#if UNITY_IOS && !UNITY_EDITOR
        if (appleAuthManager == null)
        {
            Debug.LogWarning("[SocialLogin] AppleAuthManager가 없습니다. 수동 로그인 유도.");
            tcs.TrySetResult(false);
            return await tcs.Task;
        }

        if (string.IsNullOrEmpty(appleUserId))
        {
            Debug.LogError("[SocialLogin] 저장된 Apple UserId가 없습니다.");
            tcs.TrySetResult(false);
            return await tcs.Task;
        }

        // 애플 고유 ID를 기반으로 현재 기기에서의 로그인 상태를 확인합니다.
        appleAuthManager.GetCredentialState(
            appleUserId,
            state =>
            {
                if (state == CredentialState.Authorized)
                {
                    // 상태가 유효함 (자동 로그인 통과)
                    tcs.TrySetResult(true);
                }
                else
                {
                    // Revoked(연동 해제됨) 또는 NotFound(찾을 수 없음) 상태일 때
                    Debug.LogError($"[SocialLogin] 애플 인증 상태가 유효하지 않음: {state}");
                    tcs.TrySetResult(false);
                }
            },
            error =>
            {
                Debug.LogError($"[SocialLogin] 애플 상태 검증 에러: {error.LocalizedDescription}");
                tcs.TrySetResult(false);
            }
        );
#else
        // 에디터나 AOS 환경에서는 애플 검증 로직을 타지 않으므로 에러 방지용 통과 (또는 false) 처리
        tcs.TrySetResult(true);
#endif

        return await tcs.Task;
    }
    
    private void OnValidateSignature(bool success, string error)
    {
        AppendOutput(success ? "JWT 시그니처 검증 완료" : $"JWT 시그니처 검증 실패: {error}");
    }

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (logText != null)
            logText.text += condition + "\n";
    }

    #region UI Helpers

    private void SetOutput(string message)
    {
        if (outputText != null)
            outputText.text = message;
    }

    private void AppendOutput(string message)
    {
        if (outputText != null)
            outputText.text += Environment.NewLine + message;
    }

    #endregion
    
}
