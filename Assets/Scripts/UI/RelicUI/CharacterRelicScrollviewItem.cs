using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class CharacterRelicScrollviewItem : ICell
{
    [SerializeField] Image _characterIcon;
    [SerializeField] Image _gradeBg;
    [SerializeField] Image _gradeFrame;
    [SerializeField] TMP_Text _levelTxt;
    [SerializeField] Image _factionIcon;
    [SerializeField] TMP_Text _nameTxt;

    [SerializeField] GameObject _selectImg;
    
    CharacterClassIndexItemData _data;

    public override void SetData(ItemData data, int index)
    {
        _data = data as CharacterClassIndexItemData;
        _index = index;

        DrawItemData();
    }
 
    private void DrawItemData()
    {
        if (_data == null)
            return;

        // icon 설정 
        UnitData playerUnit = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, _data.ID);
        _characterIcon.sprite = Managers.Instance.GetAtlasManager().GetCharacterIcon(playerUnit.UnitType, playerUnit.Resource);
        _gradeBg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{ReturnNoPlusGradeType(_data._characterClassItem._grade)}");
        _gradeFrame.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"Frame_grade_{_data._characterClassItem._grade}");
        _factionIcon.sprite = AtlasManager.GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)playerUnit.Faction}");

        _levelTxt.text = "Lv. " + _data._characterClassItem.Level;
        _nameTxt.text = _data._characterClassItem._unitData.Name;
        
        _selectImg.SetActive(_data._indexWrapper._index == _index); 
    }

    public void Click()
    {
        if (_data == null)
            return;
        
        _data._indexWrapper._index = _index;
        _data._clickAction?.Invoke();
    }
}
