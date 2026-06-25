using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class PolicyUIManager : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Toggle termsToggle;
    [SerializeField] Toggle privacyToggle;
    [SerializeField] Toggle pushToggle;        // (선택) 이벤트 Push 수신 동의
    [SerializeField] Toggle nightPushToggle;   // (선택) 야간 이벤트 Push 동의
    
    [SerializeField] Button acceptButton;

    [SerializeField] private Button url1Btn, url2Btn;
    
    public bool IsAccepted => PlayerPrefs.GetInt("PolicyAccepted", 0) == 1;

    UniTaskCompletionSource<bool> acceptTcs;

    public void Show()
    {
        panel.SetActive(true);

        // 상태/리스너 초기화 (중복 방지)
        termsToggle.isOn = false;
        privacyToggle.isOn = false;
        acceptButton.interactable = false;

        termsToggle.onValueChanged.RemoveAllListeners();
        privacyToggle.onValueChanged.RemoveAllListeners();
        pushToggle.onValueChanged.RemoveAllListeners();
        nightPushToggle.onValueChanged.RemoveAllListeners();
        
        acceptButton.onClick.RemoveAllListeners();

        termsToggle.onValueChanged.AddListener(_ => UpdateAcceptable());
        privacyToggle.onValueChanged.AddListener(_ => UpdateAcceptable());
        acceptButton.onClick.AddListener(OnAccept);
    }

    public void Hide()
    {
        panel.SetActive(false);

        // 필요하다면 리스너 제거, 리소스 정리 등 추가
        termsToggle.onValueChanged.RemoveAllListeners();
        privacyToggle.onValueChanged.RemoveAllListeners();
        acceptButton.onClick.RemoveAllListeners();
    }

    void UpdateAcceptable()
    {
        acceptButton.interactable = termsToggle.isOn && privacyToggle.isOn;
    }

    void OnAccept()
    {
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");

        
        // 1. 필수 약관 저장
        PlayerPrefs.SetInt("PolicyAccepted", 1);
        
        // 2. Push 동의 상태 저장 (새로운 로직)
        // 이 정보를 서버로 보내는 것이 이상적이지만, 일단 클라이언트에 저장
        PlayerPrefs.SetInt("PushAccepted", pushToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("NightPushAccepted", nightPushToggle.isOn ? 1 : 0);
        
        PlayerPrefs.Save();
        Hide();
        acceptTcs?.TrySetResult(true);
    }

    /// <summary>
    /// 타이틀 흐름에서 await로 호출
    /// </summary>
    public async UniTask WaitForAcceptAsync()
    {
        if (IsAccepted) return;

        acceptTcs = new UniTaskCompletionSource<bool>();
        Show();
        await acceptTcs.Task;
    }
    
    public void OnClickTermsAndConditions()
    {
        
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");

        
        #if UNITY_IOS
                Application.OpenURL("https://sites.google.com/view/root3games-terms/%ED%99%88");
                //Application.OpenURL("https://sites.google.com/superplanet.net/superplanet-service/terms-of-service?authuser=0");
                // OpenUrlManager.OpenURLInBrowser("https://sites.google.com/superplanet.net/superplanet-service/terms-of-service?authuser=0");
        #else
                //Application.OpenURL("https://sites.google.com/superplanet.net/superplanet-service/terms-of-service?authuser=0");
                Application.OpenURL("https://sites.google.com/view/root3games-terms/%ED%99%88");
        #endif
        
        // Application.OpenURL("https://m.cafe.naver.com/demigodidle/4");
        //https://m.cafe.naver.com/demigodidle/4
    }

    public void OnClickPersonalIinformation()
    {
        
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");

        
        #if UNITY_IOS
                Application.OpenURL("https://sites.google.com/view/root3games-privacy/%ED%99%88");
                // OpenUrlManager.OpenURLInBrowser("https://sites.google.com/superplanet.net/superplanet-service");
        #else
                // Application.OpenURL("https://sites.google.com/superplanet.net/superplanet-service");
                Application.OpenURL("https://sites.google.com/view/root3games-privacy/%ED%99%88");
        #endif
    }
}