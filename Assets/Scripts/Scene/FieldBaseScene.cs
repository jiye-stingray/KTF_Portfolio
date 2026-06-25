using Cysharp.Threading.Tasks;
using DG.Tweening;
using MarchingBytes;
using UnityEngine;
using static Define;

public class FieldBaseScene : MonoBehaviour
{
    protected MapManager MapManager => Managers.Instance.GetMapManager();
    protected ObjectUnitManager ObjectUnitManager => Managers.Instance.GetObjectUnitManager();
    protected Squad Squad => Managers.Instance.GetObjectUnitManager().playerSquad;
    protected UIManager UIManager => Managers.Instance.GetUIManager();
    protected FollowCamera FollowCamera => Managers.Instance.GetCameraManager().FollowCam;
    protected UserInfoData UserInfo => Managers.Instance.UserInfo();
    protected BestHttp_GameManager BestHttpGameManager => Managers.Instance.GetServerManager();
    protected SyncCurrencyManager SyncCurrencyManager => Managers.Instance.GetSyncCurrencyManager();
    protected TreasureBoxManager TreasureBoxManager => Managers.Instance.GetTreasureBoxManager();

    protected void Init()
    {
        Managers.Instance.GetObjectUnitManager().playerSquad = ObjectUnitManager.SpawnSquad();

        // Camera
        FollowCamera.SetTarget(Squad.transform);
        FollowCamera.SetSmooth(true);
        FollowCamera.smoothSpeed = 0.1f;
        FollowCamera.cameraFixZ = true;
        FollowCamera.LookAtNow();

        // UI
        UIJoystick uIJoystick = UIManager.ShowUIBase<UIJoystick>("UIJoystick");
        uIJoystick.transform.SetParent(UIManager.UIJoyStickTrans.transform, false);
        uIJoystick.SetJoystick(Managers.Instance.GetJoystick(), Managers.Instance.GetCameraManager().UICam);
        uIJoystick.EnableJoystick(true);
        
        MapManager._arrowObject = GameObject.Find("ArrowPoint");
        if (MapManager._arrowObject != null)
        {
            MapManager._arrowObject.SetActive(false);
            Transform arrowTransform = MapManager._arrowObject.transform.Find("Arrow");
            arrowTransform.DOLocalMoveY(0.5f, 1f)
                .SetEase(Ease.OutCubic)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    /// <summary>
    /// 맵 로드
    /// </summary>
    /// <param name="fieldInfo">필드 DB</param>
    /// <param name="fieldLevel">필드 난이도</param>
    protected async UniTask LoadFieldMap(FieldInfo fieldInfo, FieldDetail fieldDetail)
    {
        MapManager.Init(EContent.Field, EFactionType.None, fieldInfo.ID, fieldDetail.FieldLevel);
        MapManager.SetFieldData(fieldInfo, fieldDetail);
        await MapManager.LoadMap(fieldInfo.Resource);
    }
    
    /// <summary>
    /// 던전 맵 로드
    /// </summary>
    /// <param name="factionType">종족 타입</param>
    /// <param name="contentType">컨텐츠 타입</param>
    /// <param name="zoneIndex"></param>
    /// <param name="dungeonLevel">던전 난이도</param>
    protected async UniTask LoadDungeonMap(EContent contentType, EFactionType factionType, int zoneIndex, int dungeonLevel, bool enableJoyStick)
    {
        MapManager.Init(contentType, factionType, 0, dungeonLevel);
        MapManager.SetDungeonData(enableJoyStick);
        await MapManager.LoadMap($"{contentType}DungeonMap_{zoneIndex}");
    }
    
    protected async UniTask LoadFieldMap(MapStageMeta mapStageMeta, BattleTestSpawnPointInfo spawnPointInfo)
    {
        await MapManager.LoadMap(mapStageMeta, spawnPointInfo.SpawnPointID);
        SpawnPointInfoUnit spawnPointInfoUnit = MapManager.FindSpawnPoint(spawnPointInfo.SpawnPointID);
        spawnPointInfoUnit._unitList.ForEach(unit => unit.SetLevel(spawnPointInfo.Level, spawnPointInfo.StatusType));
    }
    
    /// <summary>
    /// Pvp 맵 로드
    /// </summary>
    protected void LoadPvpMap()
    {
        // MapManager.Init(EContent.Pvp);
        // MapManager.LoadMap($"Prefabs/Map/Pvp/PvpMap");
    }

    protected async UniTask SpawnCharacter(EContent contentType, EFactionType factionType)
    {
        EServerContentType serverContentType;
        if(contentType == EContent.Fog || contentType == EContent.FieldDungeon)
            serverContentType = EServerContentType.Field;
        else
            serverContentType = ReturnServerDungeonType(contentType, factionType);
        
        Squad.SetDeckData(UserInfo.GetDeckData(serverContentType));
        await Squad.RefreshSquadCharacter();
    }

    protected async UniTask CreateDamageText()
    {
        // damageText
        await EasyObjectPool.instance.CreatePoolInfo(EPoolType.DamageText, DamageTextName.Damage);
        await EasyObjectPool.instance.CreatePoolInfo(EPoolType.DamageText, DamageTextName.CriticalDamage);
        await EasyObjectPool.instance.CreatePoolInfo(EPoolType.DamageText, DamageTextName.Blood);
        await EasyObjectPool.instance.CreatePoolInfo(EPoolType.DamageText, DamageTextName.Heal);
        await EasyObjectPool.instance.CreatePoolInfo(EPoolType.DamageText, DamageTextName.Poison);
        await EasyObjectPool.instance.CreatePoolInfo(EPoolType.DamageText, DamageTextName.Gather);
    }
}
