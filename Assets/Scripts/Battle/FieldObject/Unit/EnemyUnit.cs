using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

public class EnemyUnit : BaseUnit
{
    public SpawnPointInfoUnit _spawnPointInfoUnit;
    public BaseUnit _aggroUnit;
    private Vector3 _destPos;
    private Vector3 _initPos;
    private float _disableTime;

    private CancellationTokenSource _returnHealCts;
    public MonsterStatusInfo _monsterStatusInfo;

    public override void Init()
    {
        _initPos = transform.position;
        _teamType = ETeamType.Enemy;

        _flashEffect.flashColor = Utils.HexToColor("BE0000");

        _findMask = LayerMask.GetMask("Player");

        transform.tag = "Enemy";
        SetMass(false);
        gameObject.SetActive(true);
        
        RemoveAllEffect();

        SetSkillObserver();
        SetPassiveSkillObserver();
        base.Init();
    }
    
    private void SetSkillObserver()
    {
        //DefaultSkill
        DBKey defaultSkillBaseDBkey =
            UnitType == EUnitType.PlayerCharacter ? DBKey.PcDefaultSkillBase : DBKey.NpcDefaultSkillBase;
        SkillBase defaultSkillBase = ClientLocalDB_Simple.GetData<SkillBase>(defaultSkillBaseDBkey, _unitDataTable.DefaultSkills);
        List<SkillDetailData> defaultSkillDetailData = new List<SkillDetailData>();
        foreach (var detailId in defaultSkillBase.SkillDetail)
        {
            DBKey defaultSkillDetailDBkey =
                UnitType == EUnitType.PlayerCharacter ? DBKey.PcDefaultSkillDetail : DBKey.NpcDefaultSkillDetail;
            defaultSkillDetailData.Add(ClientLocalDB_Simple.GetData<SkillDetailData>(defaultSkillDetailDBkey, detailId));
        }
        BaseSkill defaultSkill = new BaseSkill(0, defaultSkillBase, defaultSkillDetailData, 1);
        
        //ActiveSkill
        List<BaseSkill> activeSkills = new List<BaseSkill>();
        List<BaseSkill> specialSkills = new List<BaseSkill>();
        for (int i = 0; i < _unitDataTable.ActiveSkills.Length; i++)
        {
            int baseId = _unitDataTable.ActiveSkills[i];
            if (baseId == 0)
                continue;
            
            DBKey activeSkillBaseDBkey =
                UnitType == EUnitType.PlayerCharacter ? DBKey.PcActiveSkillBase : DBKey.NpcActiveSkillBase;
            SkillBase activeSkillBase = ClientLocalDB_Simple.GetData<SkillBase>(activeSkillBaseDBkey, baseId);
            List<SkillDetailData> activeSkillDetailData = new List<SkillDetailData>();
            List<SkillDetailData> specialSkillDetailData = new List<SkillDetailData>();
            
            foreach (var detailId in activeSkillBase.SkillDetail)
            {
                DBKey activeSkillDetailDBkey =
                    UnitType == EUnitType.PlayerCharacter ? DBKey.PcActiveSkillDetail : DBKey.NpcActiveSkillDetail;
                SkillDetailData detailData = ClientLocalDB_Simple.GetData<SkillDetailData>(activeSkillDetailDBkey, detailId);
                if(activeSkillBase.SkillSlotType == ESkillSlotType.ActiveSkill)
                    activeSkillDetailData.Add(detailData);
                else
                    specialSkillDetailData.Add(detailData);
            }
            
            if(activeSkillBase.SkillSlotType == ESkillSlotType.ActiveSkill)
                activeSkills.Add(new BaseSkill(activeSkills.Count, activeSkillBase, activeSkillDetailData, 1));
            else
                specialSkills.Add(new BaseSkill(specialSkills.Count, activeSkillBase, specialSkillDetailData, 1));
        }
        
        _skillObserver.InitializeSkillObserver(this, defaultSkill, activeSkills.ToArray(), specialSkills.ToArray());
    }

    private void SetPassiveSkillObserver()
    {
        _passiveSkillObserver.InitializeSkillObserver(this);
        
        if (MapManager._passiveBaseId != 0)
            _passiveSkillObserver.AddPassiveSkill(EPassiveSlotType.Field, MapManager._passiveBaseId);
        _passiveSkillObserver.Trigger(EPassiveSkillTriggerType.Always, null);
    }

    public override async UniTask SetUnitId(UserInfoData userInfoData, int id)
    {
        _unitDataTable = ClientLocalDB_Simple.GetData<UnitData>(DBKey.MonsterCharacter, id);
        UnitType = _unitDataTable.UnitType;
        await base.SetUnitId(userInfoData, id);
    }

