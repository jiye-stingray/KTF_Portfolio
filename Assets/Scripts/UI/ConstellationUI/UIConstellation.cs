using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class UIConstellation : UIBase
{
    #region Tab

    [Serializable]
    public struct EPages
    {
        public ConstellationTab DragonTab;
        public ConstellationTab TigerTab;
        public ConstellationTab PhoenixTab;
        public ConstellationTab TurtleTab;
    }
    public EPages _pages;
    public enum ETAB_TYPE
    {
        Dragon,
        Tiger,
        Phoenix,
        Turtle
    }
    public ETAB_TYPE _currentTab = ETAB_TYPE.Dragon;

    public UITabGroup _group;

    #endregion


    [SerializeField] ConstellationDetail _constellationDetail;

    [Header("RedDot")]
    [SerializeField] GameObject _DragonRedDot;
    [SerializeField] GameObject _tigerRedDot;
    [SerializeField] GameObject _phoenixRedDot;
    [SerializeField] GameObject _turtleRedDot;
    
    public ConstellationItemData _currentItemData = null;


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _pages.DragonTab.SetBoardType(EConstellationBoardType.ComstellationBoard_Dragon);
        _pages.TigerTab.SetBoardType(EConstellationBoardType.ComstellationBoard_Tiger);
        _pages.PhoenixTab.SetBoardType(EConstellationBoardType.ComstellationBoard_Phoenix);
        _pages.TurtleTab.SetBoardType(EConstellationBoardType.ComstellationBoard_Turtle);

        return true;
    }


    public override void Open()
    {
        base.Open();
        _group._currentTapGroupBtn = _group._tapGroupBtns[0];
        UIManager.TopCurrencyUI.SetCurrency(this.transform, ECurrency.StarPiece);
        OnChangeTab();
        Refresh();
    }

    public override void Refresh()
    {
        _group.Set((int)_currentTab);
        DrawConstellationDetail();
    }

    private void DrawConstellationDetail()
    {
        _constellationDetail.Init(_currentItemData);

        //RedDot
        _DragonRedDot.SetActive(RedDotManager.TypeConstellationRedDot(EConstellationBoardType.ComstellationBoard_Dragon));
        _tigerRedDot.SetActive(RedDotManager.TypeConstellationRedDot(EConstellationBoardType.ComstellationBoard_Tiger));
        _phoenixRedDot.SetActive(RedDotManager.TypeConstellationRedDot(EConstellationBoardType.ComstellationBoard_Phoenix));
        _turtleRedDot.SetActive(RedDotManager.TypeConstellationRedDot(EConstellationBoardType.ComstellationBoard_Turtle));
    }

    public void OnChangeTab()
    {
        _currentTab = (ETAB_TYPE)_group._currentTapGroupBtn._index;
        switch (_currentTab)
        {
            case ETAB_TYPE.Dragon:
                _pages.DragonTab.Open();
                _pages.TigerTab.Close();
                _pages.PhoenixTab.Close();
                _pages.TurtleTab.Close();
                break;
            case ETAB_TYPE.Tiger:
                _pages.DragonTab.Close();
                _pages.TigerTab.Open();
                _pages.PhoenixTab.Close();
                _pages.TurtleTab.Close();
                break;
            case ETAB_TYPE.Phoenix:
                _pages.DragonTab.Close();
                _pages.TigerTab.Close();
                _pages.PhoenixTab.Open();
                _pages.TurtleTab.Close();
                break;
            case ETAB_TYPE.Turtle:
                _pages.DragonTab.Close();
                _pages.TigerTab.Close();
                _pages.PhoenixTab.Close();
                _pages.TurtleTab.Open();
                break;
            default:
                break;
        }
        DrawConstellationDetail();
    }

    public void ShowStatusInfoBtnClick()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        UISubAllStatusInfo subUI = UIManager.ShowUISubBase<UISubAllStatusInfo>(this, "UISubAllStatusInfo");

        EStatus[] statusType = new EStatus[]
        {
            EStatus.Attack,
            EStatus.AttackPercent,

            EStatus.CriticalChance,     // 치명타 확률
            EStatus.CriticalMultiplier, // 치명타 데미지

            EStatus.MaxHealthPoint,
            EStatus.MaxHealthPointPercent,

            EStatus.Def,        // 물리 방어력
            EStatus.DefPercent,

            EStatus.ReduceDmg   // 피해감소 (데미지 감소?)

        };  

        Status status = UserInfoData._constellationStatus;

        subUI.SetData(statusType, status);
        subUI.OpenToStack();
    }

    /// <summary>
    /// 버튼에 연결해둔 event
    /// </summary>
    public void ReactivationSuccessAction()
    {
        UserInfoData.ConstellationReactivation(_currentItemData._id);
    }

    public void ShowHelpPopup()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        UIManager.ShowUISubBase<UISubHelp>(UIManager.UIConstellation, "UISubHelpPopup").SetType(EHelpType.Constellation);
    }

    public override void Close()
    {
        _currentItemData = null;
        base.Close();
    }

}
