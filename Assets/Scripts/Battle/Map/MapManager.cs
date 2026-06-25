using Cysharp.Threading.Tasks;
using PolyNav;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class MapManager
{
    private MapStageMeta _mapStageMeta;
    public MapStageMeta MapStageMeta => _mapStageMeta;
    public IReadOnlyList<MapStageInfo> MapStageInfos => _mapStageMeta.MapStageInfos;

    public GameObject _arrowObject;

    FollowCamera FollowCamera => Managers.Instance.GetCameraManager().FollowCam;
    Squad Squad => Managers.Instance.GetObjectUnitManager().playerSquad;
    public bool IsDungeonAutoMode;
    
    public int ZonesCount => MapStageMeta.MapStageInfos.Count;
    public PortalBuilding _townPortal; // 마을에 소환된 포탈
    public PortalBuilding _spawnPortal; // 스쿼드 위치에 소환된 포탈
    public TreasureBoxBuilding _treasureBox; // 보물상자
    public Dictionary<string, GameObject> _fieldEffect = new Dictionary<string, GameObject>();

    public int _townIndex = 2;
    public int _townPortalIndex = 2;
    
    public EContent _contentType = EContent.Field;
    public EFactionType _factionType = EFactionType.None;
    public int _contentsId;
    public int _contentsLevel;
    public bool _enableJoyStick;
    
    // 맵 전체 몬스터에게 적용되는 데이터들
    public bool _enableGrowth;
    public int _addAttackPer;
    public int _addHpPer;
    public int _relicID;
    public int _relicDropRate;
    public int _soulStoneID;
    public int _soulStoneDropRate;
    public int _passiveBaseId;
        			
    public void Init(EContent contentType, EFactionType factionType, int contentsId, int contentsLevel)
    {
        _townIndex = -1;
        _townPortalIndex = -1;
        _contentType = contentType;
        _factionType = factionType;
        _contentsId = contentsId;
        _contentsLevel = contentsLevel;
        _enableGrowth = contentType == EContent.Constellation;
    }

    public void SetFieldData(FieldInfo fieldInfo, FieldDetail fieldDetail)
    {
        _addAttackPer = fieldDetail.MonsterAttackRate;
        _addHpPer = fieldDetail.MonsterHPRate;
        _relicID = fieldDetail.RelicID;
        _relicDropRate = fieldDetail.RelicDropRate;
        _soulStoneID = fieldDetail.SoulStoneID;
        _soulStoneDropRate = fieldDetail.SoulStoneDropRate;
        _passiveBaseId = fieldInfo.FieldPassiveType;
        _enableJoyStick = true;
    }

    public void SetDungeonData(bool enableJoyStick)
    {
        _addAttackPer = 0;
        _addHpPer = 0;
        _relicID = 0;
        _relicDropRate = 0;
        _soulStoneID = 0;
        _soulStoneDropRate = 0;
        _passiveBaseId = 0;
        _enableJoyStick = enableJoyStick;
    }

    public async UniTask LoadMap(string key)
    {
        GameObject tPrefab = await Managers.Instance.GetResObjectManager().InstantiateAsync(key);
        tPrefab.transform.SetParent(Managers.Instance.GetObjectUnitManager().MapTrans);
        MapStageMeta mapStageMeta = tPrefab.GetComponent<MapStageMeta>();
        
        _mapStageMeta = mapStageMeta;
        foreach (var mapStageInfo in MapStageInfos)
        {
            mapStageInfo.SetTileData();
            if(_contentType == EContent.Field)
                await mapStageInfo.SpawnFieldUnit();
            else
                await mapStageInfo.SpawnDungeonUnit(_contentType, _factionType, _contentsLevel);

            if(mapStageInfo.IsTown)
                _townIndex = mapStageInfo.ZoneIndex;
            
            if (mapStageInfo.IsTownPortal)
            {
                _townPortalIndex = mapStageInfo.ZoneIndex;
                _townPortal = mapStageInfo.TownPortal;
            }
        }

        GenerateMap();
    }

    //GenerateMap은 한번에....
    private void GenerateMap()
    {
        if (MapStageMeta == null)
            return;
        
        MapStageMeta.GenerateAllMaps();
    }
    
    public void GenerateMap(int zoneIndex)
    {
        if (MapStageMeta == null)
            return;

        MapStageMeta.GenerateZoneMap(zoneIndex);
    }
    
    // BattleTestScene
    public async UniTask LoadMap(MapStageMeta mapStageMeta, int id)
    {
        _mapStageMeta = mapStageMeta;
        foreach (var mapStageInfo in MapStageInfos)
        {
           await mapStageInfo.SpawnTileUnit(id);
        }

        GenerateMap();
    }

    public Vector2 GetStartPosition(int id)
    {
        MapStageInfo mapStageInfo = GetMapStageInfo(id);
        return mapStageInfo.StartPosition;
    }

    public Transform GetStartTransform(int id)
    {
        MapStageInfo mapStageInfo = GetMapStageInfo(id);
        return mapStageInfo.StartTransform;
    }

    public void DestroyMap()
    {
        foreach (var mapStageInfo in _fieldEffect)
        {
            GameObject fieldEffect = mapStageInfo.Value;
            if(fieldEffect != null)
                Managers.Instance.GetResObjectManager().Destroy(fieldEffect);
        }
        _fieldEffect.Clear();
        
        if(Managers.Instance.GetResObjectManager() != null)
        {
            if(_mapStageMeta != null)
                Managers.Instance.GetResObjectManager().Destroy(MapStageMeta.gameObject);
        }
    }


    #region Dialogue
    public int EnableDialogue(int zoneIndex, Vector3 worldPosition)
    {
        return GetMapStageInfo(zoneIndex).EnableDialogue(worldPosition);
    }
    #endregion
    
    public int EnableEntrance(Vector3 worldPosition)
    {
        foreach (MapStageInfo mapStageInfo in MapStageInfos)
        {
            int zoneIndex = mapStageInfo.EnableEntrance(worldPosition);
            if(zoneIndex > -1)
                return zoneIndex;
        }
        return -1;
    }

    #region CameraMove

    public BaseBuilding FindBuilding(int id)
    {
        foreach (var mapStageInfo in MapStageInfos)
        {
            foreach (var building in mapStageInfo._buildings)
            {
                if(building.BuildingData._data.ID == id)
                    return building;
            }
        }
        
        return null;
    }
    
    public SpawnPointInfoUnit FindSpawnPoint(int id)
    {
        foreach (var mapStageInfo in MapStageInfos)
        {
            foreach (var spawnPoint in mapStageInfo._spawnPointInfoUnitList)
            {
                if(spawnPoint._spawnPointInfoData.ID == id)
                    return spawnPoint;
            }
        }
        
        return null;
    }
    
    //Auto 전투용 스폰포인트 받아오기 던전에서만 사용
    public List<SpawnPointInfoUnit> GetSpawnPoint()
    {
        return GetMapStageInfo(Squad._zoneIndex)._spawnPointInfoUnitList;
    }

    public void BuildingCameraMove(int buildingId)
    {
        BaseBuilding baseBuilding = FindBuilding(buildingId);
        if(baseBuilding == null)
            return;
        
        CameraMove(baseBuilding.transform.position, true).Forget();
    }
    
    public void SpawnPointCameraMove(int spawnPointId)
    {
        SpawnPointInfoUnit spawnPoint = FindSpawnPoint(spawnPointId);
        if(spawnPoint == null)
            return;
        
        CameraMove(spawnPoint.transform.position, true).Forget();
    }
    
    public async UniTask CameraMove(Vector2 position, bool arrowActive)
    {
        Managers.Instance.GetUIManager().JoystickUI.Close();
        _arrowObject.SetActive(arrowActive);
        _arrowObject.transform.position = position;
        FollowCamera.smoothSpeed = 0.5f;
        FollowCamera.SetTarget(_arrowObject.transform);

        await UniTask.DelayFrame(1);
        await UniTask.WaitUntil(() => FollowCamera.IsFollow);
    }

    public async UniTask CameraReturn(bool activeJoystick)
    {
        _arrowObject.SetActive(false);
        FollowCamera.SetTarget(Squad.transform);
        await UniTask.DelayFrame(1);
        await UniTask.WaitUntil(() => FollowCamera.IsFollow);
        FollowCamera.smoothSpeed = 0.1f;
        if(activeJoystick)
            Managers.Instance.GetUIManager().JoystickUI.Open();
    }

    public IEnumerator CameraZoomInCoroutine(Transform target)
    {
        Managers.Instance.GetUIManager().JoystickUI.Close();
        FollowCamera.smoothSpeed = 0.5f;
        FollowCamera.SetTarget(target);
        
        yield return null;
        yield return new WaitUntil(() => FollowCamera.IsFollow);
    }

    public IEnumerator CameraZoomOutCoroutine()
    {
        FollowCamera.SetTarget(Squad.transform);
        yield return null;
        yield return new WaitUntil(() => FollowCamera.IsFollow);
        FollowCamera.smoothSpeed = 0.1f;
        Managers.Instance.GetUIManager().JoystickUI.Open();
    }
    
    public IEnumerator DungeonOpenCoroutine(int buildingId)
    {
        Squad.BattleStop();
        BaseBuilding fogBuilding = FindBuilding(buildingId);
        yield return new WaitForSeconds(0.5f);
        fogBuilding._spineAnimation.AnimationStart();
        yield return new WaitForSeconds(2.0f);
        fogBuilding._spineAnimation.SetAnimation("idle", true);
        Squad.BattleStart();
    }
    
    public IEnumerator FieldDungeonOpenCoroutine(int buildingId)
    {
        Squad.BattleStop();
        BaseBuilding fogBuilding = FindBuilding(buildingId);
        yield return new WaitForSeconds(0.5f);
        fogBuilding._spineAnimation.AnimationStart();
        yield return new WaitForSeconds(2.0f);
        fogBuilding._spineAnimation.SetAnimation("clear", true);
        Squad.BattleStart();
    }

    #endregion

    /// <summary>
    /// 맵 이동시 오브젝트들 행동 코루틴 종료
    /// </summary>
    public void CheckMapData(int id)
    {
        MapStageInfo currentMap = GetMapStageInfo(id);

        foreach (var map in MapStageInfos)
        {
            bool isSameMap = map.ZoneIndex == id;
            bool isConnected = currentMap._connectMapId.Contains(map.ZoneIndex);

            if (isSameMap || isConnected)
                map.StartUnitState();
            else
                map.StopUnitState();
        }
    }

    #region Dungeon

    public int GetLiveUnitCount()
    {
        int count = 0;
        foreach (var mapStageInfo in MapStageInfos)
        {
            count += mapStageInfo.GetLiveUnitCount();
        }

        return count;
    }

    public int GetUnitCount()
    {
        int count = 0;
        for (int i = 0; i < MapStageInfos.Count; i++)
        {
            MapStageInfo mapStageInfo = MapStageInfos[i];
            count += mapStageInfo.GetUnitCount();
        }

        return count;
    }
    
    public EnemyUnit GetEnemyUnit(int id)
    {
        foreach (var mapStageInfo in MapStageInfos)
        {
            EnemyUnit enemyUnit = mapStageInfo.GetUnit(id);
            if (enemyUnit != null)
                return enemyUnit;
        }

        return null;
    }
    
    //RewardBox가 있는지.
    public RewardBoxBuilding FindRewardBoxBuilding()
    {
        foreach (var mapData in MapStageInfos)
        {
            if (mapData.RewardBox == null)
                continue;

            return mapData.RewardBox;
        }

        return null;
    }

    #endregion

    public MapStageInfo GetMapStageInfo(int id)
    {
        if (MapStageMeta == null)
            return null;

        MapStageMeta.TryGetStageInfo(id, out var stageInfo);
        return stageInfo;
    }
    
    public int GetZoneIndex(int index)
    {
        return MapStageInfos[index].ZoneIndex;
    }

    public bool IsTownZone(int mapId)
    {
        return mapId == _townIndex;
    }
    
    public bool IsTownPortalZone(int mapId)
    {
        return mapId == _townPortalIndex;
    }

    #region TownPortal

    public async UniTask CreateTownPortal(Vector2 position)
    {
        if (_townPortalIndex == -1)
            return;

        _spawnPortal = await Managers.Instance.GetObjectUnitManager().SpawnPortal(1, "TownPortal", position);
        _spawnPortal.gameObject.SetActive(false);
    }

    public void SpawnTownPortal(int zoneId, Vector2 position)
    {
        int zoneIndex = zoneId;
        if (_townPortalIndex == -1 || zoneIndex == -1)
            return;

        _spawnPortal.transform.position = position;
        _spawnPortal.ConnectPortal(_townPortal);
        _townPortal.ConnectPortal(_spawnPortal);
        _spawnPortal.SetZoneIndex(zoneIndex);
        _spawnPortal.gameObject.SetActive(true);
        _townPortal.gameObject.SetActive(true);
    }

    #endregion

    #region TreasureBox

    public async UniTask CreateTreasureBox(int id)
    {
        _treasureBox = await Managers.Instance.GetObjectUnitManager().SpawnTreasureBox(id, "TreasureBox", GetMapStageInfo(Squad._zoneIndex).GetTreasureBoxPosition());
        UITreasureNavigation uiTreasureNavigation = Managers.Instance.GetUIManager().UITreasureNavigation;
        uiTreasureNavigation.chest = _treasureBox.transform;
    }

    public void RemoveTreasureBox()
    {
        Managers.Instance.GetObjectUnitManager().DestroyBuilding(_treasureBox);
        UITreasureNavigation uiTreasureNavigation = Managers.Instance.GetUIManager().UITreasureNavigation;
        uiTreasureNavigation.Close();
        Managers.Instance.GetTreasureBoxManager().StartTimer();
    }

    public UITreasureNavigation CreateTreasureNavigation(Transform rewardBox)
    {
        UITreasureNavigation uiTreasureNavigation = Managers.Instance.GetUIManager().UITreasureNavigation;
        uiTreasureNavigation.chest = rewardBox;
        return uiTreasureNavigation;
    }

    #endregion

    #region WarpPoint

    public BaseBuilding FindWarpPoint(int warpPointIndex)
    {
        for (int i = 0; i < MapStageInfos.Count; i++)
        {
            MapStageInfo mapData = MapStageInfos[i];

            if (mapData.WarpPoint == null)
                continue;

            if (mapData.WarpPoint.BuildingData._data.ID == warpPointIndex)
            {
                return mapData.WarpPoint;
            }
        }

        return null;
    }

    #endregion

    #region FogPoint

    public FogObject GetFogObject(int fogIndex)
    {
        for (int i = 0; i < MapStageInfos.Count; i++)
        {
            MapStageInfo mapData = MapStageInfos[i];

            if (mapData.fogObjects == null)
                continue;

            foreach (FogObject fogObject in mapData.fogObjects.Values)
            {
                if (fogObject._FogId == fogIndex)
                    return fogObject;
            }
        }

        return null;
    }

    #endregion

    //오브젝트 삭제 후 맵 삭제
    public void UnLoadMap()
    {        
        Managers.Instance.GetObjectUnitManager().DestroyAll();
        Managers.Instance.GetObjectUnitManager().DestroySquad();
        DestroyMap();
    }
    
    // position으로 navMap가져오기
    public int FindZoneIndex(Vector2 position)
    {
        return MapStageMeta.FindZoneIndex(position);
    }

    // squad 기준으로 navMap가져오기
    public PolyNavMap GetPolyNavMap(int zoneIndex)
    {
        MapStageInfo mapStageInfo = GetMapStageInfo(zoneIndex);
        return mapStageInfo._navMap;
    }

    public async UniTask CreateWeatherEffect(int zoneIndex)
    {
        string effectName = GetMapStageInfo(zoneIndex).GetEffectName();
        foreach (var fieldEffect in _fieldEffect.Values)
        {
            if(fieldEffect != null)
                fieldEffect.SetActive(false);
        }
        if (string.IsNullOrEmpty(effectName))
            return;
        
        if(_fieldEffect.ContainsKey(effectName))
            _fieldEffect[effectName].SetActive(true);
        else
        {
            GameObject fieldEffectObject = await Managers.Instance.GetResObjectManager().InstantiateAsync(effectName, Managers.Instance.GetUIManager().WeatherEffectRoot);
            _fieldEffect.Add(effectName, fieldEffectObject);                
        }
    }
}