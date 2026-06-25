using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;
using static Define;

public class RelicPartsOption
{
    public int _relicOptionId;
    public EOptionGradeType _relicOptionGrade;
}

public class RelicPartsItemData : ItemData
{
    public int _id; //serverId
    public int _relicBaseId; //baseId
    public int _relicPartsId; //partsId
    public ERelicPartsType _partsType;
    public EOptionGradeType _grade;
    public bool _isLock;
    public RelicPartsOption[] _relicPartsOptions = new RelicPartsOption[MaxRelicPartsOptionCount];
    public RelicParts RelicParts => ClientLocalDB_Simple.GetData<RelicParts>(DBKey.RelicParts, $"{_relicBaseId}_{(int)_partsType}");
    
    public Status GetMainPartsStatus()
    {
        Status status =  new Status();
        status.Plus(RelicParts.MainStatType, GetMainOptionValue() / 100f);
        
        return status;
    }

    private double GetMainOptionValue()
    {
        RelicPartsMainOption mainOption = ClientLocalDB_Simple.GetData<RelicPartsMainOption>(DBKey.RelicPartsMainOption, $"{_relicBaseId}_{(int)_partsType}");
        double value = 0;
        switch (_grade)
        {
            case EOptionGradeType.Common:
                value = mainOption.Common;
                break;
            case EOptionGradeType.Rare:
                value = mainOption.Rare;
                break;
            case EOptionGradeType.Epic:
                value = mainOption.Epic;
                break;
            case EOptionGradeType.Legendary:
                value = mainOption.Legendary;
                break;
        }

        return value;
    }

    public EOptionGradeType GetSubPartsGrade(int index)
    {
        return _relicPartsOptions[index]._relicOptionGrade;
    }
    
    public List<Status> GetSubPartsStatusList()
    {
        List<Status> subPartsStatusList = new List<Status>();

        foreach (RelicPartsOption relicPartsOption in _relicPartsOptions)
        {
            if(relicPartsOption == null || relicPartsOption._relicOptionId == 0)
                continue;
            
            Status status = new Status();
            RelicPartsSubOption relicPartsSubOption =
                ClientLocalDB_Simple.GetData<RelicPartsSubOption>(DBKey.RelicPartsSubOption,
                    relicPartsOption._relicOptionId);
            
            status.Plus(relicPartsSubOption.Status, relicPartsSubOption.GetStatusValue(relicPartsOption._relicOptionGrade) / 100f);
            subPartsStatusList.Add(status);
        }
        
        return subPartsStatusList;
    }
    
    public EStatus GetSubPartsStatusType(int index)
    {
        RelicPartsSubOption relicPartsSubOption =
            ClientLocalDB_Simple.GetData<RelicPartsSubOption>(DBKey.RelicPartsSubOption,
                _relicPartsOptions[index]._relicOptionId);
        
        return relicPartsSubOption.Status;
    }
}
