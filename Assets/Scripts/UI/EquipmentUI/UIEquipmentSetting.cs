using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIEquipmentSetting : UIBase
{

    #region Tap
    public enum ETAP_TYPE
    {
        Human,
        Celestial,
        Guardian,
        Crusher
    }
    public ETAP_TYPE _currentTap = ETAP_TYPE.Human;

    public UITabGroup _group;

    #endregion

    public EFactionType _currentFactionType = EFactionType.Human;
    [SerializeField] TMP_Text _altarLevelTxt;

    [SerializeField] EquipmentSettingButton[] _settingBtns;

    [SerializeField] AltarLevelCostButton _altarLevelCostBtn;

    [SerializeField] Image _factionBg;
    [SerializeField] Image _factionIcon;


    [Header("RedDot")]
    [SerializeField] GameObject _AutoSettingRedDot;
    [SerializeField] GameObject _allDecompositionRedDot;
    [SerializeField] GameObject _humanTabRedDot;
    [SerializeField] GameObject _celestialTabRedDot;
    [SerializeField] GameObject _guardianTabRedDot;
    [SerializeField] GameObject _crusherTabRedDot;

    public double _battlePower;

    public override void Open()
    {
        base.Open();
        _group._currentTapGroupBtn = _group._tapGroupBtns[0];
        OnChangeTap();
        Refresh();
    }


    public override void Refresh()
    {
        _group.Set((int)_currentTap);
        DrawEquipmentData();

    }

    public void DrawEquipmentData()
    {
        _factionBg.sprite = 
            Managers.Instance.GetResObjectManager().Load<Sprite>($"Texture/FactionBg/{_currentFactionType}");
        _factionIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)_currentFactionType}");

        // UI 용 값 세팅
        _battlePower = UserInfoData.EquipmentFactionBattlePower(_currentFactionType);


        _altarLevelTxt.text = $"Lv.{UserInfoData._dicAltarLevel[_currentFactionType]}";
        // battlepower 표시 (나중에)

        for (int i = 0; i < _settingBtns.Length; i++)
        {
            _settingBtns[i].Init(UserInfoData._dicEquipment[_currentFactionType][i]);
        }

        // Currency 계산
        ECurrency slotCostID1 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "Essence").Value);
        ECurrency slotCostID2 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "FactionCostID_Cellestial").Value);      // 초기 셋팅

        switch (_currentFactionType)
        {
            case EFactionType.Celestial:
                slotCostID2 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "FactionCostID_Cellestial").Value);
                break;
            case EFactionType.Crusher:
                slotCostID2 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "FactionCostID_Crusher").Value);
                break;
            case EFactionType.Guardian:
                slotCostID2 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "FactionCostID_Guardian").Value);
                break;
            case EFactionType.Human:
                slotCostID2 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "FactionCostID_Human").Value);
                break;
            default:
                break;
        }
        ECurrency[] currencies = new ECurrency[]
        {
            slotCostID1,
            slotCostID2
        };
        EquipmentEnchant equipmentEnchant = ClientLocalDB_Simple.GetData<EquipmentEnchant>(DBKey.EquipmentEnchant, UserInfoData._dicAltarLevel[_currentFactionType]);
        int[] values = new int[]
        {
            equipmentEnchant.EssenceCostValue,
            equipmentEnchant.FactionCostValue
        };

        _altarLevelCostBtn.Init(_currentFactionType, currencies, values);


        UIManager.TopCurrencyUI.SetCurrency(this.transform,slotCostID1, ECurrency.BlueCrystal, slotCostID2);

        // Red Dot
        _AutoSettingRedDot.SetActive(RedDotManager.AutoSettingEquipmentRedDot(_currentFactionType));
        _allDecompositionRedDot.SetActive(RedDotManager.AllDecompositionRedDot(_currentFactionType));

        _humanTabRedDot.SetActive(RedDotManager.AutoSettingEquipmentRedDot(EFactionType.Human) || RedDotManager.AllDecompositionRedDot(EFactionType.Human));
        _celestialTabRedDot.SetActive(RedDotManager.AutoSettingEquipmentRedDot(EFactionType.Celestial) || RedDotManager.AllDecompositionRedDot(EFactionType.Celestial));
        _guardianTabRedDot.SetActive(RedDotManager.AutoSettingEquipmentRedDot(EFactionType.Guardian) || RedDotManager.AllDecompositionRedDot(EFactionType.Guardian));
        _crusherTabRedDot.SetActive(RedDotManager.AutoSettingEquipmentRedDot(EFactionType.Crusher) || RedDotManager.AllDecompositionRedDot(EFactionType.Crusher));
    }

    public void OnChangeTap()
    {
        _currentTap = (ETAP_TYPE)_group._currentTapGroupBtn._index;
        switch (_currentTap)
        {
            case ETAP_TYPE.Human:
                _currentFactionType = EFactionType.Human;
                break;
            case ETAP_TYPE.Celestial:
                _currentFactionType= EFactionType.Celestial;
                break;
            case ETAP_TYPE.Guardian:
                _currentFactionType = EFactionType.Guardian;
                break;
            case ETAP_TYPE.Crusher:
                _currentFactionType = EFactionType.Crusher;
                break;
        }
        DrawEquipmentData();
    }

    public void AllDecompositionBtnClick()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        // 분해 팝업 
        UIManager.ShowUISubBase<UISubEquipmentDecompositionFilter>(Managers.Instance.GetUIManager().UIEquipmentSetting, "UISubEquipmentDecompositionFilter")
            .SetFaction(_currentFactionType);
    }

    public void AltarLevelUpSuccessAction()
    {
        BestHttp_GameManager.OnPostEquipmentLevelUp((int)_currentFactionType);
    }

    public void AutoSettingBtnClick()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        // 장착 가능 장비 있는지 체크
        bool isEquip = false;
        for (int i = 0; i < UserInfoData._dicEquipment[_currentFactionType].Length; i++)
        {
            var item = UserInfoData._dicEquipment[_currentFactionType][i];
            EquipmentItemData data = null;
            if (item == null)
            {
                data = UserInfoData._dicEquipmentItemData
                        .Where(e =>
                                e.Value.data.Faction == _currentFactionType &&
                                e.Value.data.Type == (EEquipmentType)i)
                                .OrderByDescending(e => e.Value.data.Grade)      // 내림차순 정렬하면 첫 값이 가장 가까움
                        .FirstOrDefault().Value;
            }
            else
            {

                data = UserInfoData._dicEquipmentItemData
                            .Where(e => e.Value.data.Grade > item.data.Grade &&
                                    e.Value.data.Faction == (EFactionType)_currentFactionType &&
                                    e.Value.data.Type == item.data.Type)
                                    .OrderByDescending(e => e.Value.data.Grade)      // 내림차순 정렬하면 첫 값이 가장 가까움
                            .FirstOrDefault().Value;

            }


            if (data != null)
            {
                isEquip = true;
                break;
            }
        }

        if(!isEquip)
        {
            var toast = UIManager.ShowUIToast<UIToastBase>("장착할 장비가 없습니다", "ToastMessage");
            return;

        }

        BestHttp_GameManager.PostAutoEquipment((int)_currentFactionType);
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

        Status status = new Status();

        switch (_currentFactionType)
        {

            case EFactionType.Celestial:
                status = UserInfoData._CelestialEquipmentStatus;
                break;
            case EFactionType.Crusher:
                status = UserInfoData._CrusherEquipmentStatus;
                break;
            case EFactionType.Guardian:
                status = UserInfoData._GuardianEquipmentStatus;
                break;
            case EFactionType.Human:
                status = UserInfoData._HumanEquipmentStatus;
                break;
            default:
                break;
        }


        subUI.SetData(statusType,status);
        subUI.OpenToStack();
    }

   public void GoInventoryBtnclick()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        Managers.Instance.GetUIManager().UIInventory.OpenToStack();
        Managers.Instance.GetUIManager().UIInventory.TabRefresh(1);

    }

    public void ShowHelpPopup()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        UIManager.ShowUISubBase<UISubHelp>(UIManager.UIEquipmentSetting, "UISubHelpPopup").SetType(EHelpType.Equipment);
    }
}
