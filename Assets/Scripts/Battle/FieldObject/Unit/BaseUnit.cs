using Cysharp.Threading.Tasks;
using MarchingBytes;
using PolyNav;
using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using Animation = Spine.Animation;

public class SkillTargetData
{
    public BaseUnit _skillTarget;
    public Vector2 _targetPosition;
    public SpellIndicator _spellIndicator;
}

public class BaseUnit : FieldObject
{
    [Header("GIZMO OPTION")] public bool MoveTargetSearchRange = false;
    public bool MoveTargetLine = false;
    public bool SkillTargetSearchRange = false;
    public bool SkillTargetLine = false;
    public bool SkillTargetAreaRange = false;

    public UserInfoData UserInfoData;

    public UIHpBar _hpUI = null;
    [SerializeField] private float _hpPoint;
    protected HurtFlashEffect _flashEffect;

    public Dictionary<float, GameObject> _targets = new();
    public BaseUnit _moveTarget;
    public List<SkillTargetData> _skillTarget = new List<SkillTargetData>();
    public StateMachine<BaseUnit> _state = null;
    public Dictionary<int, List<DurationEffectBase>> _durationEffectDic = new();
    public Dictionary<int, GroundBuff> _groundEffectDic = new();
    public SkillObserver _skillObserver = new SkillObserver();
    public PassiveSkillObserver _passiveSkillObserver = new PassiveSkillObserver();
    public BaseSkill _activeSkill;

    [Header("resurrect")] protected Vector2 _startPos;
    protected float resurrectionTime;
    protected float maxResurrectionTime;

    public ETeamType _teamType = ETeamType.None;
    public EUnitType UnitType = EUnitType.None;
    protected LayerMask _findMask; //검색해야하는 Layer

    #region Status

    public int _unitId;
    public UnitData _unitDataTable;
    protected StatusInfo _statusInfo = null;
    public bool IsStatusInfoReady => _statusInfo != null;
    public PlayStatus _playStatus => _statusInfo._playStatus;
    public UnitCombatStats _unitCombatStats; // 전투 정보 수집
    protected float _moveSpeed = 5;

    protected float _adMoveSpeed
    {
        get
        {
            if (_teamType == ETeamType.Enemy || !Managers.Instance.UserInfo().EnableAdBuff)
                return 0;

            return (float)int.Parse(ClientLocalDB_Simple.GetData<FieldConfig>(DBKey.FieldConfig, "MoveSpeed_Ad").Value) / 100;
        }
    }
    protected float _accSpeed = 1; // PolyNavi 가속도
    protected float _unitRadius = 0.5f;

    #endregion

    #region 한정적으로 사용될 변수들

    public bool IsInvincible = false; // 무적 - 랭킹 보스일때 사용
    public bool IsImmortal  = false; // 무적 - 체력이 1인 상태에서 죽지 않음
    public bool IsStatusImmune = false; // 상태이상 무시 (보스 들)
    public bool IsDamageImmune; // 특정 데미지만 무시

    #endregion

    #region KnockBack

    public bool IsKnockBack = false;
    public Vector2 knockbackForce = Vector2.zero;

    #endregion

    [SerializeField] protected ECharacterPositionType _characterPositionType;
    public bool IsDie => _state.GetCurState() == State_Die.Instance;

    public bool IsBattle => (_moveTarget != null && !(_moveTarget as BaseGatherResUnit) && _skillTarget.Count > 0) || Managers.Instance.GetJoystick().JoystickState != EJoystickState.PointUp;
    protected bool IsMoving => _state.GetCurState() == State_Move.Instance;
    protected bool IsMoveBlocked => IsDie || GetStatusEffect(EStatusEffectType.Rooted) || GetStatusEffect(EStatusEffectType.Stun) || GetStatusEffect(EStatusEffectType.Frozen); // 움직임이 막힌 상태

    #region StatusEffect
    private int StatusCount => Enum.GetValues(typeof(EStatusEffectType)).Length;
    public bool[] _statusEffects;
    public bool[] _statusImmuneEffects;
    public bool[] _passiveImmuneEffects;
    #endregion
    
    protected MapManager MapManager => Managers.Instance.GetMapManager();
    protected int _zoneIndex;
    public bool LerpCellPosCompleted;

