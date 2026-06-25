using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
public enum ToggleSettingType
{
    BGM,
    SFX,
    //Vibration,
    Joystick,
    Damage,
    Chatting,
    Economy
}


public class SettingUI : UIPopupBase
{
    
    private const string PP_SOUND = "sound_on";
    private const string PP_VFX = "vfx_on";
    private const string PP_VIBRATION = "vibration_on";
    
    [Header("Language Components")] 
    public Button languageBtn;
    public TMP_Text languageText;
    [SerializeField] private LanguageUI _langPopup;

    [Header("BGM Components")]
    [SerializeField] private ToggleGroup _bgmToggleGroup;

    [Header("SFX Components")]
    [SerializeField] private ToggleGroup _sfxToggleGroup;

    [Header("Joystick")]
    [SerializeField] private ToggleGroup _joystickToggleGroup;
    
    [Header("Damage")]
    [SerializeField] private ToggleGroup _damageToggleGroup;

    [Header("Chatting")]
    [SerializeField] private ToggleGroup _chattingToggleGroup;

    [Header("Economy")]
    [SerializeField] private ToggleGroup _economyToggleGroup;

    private bool bEvent = false;

    [Header("Coupon")]
    [SerializeField] public GameObject _couponPanel;
    [SerializeField] private TMP_InputField _couponInputField;

    [SerializeField] private GameObject btnCoupon;
    
    [Header("Withdrawal")]
    [SerializeField] public GameObject _withdrawalPanel;
    [SerializeField] private TMP_InputField _withdrawalInputField;

    private bool _isInitializing = false;       // 진동 위해 사용했던 변수
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        LoadSettings();
        EnableEvent();

        return true;
    }
    public void LoadSettings()
    {
        _isInitializing = true;
        
        // ON/OFF 옵션들

        InitSettingToggleGroup(_bgmToggleGroup,ToggleSettingType.BGM);
        InitSettingToggleGroup(_sfxToggleGroup,ToggleSettingType.SFX);
        InitSettingToggleGroup(_joystickToggleGroup,ToggleSettingType.Joystick);
        InitSettingToggleGroup(_damageToggleGroup,ToggleSettingType.Damage);
        InitSettingToggleGroup(_chattingToggleGroup,ToggleSettingType.Chatting);
        InitSettingToggleGroup(_economyToggleGroup,ToggleSettingType.Economy);

        RefreshCouponButtonVisible();
        
        _isInitializing = false;

    }
    private void InitSettingToggleGroup(ToggleGroup toggleGroup,ToggleSettingType key)
    {
        foreach (var toggle in toggleGroup.GetComponentsInChildren<Toggle>())
        {
            if (toggle.name == "On")
            {
                toggle.isOn = UserInfoData._isToggleOnDic[key];
            }
            else
                toggle.isOn = !UserInfoData._isToggleOnDic[key];

            toggle.onValueChanged.AddListener(isOn => ToggleChangeEvent(isOn,key, toggle));

        }
    }

    public void EnableEvent()
    {
        if(bEvent) return;
        
        
        bEvent = true;
    }

    private void RefreshCouponButtonVisible()
    {
        if (btnCoupon == null)
            return;

        btnCoupon.SetActive(!RestAPIURL.IsReviewMode);
    }
    
    public override void Close()
    {
        // 구독 해제
        UIManager.SettingUIPopup = null;
        base.Close();
    }

    private void InitButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);

    }

    private void ToggleChangeEvent(bool toggleVar, ToggleSettingType key, Toggle changeToggle)
    {
        // false 되는 이벤트는 무시
        if (!toggleVar) return;

        if (changeToggle.name == "On")
            toggleVar = true;
        else
            toggleVar = false;

        UserInfoData._isToggleOnDic[key] = toggleVar;
        PlayerPrefs.SetInt(key.ToString(), toggleVar ? 1 : 0);
        PlayerPrefs.Save();

        UserInfoData.SettingToggleEvent(key);
    }

    #region Coupon

    public void ShowCouponPanel() => _couponPanel.SetActive(true);
    public void CloseCouponPanel()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuClose");

        _couponInputField.text = string.Empty;
        _couponPanel.SetActive(false);
    }

    public void OnCouponBtnClick()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        ShowCouponPanel();
    }

    public void EnterCouponEvent() 
    {
        string text = _couponInputField.text.Trim();
        if(!string.IsNullOrEmpty(text)) 
        {
            // 쿠폰 입력 서버 처리
#if USE_SERVER
            Managers.Instance.GetServerManager().OnPostCoupon(text);
#else

            CloseCouponPanel();      // 성공 후 창 닫기
            return;
#endif


        }
        
    }

