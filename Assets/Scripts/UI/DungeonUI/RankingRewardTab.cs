using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RankingRewardTab : UITabBase, IRecyclableScrollRectDataSource
{
    public RecyclableScrollRect _scrollview;
    [SerializeField] RankingDungeonRewardScrollviewItem _myRankingRewardScrollviewItem;

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
        return rankingInfoUI._rankingRewardList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as RankingDungeonRewardScrollviewItem;
        item.InitRankingRewardData(index, rankingInfoUI._rankingRewardList[index]);
    }

    #endregion

    public override void Open()
    {
        base.Open();

        Refresh();
    }

    public override void Refresh()
    {
        // scrollview 리로드 
        _scrollview.ReloadData();

        if(_rankingInfoUI._currentDungeonType == DungeonRewardType.Ranking)
        {
            // my ranking
            _myRankingRewardScrollviewItem.InitMyRankingRewardInfo(UserInfoData._myRankingItemData);

        }
        else
        {
            _myRankingRewardScrollviewItem.InitMyGuildRankingRewardInfo(_rankingInfoUI.myGuildRankingData);
        }

    }
}
