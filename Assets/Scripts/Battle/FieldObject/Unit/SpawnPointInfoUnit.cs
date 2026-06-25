using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

public class SpawnPointInfoUnit : MonoBehaviour
{
    public List<EnemyUnit> _unitList = new List<EnemyUnit>();
    public SpawnPointInfoData _spawnPointInfoData;
    private SpawnObjectGroupData _spawnObjectGroupData;
    protected SpawnGroupUnit _spawnGroupUnit;

    protected int _zoneIndex;
    private int _spawnObjectLoopCnt; // spawn Loop Count
    public int _clusterSpreadFactor;
    public BaseUnit _aggroUnit;

    private bool IsFirst => _unitList.Count == 0; // 첫 생성
    private bool _isPlaying;
    public bool _isFogIn;

    private DungeonBase _dungeon;
    private bool IsDungeon => _dungeon != null;
    private UserInfoData UserInfoData => Managers.Instance.UserInfo();
    private CancellationTokenSource _aggroCts;
    private const float AGGRO_RESET_TIME = 5f;

    // private const float BattleUpdateCullDist = 30f;
    // private const float BattleUpdateCullDistSqr = BattleUpdateCullDist * BattleUpdateCullDist;

    private void FixedUpdate()
    {
        if (!_isPlaying || _isFogIn)
            return;

        // PlayerUnit mainCharacter = Managers.Instance.GetObjectUnitManager().playerSquad.mainCharacter;
        // if (mainCharacter != null)
        // {
        //     float distSqr = ((Vector2)transform.position - mainCharacter._rigidbody.position).sqrMagnitude;
        //     if (distSqr > BattleUpdateCullDistSqr)
        //         return;
        // }

        _unitList.ForEach(unit => unit.BattleUpdate());
    }

    public void EnterFog()
    {
        _isFogIn = true;
        BattleStop();
    }

    public void BattleStop()
    {
        _isPlaying = false;
        ClearAggro();
        _unitList.ForEach(unit =>
        {
            if (unit._state != null)
            {
                unit.Init();
                unit.GetStatusInfo().ResetPlayStatus();
                unit.GetStatusInfo().ApplyPlayHealth();
                unit.BattleStop();    
            }
        });
    }

    public void BattleStart()
    {
        if (_isFogIn)
            return;

        _isPlaying = true;
        _unitList.ForEach(unit => unit.BattleStart());
    }
    
    public void SetAggro(BaseUnit aggroUnit)
    {
        if (aggroUnit == null)
            return;

        _aggroUnit = aggroUnit;

        _unitList.ForEach(unit => unit.SetAggro(_aggroUnit));

        RefreshAggroTimer().Forget();
    }
    
