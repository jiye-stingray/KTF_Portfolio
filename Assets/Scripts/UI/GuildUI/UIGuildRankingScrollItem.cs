using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIGuildRankingScrollItem : ICell
{
    [SerializeField] private TMP_Text txtGuildName;
    [SerializeField] private TMP_Text txtGuildLevel;
    [SerializeField] private TMP_Text txtGuildContribution;
    [SerializeField] private TMP_Text txtRanking;

    [SerializeField] private Image _guildMark;

    
    public GuildRankingItemData _data;

    /*
     * *
     */
    public override void SetData(ItemData data, int index)
    {
        base.SetData(data, index);
        _data = data as GuildRankingItemData;
        _index = index;
        Refresh();
    }
    public void Refresh()
    {
        if (_data.rankingData != null)
        {
            txtGuildName.text = _data.rankingData.guildName;
            txtGuildLevel.text = $"Lv.{UserInfo.guildInfo.level.ToString()}";
            txtRanking.text = _data.rankingData.ranking.ToString();
            txtGuildContribution.text = _data.rankingData.point.ToString("N0");

            string markSpriteName = $"GuildMark{_data.rankingData.guildPattern.ToString("00")}";
            _guildMark.sprite = AtlasManager.GetSprite(EAtlasType.GuildAtlas, markSpriteName);
        }
        else
        {
            txtGuildName.text = UserInfo.guildInfo.name;
            txtGuildLevel.text = UserInfo.guildInfo.level.ToString();
            txtRanking.text = "-";
            txtGuildContribution.text = "0";

            string markSpriteName = $"GuildMark{UserInfo.guildInfo.guildPattern.ToString("00")}";
            _guildMark.sprite = AtlasManager.GetSprite(EAtlasType.GuildAtlas, markSpriteName);
        }
    }
}
