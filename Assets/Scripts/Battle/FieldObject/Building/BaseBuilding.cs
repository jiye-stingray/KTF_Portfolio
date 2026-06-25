using Cysharp.Threading.Tasks;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public enum EBuildingState
{
    Idle,
    Action,
    Clear
}

public class BuildingData
{
    public bool isOpen => _isBuild.Value && isCondition;
    public ReactiveProperty<bool> _isBuild = new ReactiveProperty<bool>();
    public int _level;
    public ReactiveProperty<int> _currencyCount = new ReactiveProperty<int>();
    public DateTime _syncTime;
    public int _storageTime;
    public bool _isOpening = false; // 연출 대기

    public bool isCondition
    {
        get
        {
            UserInfoData userInfoData = Managers.Instance.UserInfo();
            if (_data.BuildOpenConditionType == EBuildingOpenConditionType.UserLevel)
                return userInfoData.userLevel.Value >= _data.BuildOpenConditionValue;
            if (_data.BuildOpenConditionType == EBuildingOpenConditionType.BuildingOpen)
                return userInfoData.GetInstallationBuilding(_data.BuildOpenConditionValue)._isBuild.Value;
            if (_data.BuildOpenConditionType == EBuildingOpenConditionType.GuideQuestClearID 
                || _data.BuildOpenConditionType == EBuildingOpenConditionType.DungeonQuestClearID)
                return  userInfoData._currentGuideQuestId.Value > _data.BuildOpenConditionValue;
            if (_data.BuildOpenConditionType == EBuildingOpenConditionType.Dialogue)
                return userInfoData.dialogKey.Value >= _data.BuildOpenConditionValue;
                    
            return true;
        }
    }
    public BuildingInfo _data;

    public void UpdateSyncTime(DateTime syncTime)
    {
        _syncTime = syncTime;
        // TimeSpan remain = ServerTime.Instance.CurrentTime() - _syncTime;
        // BuildingLevelInfo buildingLevelInfo = ClientLocalDB_Simple.GetData<BuildingLevelInfo>(DBKey.BuildingLevelInfo, $"{_data.ID}_{_level}");
        // int productTime = buildingLevelInfo.BuildingProductionTime / 100;
        // if (remain.TotalSeconds < productTime)
        //     return;
        //
        // int tick = (int)remain.TotalSeconds / productTime;
        // int addCount = tick * buildingLevelInfo.CountList[0];
        // _currencyCount = Mathf.Min(buildingLevelInfo.BuildingProductionItemCountMax, _currencyCount + addCount);
    }
    
    public void UpdateSyncTime()
    {
        TimeSpan remain = ServerTime.Instance.CurrentTime() - _syncTime;
        BuildingLevelInfo buildingLevelInfo = ClientLocalDB_Simple.GetData<BuildingLevelInfo>(DBKey.BuildingLevelInfo, $"{_data.ID}_{_level}");
        int productTime = buildingLevelInfo.BuildingProductionTime / 100;
        if (remain.TotalSeconds < productTime)
            return;
        
        int tick = (int)remain.TotalSeconds / productTime;
        int addCount = tick * buildingLevelInfo.BuildingProductionItemCount;
        _currencyCount.Value = Mathf.Min(buildingLevelInfo.BuildingProductionItemCountMax, _currencyCount.Value + addCount);

        _syncTime = ServerTime.Instance.CurrentTime();
    }
}

public class BaseBuilding : FieldObject
{
    public BuildingData BuildingData => UserInfoData.GetInstallationBuilding(_idx);

    public int _idx;
    protected UISpeechBox _SpeechBox;
    protected int _zoneIndex;
    protected EBuildingState _buildingState;
    protected EBuildingType BuildingType => BuildingData._data.BuildingType;
    protected Squad Squad => Managers.Instance.GetObjectUnitManager().playerSquad;
    protected MapManager MapManager => Managers.Instance.GetMapManager();
    protected UserInfoData UserInfoData => Managers.Instance.UserInfo();
    protected UIManager UIManager => Managers.Instance.GetUIManager();

    public int _enemyCount;
    protected bool _enterSquad;

    public override void Awake()
    {
        base.Awake();
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        _rigidbody.freezeRotation = true;
        _collider.radius = 8.0f;
        gameObject.transform.SetParent(Managers.Instance.GetObjectUnitManager().BuildingTrans);
    }

