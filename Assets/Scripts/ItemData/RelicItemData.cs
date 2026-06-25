using System.Collections;
using System.Collections.Generic;
using static Define;

public class RelicItemData
{
    public int _baseId;
    public int _level;
    public int _equipHeroId;
    private Dictionary<ERelicPartsType, int> _equipParts;
    private RelicBase RelicBase => ClientLocalDB_Simple.GetData<RelicBase>(DBKey.RelicBase, _baseId);
    private RelicBaseUpgrade RelicBaseUpgrade => ClientLocalDB_Simple.GetData<RelicBaseUpgrade>(DBKey.RelicBaseUpgrade, $"{_baseId}_{_level}");

    public RelicItemData()
    {
        _equipParts = new Dictionary<ERelicPartsType, int>();
        for (int i = 0; i <= (int)ERelicPartsType.NORTH; i++)
        {
            _equipParts.Add((ERelicPartsType)i, 0);
        }
    }

    public void SetEquipParts(ERelicPartsType relicPartsType, int id)
    {
        _equipParts[relicPartsType] = id;
    }

    public bool IsEmptyParts(ERelicPartsType relicPartsType)
    {
        return _equipParts[relicPartsType] == 0;
    }
    
    public int GetPartsId(ERelicPartsType relicPartsType)
    {
        return _equipParts[relicPartsType];
    }

    public string GetMainStatusString()
    {
        return Status.ReturnStatusString(RelicBase.StatType);
    }
    
    public Status GetMainStatus()
    {
        Status status = new Status();
        status.Plus(RelicBase.StatType, RelicBaseUpgrade.StatValue);
        return status;
    }

    public bool IsMaxLevel()
    {
        return _level == RelicBase.MaxLevel;
    }
}
