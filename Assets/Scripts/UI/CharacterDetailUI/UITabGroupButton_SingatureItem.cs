using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITabGroupButton_SingatureItem : UITabGroupButton
{
    public override void Set(bool isnotGray)
    {
        _lock.SetActive(Managers.Instance.GetUIManager().UICharacterDetail.Grade <= Define.EGradeType.Legendary);
        base.Set(isnotGray);
    }

    public override void OnClick()
    {
        if (_lock.activeSelf)
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("영웅 전설+ 등급 달성 후 궁극 스킬 해제", "ToastMessage");
            return;
        }
        base.OnClick();
    }
}
