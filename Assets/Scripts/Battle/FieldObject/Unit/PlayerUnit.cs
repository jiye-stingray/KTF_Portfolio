using Cysharp.Threading.Tasks;
using MarchingBytes;
using System;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

public class PlayerUnit : BaseUnit
{
    private UIResurrectionBar resurrectionBar = null;
    public bool isMainCharacter = false;

    public CharacterClassItemData _itemData;

    private EJoystickState joystickState;
    public PlayerStatusInfo _playerStatusInfo;

    public bool _enableResurrection;
    public Squad _squad;

    private Vector2 _accVec = Vector2.zero; // Joystick 가속도

    [Header("MODE")] private bool _isAiMode = false;
    private float idleTime;
    private const int maxIdleTime = 10;
    private bool _autoMode;
    private bool IsFarming => _skillTarget.Count == 1 && _skillTarget[0]._skillTarget as BaseGatherResUnit;
    private int _equipRelicId;
    private GameObject _relicEffect;
    
    public void SetAutoMode(bool autoMode)
    {
        _autoMode = autoMode;
    }

    void Start()
    {
        Managers.Instance.GetJoystick().OnJoystickTypeChange += OnHandleJoystickTypeChange;
        gameObject.layer = LayerMask.NameToLayer("Player");
        _flashEffect.flashColor = Utils.HexToColor("BE0000");
        maxResurrectionTime = Managers.Instance.GetSimpleDBManager().GetFieldConfigInt("ResurrectionTimeLimit");
        _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void OnHandleJoystickTypeChange(EJoystickState _joystickState)
    {
        joystickState = _joystickState;
        if (IsMoveBlocked || _isAiMode || _state.GetCurState() == State_Skill.Instance || _state.GetCurState() == State_SpecialSkill.Instance)
            return;
        
        switch (joystickState)
        {
            case EJoystickState.PointDown:
                Reset();
                break;
            case EJoystickState.Dragging:
                if (!IsMoving)
                    _state.ChangeState(State_Move.Instance);
                break;
            case EJoystickState.PointUp:
                _state.ChangeState(State_Idle.Instance);
                break;
        }
    }

    public override async UniTask SetUnitId(UserInfoData userInfoData, int id)
    {
        _unitDataTable = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, id);
        _itemData = Managers.Instance.UserInfo().GetCharacterItemData(id);
        _itemData.isOpen = true;
        _statusInfo = _itemData._statusInfo;
        _playerStatusInfo = (PlayerStatusInfo)_statusInfo;
        UnitType = _unitDataTable.UnitType;

        await base.SetUnitId(userInfoData, id);
    }

    public void SetSynergyStatus(Status status)
    {
        _playerStatusInfo.SetSynergyStatus(status);
    }

    public override void Init()
    {
        // Status Set
        _playerStatusInfo.OnCalculateStatus();
        _playerStatusInfo.ApplyPlayHealth();

        if (_teamType == ETeamType.Player)
            _findMask = LayerMask.GetMask("Enemy", "Gather");
        else if (_teamType == ETeamType.Enemy)
            _findMask = LayerMask.GetMask("Player");

        if (resurrectionBar == null)
        {
            resurrectionTime = 0f;
            GameObject goResurrectionBar = Managers.Instance.GetResObjectManager()
                .Instantiate("Prefabs/UI/Common/UIResurrectionBar", gameObject.transform);
            goResurrectionBar.transform.localPosition = new Vector3(0, 2f, 0);
            resurrectionBar = goResurrectionBar.GetComponent<UIResurrectionBar>();
        }

        resurrectionBar.Init();
        resurrectionTime = 0;
        SetMass(false);
        Reset();
        RemoveAllEffect();
        
        _skillObserver.InitializeSkillObserver(this, _itemData.Skills[ESkillSlotType.DefaultSkill][0], _itemData.Skills[ESkillSlotType.ActiveSkill].ToArray(), _itemData.Skills[ESkillSlotType.SpecialSkill].ToArray());
        _passiveSkillObserver.InitializeSkillObserver(this);
        
        if (_itemData.equipRelicId != 0)
        {
            RelicBase relicBase = ClientLocalDB_Simple.GetData<RelicBase>(DBKey.RelicBase, _itemData.equipRelicId);
            _passiveSkillObserver.AddPassiveSkill(EPassiveSlotType.Relic, relicBase.PassiveSkill);
        }
        
        _passiveSkillObserver.Trigger(EPassiveSkillTriggerType.Always, null);

        RelicEffectCheck();
        base.Init();
    }

