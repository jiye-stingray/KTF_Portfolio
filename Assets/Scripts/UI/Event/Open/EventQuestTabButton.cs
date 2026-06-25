using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventQuestTabButton : UITabGroupButton
{
    [SerializeField] GameObject _redDot;
    public override void Set(bool isnotGray)
    {
        int day = _index + 1;

        _lock?.SetActive(day > Managers.Instance.UserInfo().OpenEventCurrentDay);
        _redDot.SetActive(!_lock.activeSelf && RedDotManager.OpenEventQuestRedDot(day));
        base.Set(isnotGray);
    }

    public override void OnClick()
    {
        if(_lock.activeSelf)
        {
            Managers.Instance.GetUIManager().ShowCommonToastMessage("아직 해금되지 않았습니다.");
            return;
        }
        base.OnClick();
    }
}
