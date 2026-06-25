using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static Utils;

public class UIGuildInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text rankLabel;
    [SerializeField] private Image markSprite;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private TMP_Text MinLevelLabel;
    [SerializeField] private TMP_Text memberCountLabel;
    
    [SerializeField] private TMP_Text noticeLabel;
    [SerializeField] private TMP_Text approvalLabel;
    [SerializeField] private UIGaugeWidget expGauge;
    [SerializeField] private TMP_Text leaderNameLabel;

    GuildInfoDto guildInfo;
    AtlasManager _atlasManager => Managers.Instance.GetAtlasManager();
    /*
     * *
     */

    public void Refresh(GuildInfoDto guildInfo)
    {
        SetName(guildInfo.name);
        SetLevel(guildInfo.level);
        SetMinLevelLabel(guildInfo.minLevel);
        SetRanking(guildInfo.ranking);
        SetMark(guildInfo.guildPattern);
        SetMemberCount(guildInfo.joinNum, guildInfo.limNum);
        SetNotice(guildInfo.guildNoti);
        SetApprovalLabel((EGuildApprovalType)guildInfo.approvalType);
        SetExp(guildInfo.exp, guildInfo.maxExp);
        SetLeaderName(guildInfo.leaderName);
    }

    public void SetRanking(int value)
    {
        if (rankLabel != null)
        {
            if (value > 0)
                rankLabel.text = string.Format("{0}위", value);
            else
                rankLabel.text = string.Format("-", value);
        }
    }

    public void SetMark(int value)
    {
        if (value > 0 && markSprite != null)
        {
            string markSpriteName = $"GuildMark{value.ToString("00")}";
            markSprite.sprite = _atlasManager.GetSprite(EAtlasType.GuildAtlas, markSpriteName);
        }
    }

    public void SetName(string value)
    {
        if (nameLabel != null)
            nameLabel.text = value;
    }

    public void SetLevel(int value)
    {
        if (levelLabel != null)
            levelLabel.text = $"Lv.{value}";

    }

    public void SetMinLevelLabel(int value)
    {

        if (MinLevelLabel != null)
            MinLevelLabel.text = value == 0 ? "조건 없음" :  $"Lv.{value}이상";
    }

    public void SetMemberCount(int memberCount, int maxMemberCount)
    {
        if (memberCountLabel != null)
            memberCountLabel.text = $"{memberCount}/{maxMemberCount}";
    }

    public void SetNotice(string value)
    {
        if (noticeLabel != null)
            noticeLabel.text = value;
    }

    public void SetApprovalLabel(EGuildApprovalType type)
    {
        if (approvalLabel == null)
            return;

        StringMaker.Clear();
        //StringMaker.stringBuilder.Append("가입 조건 : ");

        switch (type)
        {
            case EGuildApprovalType.NONE:
                StringMaker.stringBuilder.Append("자유 가입");
                //approvalLabel.text = LocalizationManager.GetTranslation("1625");
                break;
            case EGuildApprovalType.APPROVAL:
                StringMaker.stringBuilder.Append("승인 가입");
                //approvalLabel.text = LocalizationManager.GetTranslation("1626");
                break;
            case EGuildApprovalType.UNABLE:
                StringMaker.stringBuilder.Append("가입 불가");
                //approvalLabel.text = LocalizationManager.GetTranslation("1627");
                break;
        }
        approvalLabel.text = StringMaker.stringBuilder.ToString();
    }

    public void SetExp(int exp, int maxExp)
    {
        if (expGauge != null && maxExp > 0)
            expGauge.Refresh((float)exp , maxExp);
    }

    public void SetLeaderName(string value)
    {
        if (leaderNameLabel != null)
            leaderNameLabel.text = value;
    }
}