    // Drag시 초기화 시킬 변수들
    private void Reset()
    {
        _moveTarget = null;
        _skillTarget.Clear();
        _targets = null;
        LerpCellPosCompleted = true;
    }

    protected override void FindTarget()
    {
        if (_squad.IsTownZone.Value)
        {
            _moveTarget = null;
            return;
        }

        if (_autoMode)
            AutoModeFindTarget();
        else
            base.FindTarget();
    }

    private void AutoModeFindTarget()
    {
        SpawnPointInfoUnit spawnPointInfoUnit = _squad.FindSpawnPoint();
        if (spawnPointInfoUnit == null)
        {
            _moveTarget = null;
            return;
        }

        float bestSqrDist = float.MaxValue;
        BaseUnit target = null;
        foreach (var unit in spawnPointInfoUnit._unitList)
        {
            if (unit.IsDie)
                continue;

            float sqrDist = (CenterPosition - unit.CenterPosition).sqrMagnitude;

            if (sqrDist < bestSqrDist)
            {
                bestSqrDist = sqrDist;
                target = unit;
            }
        }

        _moveTarget = target;
    }

    public override void UpdateIdle()
    {
        if (_isAiMode)
        {
            if (idleTime < 0)
                MoveRandomPosition();
            else
                idleTime -= Time.deltaTime;
            return;
        }

        if (joystickState == EJoystickState.Dragging)
        {
            _state.ChangeState(State_Move.Instance);
            return;
        }

        base.UpdateIdle();
    }

    public override void UpdateMove()
    {
        base.UpdateMove();

        if (_isAiMode || !LerpCellPosCompleted)
        {
            LerpToPosition();
            return;
        }
        
        if (_characterPositionType >= ECharacterPositionType.LimitOut)
        {
            if (LerpCellPosCompleted)
                ReturnMove();
            return;
        }

        if (joystickState == EJoystickState.Dragging)
        {
            _agent.Stop();
            Move((_squad.moveDir + _accVec).normalized, _accSpeed);
        }
        else
        {
            FindTarget();
            if (_moveTarget == null)
            {
                _state.ChangeState(State_Idle.Instance);
                return;
            }

            if (MoveToTargetUnit())
                AttackAction();
        }
    }

    private void Move(Vector2 dir, float speedMultiplier = 1f)
    {
        Vector2 vDir = _rigidbody.position + (dir * ((_moveSpeed + _adMoveSpeed) * speedMultiplier * Time.fixedDeltaTime));
        _rigidbody.MovePosition(vDir);

        LookAtDir(dir);
    }

    protected override void LerpToPosition()
    {
        if (LerpCellPosCompleted)
            return;

        if (!_isAiMode)
        {
            Vector2 newPos = _characterPositionType == ECharacterPositionType.LimitOut
                ? (Vector2)_squad.transform.position
                : (Vector2)GetSquadRandomPosition();

            // 슬롯이 크게 이동했을 때만 목적지 갱신 (매 프레임 갱신 시 왔다갔다 현상 방지)
            if (((Vector2)_movePos - newPos).sqrMagnitude > 0.5f * 0.5f)
                _movePos = newPos;
        }
        
        MoveToTargetNav(_movePos, _isAiMode);
        
        // 슬롯 근접 OR BasicIn 진입 시 복귀 완료 — BasicIn 경계 진동 방지
        if (_agent.remainingDistance < 0.5f || _characterPositionType == ECharacterPositionType.BasicIn)
        {
            LerpCellPosCompleted = true;
            _state.ChangeState(State_Idle.Instance);
        }
    }