    public void SetLevel(int level, int statusType)
    {
        if (_statusInfo == null)
        {
            _monsterStatusInfo = new MonsterStatusInfo();
            _statusInfo = _monsterStatusInfo;
        }
        
        MonsterStatusInfo monsterStatusInfo = _statusInfo as MonsterStatusInfo;
        StatusData statusData = ClientLocalDB_Simple.GetData<StatusData>(DBKey.MonsterStatus, (int)statusType);
        GrowStatusData growStatusData =
            ClientLocalDB_Simple.GetData<GrowStatusData>(DBKey.MonsterGrowStatus, (int)statusType);
        
        Status status = new Status();
        status._attackPercent = MapManager._addAttackPer;
        status._maxHpPercent = MapManager._addHpPer;
        
        monsterStatusInfo?.SetLevelStatus(level, statusData, growStatusData, status);
    }

    protected override bool GetLoopByState(EUnitState state)
    {
        if (state == EUnitState.Die)
            return false;

        return base.GetLoopByState(state);
    }

    protected override bool IsTargetOnSameNavMap(BaseUnit target)
    {
        if (_agent.map == null) return true;
        return _agent.map.PointIsValid(target._rigidbody.position);
    }

    protected override void FindTarget()
    {
        if (Managers.Instance.GetObjectUnitManager().playerSquad.IsTownZone.Value)
        {
            _moveTarget = null;
            return;
        }
        
        float searchRange = (float)_playStatus.MoveTargetSearchRange * 0.01f;
        float aggroRange = 0;
        if (_aggroUnit != null)
        {
            Vector2 dis = _rigidbody.position - _aggroUnit._rigidbody.position;
            aggroRange = dis.magnitude + 1;
        }
        
        _moveTarget = GetSearchUnit(Mathf.Max(aggroRange, searchRange), _findMask);
    }

    public override void UpdateMove()
    {
        base.UpdateMove();

        FindTarget();
        if (_moveTarget == null)
        {
            if (!LerpCellPosCompleted)
            {
                LerpToPosition();
                return;
            }

            if (_characterPositionType == ECharacterPositionType.LimitOut)
                ReturnMove();
        }
        else
        {
            LerpCellPosCompleted = true;
            if (MoveToTargetUnit())
                AttackAction();
        }
    }

    protected override void UpdatePolyNavMap()
    {
        // EnableEntrance 반환값(-1 edge case 등)에 의존하지 않고
        // 할당된 NavMap 기준으로 직접 위치 유효성 검사
        if (_agent.map != null && !_agent.map.PointIsValid(_rigidbody.position))
            TeleportToInitPos();
    }

    private void TeleportToInitPos()
    {
        // 어그로 해제
        _moveTarget = null;
        _aggroUnit = null;

        _agent.Stop();
        _rigidbody.position = _initPos;
        _rigidbody.velocity = Vector2.zero;

        LerpCellPosCompleted = true;
        RemoveAllEffect();
        _state.ChangeState(State_Idle.Instance);
        MyLogger.LogWarning($"[EnemyUnit] NavMesh 범위 밖 감지 → InitPos 복귀: {name}");
    }

    protected override void LerpToPosition()
    {
        if (LerpCellPosCompleted)
            return;

        MoveToTargetNav(_movePos);
        if (_agent.remainingDistance < 0.5f)
        {
            LerpCellPosCompleted = true;
            _state.ChangeState(State_Idle.Instance);
        }
    }

    protected override void ReturnMove()
    {
        if (_agent == null)
            return;

        _movePos = _initPos;
        base.ReturnMove();

        if (MapManager._contentType == EContent.Field)
            StartReturnHeal();
    }

    private void StartReturnHeal()
    {
        StopReturnHeal();
        _returnHealCts = new CancellationTokenSource();
        ReturnHealLoop(_returnHealCts.Token).Forget();
    }

    private void StopReturnHeal()
    {
        _returnHealCts?.Cancel();
        _returnHealCts?.Dispose();
        _returnHealCts = null;
    }

    private async UniTaskVoid ReturnHealLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await UniTask.Delay(500, cancellationToken: ct);
            if (ct.IsCancellationRequested)
                break;

            if (_playStatus.Hp >= _playStatus.MaxHp || _moveTarget != null)
            {
                StopReturnHeal();
                break;
            }

