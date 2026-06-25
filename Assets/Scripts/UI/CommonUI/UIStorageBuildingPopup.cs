using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using static Define;

public class UIStorageBuildingPopup : UIBase
{
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _title;
    [SerializeField] TMP_Text _nowLevel;
    [SerializeField] TMP_Text _nextLevel;
    [SerializeField] TMP_Text _productionItemCount;
    [SerializeField] TMP_Text _productionItemCountPlus;
    [SerializeField] TMP_Text _productionItemCountMax;
    [SerializeField] TMP_Text _productionItemCountMaxPlus;
    [SerializeField] TMP_Text _productionTime;
    [SerializeField] TMP_Text _productionTimePlus;
    [SerializeField] BuildingLevelUpCostButton _buildingUpgradeButton;
    [SerializeField] private GameObject _arrowRoot;
    [SerializeField] GameObject _maxLevelRoot;
    
    BuildingLevelInfo _buildingNextLevelInfo;
    private StorageBuilding _storageBuilding;
    private BuildingData BuildingData => UserInfoData.GetInstallationBuilding(_storageBuilding._idx);
    private BuildingLevelInfo BuildingLevelInfo => _storageBuilding._storageLevelData._data;

    private bool IsMaxLevel => _buildingNextLevelInfo == null;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public void InitData(StorageBuilding storageBuilding)
    {
        _storageBuilding = storageBuilding;
        
        UIManager.TopCurrencyUI.SetCurrency(this.transform,(ECurrency)BuildingLevelInfo.CurrencyList[0]);
        
        _title.text = BuildingData._data.Name;
        string iconName = $"icon_{BuildingData._data.Resource}";
        _icon.sprite = Managers.Instance.GetResObjectManager().Load<Sprite>($"Texture/BuildingInfo/{iconName}");
        
        RefreshBuildingData();
    }

    public void UpdateBuildingData()
    {
        _storageBuilding.SetLevel(BuildingData._level);
        RefreshBuildingData();
    }

    private void RefreshBuildingData()
    {
        _buildingNextLevelInfo = ClientLocalDB_Simple.GetData<BuildingLevelInfo>(DBKey.BuildingLevelInfo, $"{BuildingData._data.ID}_{BuildingLevelInfo.Level + 1}");
        RefreshUI();
    }
    
    void RefreshUI()
    {
        if (_init == false)
            return;
        
        _productionItemCount.text = $"{BuildingLevelInfo.BuildingProductionItemCount.ToString()}개";
        _productionItemCountMax.text = $"{BuildingLevelInfo.BuildingProductionItemCountMax.ToString()}개";
        _productionTime.text = $"{(BuildingLevelInfo.BuildingProductionTime / 100).ToString()}초";
        _productionItemCountPlus.text = "";
        _productionItemCountMaxPlus.text = "";
        _productionTimePlus.text = "";
        
        if (_buildingNextLevelInfo != null)
        {
            int itemCountGap = _buildingNextLevelInfo.BuildingProductionItemCount - BuildingLevelInfo.BuildingProductionItemCount;
            int maxItemCountGap = _buildingNextLevelInfo.BuildingProductionItemCountMax - BuildingLevelInfo.BuildingProductionItemCountMax;
            int maxProductionTimeGap = _buildingNextLevelInfo.BuildingProductionTime / 100 - BuildingLevelInfo.BuildingProductionTime / 100;
            _productionItemCountPlus.text = itemCountGap > 0 ? $" +{itemCountGap}" : "";
            _productionItemCountMaxPlus.text = maxItemCountGap > 0 ? $" +{maxItemCountGap}" : "";
            _productionTimePlus.text = maxProductionTimeGap < 0 ? $" -{maxProductionTimeGap}" : "";
            
            _nowLevel.text = $"Lv.{BuildingData._level}";
            _nextLevel.text = $"Lv.{BuildingData._level + 1}";
        }
        else
        {
            _nextLevel.text = $"Lv.{BuildingData._level}";
        }

        _nowLevel.gameObject.SetActive(_buildingNextLevelInfo != null);
        _arrowRoot.SetActive(_buildingNextLevelInfo != null);
        _maxLevelRoot.SetActive(_buildingNextLevelInfo != null);
        _productionTimePlus.gameObject.SetActive(_buildingNextLevelInfo != null);
        _buildingUpgradeButton.Init(BuildingLevelInfo, IsMaxLevel);
    }

    public void OnClickUpgrade()
    {
        if (IsMaxLevel)
        {
            UIManager.ShowCommonToastMessage("최대 레벨 입니다.");
            return;
        }
        
        BestHttp_GameManager.LevelUpBuilding(_storageBuilding.BuildingData._data.ID);
    }
}