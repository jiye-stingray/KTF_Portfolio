using TMPro;
using UnityEngine;
using static Define;

public class UIPayToUnlockSpeechBox : UISpeechBox
{
    [SerializeField] private TMP_Text _buildingNameTxt;
    [SerializeField] private GameObject _currencyRoot;
    [SerializeField] private GameObject _completeRoot;
    [SerializeField] private GameObject _conditionGrayRoot;
    [SerializeField] private TMP_Text _conditionTxt;
    [SerializeField] CurrencyIcon[] _currencyItems;
    [SerializeField] private GameObject[] _completeItemBg;
    [SerializeField] private GameObject[] _grayItemBg;
    
    private bool _conditionGray = false;
    private bool _currencyGray = false;

    public override void Refresh()
    {
        base.Refresh();
        if (_rootBuilding == null)
        {
            Debug.LogError("rootBuilding null!!");
            return;
        }

        _currencyGray = false;
        for(int i = 0; i < _currencyItems.Length; i++)
        {
            CurrencyIcon currencyTextIcon = _currencyItems[i];

            bool rangeOut = i >= _rootBuilding.BuildingData._data.CurrencyList.Length;
            currencyTextIcon.gameObject.SetActive(!rangeOut);
            _completeItemBg[i].SetActive(!rangeOut);
            _grayItemBg[i].SetActive(!rangeOut);

            if (rangeOut)
                continue;

            ECurrency currencyType = (ECurrency)_rootBuilding.BuildingData._data.CurrencyList[i];
            int value = _rootBuilding.BuildingData._data.CountList[i];
            if (currencyType == ECurrency.None)
                continue;

            int userValue = UserInfoData.GetCurrencyValue(currencyType);
            currencyTextIcon.Init(currencyType, value);
            
            if (userValue < value)
                _currencyGray = true;
            
            _completeItemBg[i].SetActive(!_currencyGray);
            _grayItemBg[i].SetActive(_currencyGray);
            currencyTextIcon.SetTextColor(userValue < value ? Utils.HexToColor("FF5959") : Utils.HexToColor("B98C6D"));
        }
        
        _conditionGray = !_rootBuilding.BuildingData.isCondition;
        _buildingNameTxt.text = _rootBuilding.BuildingData._data.Name;
        SetText(_rootBuilding.BuildingData._data.BuildOpenConditionType, _rootBuilding.BuildingData._data.BuildOpenConditionValue);
        //_currencyGrayRoot.SetActive(_currencyGray);
        _conditionGrayRoot.SetActive(_conditionGray);
        
        _currencyRoot.SetActive(!_conditionGray);
        _completeRoot.SetActive(!_conditionGray && !_currencyGray);
    }

    private void SetText(EBuildingOpenConditionType openConditionType, int value)
    {
        switch (openConditionType)
        {
            case EBuildingOpenConditionType.UserLevel:
                _conditionTxt.text = $"{value} 레벨\n해금";
                break;
            case EBuildingOpenConditionType.BuildingOpen:
                BuildingInfo buildingData = ClientLocalDB_Simple.GetData<BuildingInfo>(DBKey.BuildingInfo, value);
                _conditionTxt.text = $"{buildingData.Name}\n오픈";
                break;
            case EBuildingOpenConditionType.GuideQuestClearID:
                GuideQuest guideQuest = ClientLocalDB_Simple.GetData<GuideQuest>(DBKey.GuideQuest, value);
                _conditionTxt.text = $"길잡이 - {guideQuest.Order}\n완료";
                break;
            case EBuildingOpenConditionType.DungeonQuestClearID:
                GuideQuest dungeonQuest = ClientLocalDB_Simple.GetData<GuideQuest>(DBKey.DungeonQuest, value);
                _conditionTxt.text = $"길잡이 - {dungeonQuest.Order}\n완료";
                break;
            default:
                _conditionTxt.text = "";
                break;
        }
    }

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public override void OnClickSpeechButton()
    {
        // 재화 주기
        if(_currencyGray)
        {
            UIManager.ShowCommonToastMessage("재화가 부족합니다.");
            return;
        }
        
        BestHttp_GameManager.ActiveBuilding(_rootBuilding.BuildingData._data.ID);
    }

    public override void EnableButtonCheck(bool state)
    {
        if (_conditionGray)
            DisableButton();
        else if (_currencyGray)
            base.EnableButtonCheck(false);
        else
        {
            base.EnableButtonCheck(state);   
        }
    }
}
