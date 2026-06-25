using PolyAndCode.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Define;

public class UIGuildMemberScrollItem : ICell
{
    [SerializeField] private TMP_Text txtUserName;
    [SerializeField] private TMP_Text txtUserLv;
    [SerializeField] private TMP_Text txtContribution;
    [SerializeField] private TMP_Text txtAttendance;
    [SerializeField] private Image thumbnailSprite;
    [SerializeField] private Image frameImage;

    [SerializeField] private GameObject goGuildMasterCrown;
    [SerializeField] private GameObject goGuildSubMasterCrown;

    public GuildMemberItemData _data;
    
    UnityAction<UIGuildMemberScrollItem> guildMemberClickAction;

    /*
     * *
     */

    public void SetData(ItemData data, int index, UnityAction<UIGuildMemberScrollItem> _guildMemberClickAction)
    {
        base.SetData(data, index);
        _index = index;
        _data = data as GuildMemberItemData;
        
        guildMemberClickAction = _guildMemberClickAction;
        Refresh();
    }
    public void Refresh()
    {
        txtUserName.text = _data.memberInfo.userGameName;
        txtUserLv.text = "Lv." + _data.memberInfo.level.ToString();
        txtContribution.text = _data.memberInfo.contribution.ToString();
        thumbnailSprite.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.CharacterIconAtlas,
                $"Thum_SD_Cr_{_data.memberInfo.thumbnail.ToString("000")}");
        if (frameImage)
        {
            var frameSprite = _data.memberInfo.frame <= 0 ? null
                : Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.FrameAtlas, $"FrameImg_{_data.memberInfo.frame.ToString("000")}");
            frameImage.sprite = frameSprite;
            frameImage.gameObject.SetActive(frameSprite != null);
        }
        goGuildMasterCrown.SetActive((EGuildMemberType)_data.memberInfo.memberLevel == EGuildMemberType.MASTER);
        goGuildSubMasterCrown.SetActive((EGuildMemberType)_data.memberInfo.memberLevel == EGuildMemberType.SUBMASTER);


        string attendanceString = string.Empty;
        TimeSpan diff = ServerTime.Instance.CurrentTime() - Convert.ToDateTime(_data.memberInfo.attendanceDate);
        if(diff.TotalSeconds < 0) diff = TimeSpan.Zero;

        // 1분 미만이면 온라인
        if (diff.TotalSeconds < 60)
            attendanceString = $"<color=#509A5C>온라인";
        else if (diff.TotalSeconds < 3600)       // 1시간 미만: 분(m) - 초 올림
            attendanceString = $"<color=#734A2F>{Mathf.CeilToInt((float)(diff.TotalSeconds / 60f))}m";
        else if(diff.TotalSeconds < 86400)      // 24시간 미만: 시간(h) - 분 올림
            attendanceString = $"<color=#734A2F>{Mathf.CeilToInt((float)(diff.TotalSeconds / 3600f))}h";
        else // 24시간 이상: 일(d) - 시간 올림
            attendanceString = $"<color=#734A2F>{Mathf.CeilToInt((float)(diff.TotalSeconds / 86400f))}d";
        txtAttendance.text = attendanceString;
    }

    public void OnClickOpenMemberDetail()
    {
        guildMemberClickAction?.Invoke(this);
    }
}
