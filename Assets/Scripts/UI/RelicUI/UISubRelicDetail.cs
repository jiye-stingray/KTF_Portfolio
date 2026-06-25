using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
 
using static Define;

public class UISubRelicDetail : UISubBase
{
    [SerializeField] UIRelicPartDetailItem relicPartDetailItem;
    
    [SerializeField] private ScrollRectDynamicPopulator _scrollView;
    [SerializeField] TMP_Text _countTxt;

    [SerializeField] private GameObject _equipRoot;
    [SerializeField] private GameObject _notEquipRoot;
    
    List<ItemData> _items = new List<ItemData>();
    IndexWrapper _indexWrapper = new IndexWrapper();
    RelicItemData _relicItemData;
    
    private int _relicBaseId;
    private ERelicPartsType _relicPartsType;
    private int EquipPartsId => _relicItemData.GetPartsId(_relicPartsType);
    private int SelectPartsId => RelicPartsItemData._relicPartsItemData._id;
    private RelicPartsIndexItemData RelicPartsItemData => _items[_indexWrapper._index] as RelicPartsIndexItemData;
    
    public void SetData(int relicBaseId, ERelicPartsType type)
    {
        _relicBaseId = relicBaseId;
        _relicPartsType = type;
        _relicItemData = UserInfoData.GetRelicItemData(relicBaseId);
        
        SetData();
    }

    public override void Refresh()
    {
        RefreshButton();
    }

    private void SetData()
    {
        SetRelicPartsList();
        SetSelectPartsData();
        Refresh();
    }

    private void SetSelectPartsData()
    {
        relicPartDetailItem.SetItem(RelicPartsItemData._relicPartsItemData);
        RefreshButton();
    }

    private void RefreshButton()
    {
        _equipRoot.SetActive(EquipPartsId == SelectPartsId);
        _notEquipRoot.SetActive(!_equipRoot.activeSelf);
    }
    
    private void SetRelicPartsList()
    {
        _items.Clear();
        _indexWrapper._index = 0;
        var query = UserInfoData.GetRelicPartsItemList(_relicBaseId, _relicPartsType)
            .OrderByDescending(x => x._id == EquipPartsId)
            .ThenByDescending(x => x._grade);
        var list = query.ToList();

        foreach (var itemData in list)
        {
            RelicPartsIndexItemData indexItemData = new RelicPartsIndexItemData();
            indexItemData._indexWrapper = _indexWrapper;
            indexItemData._relicPartsItemData = itemData;
            indexItemData._clickAction = OnSelectRelicPartsClicked;
            
            _items.Add(indexItemData);
        }
        
        _scrollView.Init((cell, data, index) =>
        {
            cell.SetData(data, index);
        });

        _scrollView.Populate(_items);
        
        _countTxt.text = $"{_items.Count} / {MaxRelicPartsCount}";
        _countTxt.color = _items.Count == MaxRelicPartsCount ? new Color(170f/255f, 80f/255f, 70f/255f) : Color.white;
    }

    private void OnSelectRelicPartsClicked()
    {
        _scrollView.RefreshItem();
        SetSelectPartsData();
    }

    public void EquipRelicParts()
    {
        BestHttp_GameManager.OnPostEquipRelicParts(SelectPartsId);
    }
    
    public void OnRelicPartsDismiss()
    {
        if (RelicPartsItemData._relicPartsItemData._isLock)
        {
            UIManager.ShowCommonToastMessage("잠금 설정된 파츠는 분해 할 수 없습니다.");
            return;
        }
        
        if(RelicPartsItemData._relicPartsItemData._grade >= EOptionGradeType.Epic)
        {
            UIManager.ShowConfirmPopUp("영웅 등급 이상 유물 분해", "선택한 유물은 영웅 등급 이상입니다.\n분해하시겠습니까?", RelicPartsDismiss);
            return;
        }

        RelicPartsDismiss();
    }

    private void RelicPartsDismiss()
    {
        BestHttp_GameManager.OnPostRelicPartsDismiss(SelectPartsId, SetData);
    }
    
    public void OnRelicPartsAllDismiss()
    {
        if (!UserInfoData.EnableRelicPartsDisMiss(_relicBaseId, _relicPartsType))
        {
            UIManager.ShowCommonToastMessage("분해할 수 있는 유물이 없습니다.");
            return;
        }
        
        UIManager.ShowConfirmPopUp("유물 일괄 분해", "잠금 설정한 유물을 제외한 모든 유물을 분해합니다.", RelicPartsAllDismiss);
    }

    private void RelicPartsAllDismiss()
    {
        BestHttp_GameManager.OnPostRelicPartsAllDismiss(_relicBaseId, (int)_relicPartsType, SetData);
    }
    
    public void OnRelicPartsLockSetting()
    {
        RelicPartsItemData itemData = RelicPartsItemData._relicPartsItemData;
        if (itemData._isLock)
            BestHttp_GameManager.OnPostRelicUnlockParts(SelectPartsId, SetSelectPartsData);
        else
            BestHttp_GameManager.OnPostRelicLockParts(SelectPartsId, SetSelectPartsData);
    }
}
