using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PermissionDeniedPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button goToSettingsButton;
    [SerializeField] private Button confirmButton;

    private System.Action onClosed; // 콜백 지원 (필요하면)

    void Awake()
    {
        if (goToSettingsButton != null)
            goToSettingsButton.onClick.AddListener(OnClickGoToSettings);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnClickClose);
    }

    /// <summary>
    /// 팝업 노출 (메시지, 닫힘 콜백)
    /// </summary>
    public void Show(string message, System.Action onClosed = null)
    {
        if (messageText != null)
            messageText.text = message;
        this.onClosed = onClosed;
        gameObject.SetActive(true);
    }

    private void OnClickGoToSettings()
    {
       // NativeBindings.OpenSettings();
        // 팝업은 닫지 않음: 유저가 직접 돌아와서 권한 허용했는지 체크
    }

    private void OnClickClose()
    {
        gameObject.SetActive(false);
        onClosed?.Invoke();
    }
}