    public virtual async UniTask Init(int idx)
    {
        _idx = idx;
        _collider.isTrigger = true;

        // Spine SkeletonAnimation은 SpriteRenderer 를 사용하지 않고 MeshRenderer을 사용함.
        // 그렇기떄문에 2D Sort Axis가 안먹히게 되는데 SortingGroup을 SpriteRenderer, MeshRenderer을같이 계산함.
        SortingGroup tSort = Utils.GetOrAddComponent<SortingGroup>(gameObject);
        tSort.sortingOrder = SortingLayers.UNIT;

        await SetSpine();
    }

    protected virtual async UniTask SetSpine()
    {
        string spineResource = BuildingData._data.Resource;
        SkeletonDataAsset skeletonData = await Managers.Instance.GetResObjectManager().LoadAsync<SkeletonDataAsset>(
                $"{spineResource}/{spineResource}_SkeletonData.asset");
        if (skeletonData == null)
            return;

        _spineAnimation.SetSpine(skeletonData);
    }

    public virtual void SuccessSpeechBtnClick()
    {
        EBuildingType buildingType = BuildingData._data.BuildingType;
        if (buildingType == EBuildingType.WarpPoint)
            UIManager.UIWarpPointPopup.OpenToStack();
        else if (buildingType == EBuildingType.Training)
            UIManager.TrainingUI.OpenToStack();
        else if (buildingType == EBuildingType.Statue)
            UIManager.UIEquipmentSetting.OpenToStack();
        else if (buildingType == EBuildingType.Relic)
            UIManager.UIRelic.OpenToStack();
        else if (buildingType == EBuildingType.Tavern)
            UIManager.OpenGachaUI(EGachaType.PickUp);
        else if (buildingType == EBuildingType.Craft)
        {
            UIManager.UICraft.Init();
            UIManager.UICraft.OpenToStack();
        }
        else if (buildingType == EBuildingType.Dungeon)
        {
            UserInfoData.zoneId = Squad._zoneIndex;
            UserInfoData.squadPosition = Squad.transform.position;
            
            BattleData battleData = BattleData.Create();
            battleData._contentType = EContent.Fog;
            battleData._factionType = EFactionType.None;
            battleData._index = BuildingData._data.CurrencyList[0];
            
            MyLogger.Log("Fog DungeonId : "+battleData._index);
            BattleData.Set(battleData);
            Managers.Instance.GetSyncCurrencyManager().Stop(); // 싱크매니저 타이머 멈춤
            Managers.Instance.GetTreasureBoxManager().Stop(); // 보물상자 타이머 멈춤
        
            Loading.Load(Loading.Dungeon);
        }
        else if (buildingType == EBuildingType.FieldDungeon)
        {
            UserInfoData.zoneId = Squad._zoneIndex;
            UserInfoData.squadPosition = Squad.transform.position;
            
            BattleData battleData = BattleData.Create();
            battleData._contentType = EContent.FieldDungeon;
            battleData._factionType = EFactionType.None;
            battleData._index = BuildingData._data.CurrencyList[0];
            
            BattleData.Set(battleData);
            Managers.Instance.GetSyncCurrencyManager().Stop(); // 싱크매니저 타이머 멈춤
            Managers.Instance.GetTreasureBoxManager().Stop(); // 보물상자 타이머 멈춤
        
            Loading.Load(Loading.Dungeon);
        }
    }

    public void SetZoneIndex(int zoneIndex)
    {
        _zoneIndex = zoneIndex;
    }

    public void Save()
    {
        UserInfoData.SaveBuilding();
    }

    public int GetZoneIndex()
    {
        return _zoneIndex;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (_SpeechBox == null)
            return;

        if (collision.tag.Equals("Squad"))
            _enterSquad = true;
        else if (collision.tag.Equals("Enemy"))
            _enemyCount++;
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (_SpeechBox == null)
            return;

        if (collision.tag.Equals("Squad"))
            _enterSquad = false;
        else if (collision.tag.Equals("Enemy"))
            _enemyCount--;
    }

    private void FixedUpdate()
    {
        if (_SpeechBox == null)
            return;

        if (!_enterSquad || _buildingState == EBuildingState.Action)
        {
            if(_SpeechBox.gameObject.activeSelf)
                _SpeechBox.Close();
        }
        else
        {
            _SpeechBox.Open();
            _SpeechBox.EnableButtonCheck(_enemyCount <= 0);
        }
    }

    private void OnDisable()
    {
        if (_SpeechBox == null)
            return;

        _SpeechBox.Close();
    }

    public virtual void ActiveBuilding()
    {
    }
}