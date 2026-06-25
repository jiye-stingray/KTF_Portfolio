using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using static Define;

public enum ESyncQuestType
{
    NormalMonster,
    EliteMonster,
    Currency
}

public class SyncCurrencyManager : MonoBehaviour
{
    [SerializeField] private int _syncInterval = 60;
    public float _time;
    private bool _isSyncing = false;

    private UserInfoData UserInfo => Managers.Instance.UserInfo();
    private BestHttp_GameManager BestHttp_GameManager => Managers.Instance.GetServerManager();
    private List<BuildingData> _buildingDatas;
    
    public Dictionary<ECurrency, int> _dicSyncCurrency = new ();
    private Dictionary<ECurrency, int> _dicTempSyncCurrency = new ();
    
    private Dictionary<ESyncQuestType, int> _dicSyncQuest = new ()
    {
        { ESyncQuestType.NormalMonster, 0 },
        { ESyncQuestType.EliteMonster, 0 },
        { ESyncQuestType.Currency, 0 }
    };
    
    private Dictionary<ESyncQuestType, int> _dicTempSyncQuest = new ()
    {
        { ESyncQuestType.NormalMonster, 0 },
        { ESyncQuestType.EliteMonster, 0 },
        { ESyncQuestType.Currency, 0 }
    };
    
    public void Init()
    {
        _syncInterval = Managers.Instance.GetSimpleDBManager().GetFieldConfigInt("SyncCurrencyTime");
        _time = _syncInterval;
        _buildingDatas = UserInfo._dicInstallationBuildingData.Values.ToList()
            .FindAll(building => building._data.BuildingType == EBuildingType.Storage);
        InitSyncCurrency();
        ResetSync();
        Stop();
    }

    private void InitSyncCurrency()
    {
        _dicSyncCurrency.Clear();
        _dicTempSyncCurrency.Clear();
        
        string[] currencyIds = Managers.Instance.GetSimpleDBManager().GetFieldConfigString("SyncCurrencyId").Split(',');
        foreach (var currencyId in currencyIds)
        {
            _dicSyncCurrency.Add((ECurrency)int.Parse(currencyId), 0);
            _dicTempSyncCurrency.Add((ECurrency)int.Parse(currencyId), 0);
        }        
    }

    public void StartTimer()
    {
        _isSyncing = true;
    }

    public void Stop()
    {
        _isSyncing = false;
    }
    
    void Update()
    {
        if (!_isSyncing)
            return;
        
        _time -= Time.deltaTime;
        if (_time <= 0)
            RequestSyncCurrency();
    }

    public void AddCurrency(ECurrency currencyType, int amount)
    {
        if (!_dicSyncCurrency.ContainsKey(currencyType))
            return;
        
        _dicSyncCurrency[currencyType] += amount;
    }
    
    public void AddQuest(ESyncQuestType questType, int amount = 1)
    {
        _dicSyncQuest[questType] += amount;
    }

    public void ResetSyncCurrency()
    {
        var currencyKeys = _dicSyncCurrency.Keys.ToList();
        foreach (var key in currencyKeys)
            _dicSyncCurrency[key] -= _dicTempSyncCurrency[key];
    }
    
    public void ResetSync()
    {
        // 1) 퀘스트: 서버에 보낸 값을 뺀다. (서버 통신 중에도 몬스터를 잡거나 재화를 얻을 수 있음.)
        var questKeys = _dicSyncQuest.Keys.ToList(); // 스냅샷
        foreach (var key in questKeys)
            _dicSyncQuest[key] -= _dicTempSyncQuest[key];
        
        // 2) 재화 : 서버에 보낸 값을 뺀다. (서버 통신 중에도 몬스터를 잡거나 재화를 얻을 수 있음.)
        var currencyKeys = _dicSyncCurrency.Keys.ToList(); // 스냅샷
        foreach (var key in currencyKeys)
            _dicSyncCurrency[key] -= _dicTempSyncCurrency[key];
    }

    public List<myCurrencyData> GetSyncCurrencyList()
    {
        List<myCurrencyData> currencyDatas = new List<myCurrencyData>();
        
        _dicTempSyncCurrency = new Dictionary<ECurrency, int>(_dicSyncCurrency);
        foreach (var syncCurrency in _dicTempSyncCurrency)
        {
            for (int i = 0; i < 2; i++)
            {
                myCurrencyData currencyData = new myCurrencyData();
                ECurrency currencyType = syncCurrency.Key;
                if (i == 0)
                {
                    currencyData.currencyId = (int)currencyType + 1000;
                    currencyData.currentCount = _dicTempSyncCurrency[currencyType];
                }
                else
                {
                    currencyData.currencyId = (int)currencyType;
                    currencyData.currentCount = UserInfo.GetCurrencyValue(currencyType);
                }
                currencyDatas.Add(currencyData);
            }
        }
        return currencyDatas;
    }
    
    private List<SyncQuestDto> GetSyncQuestList()
    {
        List<SyncQuestDto> questDatas = new List<SyncQuestDto>();
        
        _dicTempSyncQuest = new Dictionary<ESyncQuestType, int>(_dicSyncQuest);
        foreach (var quest in _dicTempSyncQuest)
        {
            if (quest.Value > 0)
            {
                SyncQuestDto syncQuest = new SyncQuestDto();
                syncQuest.type = (int)quest.Key;
                syncQuest.count = quest.Value;
                questDatas.Add(syncQuest);
            }
        }
        return questDatas;
    }
    
    private List<SyncBuildingDto> GetSyncBuildingList()
    {
        List<SyncBuildingDto> buildingDatas = new List<SyncBuildingDto>();
        
        foreach (var buildingData in _buildingDatas)
        {
            if(!buildingData.isOpen)
                continue;
            
            buildingData.UpdateSyncTime();
            SyncBuildingDto syncBuilding = new SyncBuildingDto();
            syncBuilding.buildingId = buildingData._data.ID;
            syncBuilding.currentCount = buildingData._currencyCount.Value;
            buildingDatas.Add(syncBuilding);
        }
        return buildingDatas;
    }
    
    public void RequestSyncCurrency()
    {
        _time = _syncInterval;
        
        SyncCurrencyDto syncCurrencyDto = new SyncCurrencyDto();
        syncCurrencyDto.syncCurrencyList = GetSyncCurrencyList();
        syncCurrencyDto.questList = GetSyncQuestList();
        syncCurrencyDto.syncBuildingCurrencyCountList = GetSyncBuildingList();
        
        if (syncCurrencyDto.syncCurrencyList.Count > 0 || syncCurrencyDto.questList.Count > 0 || syncCurrencyDto.syncBuildingCurrencyCountList.Count > 0)
        {
            BestHttp_GameManager.OnPostSyncCurrency(syncCurrencyDto);
        }
    }
}
