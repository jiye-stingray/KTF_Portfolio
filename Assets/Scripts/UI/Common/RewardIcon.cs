using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class RewardIcon : MonoBehaviour
{
    [Header("Currency")] 
    [SerializeField] GameObject _currencyRoot;
    [SerializeField] Image _currencyBg;
    [SerializeField] Image _currencyIcon;
    [Header("Character")]
    [SerializeField] GameObject _characterRoot;
    [SerializeField] Image _characterBg;
    [SerializeField] Image _characterIcon;
    [SerializeField] private Image _characterFactionImg;
    [SerializeField] private TMP_Text _characterName;
    [SerializeField] private Image _factionIcon;
    [Header("Equipment")]
    [SerializeField] Image _gradeBg;
    [SerializeField] GameObject _legendayEffect;
    [SerializeField] GameObject _mythicEffect;

    [Header("Emblem")]
    [SerializeField] GameObject _emblemRoot;
    [SerializeField] Image _emblemBg;
    [SerializeField] Image _emblemIcon;

    private AtlasManager AtlasManager => Managers.Instance.GetAtlasManager();

    [Serializable]
    public struct SSynergy
    {
        public Image icon;
        public GameObject goIconObj;
    }
    public SSynergy[] synergy;


    // 특정 데이터를 받아와서 Icon 을 띄워주기

    public void Init(ERewardType rewardType, int id, string synergyCode = null)
    {
        _characterRoot.SetActive(rewardType == ERewardType.Character || rewardType == ERewardType.HeroPiece);
        _currencyRoot.SetActive(rewardType != ERewardType.Character && rewardType != ERewardType.HeroPiece);
        _gradeBg.gameObject.SetActive(rewardType == ERewardType.Equipment);
        if(_emblemRoot != null) _emblemRoot.SetActive(rewardType == ERewardType.Emblem);

        switch (rewardType)
        {
            case ERewardType.None:
                break;
            case ERewardType.QuestPoint:
                break;
            case ERewardType.PassPoint:
                break;
            case ERewardType.Currency:
                CurrencyData data = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, id);
                if (data == null) return;
                _currencyIcon.sprite = AtlasManager.GetSprite(EAtlasType.ItemIconAtlas, data.Icon);
                break;
            case ERewardType.CharacterGrade:
                break;
            case ERewardType.HeroPiece: 
            case ERewardType.Character:
                UnitData unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, id);
                if (unitData == null) return;
                _characterIcon.sprite = AtlasManager.GetSprite(EAtlasType.CharacterIconAtlas, $"Thum_Face_Cr_{id.ToString("000")}");
                if(_characterFactionImg != null)
                    _characterFactionImg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)unitData.Faction}");
                if(_characterBg != null)
                    _characterBg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{unitData.StartGrade}");
                if(_characterName != null)
                    _characterName.text = unitData.Name;
                if(_factionIcon != null)
                    _factionIcon.sprite = AtlasManager.GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)unitData.Faction}");
                break;
            case ERewardType.Equipment:
                EquipmentBasic equipmentdata = ClientLocalDB_Simple.GetData<EquipmentBasic>(DBKey.EquipmentBasic, id);
                if(equipmentdata == null) return;
                _currencyIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.EquipmentAtlas, equipmentdata.Name);
                _gradeBg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{equipmentdata.Grade}");
                _legendayEffect.SetActive(equipmentdata.Grade == EGradeType.Legendary || equipmentdata.Grade == EGradeType.Legendary_Plus);
                _mythicEffect.SetActive(equipmentdata.Grade == EGradeType.Mythic);
                break;
            case ERewardType.EquipmentBox:
                EquipmentBox equipmentBoxData = ClientLocalDB_Simple.GetData<EquipmentBox>(DBKey.EquipmentBox, id);
                _currencyIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, equipmentBoxData.Icon);
                break;
            case ERewardType.RelicParts:
                RelicBase relicBase = ClientLocalDB_Simple.GetData<RelicBase>(DBKey.RelicBase, id);
                if (relicBase == null) return;
                _currencyIcon.sprite = AtlasManager.GetSprite(EAtlasType.RelicAtlas, relicBase.ResourceName);
                break;
            case ERewardType.ItemBox:
                Item itemData = ClientLocalDB_Simple.GetData<Item>(DBKey.Item, id);
                if (itemData == null) return;
                _currencyIcon.sprite = AtlasManager.GetSprite(EAtlasType.ShopAtlas, itemData.Icon);
                break;
            default:
                break;
        }

    }
    public void InitEmblem(string synergyCode)
    {
        // for (int i = 0; i < synergy.Length; i++)
        // {
        //     synergy[i].goIconObj.SetActive(false);
        // }
        // string[] parseSynergyCode = synergyCode.Split(",");
        //
        // EGradeType grade = (EGradeType)parseSynergyCode.Length;
        // EmblemBasic data = ClientLocalDB_Simple.GetData<EmblemBasic>(DBKey.EmblemBasic, grade.ToString());
        //
        // for (int i = 0; i < parseSynergyCode.Length; i++)
        // {
        //     synergy[i].goIconObj.SetActive(true);
        //     synergy[i].icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.DeckSynergyAtlas, parseSynergyCode[i]);
        // }
        // _emblemIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.EmblemAtlas, data.Icon);
        // _emblemBg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.EmblemAtlas, "Itemslot_Bg1");
    }
}
