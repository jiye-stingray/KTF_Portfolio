using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using static Define;

public class BaseGatherResUnit : EnemyUnit
{
    public GrObjectData _data;
    public EGatherType GatherType => _data.ObjectType;

    public override void Init()
    {
        if (_state == null)
            _state = new StateMachine<BaseUnit>();
        _state.Init(this, State_Idle.Instance);

        if (_statusInfo == null)
            _statusInfo = new GatherResUnitStatusInfo();
        
        GatherResUnitStatusInfo gatherResUnitStatusInfo = _statusInfo as GatherResUnitStatusInfo;
        gatherResUnitStatusInfo?.SetLevelStatus(_data.MaxHealthPoint);
        
        transform.tag = "Gather";
        _teamType = Define.ETeamType.Gather;
        _rigidbody.excludeLayers = LayerMask.GetMask("Player", "Enemy");
        
        _collider.enabled = true;
        _rigidbody.simulated = true;
        _collider.isTrigger = false;
    }

    public override async UniTask SetUnitId(UserInfoData userInfoData, int id)
    {
        _data = ClientLocalDB_Simple.GetData<GrObjectData>(DBKey.GatherObject, id);
        UnitType = EUnitType.Gather;
        
        UserInfoData = userInfoData;
        _unitId = id;
        await SetSpine();
    }

    public override void UpdateIdle()
    {
        
    }

    public override void UpdateMove()
    {
        
    }

    /// <summary>
    /// 삭제하면 X 부활 로직 체크하면 안됨
    /// </summary>
    public override void UpdateDie()
    {
    }

    public override string GetSpineResourceName()
    {
        return _data.Resource;
    }

    protected override void ApplyReward()
    {
        if (_dropItemData._rewardType == ERewardType.None)
            return;

        int type = (int)GatherType;
        for (int i = 0; i < _dropItemData._rewardID.Length; i++)
        {
            int id = _dropItemData._rewardID[i];
            int count = _dropItemData._count[i];
            ECurrency currencyType = (ECurrency)id;
            count = Math.Min(UserInfoData.CanCurrencyAcquireNumber(currencyType, count), count);
            
            Managers.Instance.GetSyncCurrencyManager().AddQuest(ESyncQuestType.Currency, count);
            UserInfoData.UpdateCorrectQuest(EQuestType.Guide, EQuestConditionType.GatherTarget, type, count);
            UserInfoData.UpdateCorrectQuest(EQuestType.Guide, EQuestConditionType.GatherGetAnyone, type, count);

            UserInfoData.UpdateCorrectQuest(EQuestType.Daily, EQuestConditionType.GatherTarget, type, count);
            UserInfoData.UpdateCorrectQuest(EQuestType.Daily, EQuestConditionType.GatherGetAnyone, type, count);
        
            UserInfoData.UpdateCorrectQuest(EQuestType.Weekly, EQuestConditionType.GatherTarget, type, count);
            UserInfoData.UpdateCorrectQuest(EQuestType.Weekly, EQuestConditionType.GatherGetAnyone, type, count);

            UserInfoData.UpdateCorrectQuest(EQuestType.Open, EQuestConditionType.GatherGetAnyone, type, count);
            
            UserInfoData.AddCurrencyAcquireNumber(currencyType, count);
            UserInfoData.AddCurrencyValue(currencyType, count);
            SyncCurrencyManager.AddCurrency(currencyType, count);
            SpawnObject(id, count);
        }
    }
}