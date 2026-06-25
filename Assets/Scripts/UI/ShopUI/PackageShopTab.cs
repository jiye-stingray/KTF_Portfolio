using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class PackageShopTab : UISubBase
{

    #region Tab
    bool _isLimit;
    public UITabGroup _group;
    #endregion
    
    [SerializeField] ScrollRectDynamicPopulator _scrollview;

    List<ItemData> _dataList = new List<ItemData>();

    public override void Open()
    {
        base.Open();

        Refresh();
    }

    public override void Refresh()
    {
        _group.Set(_isLimit == true ? 0 : 1, false);
        DrawScrollview();
    }

    private void DrawScrollview()
    {
        _dataList = UserInfoData._dicShopItemData[EShopType.PackageShop].Values
            .OfType<PackageShopItemData>()
            .Where(data => (data.GetData() as PackageShop).Limited == _isLimit)
            .Cast<ItemData>()
            .ToList();

        _scrollview.Init((cell, data, index) =>
        {
            cell.GetComponent<UIShopScrollviewItem>().SetData(_dataList[index],index);

        });
        _scrollview.Populate(_dataList);
    }

    public void OnChangeTab()
    {
        _isLimit = _group._currentTapGroupBtn._index == 0;
        DrawScrollview();
    }
}
