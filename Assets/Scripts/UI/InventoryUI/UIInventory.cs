using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Define;

public class UIInventory : UIBase , IRecyclableScrollRectDataSource
{
    #region Tab
    public enum ETAB_TYPE
    {
        Item,               // 소모품
        Equipment,          // 장비
        Currency            // 재화
    }
    public ETAB_TYPE _currentTab = ETAB_TYPE.Item;

    [SerializeField] UITabGroup _group;

    #endregion

    [SerializeField] GameObject _countBg;
    [SerializeField] TMP_Text _maxTxt;
    [SerializeField] RecyclableScrollRect _scrollview;
    [SerializeField] GameObject _decompositionBtn;
    [SerializeField] ContentItemUI _craftBtn;
    EInventoryItemType _currentInventoryItemType;
    
    [Header("RedDot")]
    public GameObject _DecompositionRedDot;

    [SerializeField] GameObject _emptyBg;
    
    List<long> _dataIdList = new List<long>();
    #region Recycle Scrollview

    public int GetItemCount()
    {
        return _dataIdList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as InventoryScrollviewItem;
        item.SetData(index, _dataIdList[index], _currentInventoryItemType);
    }

    #endregion

    public override void Open()
    {
        base.Open();
        _group._currentTapGroupBtn = _group._tapGroupBtns[0];
        OnChangeTap();
        Refresh();
    }

    /// <summary>
    /// UI 외부 호출용
    /// </summary>
    /// <param name="index"></param>ㅇ
    public void TabRefresh(int index)
    {
        _group._currentTapGroupBtn = _group._tapGroupBtns[index];
        OnChangeTap();
        Refresh();
    }

    public override void Refresh()
    {
        _group.Set((int)_currentTab);

        DrawScrollviewItem();
        
        CheckEmptyState();
    }

    private void CheckEmptyState()
    {
        if (_emptyBg != null)
        {
            // _dataIdList에 아이템이 하나도 없으면 true, 있으면 false
            _emptyBg.SetActive(_dataIdList.Count <= 0);
        }
    }
    
    private void DrawScrollviewItem()
    {
        switch (_currentInventoryItemType)
        {
            case EInventoryItemType.Item:
                DrawItemScrollviewItem();
                break;
            case EInventoryItemType.Equipment:
                DrawEquipmentScrollviewItem();
                break;
            case EInventoryItemType.Currency:
                DrawCurrencyScrollviewItem();
                break;
            default:
                break;
        }
        
        CheckEmptyState();
    }

    private void DrawItemScrollviewItem()
    {
        _dataIdList.Clear();
        _countBg.SetActive(false);
        _decompositionBtn.SetActive(false);
        _craftBtn.gameObject.SetActive(false);

        var itemdatalst = UserInfoData._dicitemItems.Where(i => i.Value > 0);
        foreach (var item in itemdatalst)
        {
            _dataIdList.Add(item.Key);
        }

        _scrollview.Initialize(this);
        _scrollview.ReloadData();
    }


    private void DrawEquipmentScrollviewItem()
    {
        _dataIdList.Clear();
        _countBg.SetActive(true);
        _maxTxt.text = $"{UserInfoData._dicEquipmentItemData.Count}/{ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "MaxEquipmentInventory").Value}";
        _decompositionBtn.SetActive(true);
        _craftBtn.gameObject.SetActive(false);
        #if TUTO
        ContentsOpen openContentBase = ClientLocalDB_Simple.GetData<ContentsOpen>(DBKey.ContentsOpen, _decompositionBtn.name);
        _decompositionBtn.SetActive(true && UserInfoData.userLevel.Value >= openContentBase.ConditionValue);

        #endif
        


        // data 정렬
        List<EquipmentItemData> dataList = UserInfoData._dicEquipmentItemData.Values.ToList();
        dataList = dataList.OrderByDescending(e => e.isSet)
            .ThenBy(e => (int)e.data.Faction)
            .ThenBy(e => (int)e.data.Type)
            .ThenByDescending(e => (int)e.data.Grade)
            .ThenBy(e => e.id)
            .ToList();


        _dataIdList = dataList.Select(item => item.id).ToList();

        _scrollview.Initialize(this);
        _scrollview.ReloadData();


        // RedDot
        _DecompositionRedDot.SetActive(RedDotManager.InventoryAllDecompositionBtnRedDot());
    }

