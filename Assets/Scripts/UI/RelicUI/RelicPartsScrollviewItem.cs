using PolyAndCode.UI;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class RelicPartsScrollviewItem : ICell
{
    [SerializeField] Image _bg;
    [SerializeField] Image _icon;

    [SerializeField] private GameObject _selectImg;
    
    RelicPartsIndexItemData _data;
    public override void SetData(ItemData data, int index)
    {
        _data = data as RelicPartsIndexItemData;
        _index = index;

        DrawItemData();
    }
 
    private void DrawItemData()
    {
        if (_data == null)
            return;

        // icon 설정 
        RelicParts relicParts = ClientLocalDB_Simple.GetData<RelicParts>(DBKey.RelicParts, $"{_data._relicPartsItemData._relicBaseId}_{(int)_data._relicPartsItemData._partsType}");
        
        _bg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{_data._relicPartsItemData._grade.ToString()}");
        _icon.sprite = AtlasManager.GetSprite(EAtlasType.RelicAtlas, relicParts.ResourceName);
        
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