    protected Vector2 _movePos;

    [Header("PolyNav")] public PolyNavAgent _agent;

    protected DropItemData _dropItemData = new DropItemData();
    Bone _firePointBone = null;

    public override void Awake()
    {
        base.Awake();
        _characterPositionType = ECharacterPositionType.BasicIn;
        _flashEffect = Utils.GetOrAddComponent<HurtFlashEffect>(gameObject);
        _collider.offset = new Vector2(0, 0.5f);
        
        _statusEffects = new bool[StatusCount];
        _statusImmuneEffects = new bool[StatusCount];
        _passiveImmuneEffects = new bool[StatusCount];
    }

    public virtual async UniTask SetUnitId(UserInfoData userInfoData, int id)
    {
        UserInfoData = userInfoData;
        _unitId = id;

        _unitCombatStats = new UnitCombatStats(id);
        await SetSpine();
    }

    protected virtual async UniTask SetSpine()
    {
        string path = $"{GetSpineResourceName()}/{GetSpineResourceName()}_SkeletonData.asset";
        SkeletonDataAsset skeletonData = await Managers.Instance.GetResObjectManager().LoadAsync<SkeletonDataAsset>(path);
        if (skeletonData == null)
        {
            MyLogger.Log("Not Find Spine : " + path);
            return;
        }

        _spineAnimation.SetSpine(skeletonData);
        _spineAnimation.SetSortingOrder(SortingLayers.UNIT);
        
        _firePointBone = _spineAnimation.FindBone("FirePoint");
    }

    public virtual void Init()
    {
        if (_state == null)
            _state = new StateMachine<BaseUnit>();
        _state.Init(this, State_Idle.Instance);
        _unitCombatStats?.ResetStats();
        
        _moveSpeed = (float)_playStatus.MoveSpeed / 100;
        
        _agent = gameObject.GetOrAddComponent<PolyNavAgent>();
        _agent.maxForce = 30;
        _agent.repath = false; // 전투 중 매 프레임 내부 A* 재탐색 비활성화 — MoveToTargetNav의 프레임 주기로 직접 관리 (AI 모드 시 PlayerUnit에서 별도 활성화)
        _unitRadius = _statusInfo._statusData.Radius * 0.01f;

        // --------------------------------------------------------------

        // hpBar 재활용 문제
        if (_hpUI != null)
        {
            Managers.Instance.GetResObjectManager().Destroy(_hpUI.gameObject);
            _hpUI = null;
        }
        
        if (!_unitDataTable.HpBar.IsNull())
        {
            GameObject goHpBar = Managers.Instance.GetResObjectManager()
                .Instantiate($"Prefabs/UI/Common/{_unitDataTable.HpBar}", gameObject.transform);
            float hpBarHeight = _unitDataTable.HpBarY * 0.01f;               
            goHpBar.transform.localPosition = new Vector3(0, hpBarHeight, 0);
            _hpUI = goHpBar.GetComponent<UIHpBar>();                
        }

        if (_hpUI != null)
        {
            _hpUI.Init();
            _hpUI.InitData(this);
            _hpUI.gameObject.SetActive(false);
        }

        _rigidbody.simulated = true;
        _collider.enabled = true;
        _collider.isTrigger = false;
        LerpCellPosCompleted = true;
    }

    public void SetPosition(Vector2 pos)
    {
        _unitPos = pos;
        LerpCellPosCompleted = true;
    }

    public void SetMovePosition(Vector2 pos)
    {
        _movePos = pos;
    }

    protected virtual void LerpToPosition()
    {
    }

    private void AgentDirection()
    {
        _rigidbody.velocity = Vector2.zero;
        Vector2 dir = _agent.nextPoint - _rigidbody.position;
        LookAtDir(dir);
    }

    /// <summary>
    /// using agent
    /// </summary>
    /// <param name="targetVec"></param>
    /// <param name="speed"></param>
    private float _lastNavMaxSpeed = -1f;
    private const float NavDestinationThreshold = 0.15f;
    private const int NavDestinationUpdateInterval = 10;
    private int _navFrameCounter = 0;

