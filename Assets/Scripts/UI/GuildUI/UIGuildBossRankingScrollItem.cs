using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIGuildBossRankingScrollItem : ICell
{
    [SerializeField] private TMP_Text txtUserName;
    [SerializeField] private Image thumbnail;
    [SerializeField] private Image frameImage;
    [SerializeField] private TMP_Text txtLevel;
    [SerializeField] private TMP_Text txtScore;
    [SerializeField] private TMP_Text txtRanking;

    public RankingItemData _data;

    /*
     * *
     */

    public override void SetData(ItemData data, int index)
    {
        base.SetData(data, index);
        _data = data as RankingItemData;
        _index = index;
        Refresh();
    }
    public void Refresh()
    {
        txtRanking.text = _data.ranking.ToString();
        txtUserName.text = _data.name;
        txtLevel.text =  "Lv." + _data.level.ToString();
        thumbnail.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.CharacterIconAtlas,
                $"Thum_SD_Cr_{_data.thumbnail.ToString("000")}");
        if (frameImage)
        {
            var frameSprite = _data.frame <= 0 ? null
                : Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.FrameAtlas, $"FrameImg_{_data.frame.ToString("000")}");
            frameImage.sprite = frameSprite;
            frameImage.gameObject.SetActive(frameSprite != null);
        }
        txtScore.text = _data.score.ToString();
    }
}
