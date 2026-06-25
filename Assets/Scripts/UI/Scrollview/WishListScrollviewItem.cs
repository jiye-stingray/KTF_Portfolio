using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static Define;

public class WishListScrollviewItem : ICell
{
    [SerializeField] private Image _icon;
    [SerializeField] private Image _characterBg;
    [SerializeField] private Image _selectBg;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private GameObject _characterRoot;
    [SerializeField] private GameObject _selectRoot;
    [SerializeField] Image _factionIcon;

    public WishListItemData _data;

    public bool _isShowCurrentGrade = false;

    public override void SetData(ItemData data, int index)
    {
        _data = data as WishListItemData;
        _index = index;
        Refresh();
    }

    private void Refresh()
    {
        _characterRoot.SetActive(_data.id != 0);
        
        if(_selectRoot != null)
            _selectRoot.SetActive(_data.isSelected);
        
        if (_data.id == 0)
            return;
        
        UnitData data = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, _data.id);
        _icon.sprite = AtlasManager.GetCharacterIcon(data.UnitType, data.Resource);
        EGradeType grade = _isShowCurrentGrade ? UserInfo.GetCharacterItemData(_data.id)._grade : data.StartGrade;
        _characterBg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{Define.ReturnNoPlusGradeType(data.StartGrade)}");
        _name.text = data.Name;
        _factionIcon.sprite = AtlasManager.GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)data.Faction}");
    }

    public void Click()
    {
        if(OnClick != null)
            OnClick(_data.id);
    }
}
