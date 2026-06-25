using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Define;

public class TileUnit : MonoBehaviour
{
    [SerializeField] public int _id;
    [SerializeField] public ETileType _tileType;
    [Range(0,10)] public int _range = 1;

    [Header("Building")]
    [SerializeField] public bool _flip;

#if UNITY_EDITOR
    //by RimGa 2025-05-08 : PreviewTileUnitEditor 커스텀 에디터에서 쓰기 위한 변 수
    [HideInInspector] public bool isFixedPreview = false;
    [HideInInspector] public int previewId = -1;
#endif

    DungeonBase _dungeon;
    public void SetStatus(int id, DungeonBase dungeon = null)
    {
        _id = id;
        _dungeon = dungeon;
    }

    public async UniTask InitTile(MapStageInfo mapStageInfo)
    {
        switch (_tileType)
        {
            case ETileType.Circle:
                mapStageInfo.CreateSpawnPoint(_id, transform.position, _range, _dungeon);
                break;
            case ETileType.Building:
                await mapStageInfo.CreateBuilding(_id, transform.position, _flip);
                break;
            case ETileType.Portal:
                await mapStageInfo.CreatePortal(transform.position);
                break;
            case ETileType.RewardBox:
                await mapStageInfo.CreateRewardBox(transform.position);
                break;
            default:
                break;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        string tileName = gameObject.name;
        if (tileName.Contains("boss"))
            Handles.color = Color.red;
        else if (tileName.Contains("monster"))
            Handles.color = Color.blue;
        else if(tileName.Contains("tree"))
            Handles.color = Color.green;
        else if (tileName.Contains("mine"))
            Handles.color = Color.yellow;
        else
            Handles.color = Color.black;

        Handles.DrawWireDisc(this.transform.position, Vector3.forward, _range);
    }
#endif
}
