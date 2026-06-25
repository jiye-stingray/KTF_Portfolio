using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameNoticePopup : UIPopupBase
{
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject ui;
    [SerializeField] private CanvasGroup canvasGroup; // ui

    [SerializeField] private Toggle dontShowTodayToggle; // 오늘 그만보기 토글
    
    [Header("Link Buttons")]
    [SerializeField] private Button video1Button; // Video_1 버튼 연결용 (네이버 라운지)
    [SerializeField] private Button video2Button; // Video_2 버튼 연결용 (이벤트 페이지)
    [SerializeField] private Button video3Button; // Video_3 버튼 연결용 (픽업소환 안내 페이지)

    private const string HIDE_KEY = "GameNoticePopup_HideDate";
    
    // 🔗 이동할 웹 링크 주소 정의
    private const string LOUNGE_URL = "https://game.naver.com/lounge/Mini_Yokai_Hunters/board/1"; // 네이버 라운지 링크
    private const string EVENT_URL = "https://game.naver.com/lounge/Mini_Yokai_Hunters/board/8"; // 이벤트 게시판 링크 주소 입력
    private const string PICKUP_URL = "https://game.naver.com/lounge/Mini_Yokai_Hunters/board/12"; // 픽업소환 공지 링크 주소 입력

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Instance.Sound.PlaySFX("Effect", "SE_Inventory_Open_01");

        // 1. 오늘 그만보기 설정 확인 (이미 체크했다면 팝업 끄기)
        if (!ShouldShowToday())
        {
            gameObject.SetActive(false);
            return false;
        }
        
        ui.SetActive(true);

        // 2. 등장 애니메이션
        ui.transform.localScale = Vector3.one * 0.94f;
        canvasGroup.alpha = 0f;

        DOTween.Sequence()
            .Join(canvasGroup.DOFade(1f, 0.20f).SetEase(Ease.OutCubic))
            .Append(ui.transform.DOScale(1.02f, 0.18f).SetEase(Ease.OutCubic))
            .Append(ui.transform.DOScale(1f,    0.10f).SetEase(Ease.InOutSine));

        // 3. 닫기 버튼 이벤트 연결
        closeButton.onClick.RemoveAllListeners(); 
        closeButton.onClick.AddListener(OnClickClose); 

        // 4. Video_1 라운지 링크 버튼 이벤트 연결
        if (video1Button != null)
        {
            video1Button.onClick.RemoveAllListeners();
            video1Button.onClick.AddListener(() => OpenWebLink(LOUNGE_URL));
        }

        // 5. Video_2 이벤트 링크 버튼 이벤트 연결 (추가됨)
        if (video2Button != null)
        {
            video2Button.onClick.RemoveAllListeners();
            video2Button.onClick.AddListener(() => OpenWebLink(EVENT_URL));
        }

        // 6. Video_3 픽업소환 링크 버튼 이벤트 연결 (추가됨)
        if (video3Button != null)
        {
            video3Button.onClick.RemoveAllListeners();
            video3Button.onClick.AddListener(() => OpenWebLink(PICKUP_URL));
        }

        // 7. 체크박스 초기화
        if (dontShowTodayToggle != null)
            dontShowTodayToggle.isOn = false;

        return true;
    }
    
    // 💡 효과음 재생 및 외부 브라우저 오픈 통합 처리 함수
    private void OpenWebLink(string url)
    {
        // 버튼 클릭음 재생
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuSelect");
        
        // 기기의 기본 웹 브라우저를 열어 해당 URL로 이동합니다.
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
        }
    }

    private void OnClickClose()
    {
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuClose");
        
        // 1. 토글 체크 확인 및 저장
        if (dontShowTodayToggle != null && dontShowTodayToggle.isOn)
        {
            string today = DateTime.Now.ToString("yyyyMMdd");
            PlayerPrefs.SetString(HIDE_KEY, today);
            PlayerPrefs.Save();
            MyLogger.Log($"[GameNotice] 오늘 하루 보지 않기 설정됨: {today}");
        }

        // 2. 퇴장 애니메이션 및 종료
        DOTween.Kill(ui.transform);
        DOTween.Kill(canvasGroup);

        DOTween.Sequence()
            .Join(ui.transform.DOScale(0.98f, 0.12f).SetEase(Ease.InCubic))
            .Join(canvasGroup.DOFade(0f, 0.16f).SetEase(Ease.InCubic))
            .OnComplete(() => ClickCloseBtn()); // 부모 클래스의 닫기 처리
    }
    
    public static bool ShouldShowToday()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");
        string savedDate = PlayerPrefs.GetString(HIDE_KEY, "");
        return savedDate != today;
    }
}