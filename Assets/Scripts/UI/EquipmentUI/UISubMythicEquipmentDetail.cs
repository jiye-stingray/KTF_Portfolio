using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UISubMythicEquipmentDetail : UISubEquipmentDetail
{
    [SerializeField] EquipmentOptionStausText[] _optionStatusTxt;

    public override void Refresh()
    {
        // 외부에서 변경시 다시 받아오기
        _data = UserInfoData._dicEquipmentItemData[_data.id];

        base.Refresh();

        DrawRefreshCostBtn();



        for (int i = 0; i < _data._equipmentOption.Length; i++)
        {
            EquipmentOption option = _data._equipmentOption[i];
            _optionStatusTxt[i].SetDataGrade(_data.GetOptionStautsType(i),_data.GetOptionStatus(i) ,option._optionGradeType,i,option._isLock, LockOptionClick);
        }

    }

    private void DrawRefreshCostBtn()
    {
        // refresh 이후에 UpgradeBtn 재설정 으로 셋팅 다시
        _upgradeCostBtn.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonArea);

        ECurrency[] currencies = new ECurrency[]
        {
            (ECurrency)ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "RandomOptionCurrency_1").Value,
            (ECurrency)ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "RandomOptionCurrency_2").Value
        };

        // option 별로 값 설정
        int lockCount = _data.GetLockOptionCount();
        _upgradeCostBtn.Init(currencies, new int[]
        {
            ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, $"RandomOptionLockCurrency_{lockCount}_1").Value,
            ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, $"RandomOptionLockCurrency_{lockCount}_2").Value
        });
    }


    public void RefreshBtnClick()
    {
        if (_upgradeCostBtn.isGray)
        {
            UIManager.ShowCommonToastMessage("재화가 부족합니다.");
            return;
        }

        // 전투력 먼저 계산
        Managers.Instance.GetUIManager().UIEquipmentSetting._battlePower = UserInfoData.EquipmentFactionBattlePower(_data.data.Faction);

        // 변경 이전 장비 저장
        EquipmentItemData beforeEquipment = new EquipmentItemData()
        {
            id = _data.id,
            tableId = _data.tableId
        };
        for (int i = 0; i < MaxEquipmentOptionCount; i++)
        {
            if (_data._equipmentOption[i] == null)
                continue;

            beforeEquipment._equipmentOption[i] = new EquipmentOption()
            {
                _optionID        = _data._equipmentOption[i]._optionID,
                _optionGradeType = _data._equipmentOption[i]._optionGradeType,
                _isLock          = _data._equipmentOption[i]._isLock,
            };
        }
        beforeEquipment.SetStatus();

        // 전투력 미리 셋팅 
        Managers.Instance.GetUIManager().UIEquipmentSetting._battlePower = UserInfoData.EquipmentFactionBattlePower(_data.data.Faction);

        // 옵션 재설정 (서버 연결) 
        Managers.Instance.GetServerManager().OnPostChangeOption(_data.id, (equipment) =>
        {
            UIManager.ShowPopup<UIEquipmentChangeOptionPopup>("UIEquipmentChangeOptionPopup").SetDataOpenToStack(this, beforeEquipment, UserInfoData._dicEquipmentItemData[equipment.id]);
        });

    }

    public void LockOptionClick(int index)
    {
        // 옵션 잠금 서버 연결
        if (_data._equipmentOption[index]._isLock)
        { 
            Managers.Instance.GetServerManager().OnPostUnlockEquipmentOption(_data.id, index + 1, (equipment) =>
            {
                Refresh();
            });
        }
        else
        {
            if(_data.GetLockOptionCount() >= _data.GetValidOptionCount() - 1)
            {
                Managers.Instance.GetUIManager().ShowCommonToastMessage("모든 잠재 능력을 잠그고 재설정을 진행할 수 없습니다.");
                return;
            }

            Managers.Instance.GetServerManager().OnPostLockEquipmentOption(_data.id, index + 1, (equipment) =>
            {
                Refresh();
            });

        }

    }

    public void HelpBtnClick()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        UIManager.ShowUISubBase<UISubHelp>(UIManager.UIEquipmentSetting, "UISubHelpPopup").SetType(EHelpType.EquipmentUpgrade);
    }

}
