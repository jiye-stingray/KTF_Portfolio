using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIStorageSpeechBox : UISpeechBox
{
    [SerializeField] Image _currencyIcon;
    [SerializeField] TMP_Text _storageCount;

    StorageLevelData _storageLevelData;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    public void SetStorageLevelData(StorageLevelData storageLevelData)
    {
        if (storageLevelData._data == null)
            return;
        
        _storageLevelData = storageLevelData;
        CurrencyData currencyData =
            ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, storageLevelData._data.BuildingProductionItem);
        _currencyIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.ItemIconAtlas, currencyData.Icon);
        
        _disposables.Clear();
        ReactiveProperty<int> currentCountValue = _rootBuilding.BuildingData._currencyCount;
        currentCountValue
            .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
            .TakeUntilDestroy(this)
            .Subscribe(newValue =>
            {
                UpdateText();
            }).AddTo(_disposables);
    }

    private void UpdateText()
    {
        _storageCount.text = $"{_rootBuilding.BuildingData._currencyCount}/{_storageLevelData.CurrencyMaxCount}";   
    }
}
