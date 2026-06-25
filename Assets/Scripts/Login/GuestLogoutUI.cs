using Cysharp.Threading.Tasks;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class GuestLogoutUI : MonoBehaviour
{
    [Header("Popup UI References")]
    [SerializeField] private Button confirmButton;        // 팝업 "확인"
    [SerializeField] private Button cancelButton;         // 팝업 "취소"
    [SerializeField] private TMP_Text messageText;            // 안내 문구 (TMP_Text도 가능)
    
    private TitleManager _titleManager;

    
    // Start is called before the first frame update
    private void Awake()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmLogout);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelLogout);

    }
    
    public void ShowGuestLogoutPopup(TitleManager titleManager)
    {
        _titleManager = titleManager;

        if (messageText != null)
        {
            messageText.text =
                "<color=#FFD93D>게스트 계정</color>으로 플레이 중입니다.\n\n" +
                "<color=#FF5555>로그아웃하면 현재 진행 데이터가 삭제되며 복구가 불가능합니다.</color>\n\n" +
                "정말 로그아웃하시겠습니까?";
        }

        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// [확인] 버튼 클릭 시 → TitleManager.Logout() 실행
    /// </summary>
    private void OnConfirmLogout()
    {
        
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");

        MyLogger.Log("[GuestLogoutHandler] 로그아웃 확인 → TitleManager.Logout() 호출");
        gameObject.SetActive(false);

        MyLogger.Log("게스트 계정 Logout 시작");

        _titleManager?.Logout();
    }

    /// <summary>
    /// [취소] 버튼 클릭 시 → 팝업 닫기
    /// </summary>
    private void OnCancelLogout()
    {
        MyLogger.Log("[GuestLogoutHandler] 로그아웃 취소");
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuClose");

        gameObject.SetActive(false);
    }
}
