using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class EquipmentSettingButton : MonoBehaviour
{
    EquipmentItemData _data;
    [SerializeField] EEquipmentType _equipmentType;
    [SerializeField] Image _icon;
    [SerializeField] Image _bg;
    [SerializeField] GameObject _redDot;

    [SerializeField] GameObject _legendayEffect;
    [SerializeField] GameObject _mythicEffect;

    public void Init(EquipmentItemData data)
    {
        _data = data;
        Refresh();
    }

    public void Refresh()
    {
        _redDot.SetActive(RedDotManager.EquipEquipmentAbleRedDot(Managers.Instance.GetUIManager().UIEquipmentSetting._currentFactionType,_equipmentType));
        if(_data == null)
        {
            _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.PictogramAtlas, ReturnNullEquipmentDataIconString(_equipmentType));
            _bg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ScrollviewItemAtlas, "Backgroundgradient_B");
            _legendayEffect.SetActive(false);
            _mythicEffect.SetActive(false);
            return;
        }
        // 장비 icon 셋팅하기
        _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.EquipmentAtlas,_data.data.Name);
        _bg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{_data.data.Grade}");

        var grade = _data.data.Grade;
        _legendayEffect.SetActive(grade == EGradeType.Legendary || grade == EGradeType.Legendary_Plus);
        _mythicEffect.SetActive(grade == EGradeType.Mythic);
    }

    private string ReturnNullEquipmentDataIconString(EEquipmentType type)
    {
        string str = string.Empty;
        switch (type)
        {
            case EEquipmentType.Weapon:
                str = "Pictogram2_sword";
                break;
            case EEquipmentType.SubWeapon:
                str = "Pictogram2_Shield";
                break;
            case EEquipmentType.Helmet:
                str = "Pictogram2_Item";
                break;
            case EEquipmentType.Armor:
                str = "Pictogram2_Top";
                break;
            case EEquipmentType.Glove:
                str = "Pictogram2_Gloves";
                break;
            case EEquipmentType.Shoes:
                str = "Pictogram2_Shoes";
                break;
        }

        return str;
    }

    public void Click()
    {
        if (_data == null)
        {
            // 장비 선택 UI 올리기
            Managers.Instance.GetUIManager().ShowUISubBase<UISubEquipmentChange>(Managers.Instance.GetUIManager().UIEquipmentSetting, "UISubEquipmentChange").InitData(_data,
                Managers.Instance.GetUIManager().UIEquipmentSetting._currentFactionType,_equipmentType);

            return;
        }

        if (_data.data.Grade < EGradeType.Mythic)
        {

            UISubEquipmentDetail uisub = Managers.Instance.GetUIManager().ShowUISubBase<UISubEquipmentDetail>(Managers.Instance.GetUIManager().UIEquipmentSetting,
                "UISubEquipmentDetail");
            uisub.SetDataOpenToStack(_data);
            
        }
        else
        {
            UISubMythicEquipmentDetail sub = Managers.Instance.GetUIManager().ShowUISubBase<UISubMythicEquipmentDetail>(Managers.Instance.GetUIManager().UIEquipmentSetting, 
                "UISubMythicEquipmentDetail");
            sub.SetDataOpenToStack(_data);
        }

    }
}

