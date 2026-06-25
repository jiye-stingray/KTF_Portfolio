using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum DungeonRewardType
{
    Ranking,
    Guild
}
public class UISubRankingDungeonRewardInfo : UISubBase
{
    #region Tap
    [System.Serializable]
    public struct EPages
    {
        public RankingRewardTab RankingRewardTab;
        public StageRankingRewardTab StageRankingRewardTab;
        public GuildStageRankingRewardTab GuildStageRankingRewardTab;
    }
    public EPages _pages;

    public enum ETAP_TYPE
    {
        RankingRewardPage = 0,
        StageRankingRewardPage = 1,
        GuildStageRankingRewardPage = 2,
    }
    ETAP_TYPE _currentTap = ETAP_TYPE.RankingRewardPage;

    public UITabGroup _group;

    #endregion


    public DungeonRewardType _currentDungeonType;

    public List<RankingReward> _rankingRewardList = new List<RankingReward>();         // 랭킹 보상 
    public List<RankingDungeon> _rankingDungeonList = new List<RankingDungeon>();      // 단계 보상
    public List<GuildDungeon> _guildDungeonList = new List<GuildDungeon>();

    public GuildRankingItemData myGuildRankingData;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        var rankingDungeonValues = ClientLocalDB_Simple.GetDB<RankingDungeon>(DBKey.RankingDungeon).Values.ToList();
        _rankingDungeonList = rankingDungeonValues.Take(rankingDungeonValues.Count - 1).ToList();
        var guildDungeonValues = ClientLocalDB_Simple.GetDB<GuildDungeon>(DBKey.GuildDungeon).Values.ToList();
        _guildDungeonList = guildDungeonValues.Take(guildDungeonValues.Count - 1).ToList();

        _pages.StageRankingRewardTab._scrollview.Initialize(_pages.StageRankingRewardTab);
        _pages.GuildStageRankingRewardTab._scrollview.Initialize(_pages.GuildStageRankingRewardTab);
        return true;
    }

    public void SetDungeonType(DungeonRewardType type)
    {
        _currentDungeonType = type;
    }

    public override void Open()
    {
        if(_currentDungeonType == DungeonRewardType.Guild && UserInfoData.guildUserInfo != null)
        {
            Managers.Instance.GetServerManager().OnGetGuildRanking((rankingList, _myGuildRankingData) =>
            {
                myGuildRankingData.rankingData = _myGuildRankingData;
                _rankingRewardList = ClientLocalDB_Simple.GetDB<RankingReward>(DBKey.GuildRankingReward).Values.ToList();
                _pages.RankingRewardTab._scrollview.Initialize(_pages.RankingRewardTab);

                _group._currentTapGroupBtn = _group._tapGroupBtns[0];
                OnChangeTap();
                base.Open();
                Refresh();
            });

        }
        else
        {
            _rankingRewardList = ClientLocalDB_Simple.GetDB<RankingReward>(DBKey.RankingReward).Values.ToList();
            _pages.RankingRewardTab._scrollview.Initialize(_pages.RankingRewardTab);

            _group._currentTapGroupBtn = _group._tapGroupBtns[0];
            OnChangeTap();
            base.Open();
            Refresh();
        }


    }

    public override void Refresh()
    {
        _group.Set((int)_currentTap);
    }

    /// <summary>
    /// Group의 changeEvent로 연결
    /// </summary>
    public void OnChangeTap()
    {
        _currentTap = (ETAP_TYPE)_group._currentTapGroupBtn._index;
        switch (_currentTap)
        {
            case ETAP_TYPE.RankingRewardPage:
                _pages.RankingRewardTab.Open();
                _pages.StageRankingRewardTab.Close();
                _pages.GuildStageRankingRewardTab.Close();
                break;
            case ETAP_TYPE.StageRankingRewardPage:
                _pages.RankingRewardTab.Close();
                if(_currentDungeonType == DungeonRewardType.Ranking)
                    _pages.StageRankingRewardTab.Open();
                else
                    _pages.GuildStageRankingRewardTab.Open();
                break;
        }
    }
}
 