    protected void MoveToTargetNav(Vector3 targetVec, bool immediate = false)
    {
        float maxSpeed = (_moveSpeed + _adMoveSpeed) * _accSpeed;
        if (_lastNavMaxSpeed != maxSpeed)
        {
            _agent.maxSpeed = maxSpeed;
            _lastNavMaxSpeed = maxSpeed;
        }

        Vector2 dest = targetVec;

        if (immediate)
        {
            _agent.SetDestination(dest);
        }
        else
        {
            _navFrameCounter++;
            if (_navFrameCounter >= NavDestinationUpdateInterval)
            {
                _navFrameCounter = 0;
                _agent.SetDestination(dest);
            }
        }

        AgentDirection();
    }

    public void MoveStop()
    {
        if (_agent != null)
            _agent.Stop();

        if (IsKnockBack)
            return;

        _rigidbody.velocity = Vector2.zero;
    }

    #region Attack

    private float GetNavPathDistance(Vector2 from, Vector2 to)
    {
        if (_agent == null || _agent.map == null)
            return (to - from).magnitude;

        // NavMap 밖 좌표는 GetCloserEdgePoint → InflatePolygon Assert 유발 → 직선 거리 fallback
        if (!_agent.map.PointIsValid(from) || !_agent.map.PointIsValid(to))
            return (to - from).magnitude;

        // LOS가 있으면 직선 거리 = 실제 경로 거리 → A* 생략
        if (_agent.map.CheckLOS(from, to))
            return (to - from).magnitude;

        float pathDist = float.MaxValue;
        _agent.map.FindPath(from, to, path =>
        {
            if (path == null || path.Length < 2)
            {
                pathDist = float.MaxValue;
                return;
            }
            pathDist = 0f;
            for (int i = 0; i < path.Length - 1; i++)
                pathDist += Vector2.Distance(path[i], path[i + 1]);
        });
        return pathDist;
    }


    protected BaseUnit GetSearchUnit(float range, LayerMask layerMask)
    {
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(layerMask);
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = true;
        List<Collider2D> colliders = new List<Collider2D>();
        Physics2D.OverlapCircle(CenterPosition, range, contactFilter, colliders);

        if (colliders.Count == 0)
            return null;

        // 1차: sqrMagnitude로 빠르게 정렬 후 상위 N개만 A* 경로 거리 계산
        float rangeSqr = range * range;
        List<(BaseUnit unit, float sqrDist)> candidates = new List<(BaseUnit, float)>();
        for (int i = 0; i < colliders.Count; i++)
        {
            if (colliders[i] == null)
                continue;

            BaseUnit fieldObject = colliders[i].GetComponent<BaseUnit>();
            if (fieldObject == null || !IsTargetOnSameNavMap(fieldObject))
                continue;

            float sqrDist = ((Vector2)fieldObject._rigidbody.position - _rigidbody.position).sqrMagnitude;
            if (sqrDist > rangeSqr)
                continue;

            candidates.Add((fieldObject, sqrDist));
        }

        if (candidates.Count == 0)
            return null;

        // sqrMagnitude 기준 오름차순 정렬 — 타겟 선택은 직선 거리로 충분, A* 불필요
        candidates.Sort((a, b) => a.sqrDist.CompareTo(b.sqrDist));

        float dist = float.MaxValue;
        Dictionary<ETeamType, BaseUnit> objectDic = new Dictionary<ETeamType, BaseUnit>();
        for (int i = 0; i < candidates.Count; i++)
        {
            BaseUnit fieldObject = candidates[i].unit;
            float sqrDist = candidates[i].sqrDist;

            ETeamType teamType = fieldObject._teamType != ETeamType.Gather ? ETeamType.Enemy : ETeamType.Gather;

            objectDic.TryAdd(teamType, null);
            if (objectDic[teamType] == null || sqrDist < dist)
            {
                dist = sqrDist;
                objectDic[teamType] = fieldObject;
            }
        }

        BaseUnit targetObject = null;
        // 유닛과 채집물중 유닛을 우선 리턴 한다.
        if (objectDic.Count > 0)
            targetObject = objectDic.TryGetValue(ETeamType.Enemy, out BaseUnit value)
                ? value
                : objectDic[ETeamType.Gather];

        return targetObject;
    }

    protected bool MoveToTargetUnit()
    {
        _activeSkill = _skillObserver.GetEnableSkill();
        AttackRangeCheck();

        if (_skillTarget.Count > 0)
        {
            MoveStop();
            return true;
        }

        MoveToTargetNav(_moveTarget._rigidbody.position);
        return false;
    }

