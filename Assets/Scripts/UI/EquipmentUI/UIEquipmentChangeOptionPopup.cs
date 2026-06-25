using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIEquipmentChangeOptionPopup : UIPopupBase
{

    [SerializeField] protected TMP_Text _nameTxt;
    [SerializeField] protected Image _gradeBg;
    [SerializeField] protected Image _icon;
    [SerializeField] protected Image _factionIcon;

    [SerializeField] EquipmentOptionStausText[] _beforeStatusTxt;
    [SerializeField] EquipmentOptionStausText[] _afterStatusTxt;

    EquipmentItemData _beforeEquipmentItemData;
    EquipmentItemData _afterDummyEquipmentItemData;

    UISubMythicEquipmentDetail _ui;

    public void SetDataOpenToStack(UISubMythicEquipmentDetail ui,EquipmentItemData beforeEquipment, EquipmentItemData afterEquipment)
    {
        _beforeEquipmentItemData = beforeEquipment;
        _afterDummyEquipmentItemData = afterEquipment;
        _ui = ui;

        OpenToStack();
        Refresh();
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
    }
    

    public override void Refresh()
    {

        _nameTxt.text = $"[{Define.ReturnGradeString(_beforeEquipmentItemData.data.Grade)}] {_beforeEquipmentItemData.data.UIName}";
        _nameTxt.color = Utils.HexToColor(Define.GradeColorHex[_beforeEquipmentItemData.data.Grade]);
        _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.EquipmentAtlas, _beforeEquipmentItemData.data.Name);
        _factionIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)_beforeEquipmentItemData.data.Faction}");
        _gradeBg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{_beforeEquipmentItemData.data.Grade}");



        for (int i = 0; i < _beforeStatusTxt.Length; i++)
        {

            _beforeStatusTxt[i].SetDataGrade(_beforeEquipmentItemData.GetOptionStautsType(i),
                _beforeEquipmentItemData.GetOptionStatus(i),
                _beforeEquipmentItemData._equipmentOption[i]._optionGradeType,
                i, _beforeEquipmentItemData._equipmentOption[i]._isLock);
        }


        for (int i = 0; i < _afterStatusTxt.Length; i++)
        {
            _afterStatusTxt[i].SetDataGrade(_afterDummyEquipmentItemData.GetOptionStautsType(i),
                    _afterDummyEquipmentItemData.GetOptionStatus(i),
                    _afterDummyEquipmentItemData._equipmentOption[i]._optionGradeType,
                    i, _afterDummyEquipmentItemData._equipmentOption[i]._isLock);
        }
    }

    public void KeepOptionBtnClick()
    {
        ClickCloseBtn();

        Managers.Instance.GetServerManager().OnPostRollBackOption(_beforeEquipmentItemData.id, (equipment) =>
        {
            _ui.Refresh();
            ClickCloseBtn();
        });
    }

    public void ChangeBtnClick()
    {
        // 장착한 장비 적용
        if (Managers.Instance.UserInfo().IsEquipped(_beforeEquipmentItemData.data.Faction, _beforeEquipmentItemData.id))
        {
            Managers.Instance.UserInfo().RefreshEquipEquipmentItemData(_beforeEquipmentItemData.data.Faction);

            // 전투력 토스트 메시지 
            double newBattlePower = UserInfoData.EquipmentFactionBattlePower(_beforeEquipmentItemData.data.Faction) - UIManager.UIEquipmentSetting._battlePower;
            UIManager.ShowUIToast<UIToastBase>(newBattlePower.ToString("0"), "ChangeBattlePowerToastMessage");
        }

        _ui.Refresh();
        ClickCloseBtn();
    }
}