            OnHeal(_playStatus.MaxHp * 0.1);
        }
    }

    protected override void OnDie(BaseUnit attacker)
    {
        StopReturnHeal();
        base.OnDie(attacker);

        if (UserInfoData.IsSimulation)
            return;
        
        _aggroUnit = null;
        _spawnPointInfoUnit.CheckDieUnit();

        _disableTime = 3;
        _destPos = _initPos;

        if (MapManager._contentType != EContent.Field)
            return;
        
        // 퀘스트 체크
        if(UnitType != EUnitType.Gather)
        {
            if(UnitType == EUnitType.NormalMonster)
                SyncCurrencyManager.AddQuest(ESyncQuestType.NormalMonster);
            else if(UnitType == EUnitType.EliteMonster)
                SyncCurrencyManager.AddQuest(ESyncQuestType.EliteMonster);

            UserInfoData.UpdateCorrectQuest(EQuestType.Guide, EQuestConditionType.MonsterKillAnyone, _unitId);
            UserInfoData.UpdateCorrectQuest(EQuestType.Guide, EQuestConditionType.MonsterKillType, (int)_unitDataTable.UnitType);
            UserInfoData.UpdateCorrectQuest(EQuestType.Guide, EQuestConditionType.MonsterKillTarget, _unitId);
            
            UserInfoData.UpdateCorrectQuest(EQuestType.Daily, EQuestConditionType.MonsterKillAnyone, _unitId);
            UserInfoData.UpdateCorrectQuest(EQuestType.Daily, EQuestConditionType.MonsterKillType, (int)_unitDataTable.UnitType);
            UserInfoData.UpdateCorrectQuest(EQuestType.Daily, EQuestConditionType.MonsterKillTarget, _unitId);
        
            UserInfoData.UpdateCorrectQuest(EQuestType.Weekly, EQuestConditionType.MonsterKillAnyone, _unitId);
            UserInfoData.UpdateCorrectQuest(EQuestType.Weekly, EQuestConditionType.MonsterKillType, (int)_unitDataTable.UnitType);
            UserInfoData.UpdateCorrectQuest(EQuestType.Weekly, EQuestConditionType.MonsterKillTarget, _unitId);

            UserInfoData.UpdateCorrectQuest(EQuestType.Open, EQuestConditionType.MonsterKillAnyone, _unitId);
        }
        
        ApplyReward();
    }

    /// <summary>
    /// 삭제하면 X 부활 로직 체크하면 안됨
    /// </summary>
    public override void UpdateDie()
    {
        _disableTime -= Time.fixedDeltaTime;
        if (_disableTime <= 0)
            gameObject.SetActive(false);
    }

    protected override void SetUnitPositionType()
    {
        int limitFactor = _spawnPointInfoUnit._clusterSpreadFactor + 2;
        float limitDistRadius = limitFactor * limitFactor;
        Vector2 gap = (Vector2)_initPos - _rigidbody.position;

        float sqrMagnitude = gap.sqrMagnitude;
        if (sqrMagnitude < limitDistRadius)
            _characterPositionType = ECharacterPositionType.BasicIn;
        else
            _characterPositionType = ECharacterPositionType.LimitOut;
    }

    public override void SetMass(bool isLock)
    {
        if (isLock)
        {
            _rigidbody.mass = 10000;
        }
        else
        {
            switch (UnitType)
            {
                case EUnitType.NormalMonster:
                    _rigidbody.mass = 10;
                    break;
                case EUnitType.EliteMonster:
                    _rigidbody.mass = 500;
                    break;
                case EUnitType.FieldBossMonster:
                    _rigidbody.mass = 10000;
                    break;
            }

        }
    }

    //by rainful 2025-05-18 hit 사운드 몬스터만
    public override void OnDamage(double damage, string damageTextName, BaseUnit attacker)
    {
        Managers.Instance.Sound.PlaySFX("SkillEffect", "Hit");
        _spawnPointInfoUnit.SetAggro(attacker);
        base.OnDamage(damage, damageTextName, attacker);
    }
    
    public void SetSpawnPointInfoAndPos(SpawnPointInfoUnit circle, Vector2 startPos)
    {
        _spawnPointInfoUnit = circle;
        _startPos = startPos;
    }
    
    public void SetDropItemData(ERewardType rewardType, int[] rewardID, int[] count)
    {
        _dropItemData._rewardType = rewardType;
        _dropItemData._rewardID = rewardID;
        _dropItemData._count = count;
    }

    protected virtual void ApplyReward()
    {
        if (_dropItemData._rewardType == ERewardType.None)
            return;

        for (int i = 0; i < _dropItemData._rewardID.Length; i++)
        {
            int id = _dropItemData._rewardID[i];
            int count = _dropItemData._count[i];
            int multiple = (int)CalculateStatus.ToRate(MapManager._soulStoneDropRate); 
            if (MapManager._relicID == id)
            {
                int random = Random.Range(0, 10000);
                if (random >= MapManager._relicDropRate)
                    continue;
            }
            else if (MapManager._soulStoneID == id)
            {
                count *= multiple;
            }

            ECurrency currencyType = (ECurrency)id;
            count = Math.Min(UserInfoData.CanCurrencyAcquireNumber(currencyType, count), count);
            UserInfoData.AddCurrencyAcquireNumber(currencyType, count);
            UserInfoData.AddCurrencyValue(currencyType, count);
            SyncCurrencyManager.AddCurrency(currencyType, count);
            SpawnObject(id, count);   
        }
    }

    //보상 연출
    protected void SpawnObject(int rewardID, int rewardCount)
    {
        if (rewardCount == 0)
            return;

        int result = Math.Min(rewardCount, 5);
        
        for (int i = 0; i < result; i++)
        {
            Managers.Instance.GetObjectUnitManager().SpawnFieldDropItem(rewardID, transform.position).Forget();            
        }
    }

    public void SetAggro(BaseUnit aggroUnit)
    {
        _aggroUnit = aggroUnit;
    }
}