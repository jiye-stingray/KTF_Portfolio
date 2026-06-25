using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class UISubRelicSelectCharacterPopup : UISubBase
{
    [SerializeField] private ScrollRectDynamicPopulator _scrollView;

    [SerializeField] private UIButtonBase _equipCharacterButton;

    int _relicId;
    RelicItemData RelicItemData => UserInfoData.GetRelicItemData(_relicId);
    IndexWrapper _indexWrapper = new IndexWrapper();
    List<ItemData> _items = new List<ItemData>();
    CharacterClassIndexItemData CharacterClassIndexItemData => _items[_indexWrapper._index] as CharacterClassIndexItemData;
    
    public void Init(int relicId)
    {
        _relicId = relicId;
        SetCharacterList();
        RefreshButton();
    }

    private void SetCharacterList()
    {
        _items.Clear();
        _indexWrapper._index = 0;
        var query = UserInfoData._dicCharacterItemData.Values
            .Where(x => x.isOpen);
        if (RelicItemData._equipHeroId != 0)
        {
            query = query.OrderByDescending(x => x.id == RelicItemData._equipHeroId);
        }
        var list = query.ToList();

        foreach (var itemData in list)
        {
            CharacterClassIndexItemData indexItemData = new CharacterClassIndexItemData();
            indexItemData._indexWrapper = _indexWrapper;
            indexItemData._characterClassItem = itemData;
            indexItemData._clickAction = OnSelectCharacterClicked;
            
            _items.Add(indexItemData);
        }
        
        _scrollView.Init((cell, data, index) =>
        {
            cell.SetData(data, index);
        });

        _scrollView.Populate(_items);
    }

    public void OnSelectCharacterClicked()
    {
        _scrollView.RefreshItem();
        RefreshButton();
    }

    private void RefreshButton()
    {
        _equipCharacterButton.SetGray(CharacterClassIndexItemData.ID == RelicItemData._equipHeroId);
    }

    public void OnEquipRelicClicked()
    {
        if (UserInfoData.IsEquipCheck(CharacterClassIndexItemData.ID))
        {
            UIManager.ShowConfirmPopUp("유물교체", "해당 영웅은 이미 유물을 장착중입니다.\n교체 하시겠습니까?", EquipRelic);
            return;
        }
        
        EquipRelic();
    }

    private void EquipRelic()
    {
        int heroTableId = CharacterClassIndexItemData.ID;
        BestHttp_GameManager.OnPostRelicEquippedHero(_relicId, heroTableId);
        
        ClickCloseBtn();
    }
}
