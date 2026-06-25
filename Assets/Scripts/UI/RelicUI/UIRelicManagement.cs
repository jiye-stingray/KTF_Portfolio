using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIRelicManagement : UIBase
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] TMP_Text _descTxt;
    [SerializeField] TMP_Text _levelTxt;
    [SerializeField] TMP_Text _statusNameTxt;
    [SerializeField] TMP_Text _statusValueTxt;
    [SerializeField] Image _relicBaseIcon;
    [SerializeField] UIRelicPartItem[] uIRelicPartItems;
    [SerializeField] RelicLevelUpCostButton _relicLevelUpCostButton;

    [SerializeField] private GameObject _characterSlotRedDot;
    [SerializeField] Image _characterThumbnail;

    private int _equipCharacterId;
    RelicBase _relicBase;
    public RelicItemData _relicItemData;
    RelicBaseUpgrade RelicBaseUpgrade => ClientLocalDB_Simple.GetData<RelicBaseUpgrade>(DBKey.RelicBaseUpgrade, $"{_relicBase.Id}_{_relicItemData._level}");
    public void Init(int relicId)
    {
        _relicBase = ClientLocalDB_Simple.GetData<RelicBase>(DBKey.RelicBase, relicId);
        RelicpartsCraft relicPartsCraft = ClientLocalDB_Simple.GetData<RelicpartsCraft>(DBKey.RelicpartsCraft, $"{_relicBase.Id}_1");
        _relicItemData = UserInfoData.GetRelicItemData(_relicBase.Id);
        UIManager.TopCurrencyUI.SetCurrency(transform, RelicBaseUpgrade.CurrencyId[0], relicPartsCraft.CostType[0]);
        Refresh();
    }

    public override void Refresh()
    {
        _equipCharacterId = _relicItemData._equipHeroId;
        
        SetRelicBase();
        SetRelicParts();
        SetRelicEquipCharacter();
        SetRelicCostButton();
        
        base.Refresh();
    }

    private void SetRelicCostButton()
    {
        _relicLevelUpCostButton.Init(RelicBaseUpgrade.CurrencyId, RelicBaseUpgrade.CurrencyCount);
    }

    private void SetRelicBase()
    {
        _relicBaseIcon.sprite = AtlasManager.GetSprite(EAtlasType.RelicAtlas, _relicBase.ResourceName);
        _nameTxt.text = _relicBase.RelicName;
        _levelTxt.text = $"Lv.{_relicItemData._level}";
        SkillBase passiveSkillBase = ClientLocalDB_Simple.GetData<SkillBase>(DBKey.PcPassiveSkillBase, _relicBase.PassiveSkill);
        _descTxt.text = GetPassiveSkillDescription(passiveSkillBase);

        Status status = _relicItemData.GetMainStatus();
        _statusNameTxt.text = _relicItemData.GetMainStatusString();
        _statusValueTxt.text = status.GetStatusText(_relicBase.StatType);
    }
    
    private string GetPassiveSkillDescription(SkillBase passiveSkillBase)
    {
        PassiveSkillDetailData detailData =
            ClientLocalDB_Simple.GetData<PassiveSkillDetailData>(DBKey.PcPassiveSkillDetail, passiveSkillBase.SkillDetail[0]);

        EStatus status = passiveSkillBase.SkillBaseStatus;
        string statusText = $"<color=#EA8202>{Status.ReturnStatusString(status)}</color>";
        string effectPowerText = $"<color=#EA8202>{CalculateStatus.ToPercent(detailData.EffectPower)}</color>";

        string returnString = "";
        switch (detailData.PassiveSkillType)
        {
            case EPassiveSkillType.DefaultDamage:
            case EPassiveSkillType.Damage:
            case EPassiveSkillType.Buff:
            case EPassiveSkillType.Heal:
                returnString = string.Format(passiveSkillBase.Description, statusText, effectPowerText);
                break;
            case EPassiveSkillType.CoolDown:
                returnString = string.Format(passiveSkillBase.Description, detailData.TriggerValue);
                break;
            case EPassiveSkillType.Reflection:
                returnString = string.Format(passiveSkillBase.Description, effectPowerText);
                break;
        }

        return returnString;
    }

    private void SetRelicParts()
    {
        for (int i = 0; i <= (int)ERelicPartsType.NORTH; i++)
        {
            UIRelicPartItem uIRelicPartItem = uIRelicPartItems[i];
            uIRelicPartItem.Init(_relicItemData._baseId);
        }
    }

    private void SetRelicEquipCharacter()
    {
        _characterSlotRedDot.SetActive(_equipCharacterId == 0);
        _characterThumbnail.gameObject.SetActive(_equipCharacterId != 0);
        if (_equipCharacterId != 0)
        {
            UnitData unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, _equipCharacterId);
            _characterThumbnail.sprite = AtlasManager.GetCharacterIcon(EUnitType.PlayerCharacter, unitData.Resource);
        }
    }

    public void RelicLevelUp()
    {
        BestHttp_GameManager.OnPostRelicLevelUp(_relicBase.Id);
    }
    
    public void OpenSelectCharacterPopup()
    {
        UISubRelicSelectCharacterPopup selectCharacterPopup = UIManager.ShowUISubBase<UISubRelicSelectCharacterPopup>(this, "UISubRelicSelectCharacterPopup");
        selectCharacterPopup.Init(_relicBase.Id);
        selectCharacterPopup.OpenToStack();
    }
}
