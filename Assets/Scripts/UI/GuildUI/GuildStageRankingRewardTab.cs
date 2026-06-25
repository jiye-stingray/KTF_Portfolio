using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildStageRankingRewardTab : UITabBase, IRecyclableScrollRectDataSource
{
    public RecyclableScrollRect _scrollview;

    UISubRankingDungeonRewardInfo _rankingInfoUI;
    UISubRankingDungeonRewardInfo rankingInfoUI
    {
        get
        {
            if (_rankingInfoUI == null)
                _rankingInfoUI = _mainUI.GetComponent<UISubRankingDungeonRewardInfo>();

            return _rankingInfoUI;
        }
    }

    #region Recycle Scrollview
    public int GetItemCount()
    {
        return rankingInfoUI._guildDungeonList.Count;
    }


    public void SetCell(ICell cell, int index)
    {
        var item = cell as RankingDungeonRewardScrollviewItem;
        item.InitGuildDungeonRewardData(index, rankingInfoUI._guildDungeonList[index]);          // 길드 던전 Setting
    }

    #endregion

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public override void Refresh()
    {
        _scrollview.ReloadData();
    }
}