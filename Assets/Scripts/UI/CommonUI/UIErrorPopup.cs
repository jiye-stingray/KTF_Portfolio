using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum EErrorCloseType
{
    Confirm,
    ApplicationQuit
}

public class UIErrorPopup : UIPopupBase
{
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _subDescription;
    [SerializeField] private Button _okButton;
    [SerializeField] private Button _quitButton;

    private UnityAction _clickAction;

    public void Init(EErrorCloseType type, string description, string subDescription, UnityAction action)
    {
        _description.text = description;
        _subDescription.text = subDescription;
        _clickAction = action;
        
        _okButton.gameObject.SetActive(type == EErrorCloseType.Confirm);
        _quitButton.gameObject.SetActive(type == EErrorCloseType.ApplicationQuit);
    }

    public void Click()
    {
        if (_clickAction != null)
            _clickAction();
        ClickCloseBtn();
    }

    public void ApplicationQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}