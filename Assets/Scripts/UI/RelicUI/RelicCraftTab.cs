using Spine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class RelicCraftTab : UITabBase
{
    [SerializeField] Image _fieldImg;
    [SerializeField] TMP_Text _fieldNameTxt;
    [SerializeField] TMP_Text _fieldLevelTxt;

    [SerializeField] SwitchDataButton _switchDataBtn_Left;
    [SerializeField] SwitchDataButton _switchDataBtn_Right;
    
    [SerializeField] UICostButton _relicPartsCraftCostBtn;   
    IndexWrapper _index = new IndexWrapper();

    private List<string> _relicIds;
    private int _relicIndex;
    private int _fieldLevel;
    private string RelicBaseId => _relicIds[_relicIndex];
    int MaxID => _relicIds.Count - 1;
    private RelicBase RelicBase => ClientLocalDB_Simple.GetData<RelicBase>(DBKey.RelicBase, _relicIds[_relicIndex]);
    FieldInfo FieldInfo => ClientLocalDB_Simple.GetData<FieldInfo>(DBKey.FieldInfo, UserInfoData._fieldId);
    RelicpartsCraft RelicPartsCraftData => ClientLocalDB_Simple.GetData<RelicpartsCraft>(DBKey.RelicpartsCraft, $"{RelicBaseId}_{_fieldLevel}");
    RelicBaseUpgrade RelicBaseUpgrade => ClientLocalDB_Simple.GetData<RelicBaseUpgrade>(DBKey.RelicBaseUpgrade, $"{RelicBaseId}_1");
    private void Awake()
    {
        _relicIds = ClientLocalDB_Simple.GetDB<RelicBase>(DBKey.RelicBase).Keys.ToList();
        _switchDataBtn_Left.Init(_index, 0, MaxID, ChangeField);           // 설산 맵 부터 시작 (id : 2)
        _switchDataBtn_Right.Init(_index, 0, MaxID, ChangeField);
    }

    public override void Open()
    {
        base.Open();
        ChangeField(_relicIds.IndexOf(FieldInfo.RelicBaseId.ToString()));
    }

    public override void Refresh()
    {
        _fieldImg.sprite = AtlasManager.GetSprite(EAtlasType.RelicAtlas, $"Image_Relic_{RelicBaseId}");
        _fieldLevelTxt.text = $"{_fieldLevel}단계";
        
        _switchDataBtn_Left.gameObject.SetActive(_relicIndex > 0);
        _switchDataBtn_Right.gameObject.SetActive(_relicIndex < MaxID);
        
        _relicPartsCraftCostBtn.Init(RelicPartsCraftData.CostType, RelicPartsCraftData.CostValue);
    }

    private void ChangeField(int relicIndex)
    {
        _relicIndex = relicIndex;
        _fieldLevel = UserInfoData._dicFieldItemData[RelicBase.OpenFieldId].difficultyLevel;
        UIManager.TopCurrencyUI.SetCurrency(UIManager.UIRelic.transform, RelicBaseUpgrade.CurrencyId[0], RelicPartsCraftData.CostType[0]);
        Refresh();
    }

    public void OpenUISubRelicSelectLevelPopup()
    {
        UISubRelicSelectLevelPopup selectLevelPopup = UIManager.ShowUISubBase<UISubRelicSelectLevelPopup>(UIManager.UIRelic, "UISubRelicSelectLevelPopup");
        selectLevelPopup.Init(FieldInfo.ID, _fieldLevel, ChangeLevel);
        selectLevelPopup.OpenToStack();
    }

    private void ChangeLevel(int level)
    {
        _fieldLevel = level;
        Refresh();
    }

    public void RelicPartsCraft()
    {
        if (_relicPartsCraftCostBtn.isGray)
            return;

        if (!UserInfoData.EnableRelicPartsCraft(int.Parse(RelicBaseId)))
        {
            UIManager.ShowCommonToastMessage("유물을 더 이상 보유할 수 없습니다! 분해를 먼저 진행해주세요.");
            return;
        }
        
        BestHttp_GameManager.OnPostCraftRelicParts(FieldInfo.ID, _fieldLevel);
    }
}