    private void AttackRangeCheck()
    {
        if(_moveTarget == null) return;
            
        if (_moveTarget as BaseGatherResUnit)
        {
            _activeSkill = null;
            _skillTarget = SentenceManager.SkillRangeTargetSearch(_moveTarget, this, 1f);
        }
        else
        {
            SkillDetailData skillDetailData = _activeSkill.GetMainDetailData();
            List<SkillTargetData> targets = SentenceManager.SkillRangeTargetSearch(this, skillDetailData, skillDetailData.HitLimit);

            // Self/Ally 타입은 자기 주변에서 즉시 찾지만, MoveTarget이 사거리 밖이면 이동 우선
            if (skillDetailData.TargetType1 == ESkillTargetType.Self ||
                skillDetailData.TargetType1 == ESkillTargetType.Ally ||
                skillDetailData.TargetType1 == ESkillTargetType.AllyAll)
            {
                var moveTargetCheck = SentenceManager.SkillRangeTargetSearch(_moveTarget, this, skillDetailData.RangeSize * 0.01f);
                if (moveTargetCheck.Count == 0)
                    targets = new List<SkillTargetData>();
            }

            _skillTarget = targets;
        }
    }

    private int _findTargetFrameCounter = 0;
    private const int FindTargetUpdateInterval = 5;

    protected virtual void FindTarget()
    {
        // 현재 타겟이 살아있으면 재탐색 주기까지 대기
        if (_moveTarget != null && !_moveTarget.IsDie)
        {
            _findTargetFrameCounter++;
            if (_findTargetFrameCounter < FindTargetUpdateInterval)
                return;
        }

        _findTargetFrameCounter = 0;
        _moveTarget = GetSearchUnit((float)_playStatus.MoveTargetSearchRange * 0.01f, _findMask);
    }

    protected virtual bool IsTargetOnSameNavMap(BaseUnit target) => true;

    #endregion

    public void ChangeState(FSM_State<BaseUnit> state)
    {
        if (_state.GetCurState() != State_Die.Instance)
            _state.ChangeState(state);
    }

    public virtual void BattleUpdate()
    {
        if (!IsMoving)
            MoveStop();

        _skillObserver.SkillUpdate();
        _passiveSkillObserver.SkillUpdate();

        if (_state != null && isActiveAndEnabled)
            _state.Update();

        SetUnitPositionType();
        UpdateDurationEffect();
    }

    public void BattleStop()
    {
        if (_state.GetCurState() == State_Die.Instance)
            return;

        _collider.enabled = false;

        if (_state.GetCurState() != State_Idle.Instance)
        {
            _state.ChangeState(State_Idle.Instance);
            MoveStop();
        }
    }

    public virtual void BattleStart()
    {
        if (_state != null && _state.GetCurState() == State_Die.Instance)
            return;

        _collider.enabled = true;
    }

    protected virtual bool GetLoopByState(EUnitState state)
    {
        return state switch
        {
            EUnitState.Idle => true,
            EUnitState.Move => true,
            EUnitState.Die => true,
            _ => false
        };
    }

    public virtual void SetAnimation(EUnitState state)
    {
        bool loop = GetLoopByState(state);
        string animationName = GetAnimationName(state);

        Animation anim = _spineAnimation.FindAnimation(animationName);
        if (anim != null)
        {
            _spineAnimation.SetAnimation(animationName, loop);
            _state.SetAnimation(anim);
        }
    }

