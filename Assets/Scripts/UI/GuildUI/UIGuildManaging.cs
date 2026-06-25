using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIGuildManaging : UISubBase
{
    public enum ETab
    {
        Approval,
        Setting,
    }
    ETab eCurrentTab;

    [System.Serializable]
    public struct STab
    {
        public UIGuildApprovalManagePage approvalPage;
        public UIGuildSettingPage guildSettingPage;
    }
    public STab stab;
    
    [SerializeField] private UITabGroup group;

    /*
     * *
     */

    
    public override void OpenToStack()
    {
        base.OpenToStack();
        eCurrentTab = ETab.Approval;
        group.Set((int)eCurrentTab);

        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();
        stab.approvalPage.Refresh();
        stab.guildSettingPage.Refresh();
    }
    public void OnChangeTab()
    {
        eCurrentTab = (ETab)group._currentTapGroupBtn._index;
        switch (eCurrentTab)
        {
            case ETab.Approval:
                stab.guildSettingPage.Close();

                stab.approvalPage.Open();
                break;
            case ETab.Setting:
                stab.approvalPage.Close();

                stab.guildSettingPage.Open();
                break;
        }
    }

}
