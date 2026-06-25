using Cysharp.Threading.Tasks;
using PolyNav;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using static Define;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapStageInfo : MonoBehaviour
{
    [SerializeField] private int _index;
    [SerializeField] private string _effectName;
    [SerializeField] private Transform startTrans;
    [SerializeField] private Transform enemyTrans;
    [SerializeField] private Transform[] _treasureBoxPosition;
    private bool _isTown = false;
    public int[] _connectMapId;
    public PolyNavMap _navMap;
    public Vector3 StartPosition => startTrans.position;
    public Transform StartTransform => startTrans;
    public Vector3 EnemyPosition => enemyTrans.position;
    
    public int ZoneIndex {   get { return _index; }
                            set { _index = value; }}
    public bool IsTown { get { return _isTown; } }
    public bool IsTownPortal => _townPortal != null;
    public string mapName;

    public Grid CellGrid { get { return _baseTileGrid; } }

    private Grid _baseTileGrid;
    private Dictionary<Vector3Int, int> dialogueDic = new Dictionary<Vector3Int, int>();
    private List<Vector3Int> entranceDic = new ();

    private GameObject _bg;

    private PortalBuilding _townPortal;
    public PortalBuilding TownPortal { get { return _townPortal; } }

    private BaseBuilding _warpPoint;
    public BaseBuilding WarpPoint => _warpPoint;

    private RewardBoxBuilding _rewardBox;
    public RewardBoxBuilding RewardBox => _rewardBox;
    
    public List<SpawnPointInfoUnit> _spawnPointInfoUnitList = new List<SpawnPointInfoUnit>();
    public Dictionary<int, FogObject> fogObjects = new Dictionary<int, FogObject>();
    public List<BaseBuilding> _buildings = new List<BaseBuilding>();
    
    private void Awake()
    {
        _bg = Utils.FindChild(this.gameObject, "BG============", false);
        _baseTileGrid = transform.GetComponent<Grid>();
    }
    
    public void SetTileData()
    {
        GameObject baseObject = Utils.FindChild(this.gameObject, "base", false);
        if(baseObject != null)
            DestroyImmediate(baseObject);

        Tilemap dtm = Utils.FindChild<Tilemap>(this.gameObject, "Dialogue", true);
        if (dtm != null)
        {
            dtm.gameObject.SetActive(false);

            for (int y = dtm.cellBounds.yMax; y >= dtm.cellBounds.yMin; y--)
            {
                for (int x = dtm.cellBounds.xMin; x <= dtm.cellBounds.xMax; x++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y);
                    DialogueBatchTile dTile = dtm.GetTile(cellPos) as DialogueBatchTile;
                    if (dTile == null) continue;

                    dialogueDic.Add(cellPos, dTile._dialogueId);
                }
            }
        }

        // Enterance 
        Tilemap entm = Utils.FindChild<Tilemap>(this.gameObject, "Enterance", true);

        if (entm != null)
        {
            entm.gameObject.SetActive(false);

            for (int y = entm.cellBounds.yMax; y >= entm.cellBounds.yMin; y--)
            {
                for (int x = entm.cellBounds.xMin; x <= entm.cellBounds.xMax; x++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    EntranceTile eTile = entm.GetTile(cellPos) as EntranceTile;
                    if (eTile == null) continue;

                    entranceDic.Add(cellPos);
                }
            }
        }
    }
    
    public async UniTask SpawnTileUnit(int id)
    {
        // tile Unit 
        TileUnit[] tileUnits = GetComponentsInChildren<TileUnit>(true);
        foreach (TileUnit unit in tileUnits)
        {
            unit.SetStatus(id);
            await unit.InitTile(this);
            unit.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// TileUnit Load
    /// </summary>
    public async UniTask SpawnFieldUnit()
    {
        // tile Unit 
        TileUnit[] tileUnits = GetComponentsInChildren<TileUnit>();
        foreach (TileUnit unit in tileUnits)
        {
            if (unit._tileType == ETileType.Circle)
            {
                SpawnGroupUnit groupUnit = ClientLocalDB_Simple.GetData<SpawnGroupUnit>(DBKey.SpawnGroupUnit, unit._id);
                if (groupUnit == null)
                {
                    // 없는 ID가 있을때 확인용
                    MyLogger.Log("_zoneIndex : "+_index);
                    return;
                }
                
                unit.SetStatus(unit._id);
            }
            
            await unit.InitTile(this);
            unit.gameObject.SetActive(false);
        }
        
        FogObject[] fogs = GetComponentsInChildren<FogObject>();
        foreach (FogObject fog in fogs)
        {
            BaseBuilding fogBuilding = Managers.Instance.GetMapManager().FindBuilding(fog._FogId);
            fog.ActiveFog(fogBuilding !=null && !fogBuilding.BuildingData.isOpen);
            fogObjects.Add(fog._FogId, fog);
        }
    }
    
    public async UniTask SpawnDungeonUnit(EContent contentType, EFactionType factionType, int dungeonLevel)
    {
        // tile Unit 
        TileUnit[] tileUnits = GetComponentsInChildren<TileUnit>();
        foreach (TileUnit unit in tileUnits)
        {
            if (unit._tileType == ETileType.Circle)
            {
                int id = unit._id;
                if (contentType == EContent.Tower) // 타워 던전에서는 DB에 세팅된 ID적용
                {
                    TowerDungeon towerDungeon =
                        ClientLocalDB_Simple.GetData<TowerDungeon>(DBKey.TowerDungeon, $"{factionType}_{dungeonLevel}");
                    id = towerDungeon.SpawnPointInfo;
                }
                else if (contentType == EContent.GuildBoss) // 길드 보스 던전에서는 DB에 세팅된 ID적용
                {
                    GuildDungeon guildDungeon =
                        ClientLocalDB_Simple.GetData<GuildDungeon>(DBKey.GuildDungeon, dungeonLevel);
                    id = guildDungeon.SpawnPointInfo;
                }
                else if (contentType == EContent.Fog)
                {
                    FogDungeon fogDungeon =
                        ClientLocalDB_Simple.GetData<FogDungeon>(DBKey.FogDungeon, dungeonLevel);
                    id = fogDungeon.SpawnPointInfo;
                }
                
                DungeonBase dungeonBase = null;
                SpawnGroupUnit groupUnit = ClientLocalDB_Simple.GetData<SpawnGroupUnit>(DBKey.SpawnGroupUnit, id);

                if (groupUnit == null)
                {
                    // 없는 ID가 있을때 확인용
                    MyLogger.Log("_zoneIndex : "+_index);
                    return;
                }
                
                if (contentType != EContent.Ranking && contentType != EContent.Constellation)
                    dungeonBase = GetDungeonBase(contentType, factionType, dungeonLevel);
                
                unit.SetStatus(id, dungeonBase);
            }
            
            await unit.InitTile(this);
            unit.gameObject.SetActive(false);
        }
        
        FogObject[] fogs = GetComponentsInChildren<FogObject>();
        foreach (FogObject fog in fogs)
        {
            BaseBuilding fogBuilding = Managers.Instance.GetMapManager().FindBuilding(fog._FogId);
            fog.gameObject.SetActive(fogBuilding !=null && !fogBuilding.BuildingData.isOpen);
            fogObjects.Add(fog._FogId, fog);
        }
    }


    //던전별 Override되는 Status를 따른다.
    private DungeonBase GetDungeonBase(EContent contentType, EFactionType factionType, int dungeonLevel)
    {
        switch (contentType)
        {
            case EContent.Equipment:
                return ClientLocalDB_Simple.GetData<Dungeon>(DBKey.Dungeon, $"{EDungeonType.Equipment}_{dungeonLevel}");
            case EContent.Gold:
                return ClientLocalDB_Simple.GetData<Dungeon>(DBKey.Dungeon, $"{EDungeonType.Gold}_{dungeonLevel}");
            case EContent.Fog:
                return ClientLocalDB_Simple.GetData<FogDungeon>(DBKey.FogDungeon, dungeonLevel);
            case EContent.Tower:
                return ClientLocalDB_Simple.GetData<TowerDungeon>(DBKey.TowerDungeon, $"{factionType}_{dungeonLevel}");
            case EContent.GuildBoss:
                return ClientLocalDB_Simple.GetData<GuildDungeon>(DBKey.GuildDungeon, dungeonLevel);
            case EContent.FieldDungeon:
                return ClientLocalDB_Simple.GetData<FieldDungeon>(DBKey.FieldDungeon, dungeonLevel);
        }

        return null;
    }
    
    private void ExtractLevelAndStatusDungeon(DungeonBase dungeon, EUnitType unitType, out int level, out int statusType)
    {
        if (unitType == EUnitType.NormalMonster)
        {
            level = dungeon.NormalMonsterLevel;
            statusType = dungeon.NormalMonsterStatusType;
        }
        else
        {
            level = dungeon.EliteMonsterLevel;
            statusType = dungeon.EliteMonsterStatusType;
        }
    }

    public int EnableDialogue(Vector3 worldPosition)
    {
        Vector3Int cellPosition = _baseTileGrid.WorldToCell(worldPosition);
        return dialogueDic.GetValueOrDefault(cellPosition, -1);
    }
    
    public int EnableEntrance(Vector3 position)
    {
        Vector3Int cellPosition = _baseTileGrid.WorldToCell(position);
        if (entranceDic.Contains(cellPosition))
            return ZoneIndex;
        
        return -1;
    }

    #region SpawnUnit Coroutine

    public void StartUnitState()
    {
        StartCoroutine(StartUnitStateWithDelay());
    }
    
    private IEnumerator StartUnitStateWithDelay()
    {
        foreach (var spawn in _spawnPointInfoUnitList)
        {
            spawn.BattleStart();
            yield return null;
        }
    }

    public void StopUnitState()
    {
        StartCoroutine(StopUnitStateWithDelay());
    }
    
    private IEnumerator StopUnitStateWithDelay()
    {
        foreach (var spawn in _spawnPointInfoUnitList)
        {
            spawn.BattleStop();
            yield return null; // 한 프레임 쉬기, 또는 yield return new WaitForSeconds(0.05f);
        }
    }
    
    public int GetUnitCount()
    {
        int count = 0;
        for (int i = 0; i < _spawnPointInfoUnitList.Count; i++)
        {
            SpawnPointInfoUnit spawnPointInfoUnit = _spawnPointInfoUnitList[i];
            count += spawnPointInfoUnit._unitList.Count;
        }

        return count;
    }

    public int GetLiveUnitCount()
    {
        int count = 0;
        for (int i = 0; i < _spawnPointInfoUnitList.Count; i++)
        {
            SpawnPointInfoUnit spawnPointInfoUnit = _spawnPointInfoUnitList[i];
            count += spawnPointInfoUnit.LiveUnitCount();
        }

        return count;
    }
    
    public EnemyUnit GetUnit(int id)
    {
        foreach (var spawnPointInfoUnit in _spawnPointInfoUnitList)
        {
            foreach (EnemyUnit enemyUnit in spawnPointInfoUnit._unitList)
            {
                if (enemyUnit._unitId == id)
                    return enemyUnit;
            }
        }

        return null;
    }

    #endregion

    public void ActiveBg(bool state)
    {
        _bg.SetActive(state);
    }

    #region CreateUnit

    public void CreateSpawnPoint(int id, Vector3 worldPos, int clusterSpreadFactor, DungeonBase dungeon = null)
    {
        SpawnPointInfoUnit spawnPointInfo = Managers.Instance.GetMapManager()._enableGrowth ? Managers.Instance.GetObjectUnitManager().SpawnCircleUnit<SpawnPointGrowthUnit>(worldPos, "SpawnPointInfoObject") : Managers.Instance.GetObjectUnitManager().SpawnCircleUnit<SpawnPointInfoUnit>(worldPos, "SpawnPointInfoObject");
        spawnPointInfo.Init(_index, id, clusterSpreadFactor, dungeon);
        _spawnPointInfoUnitList.Add(spawnPointInfo);
    }

    public async UniTask CreateBuilding(int id, Vector3 worldPos, bool isFlip)
    {
        BuildingInfo buildingData = ClientLocalDB_Simple.GetData<BuildingInfo>(DBKey.BuildingInfo, id);
        BaseBuilding baseBuilding;
        switch(buildingData.BuildingType)
        {
            case EBuildingType.FogPoint:
                baseBuilding = await Managers.Instance.GetObjectUnitManager().SpawnFogPoint(id, buildingData.Resource, worldPos);
                FogPointBuilding fogPointBuilding = (FogPointBuilding)baseBuilding;
                if (fogPointBuilding != null)
                {
                    fogPointBuilding.SetZoneIndex(ZoneIndex);
                    fogPointBuilding.SetFlip(isFlip);
                }
                break;
            case EBuildingType.Dungeon:
                baseBuilding = await Managers.Instance.GetObjectUnitManager().SpawnFogDungeonBuilding(id, buildingData.Resource, worldPos);
                FogDungeonBuilding fogDungeonBuilding = (FogDungeonBuilding)baseBuilding;
                if (fogDungeonBuilding != null)
                    fogDungeonBuilding.SetFlip(isFlip);
                
                break;
            case EBuildingType.FieldDungeon:
                baseBuilding = await Managers.Instance.GetObjectUnitManager().SpawnFieldDungeonBuilding(id, buildingData.Resource, worldPos);
                FieldDungeonBuilding fieldDungeonBuilding = (FieldDungeonBuilding)baseBuilding;
                if (fieldDungeonBuilding != null)
                    fieldDungeonBuilding.SetFlip(isFlip);
                
                break;
            case EBuildingType.WarpPoint:
                baseBuilding = await Managers.Instance.GetObjectUnitManager().SpawnBuildingUnit(id, worldPos, buildingData.Resource);
                _warpPoint = baseBuilding;
                if (_warpPoint != null)
                    _warpPoint.SetZoneIndex(ZoneIndex);

                break;
            case EBuildingType.FieldQuest:
                baseBuilding = await Managers.Instance.GetObjectUnitManager().SpawnFieldQuestObject(id, buildingData.Resource, worldPos);
                FieldQuestBuilding fieldQuestBuilding = (FieldQuestBuilding)baseBuilding;
                if (fieldQuestBuilding != null)
                    fieldQuestBuilding.SetFlip(isFlip);

                break;
            case EBuildingType.Storage:
                baseBuilding = await Managers.Instance.GetObjectUnitManager().SpawnStorageBuildingUnit(id, buildingData.Resource, worldPos);
                break;
            default:
                baseBuilding = await Managers.Instance.GetObjectUnitManager().SpawnBuildingUnit(id, worldPos, buildingData.Resource);
                break;
        }
        _buildings.Add(baseBuilding);
    }

    public async UniTask CreatePortal(Vector3 worldPos)
    {
        _townPortal = await Managers.Instance.GetObjectUnitManager().SpawnPortal((int)EPortalType.TownPortal, "TownPortal", worldPos);
        _townPortal.SetZoneIndex(ZoneIndex);
        _townPortal.gameObject.SetActive(false);
    }
    
    public async UniTask CreateRewardBox(Vector3 worldPos)
    {
        _rewardBox = await Managers.Instance.GetObjectUnitManager().SpawnRewardBox(ZoneIndex, "RewardBox", worldPos);
        _rewardBox.SetZoneIndex(ZoneIndex);
    }

    #endregion

    public Vector2 GetTreasureBoxPosition()
    {
        int ran = Random.Range(0, _treasureBoxPosition.Length);
        return _treasureBoxPosition[ran].position;
    }

    public string GetEffectName()
    {
        return _effectName;
    }
    
    #region Collider Refresh
    [ContextMenu("Refresh_Map")]
    public void RefreshMap()
    {
#if UNITY_EDITOR
        GameObject tileMap = _navMap.GetComponentInChildren<Tilemap>(true).gameObject;
        CompositeCollider2D composite = _navMap.GetComponent<CompositeCollider2D>();
        
        tileMap.SetActive(true);
        composite.GenerateGeometry();
        tileMap.SetActive(false);
        
        // 🟡 프리팹 오버라이드 반영을 위한 처리
        EditorUtility.SetDirty(composite);
        PrefabUtility.RecordPrefabInstancePropertyModifications(composite);
#endif
    }

    [ContextMenu("Refresh_Collider")]
    public void RefreshCollider()
    {
#if UNITY_EDITOR
        CompositeCollider2D composite = transform.Find("Collider").GetComponent<CompositeCollider2D>();
        GameObject tileMap = composite.GetComponentInChildren<Tilemap>(true).gameObject;

        tileMap.SetActive(true);
        composite.GenerateGeometry();
        tileMap.SetActive(false);

        // 🟡 프리팹 오버라이드 반영을 위한 처리
        EditorUtility.SetDirty(composite);
        PrefabUtility.RecordPrefabInstancePropertyModifications(composite);
#endif
    }
    #endregion
}