    protected virtual string GetAnimationName(EUnitState state)
    {
        string animationName = "";
        switch (state)
        {
            case EUnitState.Idle:
                animationName = UnitType == EUnitType.PlayerCharacter ? CharacterAnimationName.IDLE : ObjectAnimationName.IDLE;
                break;
            case EUnitState.Move:
                animationName = UnitType == EUnitType.PlayerCharacter ? CharacterAnimationName.MOVE : ObjectAnimationName.MOVE;
                break;
            case EUnitState.Attack:
                animationName = UnitType == EUnitType.PlayerCharacter ? CharacterAnimationName.ATTACK : ObjectAnimationName.ATTACK;
                break;
            case EUnitState.Skill:
                animationName = UnitType == EUnitType.PlayerCharacter ? CharacterAnimationName.ACTIVE_SKILL : $"{ObjectAnimationName.ACTIVE_SKILL}{_activeSkill._index + 1}";
                break;
            case EUnitState.SpecialSkill:
                animationName = UnitType == EUnitType.PlayerCharacter ? CharacterAnimationName.Special_SKILL : $"{ObjectAnimationName.Special_SKILL}{_activeSkill._index + 1}";
                break;
            case EUnitState.Die:
                animationName = UnitType == EUnitType.PlayerCharacter ? CharacterAnimationName.DIE : ObjectAnimationName.DIE;
                break;
            case EUnitState.Axe:
                animationName = CharacterAnimationName.AXE;
                break;
            case EUnitState.Mine:
                animationName = CharacterAnimationName.MINE;
                break;
            case EUnitState.Portal_Start:
                animationName = CharacterAnimationName.PORTAL_START;
                break;
            case EUnitState.Portal_End:
                animationName = CharacterAnimationName.PORTAL_END;
                break;
            case EUnitState.Hit:
                animationName = UnitType == EUnitType.PlayerCharacter ? "" : ObjectAnimationName.HIT;
                break;
        }

        return animationName;
    }

    public virtual void UpdateIdle()
    {
        FindTarget();

        if (_moveTarget != null)
        {
            _activeSkill = _skillObserver.GetEnableSkill();
            AttackRangeCheck();
            if (_skillTarget.Count > 0)
                AttackAction();
            else
            {
                if (GetStatusEffect(EStatusEffectType.Rooted)) // 속박 상태면 못움직임
                    return;

                _state.ChangeState(State_Move.Instance);
            }
        }
        else
        {
            if (_characterPositionType != ECharacterPositionType.BasicIn && LerpCellPosCompleted)
                ReturnMove();
        }
    }

    public virtual void UpdateMove()
    {
        UpdatePolyNavMap();
    }

    protected virtual void AttackAction()
    {
        PlayStartEffect().Forget();
        
        ESkillSlotType skillType = _activeSkill._skillData.SkillSlotType;
        if (skillType == ESkillSlotType.DefaultSkill)
            _state.ChangeState(State_Attack.Instance);
        else if (skillType == ESkillSlotType.ActiveSkill)
            _state.ChangeState(State_Skill.Instance);
        else if (skillType == ESkillSlotType.SpecialSkill)
            _state.ChangeState(State_SpecialSkill.Instance);
    }

    public void SetSkillDetailIndex(int index)
    {
        if (_activeSkill == null)
            return;

        _activeSkill.SetDetailIndex(index);
    }

    public async UniTask ActiveSkillPreview(float time)
    {
        if(_activeSkill == null)
            return;
        
        SkillDetailData detailData = _activeSkill.GetCurrentDetailData();
        if (detailData.Indicator)
        {
            // Spine Flip때문에 한 프레임 넘긴다.
            await UniTask.DelayFrame(2);
            foreach (var targetData in _skillTarget)
            {
                 SpellIndicator spellIndicator = SentenceManager.ActivePreviewType(targetData._targetPosition, this, detailData, time);
                 targetData._spellIndicator = spellIndicator;
            }
        }
    }

    public virtual void UpdateDie()
    {
        if (resurrectionTime < maxResurrectionTime)
        {
            resurrectionTime += Time.fixedDeltaTime;
        }
        else
        {
            Resurrection();
        }
    }

    protected virtual void ReturnMove()
    {
        if (IsMoveBlocked)
            return;

        _rigidbody.velocity = Vector2.zero;
        _agent.Stop();
        _navFrameCounter = NavDestinationUpdateInterval;
        LerpCellPosCompleted = false;
        _state.ChangeState(State_Move.Instance);
    }

    protected virtual void SetUnitPositionType()
    {
        
    }

    public void TargetHit()
    {
        foreach (var targetData in _skillTarget)
        {
            if (targetData._skillTarget as BaseGatherResUnit)
            {
                if (targetData._skillTarget.IsDie)
                    continue;

                targetData._skillTarget.OnDamage(1, DamageTextName.Gather, this);
            }
            else
                SentenceManager.ActiveSkillDetail(targetData, this, _activeSkill);
        }
    }

