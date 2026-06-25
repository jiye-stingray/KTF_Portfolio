using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LuckySpinUI : UIBase
{
    private const int FULL_CIRCLE = 360;

    // ── 저장 키 / 설정 ────────────────────────────────────────────────
    private const string KEY_PAID_SPIN_REMAIN   = "LUCKY_SPIN_PAID_REMAIN";  // 결제 충전 잔여
    private const string KEY_FREE_SPIN_USED_DAY = "LUCKY_SPIN_FREE_DAY";     // 무료 쓴 날(서버 DayOfYear)
    private const string KEY_IAP_LAST_TICKS      = "LUCKY_SPIN_IAP_TICKS";    // 마지막 결제 시각(Ticks)
    private const int FREE_SPIN_PER_DAY = 1;                                  // 하루 무료 횟수
    private const double IAP_COOLDOWN_HOURS = 24.0;                           // 결제 후 재결제 쿨다운(시간)

    // 테스트용: true면 Open 시 저장 키를 모두 초기화 (배포 전 반드시 false)
    [Header("DEBUG")]
    [Tooltip("켜면 Open 시 저장 키 초기화 (테스트 전용)")]
    [SerializeField] private bool _debugResetOnOpen = false;

    [Header("UI References")]
    [SerializeField] private int[] _amounts;
    [SerializeField] private Button _btnStartTurn;

    [SerializeField] private Image[] _imgRewards;
    [SerializeField] private Image _imgFocusLine;
    [SerializeField] private Image _imgRewardIcon;
    [SerializeField] private TextMeshProUGUI _rewardAmount;
    [SerializeField] private TextMeshProUGUI[] _amountRewards;

    [SerializeField] private GameObject _circle;
    [SerializeField] private GameObject _goRewardPopup;

    [Header("Spin Mode UI (선택 — 비워둬도 동작)")]
    [Tooltip("상단 남은 횟수 표시 (MaxCntText)")]
    [SerializeField] private TextMeshProUGUI _maxCntText;
    [Tooltip("결제(IAP) 버튼 루트 — 하루 1회, 결제 시 숨김")]
    [SerializeField] private GameObject _goIapButton;
    [SerializeField] private Button _btnIap;
    [Tooltip("결제 버튼에 표시할 가격 텍스트")]
    [SerializeField] private TextMeshProUGUI _priceText;
    [Tooltip("결제 후 다음 결제까지 남은 시간 (DateText) — 처음 숨김, 결제 시 표시")]
    [SerializeField] private GameObject _goDateText;
    [SerializeField] private TextMeshProUGUI _dateText;
    [Tooltip("스핀 중 막을 닫기 버튼 (LuckySpinUI/BottomArea/CloseButton)")]
    [SerializeField] private Button _btnClose;

    [Header("Config Parameters")]
    [SerializeField] private int _spinRounds = 5;
    [SerializeField] private float _spinDuration = 3f;
    [SerializeField] private List<AnimationCurve> _animationCurves;

    [Header("Paid Pack Config")]
    [Tooltip("$1.99 결제 1건당 충전되는 spin 횟수")]
    [SerializeField] private int _spinsPerPaidPack = 3;
    [Tooltip("결제 버튼 가격 표시 문자열")]
    [SerializeField] private string _priceString = "$1.99";

    // ── 결제 요청 콜백 (실제 결제 연동 시 사용. 비워두면 결제 성공 가정) ──
    public Action<Action> RequestPaidSpin;

    private bool _isSpinning;
    private bool _isCanClose = true;
    private float _anglePerItem;
    private int _rewardIndex;

    // 글로벌 시간 (서버 시간) — 기기 시계 조작 우회 방지.
    // 프로젝트 전역에서 쓰는 ServerTime.Instance.CurrentTime()와 동일 기준.
    private DateTime UtcNow => ServerTime.Instance.CurrentTime();

    // 결제 충전 잔여 — GameDefineData 영속 저장
    private int _paidSpinRemain
    {
        get => GameDefineData.GetInt(KEY_PAID_SPIN_REMAIN, 0);
        set
        {
            GameDefineData.SetInt(KEY_PAID_SPIN_REMAIN, Mathf.Max(0, value));
            GameDefineData.Save();
        }
    }

    // 오늘 무료 가능 여부 (UTC DayOfYear 비교)
    private bool IsFreeAvailable
    {
        get
        {
            int today = UtcNow.DayOfYear;
            int usedDay = GameDefineData.GetInt(KEY_FREE_SPIN_USED_DAY, -1);
            return usedDay != today;
        }
    }

    // 오늘 결제 가능 여부 (마지막 결제 후 24시간 경과 시 가능)
    private bool IsIapAvailable => IapCooldownRemain <= TimeSpan.Zero;

    // 다음 결제까지 남은 시간 (결제 시각 + 24h - 현재). 결제 이력 없으면 0.
    private TimeSpan IapCooldownRemain
    {
        get
        {
            string s = GameDefineData.GetString(KEY_IAP_LAST_TICKS, "");
            if (string.IsNullOrEmpty(s) || !long.TryParse(s, out long lastTicks))
                return TimeSpan.Zero;

            DateTime last = new DateTime(lastTicks);
            DateTime unlock = last.AddHours(IAP_COOLDOWN_HOURS);
            TimeSpan remain = unlock - UtcNow;
            return remain > TimeSpan.Zero ? remain : TimeSpan.Zero;
        }
    }

    private int TotalRemain => (IsFreeAvailable ? FREE_SPIN_PER_DAY : 0) + _paidSpinRemain;

    public override bool Init()
    {
        if (base.Init() == false) return false;

        _btnStartTurn.onClick.AddListener(OnClickStartSpinWheel);
        if (_btnIap != null)
            _btnIap.onClick.AddListener(OnClickIap);
        return true;
    }

    public override void Open()
    {
        base.Open();

#if UNITY_EDITOR || DEV_SERVER_SET
        if (_debugResetOnOpen)
            DebugResetKeys();
#endif

        _isSpinning = false;
        _isCanClose = true;
        _goRewardPopup.SetActive(false);
        _imgFocusLine.gameObject.SetActive(true);
        _btnStartTurn.interactable = true;
        SetCloseButtonActive(true);

        RefreshUI();
    }

    public override void Close()
    {
        if (!_isCanClose) return;   // 스핀 중 닫기 차단
        base.Close();
    }

    private void Start()
    {
        _isSpinning = false;
        _anglePerItem = (_imgRewards != null && _imgRewards.Length > 0)
            ? (float)FULL_CIRCLE / _imgRewards.Length
            : FULL_CIRCLE / 8f;
    }

    public void OnClickClaimReward()
    {
        _goRewardPopup.SetActive(false);
    }

    // ── Start 버튼: 무료 → 충전 잔여 순으로 1회 소모하여 spin ─────────────
    private void OnClickStartSpinWheel()
    {
        if (_isSpinning) return;

        if (_imgRewards == null || _imgRewards.Length == 0)
        {
            Debug.LogWarning("[LuckySpinUI] _imgRewards가 비어 있습니다.");
            return;
        }

        // 1) 오늘 무료 가능 → 무료 사용 처리 (UTC 날짜 기록)
        if (IsFreeAvailable)
        {
            GameDefineData.SetInt(KEY_FREE_SPIN_USED_DAY, UtcNow.DayOfYear);
            GameDefineData.Save();
            BeginSpin();
            return;
        }

        // 2) 결제 충전 잔여 → 1회 소모
        if (_paidSpinRemain > 0)
        {
            _paidSpinRemain--;
            BeginSpin();
            return;
        }

        // 3) 둘 다 없음 → 부족 토스트
        Managers.Instance.GetUIManager()
            .ShowUIToast<UIToastBase>("스핀 횟수가 부족합니다.", "ToastMessage");
        RefreshUI();
    }

    // ── IAP 버튼: 하루 1회 제한 ──────────────────────────────────────────
    private void OnClickIap()
    {
        if (_isSpinning) return;

        if (!IsIapAvailable)
        {
            Debug.Log("[LuckySpinUI] 오늘 결제는 이미 완료됨 (하루 1회).");
            RefreshUI();
            return;
        }

        if (RequestPaidSpin != null)
            RequestPaidSpin(OnIapSuccess);
        else
            OnIapSuccess(); // 결제 성공 가정 (실제 연동 전 임시)
    }

    private void OnIapSuccess()
    {
        _paidSpinRemain += _spinsPerPaidPack;

        // 결제 시각(Ticks) 저장 → 24시간 쿨다운 시작
        GameDefineData.SetString(KEY_IAP_LAST_TICKS, UtcNow.Ticks.ToString(), encrypt: false);
        GameDefineData.Save();

        RefreshUI();
        Debug.Log($"[LuckySpinUI] IAP 결제 성공 → {_spinsPerPaidPack}회 충전. 잔여 {_paidSpinRemain}. 24h 쿨다운 시작.");
    }

    private void BeginSpin()
    {
        if (_anglePerItem <= 0f)
            _anglePerItem = (float)FULL_CIRCLE / _imgRewards.Length;

        _imgFocusLine.gameObject.SetActive(false);

        _rewardIndex = UnityEngine.Random.Range(0, _imgRewards.Length);
        float targetAngle = _spinRounds * FULL_CIRCLE + (_rewardIndex * _anglePerItem);

        StartCoroutine(CoSpinTheWheel(_spinDuration, targetAngle));
    }

    // 상단 횟수 + 결제 버튼/타이머 표시 갱신
    private void RefreshUI()
    {
        if (_maxCntText != null)
            _maxCntText.text = TotalRemain.ToString();

        if (_priceText != null)
            _priceText.text = _priceString;

        UpdateIapButtonVisible();
    }

    // 오늘 결제 가능 → 버튼 표시 / 타이머 숨김
    // 오늘 결제 완료 → 버튼 숨김 / 타이머 표시
    private void UpdateIapButtonVisible()
    {
        bool canBuy = IsIapAvailable;

        if (_goIapButton != null)
            _goIapButton.SetActive(canBuy);

        if (_btnIap != null)
            _btnIap.interactable = canBuy && !_isSpinning;

        if (_goDateText != null)
            _goDateText.SetActive(!canBuy);
    }

    private void Update()
    {
        // 결제 완료(쿨다운 중) 상태에서만 남은 시간 갱신
        if (_dateText == null || _goDateText == null || !_goDateText.activeSelf)
            return;

        TimeSpan remain = IapCooldownRemain;   // 결제 후 24시간 - 경과

        _dateText.text = string.Format("초기화 시간\n{0:00}:{1:00}:{2:00}",
            (int)remain.TotalHours, remain.Minutes, remain.Seconds);
    }

    private void SetCloseButtonActive(bool enable)
    {
        if (_btnClose == null) return;
        _btnClose.interactable = enable;            // 클릭만 막기
        // _btnClose.gameObject.SetActive(enable);  // 아예 숨기려면 이 줄 사용
    }

    private IEnumerator CoSpinTheWheel(float duration, float totalAngle)
    {
        _isSpinning = true;
        _isCanClose = false;
        _btnStartTurn.interactable = false;
        if (_btnIap != null) _btnIap.interactable = false;
        SetCloseButtonActive(false);

        float timer = 0f;
        float startAngle = _circle.transform.eulerAngles.z;
        float finalAngle = totalAngle - startAngle;

        AnimationCurve curve = (_animationCurves != null && _animationCurves.Count > 0)
            ? _animationCurves[UnityEngine.Random.Range(0, _animationCurves.Count)]
            : null;

        while (timer < duration)
        {
            float t = timer / duration;
            float eased = (curve != null) ? curve.Evaluate(t) : EaseOutCubic(t);
            _circle.transform.eulerAngles = new Vector3(0f, 0f, finalAngle * eased + startAngle);
            timer += Time.deltaTime;
            yield return null;
        }

        _circle.transform.eulerAngles = new Vector3(0f, 0f, totalAngle);
        _isSpinning = false;

        StartCoroutine(CoShowRewardPopup(_rewardIndex));
    }

    private static float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        float inv = 1f - t;
        return 1f - inv * inv * inv;
    }

    private IEnumerator CoShowRewardPopup(int index)
    {
        yield return new WaitForSeconds(0.4f);

        _btnStartTurn.interactable = true;
        _isCanClose = true;
        SetCloseButtonActive(true);
        _imgFocusLine.gameObject.SetActive(true);
        _goRewardPopup.SetActive(true);

        if (_imgRewards != null && index >= 0 && index < _imgRewards.Length && _imgRewards[index] != null)
            _imgRewardIcon.sprite = _imgRewards[index].sprite;

        if (_amountRewards != null && index >= 0 && index < _amountRewards.Length && _amountRewards[index] != null)
            _rewardAmount.text = _amountRewards[index].text;

        RefreshUI();
    }

#if UNITY_EDITOR || DEV_SERVER_SET
    // 테스트용: 저장 키 전체 초기화
    private void DebugResetKeys()
    {
        GameDefineData.SetInt(KEY_PAID_SPIN_REMAIN, 0);
        GameDefineData.SetInt(KEY_FREE_SPIN_USED_DAY, -1);
        GameDefineData.SetString(KEY_IAP_LAST_TICKS, "", encrypt: false);
        GameDefineData.Save();
        Debug.Log("[LuckySpinUI] DEBUG 저장 키 초기화 완료 (무료/충전/결제쿨다운 리셋).");
    }
#endif
}