    protected override void AttackAction()
    {
        if (IsFarming)
        {
            _state.ChangeState(State_Farming.Instance);
            return;
        }
        
        base.AttackAction();
    }

    protected override void OnDie(BaseUnit attacker)
    {
        base.OnDie(attacker);
        _squad.GameOverCheck();
        if (isMainCharacter)
            _squad.MainCharacterCheck();
    }

    public override void UpdateDie()
    {
        if (!_enableResurrection)
            return;

        if (resurrectionTime < maxResurrectionTime)
        {
            resurrectionTime += Time.fixedDeltaTime;
            resurrectionBar.UpdateUI(resurrectionTime / maxResurrectionTime);
        }
        else
        {
            Resurrection();
        }
    }

    public override void Resurrection()
    {
        Init();

        if (_squad.mainCharacter == null)
            _squad.MainCharacterCheck();

        if (_characterPositionType >= ECharacterPositionType.LimitOut)
        {
            _rigidbody.position = GetSquadRandomPosition();
            SetZoneIndex(_squad._zoneIndex);
            SetPosition(_rigidbody.position);
        }
    }

    private Vector3 GetSquadRandomPosition()
    {
        // int ran = Random.Range(0, _squad._offsets.Count - 1);
        int index = _squad._playerUnits.IndexOf(this);
        return _squad._offsets[index].GetWorldPosition();
    }

    protected override void SetUnitPositionType()
    {
        if (_squad == null)
        {
            _characterPositionType = ECharacterPositionType.None;
            _accSpeed = 1f;
            return;
        }

        float basicDistRadius = _squad.BasicDistance * _squad.BasicDistance;
        float limitDistRadius = _squad.LimitDistance * _squad.LimitDistance;
        float loseDistRadius = _squad.ResurrectionLimitDistance * _squad.ResurrectionLimitDistance;
        Vector2 gap = _squad.circleRigidbody.position - _rigidbody.position;

        float sqrMagnitude = gap.sqrMagnitude;
        ECharacterPositionType temp = _characterPositionType;
        if (sqrMagnitude < basicDistRadius)
        {
            _characterPositionType = ECharacterPositionType.BasicIn;
            _accSpeed = 1f;
        }
        else if (sqrMagnitude < limitDistRadius)
        {
            _characterPositionType = ECharacterPositionType.LimitIn;
            _accSpeed = 1.2f;
        }
        else if (sqrMagnitude < loseDistRadius)
        {
            _characterPositionType = ECharacterPositionType.LimitOut;
            _accSpeed = 1.5f;
        }
        else
        {
            _characterPositionType = ECharacterPositionType.LoseRange;
            _accSpeed = 3f;
        }

        // 스쿼드 중심이 아닌 각자의 편대 슬롯 위치로 당김 → 간격 유지하며 뭉침
        Vector2 slotPos = GetSquadRandomPosition();
        Vector2 slotGap = slotPos - _rigidbody.position;

        if (_squad.moveDir == Vector2.zero)
        {
            _accVec = slotGap * (_accSpeed * 0.1f);
            return;
        }

        Vector2 perpDir = new Vector2(-_squad.moveDir.y, _squad.moveDir.x);
        float forwardDot = Vector2.Dot(slotGap, _squad.moveDir);
        float lateralDot = Vector2.Dot(slotGap, perpDir);

        // 이동 방향 기준 앞에 있는 유닛(forwardDot < 0)은 속도를 기본으로 제한
        if (forwardDot < 0f)
            _accSpeed = 1f;

        _accVec = _squad.moveDir * forwardDot * 0.05f
                + perpDir * lateralDot * 0.4f;
    }

