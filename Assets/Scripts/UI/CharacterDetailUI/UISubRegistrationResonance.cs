using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UISubRegistrationResonance : UISubBase
{
    [SerializeField] private ScrollRectDynamicPopulator _scrollview;
    private List<ItemData> _characterItems = new List<ItemData>();

    private int _slotIndex;
    private int _characterId;

    public void InitIndex(int slotIndex)
    {
        _slotIndex = slotIndex;
        Refresh();
        OpenToStack();
    }

    public override void Refresh()
    {
        DrawScrollview();
    }

    private void DrawScrollview()
    {
        // SetData
        _characterItems.Clear();
        List<CharacterClassItemData> tempList = UserInfoData._dicCharacterItemData.Values
            .Where(c => c.isOpen)
            .Where(c => c._unitData.Live)
            .Where(c => !UserInfoData._TopLevelCharacterList.Any(t => t.id == c.id))
            .Where(c => !UserInfoData._dicResonanceItemData.Any(t => t.Value._characterId == c.id))
            .ToList();
        for (int i = 0; i < tempList.Count; i++)
        {
            WishListItemData wishListItemData = new WishListItemData()
            {
                id = tempList[i].id,
                isSelected = _characterId == tempList[i].id,
            };
            _characterItems.Add(wishListItemData);
        }

        _scrollview.Init((cell, data, index) =>
        {
            cell.GetComponent<WishListScrollviewItem>()._isShowCurrentGrade = true;     // set data 전에 setting
            cell.SetData(data,index);
            cell.OnClick = OnclickEvent;
        });

        _scrollview.Populate(_characterItems);
    }

    private void OnclickEvent(int id)
    {
        _characterId = id;
        Refresh();
    }

    public void Click()
    {
        #if USE_SERVER
        // 서버 연결 
        if(_characterId != 0)
            Managers.Instance.GetServerManager().OnPostEquipResonanceSlot(_characterId, _slotIndex);
#else
        UserInfoData.RegistrationResonanceCharacter(_slotIndex,_characterId);
        UIManager.UICharacterInventory.Refresh();

#endif
        ClickCloseBtn();

    }
}