#endregion Coupon

    #region Withdrawall

    public void ShowWithdrawalPanel() => _withdrawalPanel.SetActive(true);
    public void CloseWithdrawalPanel()
    {
        _withdrawalInputField.text = string.Empty;
        _withdrawalPanel.SetActive(false);
    }

    public void OnWithdrawalBtnClick()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        
        ShowWithdrawalPanel();
    }

    public void WithdrawalEvent()
    {
        string text = _withdrawalInputField.text.Trim();
        if (text.Equals("delete", StringComparison.OrdinalIgnoreCase))
        {
            // 1. 길드 가입 여부 예외 처리
            var guildInfo = Managers.Instance.UserInfo().guildUserInfo;
            if (guildInfo != null && guildInfo.guildId != 0)
            {
                UIManager.ShowUIToast<UIToastBase>("길드 탈퇴 후에만 회원 탈퇴가 가능합니다.", "ToastMessage");
                return;
            }
            
#if USE_SERVER
            // [라이브 서버 환경]
            // 서버 매니저를 통해 회원 탈퇴 API 요청만 전송합니다.
            // 이후 서버 통신 완결 사인이 오면, 세팅하신 'GetSecessionSuccess' 콜백 함수가 호출되면서 
            // 클라이언트 데이터(PlayerPrefs 등)가 청소되고 앱이 안전하게 꺼집니다.
            Managers.Instance.GetServerManager().OnGetSecession();
#else
            // [로컬 테스트 환경]
            // 서버가 없으므로 UI 스크립트 내부에서 즉시 로컬 데이터를 세척하고 앱을 종료합니다.
            CloseWithdrawalPanel(); // 탈퇴 팝업 창 닫기

            Debug.Log("[Withdrawal-Local] 로컬 테스트 환경 유저 데이터 초기화 및 앱 종료");
            
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            GameDefineData.DeleteAll();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(); 
#endif
#endif
        }
        else
        {
            UIManager.ShowUIToast<UIToastBase>("정확한 문구를 입력해주세요", "ToastMessage");
        }
    }

    /// <summary>
    /// 회원 탈퇴 성공 시 클라이언트 로컬 유저 데이터를 완전히 삭제하고 앱을 종료하는 공통 함수
    /// </summary>
    public void ClearLocalUserDataAndQuit()
    {
        Debug.Log("[Withdrawal] 회원 탈퇴로 인한 클라이언트 로컬 데이터 초기화 및 앱 종료 프로세스 시작");

        // 1. 유니티 로컬 저장소(자동로그인 토큰, 계정 UID, 옵션 세팅 등) 완전히 제거
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2. 별도 파일이나 인메모리에 저장되는 커스텀 정의 데이터 초기화 
        // (프로젝트 구조에 맞게 static 메서드 혹은 인스턴스 초기화 호출)
        GameDefineData.DeleteAll(); 

        // 3. (추가 권장) 현재 메모리(싱글톤) 상에 남아있는 유저 휘발성 정보도 즉시 비워줌
        if (Managers.Instance != null && Managers.Instance.UserInfo() != null)
        {
            //UserInfoData 내부를 초기화하는 함수가 있다면 호출해 주면 더욱 안전합니다.
            //예: Managers.Instance.UserInfo().Clear(); 
        }

        // 4. 플랫폼별 앱 프로세스 종료
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // AOS / iOS 어플리케이션 종료
#endif
    }
#endregion

}
