using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class TicketShopTab : UITabBase
{
    [SerializeField] ScrollRectDynamicPopulator _scrollview;

    List<ItemData> _dataList = new List<ItemData>();

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public override void Refresh()
    {
        DrawScrollview();
    }

    private void DrawScrollview()
    {
        _dataList = UserInfoData._dicShopItemData[EShopType.TicketShop].Values
            .Cast<ItemData>()
            .ToList();

        _scrollview.Init((cell, data, index) =>
        {
            cell.GetComponent<UIShopScrollviewItem>().SetData(_dataList[index], index);
        });
        _scrollview.Populate(_dataList);
    }
}
