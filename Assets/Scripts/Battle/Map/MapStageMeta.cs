using System.Collections.Generic;
using UnityEngine;

public class MapStageMeta : MonoBehaviour
{
    [SerializeField] private List<MapStageInfo> _mapStageInfos = new();

    private Dictionary<int, MapStageInfo> _stageInfoByZone;

    public IReadOnlyList<MapStageInfo> MapStageInfos => _mapStageInfos;

    private void Awake()
    {
        BuildCache();
    }

    private void OnValidate()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        _stageInfoByZone = new Dictionary<int, MapStageInfo>();

        if (_mapStageInfos == null)
            return;

        foreach (var info in _mapStageInfos)
        {
            if (info == null)
                continue;

            _stageInfoByZone[info.ZoneIndex] = info;
        }
    }

    public bool TryGetStageInfo(int zoneIndex, out MapStageInfo mapStageInfo)
    {
        if (_stageInfoByZone == null)
            BuildCache();

        return _stageInfoByZone.TryGetValue(zoneIndex, out mapStageInfo);
    }

    public int FindZoneIndex(Vector2 position)
    {
        if (_mapStageInfos == null)
            return -1;

        foreach (var mapStage in _mapStageInfos)
        {
            if (mapStage == null || mapStage._navMap == null)
                continue;

            if (mapStage.IsTown)
                continue;

            if (mapStage._navMap.PointIsValid(position))
                return mapStage.ZoneIndex;
        }

        return -1;
    }

    public void GenerateZoneMap(int zoneIndex)
    {
        if (!TryGetStageInfo(zoneIndex, out var mapStageInfo))
            return;

        GenerateSingleMap(mapStageInfo);

        if (mapStageInfo._connectMapId == null)
            return;

        foreach (int connectedZoneIndex in mapStageInfo._connectMapId)
        {
            if (!TryGetStageInfo(connectedZoneIndex, out var connectedStage))
                continue;

            GenerateSingleMap(connectedStage);
        }
    }

    public void GenerateAllMaps()
    {
        if (_mapStageInfos == null)
            return;

        foreach (var mapStageInfo in _mapStageInfos)
        {
            GenerateSingleMap(mapStageInfo);
        }
    }

    private void GenerateSingleMap(MapStageInfo mapStageInfo)
    {
        if (mapStageInfo == null || mapStageInfo._navMap == null)
            return;

        mapStageInfo._navMap.GenerateMap();
    }

    #region Collider Refresh

    [ContextMenu("Refresh_Map")]
    public void RefreshMap()
    {
        if (_mapStageInfos == null)
            return;

        foreach (var mapStage in _mapStageInfos)
        {
            if (mapStage == null)
                continue;

            mapStage.RefreshMap();
        }
    }

    [ContextMenu("Refresh_Collider")]
    public void RefreshCollider()
    {
        if (_mapStageInfos == null)
            return;

        foreach (var mapStage in _mapStageInfos)
        {
            if (mapStage == null)
                continue;

            mapStage.RefreshCollider();
        }
    }

    #endregion
}