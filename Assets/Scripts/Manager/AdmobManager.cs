#if ADMOB

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
#if ANALYTICS
using AnalyticsLogEventHelp;
#endif
/*
 *
   씬에 빈 GameObject 생성 → AdmobManager 컴포넌트 부착 (DontDestroyOnLoad 자동)
   AdmobManager.Instance.Initialize();

   
   // 리워드
   AdmobManager.Instance.ShowRewarded(earned =>
   {
       if (earned)
       {
           // 보상 지급
       }
   });

   
 */


// 리워드 광고 결과/실패 사유
public enum RewardedResult
{
    Earned,            // 보상 적립 성공
    Skipped,           // 광고는 떴으나 끝까지 보지 않음(보상 없음)
    NotReady,          // 광고가 아직 로드되지 않음
    AlreadyInProgress, // 이미 광고 진행 중(중복 호출)
    ShowFailed,        // 광고 표시 시도 중 실패
    NotInitialized     // SDK 미초기화
}

public class AdmobManager : MonoBehaviour
{
    public static AdmobManager Instance { get; private set; }

    #region User Toast Messages

    // 광고가 정상적으로 나오지 않을 때 유저에게 보여줄 안내 문구
    private const string MSG_NOT_READY    = "광고가 아직 준비되지 않았습니다. 잠시 후 다시 시도해 주세요.";
    private const string MSG_IN_PROGRESS  = "이미 광고를 준비 중입니다. 잠시만 기다려 주세요.";
    private const string MSG_SHOW_FAILED  = "광고 재생에 실패했습니다. 잠시 후 다시 시도해 주세요.";
    private const string MSG_NOT_INIT     = "광고 모듈을 준비 중입니다. 잠시 후 다시 시도해 주세요.";

    #endregion


    #region Ad Unit Id

#if UNITY_EDITOR || DEV_SERVER_SET || ONE

    // 테스트 광고 (공식 Test ID)
    private const string REWARDED_ID = "ca-app-pub-3940256099942544/5224354917";
#else

    #if UNITY_ANDROID
        //private const string REWARDED_ID = "ca-app-pub-8270124078322153/3953030985";
        private const string REWARDED_ID = "ca-app-pub-3940256099942544/5224354917";
    #elif UNITY_IOS
        private const string REWARDED_ID = "ca-app-pub-8270124078322153/3792519934";
    #else
        private const string REWARDED_ID = "unused";
    #endif

#endif
    #endregion

    private bool _initialized;

    private RewardedAd _rewarded;
    private int _rewardedRetry;
    private bool _rewardEarned;
    private bool _finished;              // 보상 콜백 1회 보장 플래그
    private bool _notifyOnFail = true;   // 현재 진행 중 광고의 실패 시 자체 토스트 노출 여부
    private Action<RewardedResult> _onRewardedFinished;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize(Action onComplete = null)
    {
        if (_initialized)
        {
            MyLogger.Log("[Admob] Already initialized");
            onComplete?.Invoke();
            return;
        }

        MobileAds.RaiseAdEventsOnUnityMainThread = true;

#if UNITY_EDITOR || DEV_SERVER_SET
        var testDeviceIds = new List<string>();

#if UNITY_IOS
        // iOS 시뮬레이터 테스트용
        testDeviceIds.Add(AdRequest.TestDeviceSimulator);

        // iOS 실기기 테스트용
        // Xcode / Unity 로그에 찍히는 TestDeviceId를 아래에 추가
        // 예: testDeviceIds.Add("ABCDEF1234567890");
#endif

        if (testDeviceIds.Count > 0)
        {
            var requestConfiguration = new RequestConfiguration
            {
                TestDeviceIds = testDeviceIds
            };

            MobileAds.SetRequestConfiguration(requestConfiguration);
            MyLogger.Log($"[Admob] Test device config applied. Count = {testDeviceIds.Count}");
        }
        else
        {
            MyLogger.Log("[Admob] No test device ids configured.");
        }
#else
    MyLogger.Log("[Admob] Release build - test device config skipped.");
#endif

        try
        {
            MobileAds.Initialize(initStatus =>
            {
                try
                {
                    _initialized = true;

                    MyLogger.Log("[Admob] Initialized (Rewarded only)");

                    if (initStatus != null)
                    {
                        var map = initStatus.getAdapterStatusMap();
                        if (map != null)
                        {
                            foreach (var pair in map)
                            {
                                var adapterName = pair.Key;
                                var status = pair.Value;
                                MyLogger.Log($"[Admob] Adapter: {adapterName}, State: {status.InitializationState}, Description: {status.Description}");
                            }
                        }
                    }

                    LoadRewarded();
                }
                catch (Exception e)
                {
                    MyLogger.LogWarning($"[Admob] Initialize 콜백 처리 중 예외: {e}");
                }
                finally
                {
                    onComplete?.Invoke();
                }
            });
        }
        catch (Exception e)
        {
            // SDK 초기화 자체가 실패해도 게임이 멈추지 않도록 방어
            MyLogger.LogWarning($"[Admob] MobileAds.Initialize 호출 실패: {e}");
            onComplete?.Invoke();
        }
    }