    private async UniTaskVoid RefreshAggroTimer()
    {
        _aggroCts?.Cancel();
        _aggroCts?.Dispose();

        _aggroCts = new CancellationTokenSource();

        try
        {
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(AGGRO_RESET_TIME),
                cancellationToken: _aggroCts.Token);

            ClearAggro();
        }
        catch (OperationCanceledException)
        {
            // 새 Aggro 갱신됨
            MyLogger.Log("aggro cancelled");
        }
    }
    
    private void OnDestroy()
    {
        _aggroCts?.Cancel();
        _aggroCts?.Dispose();
    }
    
    private void ClearAggro()
    {
        _aggroCts?.Cancel();
        _aggroCts?.Dispose();
        _aggroCts = null;

        _aggroUnit = null;

        _unitList.ForEach(unit => unit.SetAggro(null));
    }

    public int LiveUnitCount()
    {
        int count = 0;
        for (int i = 0; i < _unitList.Count; i++)
        {
            BaseUnit unit = _unitList[i];
            if (!unit.IsDie)
                count++;
        }

        return count;
    }

    public virtual void Init(int zoneIndex, int id, int clusterSpreadFactor, DungeonBase dungeonBase = null)
    {
        _zoneIndex = zoneIndex;
        _clusterSpreadFactor = clusterSpreadFactor;
        _isFogIn = false;

        _dungeon = dungeonBase;
        _spawnPointInfoData = ClientLocalDB_Simple.GetData<SpawnPointInfoData>(DBKey.SpawnPointInfo, id);
        _spawnGroupUnit = ClientLocalDB_Simple.GetData<SpawnGroupUnit>(DBKey.SpawnGroupUnit, id);

        if (_spawnPointInfoData == null)
        {
            // 없는 ID가 있을때 확인용
            MyLogger.Log("_zoneIndex : "+_zoneIndex);
            return;
        }
        
        _spawnObjectGroupData =
            ClientLocalDB_Simple.GetData<SpawnObjectGroupData>(DBKey.SpawnObjectGroup,
                _spawnPointInfoData.SpawnObjectGroup);

        switch (_spawnPointInfoData.SpawnObjectType)
        {
            case ESpawnObjctType.Monster:
                gameObject.transform.SetParent(Managers.Instance.GetObjectUnitManager().MonsterTrans);
                break;
            case ESpawnObjctType.Gather:
                gameObject.transform.SetParent(Managers.Instance.GetObjectUnitManager().GatherResTransform);
                break;
            default:
                break;
        }

        CreateUnit().Forget();
    }

    private async UniTask CreateUnit()
    {
        if (_spawnPointInfoData == null)
        {
            Debug.LogError("null Circle Data!!");
            return;
        }

        switch (_spawnPointInfoData.SpawnObjectType)
        {
            case ESpawnObjctType.Monster:
                await CreateMonster();
                break;
            case ESpawnObjctType.Gather:
                await CreateGrUnit();
                break;
            default:
                break;
        }
    }

    private async UniTask CreateGrUnit()
    {
        if (IsFirst)
        {
            for (int i = 0; i < _spawnObjectGroupData.CountList.Length; i++)
            {
                int unitId = _spawnObjectGroupData.ObjectList[i];
                int count = _spawnObjectGroupData.CountList[i];
                for (int j = 0; j < count; j++)
                {
                    GrObjectData data = ClientLocalDB_Simple.GetData<GrObjectData>(DBKey.GatherObject, unitId);

                    if (data.Resource.IsNull())
                        continue;

                    GrTree unit = Managers.Instance.GetObjectUnitManager()
                        .SpawnUnit<GrTree>(transform.position, data.Resource);

                    if (unit == null)
                        continue;

                    unit.gameObject.transform.SetParent(this.gameObject.transform);
                    await unit.SetUnitId(UserInfoData, data.ID);
                    if (!_spawnGroupUnit.RewardType.IsNull())
                        unit.SetDropItemData(Utils.ParseEnum<ERewardType>(_spawnGroupUnit.RewardType),
                            _spawnGroupUnit.RewardID, _spawnGroupUnit.RewardCount);
                    Vector2 pos = GetRandomPosition();
                    unit.transform.localPosition = pos;
                    _unitList.Add(unit);
                }
            }
        }

        foreach (EnemyUnit unit in _unitList)
        {
            unit.SetZoneIndex(_zoneIndex);
            unit.Init();
            unit.GetStatusInfo().ResetPlayStatus();
            unit.GetStatusInfo().ApplyPlayHealth();
            unit.SetPosition(unit.transform.position);
            unit.SetSpawnPointInfoAndPos(this, unit.transform.position);
        }
    }

    private async UniTask CreateMonster()
    {
        if (IsFirst)
        {
            for (int i = 0; i < _spawnObjectGroupData.CountList.Length; i++)
            {
                int unitId = _spawnObjectGroupData.ObjectList[i];
                int count = _spawnObjectGroupData.CountList[i];
                for (int j = 0; j < count; j++)
                {
                    UnitData monsterData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.MonsterCharacter, unitId);

                    if (monsterData.Resource.IsNull())
                        continue;

                    EnemyUnit unit = null;
                    if (monsterData.UnitType == EUnitType.FieldBossMonster || monsterData.UnitType == EUnitType.EliteMonster)
                        unit = Managers.Instance.GetObjectUnitManager().SpawnUnit<BossUnit>(transform.position, "Monster");                        
                    else if (monsterData.UnitType == EUnitType.GuildBossMonster)
                        unit = Managers.Instance.GetObjectUnitManager().SpawnUnit<GuildBossUnit>(transform.position, "Monster");
                    else if (monsterData.UnitType == EUnitType.RankingBossMonster)
                        unit = Managers.Instance.GetObjectUnitManager().SpawnUnit<RankingBossUnit>(transform.position, "Monster");
                    else
                        unit = Managers.Instance.GetObjectUnitManager().SpawnUnit<EnemyUnit>(transform.position, "Monster");


                    if (unit == null)
                        continue;

                    unit.gameObject.transform.SetParent(transform, false);
                    await unit.SetUnitId(UserInfoData, monsterData.ID);

                    if (!_spawnGroupUnit.RewardType.IsNull())
                        unit.SetDropItemData(Utils.ParseEnum<ERewardType>(_spawnGroupUnit.RewardType),
                            _spawnGroupUnit.RewardID, _spawnGroupUnit.RewardCount);
                    _unitList.Add(unit);
                }
            }
        }

        UnitInitAndSetPosition();
    }

    protected virtual void UnitInitAndSetPosition()
    {
        foreach (EnemyUnit unit in _unitList)
        {
            Vector2 pos = GetRandomPosition();
            unit.transform.localPosition = pos;
            if(IsDungeon)
                unit.SetLevel(unit.UnitType == EUnitType.NormalMonster ? _dungeon.NormalMonsterLevel : _dungeon.EliteMonsterLevel,
                    unit.UnitType == EUnitType.NormalMonster ? _dungeon.NormalMonsterStatusType : _dungeon.EliteMonsterStatusType);
            else
                unit.SetLevel(_spawnGroupUnit.Level, _spawnGroupUnit.StatusType);
            unit.Init();
            unit.SetZoneIndex(_zoneIndex);
            unit.SetPosition(unit.transform.position);
            unit.SetSpawnPointInfoAndPos(this, unit.transform.position);
        }

        // Init 이전에 Fog Trigger가 먼저 발생한 경우, 초기화 완료 후 BattleStop 처리
        if (_isFogIn)
            BattleStop();
    }

    /// <summary>
    /// clusterSpreadFactor 에 따른 monster unit 생성 범위 조정 
    /// </summary>
    /// <returns></returns>
    protected Vector2 GetRandomPosition()
    {
        return Random.insideUnitCircle * (_clusterSpreadFactor * 0.9f);
    }

    /// <summary>
    /// clusterSpreadFactor 에 따른 gather 생성 범위 조정 
    /// </summary>
    /// <returns></returns>
    public Vector2 GetGatherRandomPosition()
    {
        List<Vector2> posList = new List<Vector2>();

        for (int i = 0; i < _unitList.Count; i++)
        {
            EnemyUnit unit = _unitList[i];
            Vector2 pos = new Vector2((int)unit.transform.localPosition.x, (int)unit.transform.localPosition.y);
            if (posList.Contains(pos))
                continue;

            posList.Add(pos);
        }

        while (true)
        {
            Vector2 offsetPos = new Vector2(
                Random.Range(-1 * _clusterSpreadFactor, _clusterSpreadFactor + 1f),
                Random.Range(-1 * _clusterSpreadFactor, _clusterSpreadFactor + 1f));

            if (!posList.Contains(offsetPos))
                return offsetPos;
        }
    }

    public void CheckDieUnit()
    {
        // spawnLoopCount Check
        if (_spawnPointInfoData.SpawnLoopType == ESpawnLoopType.Default &&
            _spawnPointInfoData.SpawnLoopCount != -1 &&
            _spawnObjectLoopCnt >= _spawnPointInfoData.SpawnLoopCount)
            return;

        // 같은 써클 유닛이 모두 죽은 경우
        if (!AllDieUnit())
            return;

        // 스폰 중일 때 
        if (isStartSpawnCor) return;
        StartCoroutine(StartSpawnCor());
    }

    public bool AllDieUnit()
    {
        foreach (EnemyUnit unit in _unitList)
        {
            if (!unit.IsDie)
                return false;
        }

        return true;
    }


    bool isStartSpawnCor;

    IEnumerator StartSpawnCor()
    {
        isStartSpawnCor = true;
        yield return new WaitForSeconds(_spawnPointInfoData.SpawntLoopRepeatDelay / 100f); // 스폰 딜레이

        Respawn();

        isStartSpawnCor = false;
    }

    public void Respawn()
    {
        CreateUnit();
        _spawnObjectLoopCnt++;
    }
}