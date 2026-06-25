using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISubBase : UIPopupBase
{
    public UIBase mainUI;      // 본인의 mainUI

    public void SetMainUI(UIBase mainUI)
    {
        this.mainUI = mainUI;
    }

    public override void OpenToStack()
    {
        mainUI._subUIStack.Push(this);
        Open();
    }

    public override void ClickCloseBtn()
    {
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuClose");
        
        mainUI._subUIStack.Pop();
        DestroyClose();
    }
}
