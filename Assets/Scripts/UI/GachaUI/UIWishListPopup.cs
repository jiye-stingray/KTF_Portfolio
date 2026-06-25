using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class UIWishListPopup : UISubBase
{
    [SerializeField] private List<WishListScrollviewItem> _selectedWishListItems;
    [SerializeField] private ScrollRectDynamicPopulator _scrollView;
    
    private List<ItemData> _characterItems = new List<ItemData>(); // select 상태가 바뀜
    private GachaItemData _gachaItemData; 
    private EGachaType _gachaType;
    private int _selectCount = 0;
    private int[] _tempWishListItems;
    private int[] _tempCharacterListItems;
    Dictionary<int, UnitData> _dicCharacterData = new Dictionary<int, UnitData>();
    
    int _wishIndex = -1;
    int _listIndex = -1;

    public void Init(EGachaType gachaType)
    {
        _gachaType = gachaType;
        _selectCount = _gachaType == EGachaType.General ? 5 : 1;
        _gachaItemData = UserInfoData._dicGachaItemData[_gachaType];
        _tempWishListItems = new int[_selectCount];
        for (int i = 0; i < _gachaItemData._wishList.Length; i++)
        {
            _tempWishListItems[i] = _gachaItemData._wishList[i];
        }

        InitCharacterList();
        SetScrollViewItem();
        InitWishList();
    }

    private void InitCharacterList()
    {
        _dicCharacterData.Clear();
        Dictionary<string, UnitData> dicCharacterData = ClientLocalDB_Simple.GetDB<UnitData>(DBKey.PlayerCharacter);
        
        foreach (var unitData in dicCharacterData.Values)
        {
            if (!unitData.Gatcha || !unitData.WishList || !unitData.Live)
                continue;
            
            if (_gachaType == EGachaType.General)
            {
                if(unitData.Faction == EFactionType.Celestial)
                    continue;
            }
            else if (_gachaType == EGachaType.Celestial)
            {
                if (unitData.Faction != EFactionType.Celestial)
                    continue;
            }
            
            _dicCharacterData.Add(unitData.ID, unitData);
        }
    }

    private void InitWishList()
    {
        for (int i = 0; i < _selectedWishListItems.Count; i++)
        {
            WishListScrollviewItem item = _selectedWishListItems[i];
            item.gameObject.SetActive(_selectCount > i);
            item.OnClick = OnClickItem;
            
            if (_selectCount <= i)
                continue;

            WishListItemData wishListItemData = new WishListItemData();
            if (_tempWishListItems.Length > i)
                wishListItemData.id = _tempWishListItems[i];
            wishListItemData.isWish = true;
            item.SetData(wishListItemData, i);
            item.OnClick = OnClickItem;
        }
    }
    
    // 스크롤 뷰 세팅
    private void SetScrollViewItem()
    {
        _characterItems.Clear();
        
        Dictionary<int, UnitData> dicCharacterData = new Dictionary<int, UnitData>(_dicCharacterData);
        foreach (var key in _tempWishListItems)
        {
            dicCharacterData.Remove(key);
        }

        int[] keys = dicCharacterData.Keys.ToArray();
        _tempCharacterListItems = new int[keys.Length];
        for (int i = 0; i < keys.Length; i++)
        {
            int id = keys[i];
            WishListItemData wishListItemData = new WishListItemData();
            wishListItemData.id = id;
            wishListItemData.isWish = false;
            wishListItemData.isSelected = false;
            _tempCharacterListItems[i] = id;
            _characterItems.Add(wishListItemData);
        }
        
        _scrollView.Init((cell, data, index) =>
        {
            cell.SetData(data, index);
            cell.OnClick = OnClickItem;
        });
        
        _scrollView.Populate(_characterItems);
    }

    public void OnClickItem(int id)
    {
        int idxInWish = Array.IndexOf(_tempWishListItems, id);
        int idxInList = Array.IndexOf(_tempCharacterListItems, id);
        
        bool isWishSlot = id == 0 || idxInWish >= 0;
        
        if (isWishSlot)
        {
            if (id == 0)
                idxInWish = GetEmptyWishSlotIndex();
            if (_wishIndex != idxInWish)
            {
                // 위시 리스트 클릭
                _wishIndex = (_wishIndex == idxInWish) ? -1 : idxInWish;

                for (int i = 0; i < _selectedWishListItems.Count; i++)
                {
                    if (_selectCount <= i)
                        continue;
                
                    var item = _selectedWishListItems[i];
                    var data = item._data;
                    data.isSelected = (_wishIndex == i);
                    item.SetData(data, i);
                }
            }
        }
        else
        {
            // 일반 캐릭터 리스트 클릭
            _listIndex = (_listIndex == idxInList) ? -1 : idxInList;

            for (int i = 0; i < _characterItems.Count; i++)
            {
                if (_characterItems[i] is WishListItemData data)
                    data.isSelected = (_listIndex == i);
            }
            _scrollView.RefreshData(_characterItems);
        }
        
        if(_wishIndex != -1 && _listIndex != -1)
            SwapItem();
    }

    private void SwapItem()
    {
        if (_wishIndex < 0 || _listIndex < 0) return;
        if (_wishIndex >= _tempWishListItems.Length) return;
        if (_listIndex >= _tempCharacterListItems.Length) return;
        
        int characterId = _tempCharacterListItems[_listIndex];
        _tempWishListItems[_wishIndex] = characterId;
        
        _wishIndex = -1;
        _listIndex = -1;
        
        SetScrollViewItem();
        RefreshWishList();
    }
    
    private void RefreshWishList()
    {
        for (int i = 0; i < _selectedWishListItems.Count; i++)
        {
            if (_selectCount <= i)
                continue;
            
            WishListScrollviewItem item = _selectedWishListItems[i];
            WishListItemData wishListItemData = item._data;
            wishListItemData.id = _tempWishListItems[i];
            wishListItemData.isSelected = false;
            item.SetData(wishListItemData, i);
        }
    }

    public void OnClickSaveButton()
    {
        if (GetEmptyWishSlotIndex() > -1)
        {
            UIManager.ShowCommonToastMessage("세팅이 완료되지 않았습니다.");
            return;
        }

        if (!Utils.AreArraysEquivalent(_tempWishListItems, _gachaItemData._wishList))
        {
            if (_gachaType == EGachaType.General)
            {
                BestHttp_GameManager.OnChangeWishList(_tempWishListItems);
            }
            else if (_gachaType == EGachaType.Celestial)
            {
                BestHttp_GameManager.OnChangeCelestialWishList(_tempWishListItems);
            }
        }
        ClickCloseBtn();
    }

    private int GetEmptyWishSlotIndex()
    {
        for (int i = 0; i < _tempWishListItems.Length; i++)
        {
            if(_tempWishListItems[i] == 0)
                return i;
        }

        return -1;
    }
}