    #region DurationEffect

    public void AddDurationEffect(DurationEffectBase durationEffectBase)
    {
        if (EnableAddDurationEffect(durationEffectBase))
        {
            if (!_durationEffectDic.ContainsKey(durationEffectBase._id))
                _durationEffectDic.Add(durationEffectBase._id, new List<DurationEffectBase>());
            durationEffectBase.EnterState();
            _durationEffectDic[durationEffectBase._id].Add(durationEffectBase);
        }
    }

    private bool EnableAddDurationEffect(DurationEffectBase durationEffectBase)
    {
        if (durationEffectBase._statusType != EStatusEffectType.None)
        {
            EStatusEffectType statusEffectType = durationEffectBase._statusType;
            if(GetImmuneStatusEffect(statusEffectType) || GetPassiveImmuneStatusEffect(statusEffectType) || IsStatusImmune)
                return false;
        }
        
        if (_durationEffectDic.TryGetValue(durationEffectBase._id, out var buffList))
            return buffList.Count < durationEffectBase._overlapCount;

        return true;
    }

    public void RemoveDurationEffect(DurationEffectBase durationEffectBase)
    {
        if (_durationEffectDic.TryGetValue(durationEffectBase._id, out var buffList))
        {
            if (!buffList.Contains(durationEffectBase))
                return;

            buffList.Remove(durationEffectBase);
            durationEffectBase.ExitState();
        }
    }

