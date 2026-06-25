using DG.Tweening.Core.Easing;
using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class RankingDungeonRewardScrollviewItem : ICell
{
    [SerializeField] TMP_Text _rankingTxt;
    [SerializeField] Image _rankingIcon;
    [SerializeField] Transform _IconArea;

    RankingReward _rankingRewardData;
    RankingDungeon _rankingDungeonData;
    GuildDungeon _guildDungeonData;
    
    List<RewardItem> iconList  = new List<RewardItem>();


    public void InitMyRankingRewardInfo(RankingItemData rankingItemData)
    {
        _rankingIcon.gameObject.SetActive(false);
        // reward Clear
        for (int i = 0; i < iconList.Count; i++)
        {
            Destroy(iconList[i].gameObject);
        }
        iconList.Clear();

        if (rankingItemData.ranking == 0)
        {
            _rankingTxt.text = "-";
            return;
        }
        else
        {
            _rankingTxt.text = rankingItemData.ranking <= 100 ? $"{rankingItemData.ranking}위" : $"{rankingItemData.totalPercent}%";
            DrawBg(rankingItemData.ranking);
        }
    }

    public void InitMyGuildRankingRewardInfo(GuildRankingItemData guildRankingItemData)
    {
        _rankingIcon.gameObject.SetActive(false);
        if (guildRankingItemData.rankingData == null)
            _rankingTxt.text = "-";
        else
        {
            _rankingTxt.text = guildRankingItemData.rankingData.ranking <= 100 ? $"{guildRankingItemData.rankingData.ranking}위" : $"{guildRankingItemData.rankingData.totalPercent}%";
            DrawBg(guildRankingItemData.rankingData.ranking);
        }
        // reward Clear
        for (int i = 0; i < iconList.Count; i++)
        {
            Destroy(iconList[i].gameObject);
        }
        iconList.Clear();
    }

    public void InitRankingRewardData(int index, RankingReward data)
    {
        _index = index;
        _rankingRewardData = data;

        RefreshRankingReward();
    }


    private void RefreshRankingReward()
    {
        string rankTxt = string.Empty;
        if (_rankingRewardData.RankingType == 0)
            rankTxt = $"{_rankingRewardData.Min}위";
        else if (_rankingRewardData.RankingType == 2)
            rankTxt = $"{_rankingRewardData.Min}위 ~ {_rankingRewardData.Max}%";
        else if(_rankingRewardData.RankingType == 1)
            rankTxt = $"{_rankingRewardData.Min}% ~ {_rankingRewardData.Max}%";
        else
            rankTxt = $"{_rankingRewardData.Min}위 ~ {_rankingRewardData.Max}위";

        _rankingTxt.text = rankTxt ;
        DrawBg(_rankingRewardData.ID);


        // rewardz
        for (int i = 0; i < iconList.Count; i++)
        {
            Destroy(iconList[i].gameObject);
        }
        iconList.Clear();

        for (int i = 0; i < _rankingRewardData.RewardID.Length; i++)
        {
            RewardItem icon = Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/Common/RewardItem_126", _IconArea).GetComponent<RewardItem>();

            // icon Init
            icon.Init(ERewardType.Currency, _rankingRewardData.RewardID[i], _rankingRewardData.RewardValue[i]);

            iconList.Add(icon);
        }
    }

    private void DrawBg(int ranking)
    {
        if (_rankingIcon == null) return;
        _rankingIcon.gameObject.SetActive(true);

        switch (ranking)
        {
            case 1:
                _rankingIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.PictogramAtlas, "Mapae_1");
                break;
            case 2:
                _rankingIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.PictogramAtlas, "Mapae_2");
                break;
            case 3:
                _rankingIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.PictogramAtlas, "Mapae_3");
                break;
            default:
                _rankingIcon.gameObject.SetActive(false);
                break;
        }
    }

    #region Stage

    public void InitRankingDungeonData(int index, RankingDungeon data)
    {
        _index = index;
        _rankingDungeonData = data;
        _guildDungeonData = null;
        RefreshRankingDungeon();
    }

    public void InitGuildDungeonRewardData(int index, GuildDungeon data)
    {
        _index = index;
        _guildDungeonData = data;
        _rankingDungeonData = null;
        RefreshRankingDungeon();
    }


    private void RefreshRankingDungeon()
    {
        int phase = 0;
        int rewardId = 0;
        if(_guildDungeonData == null)
        {
            phase = _rankingDungeonData.Phase;
            rewardId = _rankingDungeonData.RewardId;
        }
        else
        {
            phase = _guildDungeonData.Stage;
            rewardId = _guildDungeonData.RewardID;
        }

        _rankingTxt.text = $"{phase} 단계";

        // reward
        for (int i = 0; i < iconList.Count; i++)
        {
            Destroy(iconList[i].gameObject);
        }
        iconList.Clear();
        _rankingIcon.gameObject.SetActive(false);
        RewardData rankingDungeonReward = ClientLocalDB_Simple.GetData<RewardData>(DBKey.DungeonReward, rewardId);
        for (int i = 0; i < rankingDungeonReward.RewardId.Length; i++)
        {
            RewardItem icon = Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/Common/RewardItem", _IconArea).GetComponent<RewardItem>();

            // icon Init
            icon.Init(rankingDungeonReward.RewardType[i], rankingDungeonReward.RewardId[i], rankingDungeonReward.RewardValue[i]);

            iconList.Add(icon);
        }
    }

    #endregion

}
