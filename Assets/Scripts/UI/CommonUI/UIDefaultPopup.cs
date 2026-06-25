using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIDefaultPopup : UIPopupBase
{
    [SerializeField] TMP_Text _description;
    [SerializeField] TMP_Text _subDescription;

    private UnityAction _clickAction;
    private UnityAction _cancelAction;

    public void Init(string description, string subDescription, UnityAction action, UnityAction cancelAction = null)
    {
        _description.text = description;
        _subDescription.text = subDescription;
        _clickAction = action;
        _cancelAction = cancelAction;
    }

    public void Click()
    {
        if (_clickAction != null)
            _clickAction();
        ClickCloseBtn();
    }

    public void Cancel()
    {
        if (_cancelAction != null)
            _cancelAction();
        ClickCloseBtn();
    }
}
