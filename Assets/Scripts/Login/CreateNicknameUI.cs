using Cysharp.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateNicknameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button confirmButton;

    private UniTaskCompletionSource<string> _tcs;

    private const string TEMP_KEY = "USER_NICKNAME_TEMP";
    private bool _submitting = false;
    
    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        // warningText.gameObject.SetActive(false);
        
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(text =>
            {
                if (!string.IsNullOrEmpty(text))
                {
                    GameDefineData.SetString(TEMP_KEY, text);
                    GameDefineData.Save();
                }
            });
        }
    }

    private void OnEnable()
    {
        // 열릴 때 임시 저장된 텍스트 복구
        string cached = GameDefineData.GetString(TEMP_KEY, null);
        if (!string.IsNullOrEmpty(cached) && inputField != null)
            inputField.text = cached;
        
        _submitting = false;
        if (confirmButton != null) confirmButton.interactable = true;
        
        var sm = Managers.Instance?.GetServerManager();
        if (sm != null)
        {
            sm.OnCreateNicknameSuccess -= HandleCreateNicknameSuccess;
            sm.OnCreateNicknameSuccess += HandleCreateNicknameSuccess;

            sm.OnCreateNicknameError   -= HandleCreateNicknameError;
            sm.OnCreateNicknameError   += HandleCreateNicknameError;
        }
        
    }
    
    private void OnDisable()
    {
        // 이벤트 해제
        var sm = Managers.Instance?.GetServerManager();
        if (sm != null)
        {
            sm.OnCreateNicknameSuccess -= HandleCreateNicknameSuccess;
            sm.OnCreateNicknameError   -= HandleCreateNicknameError;
        }
    }
    
    public UniTask<string> WaitForNicknameAsync()
    {
        if (this == null || gameObject == null)
        {
            MyLogger.LogWarning("[CreateNicknameUI] 이미 파괴된 객체 접근 방지됨");
            return UniTask.FromResult<string>(null);
        }

        if (_tcs != null && !_tcs.Task.Status.IsCompleted())
        {
            _tcs.TrySetCanceled();
        }

        _tcs = new UniTaskCompletionSource<string>();

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        return _tcs.Task;
        
    }

    private void OnConfirm()
    {
        if (_submitting) return;  // 중복체크
        if (inputField == null) return;
        
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");
        
        string nickname = inputField.text.Trim();
        
        if (string.IsNullOrEmpty(nickname))
        {
            MyLogger.LogWarning("InputField 또는 NicknameText가 연결되지 않았습니다!");
            return;
        }
        
        if (inputField == null) return;
        
        // Check
        if(nickname.Length < 2)
        {
            
            Managers.Instance.GetUIManager().ShowCommonToastMessage("닉네임은 2글자 이상이어야 합니다");
            return;
        }
        if (nickname.Contains(" "))
        {
            Managers.Instance.GetUIManager().ShowCommonToastMessage("닉네임은 공백을 포함할 수 없습니다");
            return;
        }
        
        if (Utils.CheckBanText(nickname))
        {
            Managers.Instance.GetUIManager().ShowCommonToastMessage("닉네임에 사용할 수 없는 단어가 포함되어 있습니다");
            return;
        }
        if(!Regex.IsMatch(nickname, @"^[\p{L}\p{N}]+$")) // 모든 언어 문자(\p{L})와 숫자(\p{N})만 허용
        {
            Managers.Instance.GetUIManager().ShowCommonToastMessage("닉네임에는 특수문자나 이모지를 포함할 수 없습니다");
            return;
        }
        
        _submitting = true;
        if (confirmButton != null) confirmButton.interactable = false;

#if USE_SERVER
        Managers.Instance.GetServerManager().OnPostCreateGameName(nickname);
#endif
    }

    private void HandleCreateNicknameSuccess(string name)
    {
        // UI 쪽 성공 처리 일원화
        NotifyCreateSuccess(name);
    }

    private void HandleCreateNicknameError(BestHttp_APIServiceManager.ErrorResponse error)
    {
        // -2(중복/금지어 등 서버 정의 실패) → 창 유지 + 재입력 대기
        if (error?.ErrorCode != null && error.ErrorCode.ToString().Trim() == "-2")
        {
            // Managers.Instance.GetUIManager().ShowToast("이미 사용 중인 닉네임입니다. 다시 입력해 주세요.");
            var uiManager = Managers.Instance?.GetUIManager();
            var toast = uiManager.ShowUIToast<UIToastBase>("이미 사용 중인 닉네임입니다. 다른 닉네임을 입력해 주세요.", "ToastMessage");
            
            NotifyCreateFail(); // 버튼 활성화 + 포커스 복귀, 창 유지
            return;
        }
    }
    


    public void NotifyCreateSuccess(string nickname)
    {
        _tcs?.TrySetResult(nickname);

        // 임시 캐시 제거(선택)
        GameDefineData.DeleteData(TEMP_KEY);
        GameDefineData.Save();

        _submitting = false;
        if (confirmButton != null) confirmButton.interactable = true;

        gameObject.SetActive(false);
    }

    public void NotifyCreateFail(string message = null)
    {
        _submitting = false;
        if (confirmButton != null) confirmButton.interactable = true;

        if (!string.IsNullOrEmpty(message))
            Managers.Instance?.GetUIManager().ShowUIToast<UIToastBase>(message, "ToastMessage");
            //Managers.Instance.GetUIManager().ShowToast(message);

        if (inputField != null)
        {
            inputField.ActivateInputField();
            inputField.caretPosition = inputField.text?.Length ?? 0;
        }
    }
    
}