    //전부 삭제 (죽었을때 및 캐릭터가 변경 되었을때)
    private void RemoveAllDurationEffect()
    {
        var keys = new List<int>(_durationEffectDic.Keys);
        foreach (var key in keys)
        {
            if (!_durationEffectDic.TryGetValue(key, out var list))
                continue;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                DurationEffectBase durationEffectBase = list[i];
                list.Remove(durationEffectBase);
                durationEffectBase.ExitState();
            }
        }
    }

    private void UpdateDurationEffect()
    {
        if (IsDie || _durationEffectDic.Count == 0)
            return;

        var keys = new List<int>(_durationEffectDic.Keys);
        foreach (var key in keys)
        {
            if (!_durationEffectDic.TryGetValue(key, out var list))
                continue;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (i >= list.Count) continue;
                DurationEffectBase durationEffectBase = list[i];
                if (durationEffectBase != null)
                    durationEffectBase.UpdateState();
            }
        }
    }

    //상태이상이 존재하는지 체크
    private bool IsStatusEffectActive(EStatusEffectType effectType)
    {
        foreach (var list in _durationEffectDic.Values)
        {
            foreach (var durationEffect in list)
            {
                if (durationEffect == null)
                    continue;
                
                if (durationEffect._statusType == effectType)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 상태이상 걸렸을시, 풀렸을시 변수 상태 변경
    /// </summary>
    /// <param name="effectType"></param>
    /// <param name="isActive"></param>
    public void ActiveStatus(EStatusEffectType effectType, bool isActive)
    {
        if (effectType == EStatusEffectType.None)
            return;

        _statusEffects[(int)effectType] = isActive;
    }

    public void DisableStatus(EStatusEffectType effectType)
    {
        bool existStatus = IsStatusEffectActive(effectType);
        if (existStatus) // 제거를 했는데도 같은 타입이 존재하면 상태를 바꾸지 않는다.
            return;

        ActiveStatus(effectType, false);
    }

    public bool GetStatusEffect(EStatusEffectType effectType)
    {
        return _statusEffects[(int)effectType];
    }
    
    public void ActiveImmuneStatus(EStatusEffectType effectType, bool isActive)
    {
        if (effectType == EStatusEffectType.None)
            return;

        _statusImmuneEffects[(int)effectType] = isActive;
    }
    
    public bool GetImmuneStatusEffect(EStatusEffectType effectType)
    {
        return _statusImmuneEffects[(int)effectType];
    }
    
    public void ActivePassiveImmuneStatus(EStatusEffectType effectType, bool isActive)
    {
        if (effectType == EStatusEffectType.None)
            return;

        _passiveImmuneEffects[(int)effectType] = isActive;
    }
    public bool GetPassiveImmuneStatusEffect(EStatusEffectType effectType)
    {
        return _passiveImmuneEffects[(int)effectType];
    }

    #endregion

    #region GroundEffect

    public void AddGroundEffect(GroundBuff groundEffectBase)
    {
        if (_groundEffectDic.ContainsKey(groundEffectBase._id))
            return;
        
        groundEffectBase.EnterState();
        _groundEffectDic.Add(groundEffectBase._id, groundEffectBase);
    }

    public void RemoveGroundEffect(int id)
    {
        if (!_groundEffectDic.ContainsKey(id))
            return;
        
        GroundBuff groundEffectBase = _groundEffectDic[id];
        _groundEffectDic.Remove(groundEffectBase._id);
        groundEffectBase.ExitState();
    }

    //전부 삭제 (죽었을때 및 캐릭터가 변경 되었을때)
    private void RemoveAllGroundEffect()
    {
        foreach (var effectBase in _groundEffectDic.Values)
        {
            effectBase.ExitState();
        }
        
        _groundEffectDic.Clear();
    }

    protected void RemoveAllEffect()
    {
        RemoveAllGroundEffect();
        RemoveAllDurationEffect();
        ResetSkillTargetData();
        _statusInfo?.ResetPlayStatus();
        _statusInfo?.ResetSumStatus();
        Array.Clear(_statusEffects, 0, _statusEffects.Length);
        Array.Clear(_statusImmuneEffects, 0, _statusEffects.Length);
        Array.Clear(_passiveImmuneEffects, 0, _statusEffects.Length);
    }
    
    #endregion

    private async UniTask PlayStartEffect()
    {
        if (_activeSkill == null)
            return;

        if (_activeSkill.GetMainDetailData().StartEffect.IsNull())
            return;

        if (_activeSkill.GetMainDetailData().StartEffectTarget == ESentenceEffectType.Target)
        {
            foreach (var skillTargetData in _skillTarget)
            {
                BaseUnit target = skillTargetData._skillTarget;
                SentenceEffect effect = await SentenceManager.ActiveEffect(EPoolType.StartEffect, _activeSkill.GetMainDetailData().StartEffect);
                effect.PlayAttached(target, _activeSkill.GetMainDetailData().StartEffectTarget, target.LeftDir);   
            }
        }
        else
        {
            SentenceEffect effect = await SentenceManager.ActiveEffect(EPoolType.StartEffect, _activeSkill.GetMainDetailData().StartEffect);
            effect.PlayAttached(this, _activeSkill.GetMainDetailData().StartEffectTarget, LeftDir);   
        }
        
    }

    public async UniTask PlayHitEffect(string effectName)
    {
        if (effectName.IsNull())
            return;

        if (!Managers.Instance.UserInfo()._isDamageOn)
            return;
        
        SentenceEffect effect = await SentenceManager.ActiveEffect(EPoolType.CommonEffect, effectName);
        effect.SetTransform(CenterPosition, Quaternion.identity);
        effect.PlayAtPosition(LeftDir);
    }

    public void RefreshPlayStatus()
    {
        _statusInfo.ResetPlayStatus();
    }

    public virtual void OnDamage(double damage, string damageTextName, BaseUnit attacker)
    {
        _flashEffect.Flash();

        //EE4E6C
        Managers.Instance.GetObjectUnitManager().ShowDamageText(CenterPosition, damage, damageTextName).Forget();

        if (IsInvincible)
            return;

        double hp = _playStatus.Hp - damage;
        if (hp < 0)
            hp = 0;

        if (IsImmortal)
            hp = Math.Max(hp, 1);
        
        _playStatus.Hp = hp;

        float ratio = _playStatus.HpRatio;

        if (_hpUI != null)
            _hpUI.UpdateUI(ratio);

        _passiveSkillObserver?.Trigger(EPassiveSkillTriggerType.HpPercent, this, ratio);
        if (_playStatus.Hp <= 0)
        {
            if (IsDie)
                return;

            OnDie(attacker);
            MyLogger.Log("Die");
        }
    }

    public virtual void OnHeal(double heal)
    {
        double hp = _playStatus.Hp + heal;
        if (hp > _playStatus.TotalMaxHp)
            hp = _playStatus.TotalMaxHp;
        else if (hp < 0)
            hp = 0;

        _playStatus.Hp = hp;

        //EE4E6C
        Managers.Instance.GetObjectUnitManager().ShowDamageText(CenterPosition, heal, DamageTextName.Heal).Forget();

        float ratio = _playStatus.HpRatio;

        if (_hpUI != null)
            _hpUI.UpdateUI(ratio);
    }

    protected virtual void OnDie(BaseUnit attacker)
    {
        if (UnitType != EUnitType.Gather)
        {
            attacker._unitCombatStats.AddKillCount(1);
            attacker._passiveSkillObserver.Trigger(EPassiveSkillTriggerType.KillEnemy, attacker);    
        }
        
        _state.ChangeState(State_Die.Instance);
        RemoveAllEffect();
        _collider.enabled = false;
        _rigidbody.simulated = false;
        _moveTarget = null;
        

        if (_agent != null)
            _agent.Stop();
    }

    public void LookAtTarget(Vector2 targetPosition)
    {
        Vector2 dir = targetPosition - _rigidbody.position;
        LookAtDir(dir);
    }

    protected void LookAtDir(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) < 0.2f)
            return;

        LeftDir = dir.x < 0;
    }

    public virtual void Resurrection()
    {
        
    }

    public void SetPush(Vector2 pushVector)
    {
        if (IsKnockBack)
            return;

        IsKnockBack = true;
        knockbackForce = pushVector;

        _state.ChangeState(State_KnockBack.Instance);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_unitDataTable == null)
            return;

        if (MoveTargetSearchRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(CenterPosition, (float)_playStatus.MoveTargetSearchRange / 100);
        }

        if (SkillTargetSearchRange)
        {
            if (_activeSkill == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(CenterPosition, (float)_activeSkill.GetMainDetailData().RangeSize * 0.01f);
        }

        if (SkillTargetAreaRange || SkillTargetLine)
        {
            if (_skillTarget.Count == 0)
                return;
            
            if (SkillTargetAreaRange)
            {
                Gizmos.color = Color.blue;
                int size = _activeSkill.GetMainDetailData().AreaSize[_activeSkill.GetMainDetailData().AreaSize.Length];
                foreach (var targetData in _skillTarget)
                {
                    Gizmos.DrawWireSphere(targetData._targetPosition, size * 0.01f);
                    
                    if (SkillTargetLine)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawLine(CenterPosition, targetData._targetPosition);
                    }
                }
            }
        }

        if (MoveTargetLine)
        {
            if (_moveTarget == null)
                return;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(CenterPosition, _moveTarget.CenterPosition);
        }
    }