    // 광고 실패 시 유저에게 안내 토스트를 띄운다. (UI 미준비/예외 상황에도 게임 흐름이 끊기지 않도록 방어)
    private static void ShowAdToast(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        try
        {
            Managers.Instance.GetUIManager().ShowCommonToastMessage(message);
        }
        catch (Exception e)
        {
            MyLogger.LogWarning($"[Admob] 토스트 표시 실패: {e.Message}");
        }
    }

    // 실패 사유에 맞는 안내 문구 반환 (Earned/Skipped는 호출부가 처리하므로 null)
    private static string GetFailMessage(RewardedResult result)
    {
        switch (result)
        {
            case RewardedResult.NotReady:          return $"{MSG_NOT_READY} (E-NOTREADY)";
            case RewardedResult.AlreadyInProgress: return $"{MSG_IN_PROGRESS} (E-INPROGRESS)";
            case RewardedResult.ShowFailed:        return $"{MSG_SHOW_FAILED} (E-SHOWFAIL)";
            case RewardedResult.NotInitialized:    return $"{MSG_NOT_INIT} (E-NOINIT)";
            default:                               return null;
        }
    }
    
    // 광고를 시작조차 못한 실패 통지 (자체 토스트 + 콜백 1회)
    private static void NotifyFail(Action<RewardedResult> cb, RewardedResult result, bool notify)
    {
        if (notify) ShowAdToast(GetFailMessage(result));

        try { cb?.Invoke(result); }
        catch (Exception e) { MyLogger.LogWarning($"[Admob] 실패 콜백 처리 중 예외: {e}"); }
    }

    private AdRequest BuildRequest()
    {
        return new AdRequest();
    }

