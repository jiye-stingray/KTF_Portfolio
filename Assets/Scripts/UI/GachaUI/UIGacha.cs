using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class UIGacha : UIBase
{
    #region Tap

    [Serializable]
    public struct EPages
    {
        public PickupGachaTap PickupTap;
        public GeneralGachaTap GeneralGachaTap;
        public CelestialGachaTap CelestialGachaTap;
    }
    public EPages _pages;
    
    private EGachaType _gachaType;

    public enum ETAP_TYPE
    {
        PickUp = 0,
        General = 1,
        Celestial = 2,
        
        None = 99
    }
    
    ETAP_TYPE _currentTap = ETAP_TYPE.None;
    #endregion
    
    [SerializeField] GameObject _leftArrow;
    [SerializeField] GameObject _rightArrow;
    
    [SerializeField] GameObject[] _dotList;
    
    private UIGachaResultPopup _gachaResultPopup;
    private ScheduleDto _schedule;
    public void Init(EGachaType gachaType, ScheduleDto schedule)
    {
        _gachaType = gachaType;
        _schedule = schedule;
        _currentTap = Utils.ParseEnum<ETAP_TYPE>(_gachaType.ToString());
        OpenToStack();
    }

    public override void Open()
    {
        base.Open();
        OnChangeTap();
    }

    public override void Refresh()
    {
        switch (_currentTap)
        {
            case ETAP_TYPE.PickUp:
                _pages.PickupTap.Refresh();
                break;
            case ETAP_TYPE.General:
                _pages.GeneralGachaTap.Refresh();
                break;
            case ETAP_TYPE.Celestial:
                _pages.CelestialGachaTap.Refresh();
                break;
        }
    }

    public void OnChangeTap()
    {
        _gachaType = Utils.ParseEnum<EGachaType>(_currentTap.ToString());
        RefreshArrow();
        switch (_currentTap)
        {
            case ETAP_TYPE.PickUp:
                UIManager.TopCurrencyUI.SetCurrency(this.transform, ECurrency.GachaTicket_PickUp, ECurrency.Cash_Free);
                _pages.PickupTap.Init(_schedule);
                _pages.GeneralGachaTap.Close();
                _pages.CelestialGachaTap.Close();
                _leftArrow.SetActive(false);
                _rightArrow.SetActive(true);
                break;
            case ETAP_TYPE.General:
                UIManager.TopCurrencyUI.SetCurrency(this.transform, ECurrency.GachaTicket_Normal, ECurrency.Cash_Free);
                _pages.PickupTap.Close();
                _pages.GeneralGachaTap.Open();
                _pages.CelestialGachaTap.Close();
                _leftArrow.SetActive(true);
                _rightArrow.SetActive(true);
                break;
            case ETAP_TYPE.Celestial:
                UIManager.TopCurrencyUI.SetCurrency(this.transform, ECurrency.GachaTicket_Celestial, ECurrency.Cash_Free);
                _pages.PickupTap.Close();
                _pages.GeneralGachaTap.Close();
                _pages.CelestialGachaTap.Open();
                _leftArrow.SetActive(true);
                _rightArrow.SetActive(false);
                break;
        }
    }

    private void RefreshArrow()
    {
        _leftArrow.SetActive(_currentTap == ETAP_TYPE.PickUp);
        _rightArrow.SetActive(_currentTap == ETAP_TYPE.Celestial);

        for (int i = 0; i < _dotList.Length; i++)
        {
            GameObject dot = _dotList[i];
            dot.SetActive((int)_currentTap != i);
        }
    }

    public void OnRightArrowClick()
    {
        _currentTap += 1;
        OnChangeTap();
    }
    
    public void OnLeftArrowClick()
    {
        _currentTap -= 1;
        OnChangeTap();
    }

    public void ShowWishListPopup()
    {
        UIWishListPopup popup = UIManager.ShowUISubBase<UIWishListPopup>(this, "UIWishListPopup");
        popup.Init(_gachaType);
        popup.OpenToStack();
    }
    
    public void ShowProbabilityInfoPopup()
    {
        UIGachaProbabilityInfo popup = UIManager.ShowUISubBase<UIGachaProbabilityInfo>(this, "UIGachaProbabilityInfo");
        popup.SetData(_gachaType);
        popup.OpenToStack();
    }

    public void OpenGachaResultPopup(EGachaType gachaType, EGachaCountType gachaCountType, GachaRewardListDto[] gachaRewardList)
    {
        if (_gachaResultPopup == null)
        {
            _gachaResultPopup = UIManager.ShowUISubBase<UIGachaResultPopup>(this, "UIGachaResultPopup");
            _gachaResultPopup.OpenToStack();
        }
        _gachaResultPopup.Init(gachaType, gachaCountType, gachaRewardList);
    }
    
    public void ShowUIGachaCharacterDetail()
    {
        if (_gachaType == EGachaType.General)
            return;

        UnitData unitData = null;
        if (_gachaType == EGachaType.PickUp)
        {
            unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter,
                ClientLocalDB_Simple.GetData<GachaSetting>(DBKey.GachaSetting, "PickUpCharacter").Value);
        }
        else
        {
            unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter,
                UserInfoData._dicGachaItemData[EGachaType.Celestial]._wishList[0]);
        }
        
        EGradeType grade = unitData.StartGrade;
        int level = ClientLocalDB_Simple.GetDB<CharacterLevel>(DBKey.CharacterLevel).Last().Value.Level;
        
        CharacterClassItemData itemData = new CharacterClassItemData();
        itemData.id = unitData.ID;
        itemData.Level = level;
        itemData._grade = grade;
        itemData.activeSkillLevel = ReturnMaxSkillLevel();
        itemData.InitStatus(null);
        itemData.RefreshStatus();
        
        UIManager.UICharacterDetail.SetDataOpenToStack(itemData, true);
    }
}