    public override void SetMass(bool isLock)
    {
        if (isLock)
        {
            _rigidbody.mass = 10000;
        }
        else
        {
            _rigidbody.mass = isMainCharacter ? 500 : 100;
        }
    }

    private void RelicEffectCheck()
    {
        if (_itemData.equipRelicId == 0)
        {
            if (_relicEffect != null)
            {
                EasyObjectPool.instance.ReturnObjectToPool(_relicEffect);
                _relicEffect = null;
            }
            _equipRelicId = 0;
            return;
        }

        if (_equipRelicId == _itemData.equipRelicId)
            return;

        if (_relicEffect != null)
        {
            EasyObjectPool.instance.ReturnObjectToPool(_relicEffect);
            _relicEffect = null;
        }

        _equipRelicId = _itemData.equipRelicId;
        RelicBase relicBase = ClientLocalDB_Simple.GetData<RelicBase>(DBKey.RelicBase, _itemData.equipRelicId);
        ActiveRelicEffect(relicBase.RelicEffect).Forget();
    }

    private async UniTask ActiveRelicEffect(string effectName)
    {
        if (effectName.IsNull())
            return;

        SentenceEffect effect = await SentenceManager.ActiveEffect(EPoolType.CommonEffect, effectName);
        effect.transform.parent = transform;
        effect.transform.localPosition = Vector3.zero;
        _relicEffect = effect.gameObject;
    }

    #region AiMode

    public void TownIn(int zoneIndex)
    {
        Init();
        SetZoneIndex(zoneIndex);
        
        if (_characterPositionType >= ECharacterPositionType.LimitOut)
        {
            _rigidbody.position = GetSquadRandomPosition(); 
            SetPosition(_rigidbody.position);
        }
        
        if (!isMainCharacter)
            StartAiMode();
    }

    public void TownOut(int zoneIndex)
    {
        SetZoneIndex(zoneIndex);
        if (!isMainCharacter)
            StopAiMode();
    }

    public void StartAiMode()
    {
        OnHandleJoystickTypeChange(EJoystickState.PointUp);
        _isAiMode = true;
        _agent.repath = true; // AI 모드에서는 경로 막힘 시 자동 재탐색 필요
        _squad = null;
        _collider.enabled = false;
        Reset();
        MoveRandomPosition();
        idleTime = -1f; // MoveRandomPosition 실패(map 미준비 등) 시에도 즉시 재시도
    }

    public void StopAiMode()
    {
        if (!_isAiMode)
            return;

        Reset();
        _isAiMode = false;
        _agent.repath = false; // 전투 모드 복귀 — 프레임 주기 관리로 전환
        _rigidbody.velocity = Vector2.zero;
        _agent.Stop();
        _collider.enabled = true;
        _squad = Managers.Instance.GetObjectUnitManager().playerSquad;
    }

    public override void BattleStart()
    {
        if (_isAiMode)
            return;

        base.BattleStart();
    }

    // 이동 후 머무는 시간 랜덤하게 설정
    private void ResetTime()
    {
        int time = Random.Range(3, maxIdleTime);
        idleTime = time;
    }

    // NavMap상의 랜덤한 위치로 이동
    private void MoveRandomPosition()
    {
        ResetTime();
        Vector2 position = _agent.map.GetRandomPointInside();
        if (position == Vector2.zero)
            return;

        _movePos = position;
        _rigidbody.velocity = Vector2.zero;
        _agent.Stop();
        LerpCellPosCompleted = false;
        _state.ChangeState(State_Move.Instance);
    }

    protected override void UpdatePolyNavMap()
    {
        if (_isAiMode) //ai 모드에 돌입 하면 체크 안함
            return;

        base.UpdatePolyNavMap();
    }

    #endregion

    private void OnDestroy()
    {
        Managers.Instance.GetJoystick().OnJoystickTypeChange -= OnHandleJoystickTypeChange;
    }
}