    public void LoadRewarded()
    {
    #if UNITY_EDITOR
        return;
    #endif

        MyLogger.Log($"[Admob] Using AdUnitId = {REWARDED_ID}");

        _rewarded?.Destroy();
        _rewarded = null;

        MyLogger.Log($"[Admob] LoadRewarded Start / UnitId = {REWARDED_ID}");

        try
        {
        RewardedAd.Load(REWARDED_ID, BuildRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                MyLogger.LogWarning($"[Admob] Rewarded Load Failed: {error?.GetMessage()}");

                if (error != null)
                {
                    MyLogger.LogWarning(
                        $"[Admob] Rewarded Load Failed Domain/Code/Message: " +
                        $"{error.GetCode()} / {error.GetMessage()}"
                    );
                }

                RetryLater(ref _rewardedRetry, LoadRewarded);
                return;
            }

            _rewardedRetry = 0;
            _rewarded = ad;

            var loadedResponseInfo = _rewarded.GetResponseInfo();
            MyLogger.Log($"[Admob] Rewarded Loaded. ResponseInfo = {loadedResponseInfo}");

            _rewarded.OnAdPaid += (adValue) =>
            {
                string adapterName = string.Empty;

                var paidResponseInfo = _rewarded?.GetResponseInfo();
                if (paidResponseInfo != null)
                {
                    adapterName = paidResponseInfo.GetMediationAdapterClassName();
                }

                // 중요:
                // GoogleMobileAds AdValue.Value는 micros 단위.
                // 1,000,000 micros = 통화 1 단위.
                double revenueMicros = adValue.Value;
                double revenue = revenueMicros / 1_000_000d;

    #if SINGULAR
                if (Managers.Instance != null && Managers.Instance.Singular != null)
                {
                    // SingularManager 내부에서 / 1_000_000d 처리하므로
                    // 여기서는 원본 micros 값을 그대로 전달한다.
                    Managers.Instance.Singular.TrackAdRevenueAdmob(
                        adValue.CurrencyCode,
                        revenueMicros,
                        adapterName
                    );
                }
    #endif

    #if ANALYTICS
                // Firebase에는 반드시 변환된 실제 수익값을 전달한다.
                AnalyticsLogEventHelper.LogAdImpression(
                    "admob",
                    REWARDED_ID,
                    adValue.CurrencyCode,
                    revenue,
                    adapterName
                );
    #endif

                MyLogger.Log(
                    $"[Admob] OnAdPaid - " +
                    $"RevenueMicros: {revenueMicros}, " +
                    $"Revenue: {revenue}, " +
                    $"Currency: {adValue.CurrencyCode}, " +
                    $"Network: {adapterName}"
                );
            };

            _rewarded.OnAdFullScreenContentClosed += () =>
            {
                MyLogger.Log("[Admob] Rewarded Closed");

                LoadRewarded();

                // earned 콜백이 이미 처리했으면 무시되고, 적립 안 된 경우(스킵/취소)에만 Skipped로 마감
                InvokeRewardedFinished(_rewardEarned ? RewardedResult.Earned : RewardedResult.Skipped);
            };

            _rewarded.OnAdFullScreenContentFailed += err =>
            {
                MyLogger.LogWarning($"[Admob] Rewarded Show Failed: {err?.GetMessage()}");

                LoadRewarded();

                // 표시 실패는 기술적 오류이므로 안내 토스트 노출
                InvokeRewardedFinished(RewardedResult.ShowFailed);
            };
        });
        }
        catch (Exception e)
        {
            // Load 호출 자체가 실패해도 다음 시도를 위해 재시도 예약
            MyLogger.LogWarning($"[Admob] RewardedAd.Load 호출 실패: {e}");
            RetryLater(ref _rewardedRetry, LoadRewarded);
        }
    }

    // 기존 호출부 호환용 오버로드. (earned == true 일 때만 성공)
    public bool ShowRewarded(Action<bool> onFinished)
    {
        return ShowRewarded(result => onFinished?.Invoke(result == RewardedResult.Earned), true);
    }

    // 결과 사유까지 전달하는 메인 진입점.
    // notifyOnFail = true 이면 광고가 안 나오는 기술적 실패(NotReady/NotInitialized/InProgress/ShowFailed)에
    // 대해 AdmobManager가 직접 안내 토스트를 띄운다. (Earned/Skipped는 호출부가 처리)
    public bool ShowRewarded(Action<RewardedResult> onFinished, bool notifyOnFail = true)
    {
#if UNITY_EDITOR
        MyLogger.Log("[Admob] Editor - Rewarded simulated (earned=true)");
        try { onFinished?.Invoke(RewardedResult.Earned); }
        catch (Exception e) { MyLogger.LogWarning($"[Admob] Editor 콜백 예외: {e}"); }
        return true;
#else
        try
        {
            // 이전 광고 흐름이 아직 끝나지 않았으면 중복 호출 방지
            if (_onRewardedFinished != null)
            {
                MyLogger.LogWarning("[Admob] Rewarded already in progress");
                NotifyFail(onFinished, RewardedResult.AlreadyInProgress, notifyOnFail);
                return false;
            }

            // SDK 미초기화 상태면 안내 후 초기화 시도
            if (!_initialized)
            {
                MyLogger.LogWarning("[Admob] Not initialized yet");
                NotifyFail(onFinished, RewardedResult.NotInitialized, notifyOnFail);
                Initialize();
                return false;
            }

            // 광고가 아직 준비되지 않음
            if (_rewarded == null || !_rewarded.CanShowAd())
            {
                MyLogger.Log("[Admob] Rewarded Not Ready");
                NotifyFail(onFinished, RewardedResult.NotReady, notifyOnFail);
                LoadRewarded();
                return false;
            }

            // 정상 표시
            _onRewardedFinished = onFinished;
            _notifyOnFail = notifyOnFail;
            _rewardEarned = false;
            _finished = false;

            _rewarded.Show((Reward reward) =>
            {
                try
                {
                    _rewardEarned = true;
                    MyLogger.Log($"[Admob] Reward Earned: {reward.Amount} {reward.Type}");

                    // 보상은 적립 콜백 기준으로 즉시 확정한다.
                    // (네트워크/미디에이션에 따라 earned-close 순서가 보장되지 않으므로 close에 의존하지 않음)
                    InvokeRewardedFinished(RewardedResult.Earned);
                }
                catch (Exception e)
                {
                    MyLogger.LogWarning($"[Admob] Reward 콜백 처리 중 예외: {e}");
                    InvokeRewardedFinished(_rewardEarned ? RewardedResult.Earned : RewardedResult.Skipped);
                }
            });
            return true;
        }
        catch (Exception e)
        {
            // Show 과정에서 예외가 나도 보상 흐름이 멈추지 않도록 실패로 마감
            MyLogger.LogWarning($"[Admob] ShowRewarded 예외: {e}");

            if (_onRewardedFinished != null)
            {
                // 이미 진행 상태가 등록됐다면 1회 보장 경로로 마감
                InvokeRewardedFinished(RewardedResult.ShowFailed);
            }
            else
            {
                NotifyFail(onFinished, RewardedResult.ShowFailed, notifyOnFail);
            }

            LoadRewarded();
            return false;
        }
#endif
    }

    // 보상 완료 콜백을 어디서 불리든 딱 1회만 실행한다. (중복 지급/누락 동시 방지)
    private void InvokeRewardedFinished(RewardedResult result)
    {
        if (_finished) return;
        _finished = true;

        var cb = _onRewardedFinished;
        bool notify = _notifyOnFail;
        _onRewardedFinished = null;
        _rewardEarned = false;

        // 기술적 실패(ShowFailed 등)는 유저에게 안내. (Earned/Skipped는 null 반환되어 토스트 안 뜸)
        if (notify) ShowAdToast(GetFailMessage(result));

        try { cb?.Invoke(result); }
        catch (Exception e) { MyLogger.LogWarning($"[Admob] 보상 완료 콜백 예외: {e}"); }
    }

    public bool IsRewardedReady()
    {
#if UNITY_EDITOR
        return true;
#else
        return _rewarded != null && _rewarded.CanShowAd();
#endif
    }

    private void RetryLater(ref int retryCount, Action retryAction)
    {
        retryCount = Mathf.Min(retryCount + 1, 6);
        float delay = Mathf.Pow(2, retryCount);
        MyLogger.Log($"[Admob] Retry in {delay:F0}s (Rewarded)");
        StartCoroutine(CoRetry(delay, retryAction));
    }

    private IEnumerator CoRetry(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    private void OnDestroy()
    {
        _rewarded?.Destroy();
        _rewarded = null;
    }
}

#endif