#endif

    public virtual void SetMass(bool isLock)
    {
    }

    public void SetRadiusScaling(float scaling)
    {
        _collider.radius = _unitRadius * scaling;
    }

    public bool InBattleState()
    {
        return _moveTarget != null || _skillTarget.Count > 0;
    }

    public virtual string GetSpineResourceName()
    {
        return _unitDataTable.Resource;
    }

    public Vector2 GetFirePoint()
    {
        if (_firePointBone == null)
            return _rigidbody.position;

        return _firePointBone.GetWorldPosition(transform);
    }

    public bool IsSkillTargetValid()
    {
        return _skillTarget.Count > 0 && _skillTarget[0]._skillTarget.IsValid();
    }

    protected virtual void UpdatePolyNavMap()
    {
        int zoneIndex = MapManager.EnableEntrance(_rigidbody.position);

        if (MapManager.IsTownZone(zoneIndex)) // 마을은 Squad에서 세팅
            return;

        if (zoneIndex == -1 || _zoneIndex == zoneIndex)
            return;

        SetZoneIndex(zoneIndex);
    }

    public void SetZoneIndex(int zoneIndex)
    {
        _zoneIndex = zoneIndex;

        if (_agent)
            _agent.map = MapManager.GetPolyNavMap(_zoneIndex);
    }

    public void SetCoolDown(float time)
    {
        _skillObserver.ActiveCoolDown(time);
    }

    public void ResetSkillTargetData()
    {
        foreach (var skillTarget in _skillTarget)
        {
            if (skillTarget != null)
            {
                skillTarget._skillTarget = null;
                if(skillTarget._spellIndicator != null)
                    skillTarget._spellIndicator.Cancel();
            }
        }
        _skillTarget.Clear();
    }

    public StatusInfo GetStatusInfo()
    {
        return _statusInfo;
    }
}