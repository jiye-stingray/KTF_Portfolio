using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentInfoChangeScrollviewItem : EquipmentInfoScrollviewItem
{

    [SerializeField] GameObject _eIcon;
    [SerializeField] GameObject _selectedGo;

    UISubEquipmentChange uISubEquipmentChange;
    public void Init(EquipmentItemData data,int index, UISubEquipmentChange uiSubChange)
    {
        _data = data;
        _index = index;

        uISubEquipmentChange = uiSubChange;

        Refresh();
    }

    protected override void Refresh()
    {
        base.Refresh();
        _eIcon.SetActive(_data.isSet);
        _selectedGo.SetActive(_data == uISubEquipmentChange.selecteEquipItemData);

    }

    public void Click()
    {
        uISubEquipmentChange.selecteEquipItemData = _data;
        uISubEquipmentChange.Refresh();
    }
}
