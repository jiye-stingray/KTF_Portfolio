using UnityEngine;

public class SpawnPointGrowthUnit : SpawnPointInfoUnit
{
    private int _growthLevel;
    private int _growthStep;

    public override void Init(int zoneIndex, int id, int clusterSpreadFactor, DungeonBase dungeonBase = null)
    {
        base.Init(zoneIndex, id, clusterSpreadFactor, dungeonBase);
        _growthLevel = ClientLocalDB_Simple.GetData<DungeonSetting>(DBKey.DungeonSetting, "ConstellationDungeonLevelUp")
            .Value;
        ;
        _growthStep = 0;
    }

    protected override void UnitInitAndSetPosition()
    {
        foreach (EnemyUnit unit in _unitList)
        {
            Vector2 pos = GetRandomPosition();
            unit.transform.localPosition = pos;
            unit.SetLevel(_spawnGroupUnit.Level + (_growthLevel * _growthStep), _spawnGroupUnit.StatusType);
            unit.Init();
            unit.SetZoneIndex(_zoneIndex);
            unit.SetPosition(unit.transform.position);
            unit.SetSpawnPointInfoAndPos(this, unit.transform.position);
        }

        _growthStep++;
    }
}