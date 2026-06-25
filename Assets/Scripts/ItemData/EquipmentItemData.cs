using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class  EquipmentOption
{
    public int _optionID;
    public EOptionGradeType _optionGradeType;
    public bool _isLock;
}

public class EquipmentItemData : ItemData
{ 
    public long id;
    public int tableId;
    public bool isLock;     // 장비 잠금 (분해 막기)
    public EquipmentBasic data => ClientLocalDB_Simple.GetData<EquipmentBasic>(DBKey.EquipmentBasic, tableId);

    public bool isSet => Managers.Instance.UserInfo().GetEquipmentIDList(data.Faction).Any(e => e != null && e.id == id);

    public EquipmentOption[] _equipmentOption = new EquipmentOption[Define.MaxEquipmentOptionCount];

    public Status mainStatus = new Status();

    public Status allStauts = new Status();     // 장비에서 최종적으로 나오는 Status (옵션 포함)

    public void SetStatus()
    {
        Status stat = new Status();
        for (int i = 0; i < data.StatType.Length; i++)
        {
            stat.Plus(data.StatType[i], (data.Basicstat[i] / 100f));
        }


        mainStatus.Set(stat);

        Status optionStatus = new Status();
        // 옵션이 존재한다면 
        for (int i = 0; i < _equipmentOption.Length; i++)
        {
            EquipmentOption option = _equipmentOption[i];
            if (option == null || option._optionGradeType == EOptionGradeType.None)
                continue;
            EquipmentRandomOption data = ClientLocalDB_Simple.GetData<EquipmentRandomOption>(DBKey.EquipmentRandomOption, option._optionID);
            optionStatus.Plus(GetOptionStautsType(i),(data.GetOptionStatusValue(option._optionGradeType) / 100f));
        }
        allStauts.Set(mainStatus + optionStatus);

    }





    public EStatus GetOptionStautsType(int idx)
    {
        EquipmentOption option = _equipmentOption[idx];
        if (option._optionID <= 0) return EStatus.NULL;
        EquipmentRandomOption data =ClientLocalDB_Simple.GetData<EquipmentRandomOption>(DBKey.EquipmentRandomOption, option._optionID);
        return data.Status;
    }
    
    public Status GetOptionStatus(int idx)
    {
        Status status = new Status();
        foreach (EquipmentOption option in _equipmentOption)
        {
            if (option == null || option._optionID == 0)
                continue;

            EquipmentRandomOption data =
                ClientLocalDB_Simple.GetData<EquipmentRandomOption>(DBKey.EquipmentRandomOption,
                    option._optionID);

            status.Plus(data.Status, data.GetOptionStatusValue(option._optionGradeType) / 100f);
        }

        return status;
    }

    public int GetLockOptionCount()
    {
        int count = 0;
        for (int i = 0; i < _equipmentOption.Length; i++)
        {
            if (_equipmentOption[i] == null || _equipmentOption[i]._optionGradeType == EOptionGradeType.None) continue;
            if (_equipmentOption[i]._isLock) count++;
        }
        return count;
    }

    public int GetValidOptionCount()
    {
        int count = 0;
        for (int i = 0; i < _equipmentOption.Length; i++)
        {
            if (_equipmentOption[i] == null || _equipmentOption[i]._optionGradeType == EOptionGradeType.None) continue;
            count++;
        }
        return count;
    }


    public double battlePower => CalculateStatus.CalculateBattlePoint(allStauts, EGradeType.None, 0);

}