    private void DrawCurrencyScrollviewItem()
    {
        _dataIdList.Clear();
        _countBg.SetActive(false);
        _decompositionBtn.SetActive(false);

        _craftBtn.Refresh();


        List<long> dataList = UserInfoData._currencyItems.Where(c => c.Key > 0 && (int)c.Key < 100  && c.Value.Value > 0).Select(c => (long)c.Key).ToList();
        _dataIdList = dataList;

        _scrollview.Initialize(this);
        _scrollview.ReloadData();
    }

    public void OnChangeTap()
    {
        _currentTab = (ETAB_TYPE)_group._currentTapGroupBtn._index;
        switch (_currentTab)
        {
            case ETAB_TYPE.Item:
                _currentInventoryItemType = EInventoryItemType.Item;
                break;
            case ETAB_TYPE.Equipment:
                _currentInventoryItemType= EInventoryItemType.Equipment;
                break;
            case ETAB_TYPE.Currency:
                _currentInventoryItemType = EInventoryItemType.Currency;
                break;
        }

        DrawScrollviewItem();
    }

    public void AllDecompositionBtnClick()
    {
        // 전체 분해
        // 분해할 장비 
        var decompositionItmes = UserInfoData._dicEquipmentItemData
            .Where(item => !item.Value.isSet &&
            (UserInfoData._dicEquipment[item.Value.data.Faction][(int)item.Value.data.Type] != null &&
            item.Value.data.Grade <= UserInfoData._dicEquipment[item.Value.data.Faction][(int)item.Value.data.Type].data.Grade))
            .ToDictionary(item => item.Key, item => item.Value);

        if (decompositionItmes.Count <= 0)
        {
            // 분해 한 장비가 없다는 토스트 메시지
            UIManager.ShowUIToast<UIToastBase>("분해할 장비가 없습니다", "ToastMessage");
            return;
        }

        UIManager.ShowUISubBase<UISubEquipmentDecompositionFilter>(Managers.Instance.GetUIManager().UIInventory, "UISubEquipmentDecompositionFilter")
            .SetFaction(EFactionType.All);
/*#if USE_SERVER
        BestHttp_GameManager.OnGetAllBatchDisassembly();
#else
        int dismiss = 0;
        foreach (var item in decompositionItmes)
        {
            dismiss += (ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "EquipmentDismissRewardValue").Value / 100) * item.Value.level;
        }

        foreach (var item in decompositionItmes)
        {
            UserInfoData._dicEquipmentItemData.Remove(item.Key);
        }

        if(decompositionItmes.Count > 0)
        {

            Refresh();

            RewardBundleDto rewardDto = new RewardBundleDto()
            {
                characterRewardDtoList = new List<RewardDto>(),
                currencyRewardDtoList = new List<RewardDto>(),
                equipmentRewardDtoList = new List<EquipmentDto>()
            };
            RewardDto dto = new RewardDto()
            {
                tableId = ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "EquipmentDismissRewardID").Value,
                count = dismiss
            };
            rewardDto.currencyRewardDtoList.Add(dto);

            // 팝업
            UISubRewards subUI = UIManager.ShowPopup<UISubRewards>("UIRewardPopup");
            subUI.SetRewardData(rewardDto);
            subUI.OpenToStack();

        }
#endif*/
    }

    public void OnClickCraft()
    {
        if(_craftBtn.IsLock)
        {
            UIManager.ShowCommonToastMessage("점검중 입니다.");
            return;
        }
        Managers.Instance.GetUIManager().UICraft.OpenToStack();
    }
}

