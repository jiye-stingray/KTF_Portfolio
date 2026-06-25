using PolyAndCode.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIRankingDungeonEntrance : UIBase, IRecyclableScrollRectDataSource
{
    [SerializeField] RecyclableScrollRect _scrollRect;
    [SerializeField] RankingDungeonScrollviewItem _myRankingscrollviewItem;
    [SerializeField] UITimer _uiTimer;
    [SerializeField] GameObject _gray;
    [SerializeField] GameObject _endGo;

    [SerializeField] TMP_Text _rankdescTxt;

    [SerializeField] TMP_Text _rankingDescTxt;

    List<RankingItemData> _dataList = new List<RankingItemData>();
    UISubRankingDungeonRewardInfo subUi;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        return true;

    }

    #region Recycle Scrollview
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as RankingDungeonScrollviewItem;
        item.SetData(_dataList[index], index);          // 나중에 정돈
    }
    #endregion

    public void SetRankingDataOpenToStack(RankingItemData myRanking, List<RankingItemData> RankingList, ScheduleDto schedule)
    {
        // 랭킹 던전 스케줄 타이머 
        DateTime endTime = DateTime.Parse(schedule.endTime);
        DateTime now = ServerTime.Instance.CurrentTime();
        TimeSpan durationTimeSpan = endTime - now;
        bool isEnd = durationTimeSpan <= TimeSpan.Zero; //종료
        _gray.SetActive(isEnd);         // 종료 
        _endGo.SetActive(isEnd);
        _uiTimer.gameObject.SetActive(!isEnd);

        if (!isEnd)
        {
            TimeData timeData = new TimeData();
            timeData.SetByDuration(durationTimeSpan.TotalSeconds);
            _uiTimer.Set(timeData, "{0} 남음");
        }

        _dataList = RankingList;
        UserInfoData._myRankingItemData = myRanking;

        OpenToStack();
    }

    public override void Open()
    {
        base.Open();
        Refresh();
    }


    public override void Refresh()
    {
        _scrollRect.Initialize(this);

        _rankdescTxt.text = !_gray.activeSelf ? "현재 순위" : "이전 시즌 순위";
        // my ranking setting
        if (UserInfoData._myRankingItemData == null)
            return;

        _myRankingscrollviewItem.SetData(UserInfoData._myRankingItemData, 0);
        _scrollRect.ReloadData();
    }

    public void ClickRewardInfoBtn()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        if (subUi != null)
        {
            subUi.ClickCloseBtn();
            subUi = null;
        }
        subUi = UIManager.ShowUISubBase<UISubRankingDungeonRewardInfo>(this, "UISubRankingDungeonRewardInfo");
        subUi.SetDungeonType(DungeonRewardType.Ranking);
        subUi.OpenToStack();
    }

    public void ClickRankingDungeonEntranceBtn()
    {
        if(_gray.activeSelf)
        {
            UIManager.ShowCommonToastMessage("시즌이 종료되었습니다.\n다음 시즌을 기대해주세요!");
            return;
        }

        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        UIManager.UIDeckSetting.InitContentType(EContent.Ranking);
        UIManager.UIDeckSetting.OpenToStack();
    }
}
