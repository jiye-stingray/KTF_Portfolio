using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIRelic : UIBase
{
    #region Tab

    [Serializable]
    public struct EPages
    {
        public RelicManagermentTab RelicManagermentTab;
        public RelicCraftTab RelicCraftTab;
    }

    public EPages _pages;

    public enum ETAP_TYPE
    {
        RelicManagementPage,
        RelicCraftPage,
    }
    ETAP_TYPE _currentTab = ETAP_TYPE.RelicManagementPage;

    public UITabGroup _group;

    #endregion

    [SerializeField] TMP_Text _nameTxt;
    public override void Refresh()
    {
        base.Refresh();

        _group.Set((int)_currentTab);
    }

    public override void Open()
    {
        base.Open();
        _currentTab = ETAP_TYPE.RelicManagementPage;
        Refresh();
    }

    public void OnChangeTab()
    {
        _currentTab = (ETAP_TYPE)_group._currentTapGroupBtn._index;
        switch (_currentTab)
        {
            case ETAP_TYPE.RelicManagementPage:
                _nameTxt.text = "유물 관리";
                UIManager.MainInfoUI.BindCurrency();
                _pages.RelicManagermentTab.Open();
                _pages.RelicCraftTab.Close();
                break;
            case ETAP_TYPE.RelicCraftPage:
                _nameTxt.text = "유물 제작";
                _pages.RelicCraftTab.Open();
                _pages.RelicManagermentTab.Close();
                break;
            default:
                break;
        }
    }

    public void ShowHelpPopup()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        UIManager.ShowUISubBase<UISubHelp>(UIManager.UIRelic, "UISubHelpPopup").SetType(EHelpType.Relic);
    }
}
