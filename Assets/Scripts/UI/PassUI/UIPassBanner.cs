using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIPassBanner : UIBase
{
    [SerializeField] private ScrollRectDynamicPopulator _scrollview;
    List<ItemData> _passItemDataList = new List<ItemData>();

    public override void Open()
    {
        base.Open();
        Managers.Instance.GetServerManager().OnGetPassInfo(() =>
        {
            Refresh();
        });
    }

    public override void Refresh()
    {
        _passItemDataList.Clear();
        _passItemDataList = UserInfoData._dicPassItem.Values
            .Where(pass => !(pass.isEnd && ClientLocalDB_Simple.GetData<PassGroup>(DBKey.PassGroup,(int)pass.passType).BannerClose))
            .Cast<ItemData>().ToList();
        base.Refresh();
        _scrollview.Init((cell, data, index) =>
        {
            cell.SetData(data,index);
            cell.OnClick = OnClickEvent;
        });
        _scrollview.Populate(_passItemDataList);
    }

    private void OnClickEvent(int id)
    {
        UIManager.UIBattlePass.SetPassTypeOpenToStack(id);
    }
}
