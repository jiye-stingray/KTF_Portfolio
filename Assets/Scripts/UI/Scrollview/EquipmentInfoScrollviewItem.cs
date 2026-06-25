using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class EquipmentInfoScrollviewItem : ICell
{
    [SerializeField] protected Image _factionIcon;
    [SerializeField] protected Image _gradeBg;
    [SerializeField] protected Image _icon;
    [SerializeField] protected GameObject _legendayEffect;
    [SerializeField] protected GameObject _mythicEffect;

    protected EquipmentItemData _data;

    public void Init(EquipmentItemData data, int index)
    {
        _data = data;
        _index = index;
        Refresh();

    }

    protected virtual void Refresh()
    {
        _factionIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)_data.data.Faction}");
        _gradeBg.sprite =
            Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{ _data.data.Grade}");
        _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.EquipmentAtlas, _data.data.Name);

        var grade = _data.data.Grade;
        _legendayEffect.SetActive(grade == EGradeType.Legendary || grade == EGradeType.Legendary_Plus);
        _mythicEffect.SetActive(grade == EGradeType.Mythic);

    }
}
