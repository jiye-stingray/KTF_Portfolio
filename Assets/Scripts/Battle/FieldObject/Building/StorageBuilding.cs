using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class StorageLevelData
{
    public int _idx;
    public int _level;
    public int _tickTime;
    public BuildingLevelInfo _data;

    public int CurrencyMaxCount => _data != null ? _data.BuildingProductionItemCountMax : 0;
    public int ProductionItemCount => _data != null ? _data.BuildingProductionItemCount : 0;
    public int ProductionItem => _data != null ? _data.BuildingProductionItem : 0;
}

public class StorageBuilding : InstallationBuilding
{
    private Define.EStorageState _state = Define.EStorageState.None;
    public StorageLevelData _storageLevelData = new StorageLevelData();

    [SerializeField] private float _getCurrencyRange = 5.0f;
    [SerializeField] private float _checkInterval = 0.2f;

    private Coroutine _coStorageUpdate;
    private Coroutine _coDetectInteraction;

    public override async UniTask Init(int idx)
    {
        await base.Init(idx);

        StopRunningCoroutines();

        _coStorageUpdate = StartCoroutine(CoUpdateStorage());
        _coDetectInteraction = StartCoroutine(CoDetectNearbyAndTransfer());
    }

    public void SetLevel(int level)
    {
        _storageLevelData._idx = _idx;
        _storageLevelData._level = level;
        _storageLevelData._data =
            ClientLocalDB_Simple.GetData<BuildingLevelInfo>(DBKey.BuildingLevelInfo, $"{_idx}_{level}");

        _storageLevelData._tickTime = _storageLevelData._data != null
            ? _storageLevelData._data.BuildingProductionTime / 100
            : 0;

        RefreshStorageByServerTime();

        (_SpeechBox as UIStorageSpeechBox)?.SetStorageLevelData(_storageLevelData);
    }

    public override void ChangeSpeechBox(int idx)
    {
        if (_SpeechBox != null)
            Destroy(_SpeechBox.gameObject);

        if (BuildingData.isOpen)
        {
            _SpeechBox = Managers.Instance.GetUIManager()
                .ShowUIBase<UIStorageSpeechBox>("UIStorageSpeechBox", UIManager.SpeechCanvas);
            _SpeechBox.InitData(idx, this);
            SetLevel(BuildingData._level);
            _SpeechBox.Close();
        }
        else
        {
            _SpeechBox = Managers.Instance.GetUIManager()
                .ShowUIBase<UIPayToUnlockSpeechBox>("UIPayToUnlockSpeechBox", UIManager.SpeechCanvas);
            _SpeechBox.InitData(idx, this);
            _SpeechBox.Close();
        }
    }

    public override void SuccessSpeechBtnClick()
    {
        UIManager.UIStorageBuildingPopup.InitData(this);
        UIManager.UIStorageBuildingPopup.OpenToStack();
    }

    public override void ActiveBuilding()
    {
        base.ActiveBuilding();
        BuildingData._syncTime = ServerTime.Instance.CurrentTime();
        RefreshStorageByServerTime();
    }

    private void OnDestroy()
    {
        StopRunningCoroutines();
    }

    private void StopRunningCoroutines()
    {
        if (_coStorageUpdate != null)
        {
            StopCoroutine(_coStorageUpdate);
            _coStorageUpdate = null;
        }

        if (_coDetectInteraction != null)
        {
            StopCoroutine(_coDetectInteraction);
            _coDetectInteraction = null;
        }
    }

    private IEnumerator CoUpdateStorage()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);

        while (true)
        {
            if (BuildingData.isOpen)
            {
                RefreshStorageByServerTime();
            }

            yield return wait;
        }
    }

    private IEnumerator CoDetectNearbyAndTransfer()
    {
        WaitForSeconds wait = new WaitForSeconds(_checkInterval);

        while (true)
        {
            DetectNearbyAndTransfer();
            yield return wait;
        }
    }

    /// <summary>
    /// 서버 시간 기준으로 저장소 생산량 갱신
    /// </summary>
    private void RefreshStorageByServerTime()
    {
        if (!BuildingData.isOpen)
            return;

        if (_storageLevelData._data == null)
            return;

        if (_storageLevelData._tickTime <= 0)
            return;

        DateTime now = ServerTime.Instance.CurrentTime();
        DateTime lastSync = BuildingData._syncTime;

        if (lastSync == default)
        {
            BuildingData._syncTime = now;
            return;
        }

        int elapsedSeconds = Mathf.Max(0, (int)(now - lastSync).TotalSeconds);
        if (elapsedSeconds <= 0)
            return;

        int totalStorageTime = BuildingData._storageTime + elapsedSeconds;
        int producedTick = totalStorageTime / _storageLevelData._tickTime;

        BuildingData._storageTime = totalStorageTime % _storageLevelData._tickTime;

        if (producedTick > 0 && BuildingData._currencyCount.Value < _storageLevelData.CurrencyMaxCount)
        {
            int addAmount = producedTick * _storageLevelData.ProductionItemCount;

            BuildingData._currencyCount.Value = Mathf.Min(
                _storageLevelData.CurrencyMaxCount,
                BuildingData._currencyCount.Value + addAmount
            );
        }

        BuildingData._syncTime = now;
    }

    private void DetectNearbyAndTransfer()
    {
        if (!BuildingData.isOpen)
            return;

        if (_SpeechBox == null || !_SpeechBox.gameObject.activeSelf)
            return;

        if (Squad == null || Squad.circleRigidbody == null || _rigidbody == null)
            return;

        bool isNearby = Vector2.Distance(_rigidbody.position, Squad.circleRigidbody.position) <= _getCurrencyRange;

        if (!isNearby)
            return;

        GetStorageCurrency();
    }

    private void GetStorageCurrency()
    {
        RefreshStorageByServerTime();

        if (BuildingData._currencyCount.Value <= 0)
            return;

        int productionUnit = Mathf.Max(_storageLevelData.ProductionItemCount, 1);
        int spawnCount = Mathf.Clamp(BuildingData._currencyCount.Value / productionUnit, 1, 10);

        StartCoroutine(SpawnObjectCoroutine(spawnCount));

        UserInfoData.AddCurrencyValue(
            (Define.ECurrency)_storageLevelData.ProductionItem,
            BuildingData._currencyCount.Value
        );

        BuildingData._currencyCount.Value = 0;
        BuildingData._syncTime = ServerTime.Instance.CurrentTime();

        Managers.Instance.GetSyncCurrencyManager().RequestSyncCurrency();
    }

    private IEnumerator SpawnObjectCoroutine(int spawnCount)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnObject();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SpawnObject()
    {
        Managers.Instance.GetObjectUnitManager().SpawnFieldDropItem(
            _storageLevelData.ProductionItem,
            Squad.circleRigidbody.position
        );
    }
}