using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGuildRanking : UISubBase, IRecyclableScrollRectDataSource
{
    [SerializeField] private RecyclableScrollRect scrollView;
    [SerializeField] private UIGuildRankingScrollItem myGuildRankingItem;

    GuildRankingItemData myGuildRankingData = new GuildRankingItemData();

    List<GuildRankingItemData> _dataList = new List<GuildRankingItemData>();
    BestHttp_GameManager bestHttp_GameManager => Managers.Instance.GetServerManager();
    /*
     * *
     */
    #region ScrollView
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as UIGuildRankingScrollItem;
        item.SetData(_dataList[index], index);
    }
    #endregion

    public override void Open()
    {
        base.Open();
        bestHttp_GameManager.OnGetGuildRanking((rankingList, _myGuildRankingData) =>
        {
            _dataList.Clear();
            for (int i = 0; i < rankingList.Count; i++)
            {
                GuildRankingItemData guildRankingItemData = new GuildRankingItemData();
                guildRankingItemData.rankingData = rankingList[i];
                _dataList.Add(guildRankingItemData);
            }
            myGuildRankingData.rankingData = _myGuildRankingData;
            if (myGuildRankingItem != null) myGuildRankingItem.SetData(myGuildRankingData, 0);
            if (scrollView != null) scrollView.Initialize(this);
            Refresh();
        });


    }
    public override void Refresh()
    {
        base.Refresh();
        if (scrollView != null) scrollView.ReloadData();
        if (myGuildRankingItem != null) myGuildRankingItem.Refresh();
    }

}
