using PolyAndCode.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UIGuildHomeMemberPage : UITabBase, IRecyclableScrollRectDataSource
{
    [SerializeField] private RecyclableScrollRect scrollView;
    UIGuildMemberDetail guildMemberDetail;
    UserInfoData userInfoData => Managers.Instance.UserInfo();
    BestHttp_GameManager bestHttp_GameManager => Managers.Instance.GetServerManager();

    List<GuildMemberItemData> _dataList = new List<GuildMemberItemData>();

    /*
     * *
     */
    TimeData _guildScheduleTimedata = new TimeData();

    #region ScrollView
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as UIGuildMemberScrollItem;
         item.SetData(_dataList[index], index, OnClickGuildMember);
    }
    #endregion

    public override void Open()
    {
        base.Open();
        RefreshMember();
        GetGuildSchedule();

    }
    public override void Refresh()
    {
        base.Refresh();
        scrollView.ReloadData();
    }

    /// <summary>
    /// 서버 통신 길드원 정보 받기
    /// </summary>
    public void RefreshMember()
    {
        bestHttp_GameManager.OnRequestGuildMemberList((memberList) =>
        {
            _dataList.Clear();
            for (int i = 0; i < memberList.Count; i++)
            {
                GuildMemberItemData guildMemberItemData = new GuildMemberItemData();
                guildMemberItemData.memberInfo = memberList[i];
                _dataList.Add(guildMemberItemData);
            }
            scrollView.Initialize(this);
        });
    }

    private void GetGuildSchedule()
    {
        BestHttp_GameManager.OnGetGuildBossSchedule((s) =>
        {
            ScheduleDto schedule = s;
            if (schedule != null)
            {
                DateTime endTime = DateTime.Parse(schedule.endTime);
                DateTime now = ServerTime.Instance.CurrentTime();
                TimeSpan durationTimeSpan = endTime - now;
                _guildScheduleTimedata.SetByDuration(durationTimeSpan.TotalSeconds);

            }
        });
    }

    public void OnClickGuildMasterChange(UIGuildMemberScrollItem _item)
    {        
        UIManager.ShowConfirmPopUp("길드장 위임","길드장을 위임하시겠습니까?",() =>
        {
            GuildTargetIDRequestDto guildTargetIDRequestDto = new GuildTargetIDRequestDto();
            guildTargetIDRequestDto.guildId = userInfoData.guildInfo.id;
            guildTargetIDRequestDto.targetId = _item._data.memberInfo.useridUser;

            bestHttp_GameManager.OnPostGuildMasterChange(guildTargetIDRequestDto, () =>
            {
                bestHttp_GameManager.OnRequestGuildMemberList((memberList) =>
                {
                    _dataList.Clear();
                    for (int i = 0; i < memberList.Count; i++)
                    {
                        GuildMemberItemData guildMemberItemData = new GuildMemberItemData();
                        guildMemberItemData.memberInfo = memberList[i];
                        _dataList.Add(guildMemberItemData);
                    }
                    scrollView.Initialize(this);
                });
                Managers.Instance.GetUIManager().UIGuildHome.Refresh();
            });
        });
    }
    public void OnClickGuildSubMasterChange(UIGuildMemberScrollItem _item)
    {
        UIManager.ShowConfirmPopUp("부길드장 임명", $"{_item._data.memberInfo.userGameName}을 부길드장에 임명하시겠습니까?", () =>
        {
            GuildTargetIDRequestDto guildTargetIDRequestDto = new GuildTargetIDRequestDto();
            guildTargetIDRequestDto.guildId = userInfoData.guildInfo.id;
            guildTargetIDRequestDto.targetId = _item._data.memberInfo.useridUser;

            bestHttp_GameManager.OnPostGuildSubMasterChange(guildTargetIDRequestDto, () =>
            {
                bestHttp_GameManager.OnRequestGuildMemberList((memberList) =>
                {
                    _dataList.Clear();
                    for (int i = 0; i < memberList.Count; i++)
                    {
                        GuildMemberItemData guildMemberItemData = new GuildMemberItemData();
                        guildMemberItemData.memberInfo = memberList[i];
                        _dataList.Add(guildMemberItemData);
                    }
                    scrollView.Initialize(this);
                });
                Managers.Instance.GetUIManager().UIGuildHome.Refresh();
            });
        });
        
    }
    public void OnClickGuildSubMasterImpeachment(UIGuildMemberScrollItem _item)
    {
        UIManager.ShowConfirmPopUp("부길드장 해임", $"{_item._data.memberInfo.userGameName}을 부길드장에서 해임하시겠습니까?", () =>
        {
            GuildIDRequestDto guildTargetIDRequestDto = new GuildIDRequestDto();
            guildTargetIDRequestDto.guildId = userInfoData.guildInfo.id;

            bestHttp_GameManager.OnPostGuildSubMasterImpeachment(guildTargetIDRequestDto, () =>
            {
                bestHttp_GameManager.OnRequestGuildMemberList((memberList) =>
                {
                    _dataList.Clear();
                    for (int i = 0; i < memberList.Count; i++)
                    {
                        GuildMemberItemData guildMemberItemData = new GuildMemberItemData();
                        guildMemberItemData.memberInfo = memberList[i];
                        _dataList.Add(guildMemberItemData);
                    }
                    scrollView.Initialize(this);
                });
                Managers.Instance.GetUIManager().UIGuildHome.Refresh();
            });
        });
        
    }
    public void OnClickGuildMemberOut(UIGuildMemberScrollItem _item)
    {

        if (_guildScheduleTimedata.GetRemain() > 0)
        {
            UIManager.ShowCommonToastMessage("연합 괴수 토벌 기간에는 퇴출할 수 없습니다.");
            return;

        }
        string memberName = _item._data.memberInfo.userGameName.ToString();
        UIManager.ShowConfirmPopUp("길드원 퇴출", $"{memberName}님을 퇴출시키시겠습니까?", () =>
        {
            GuildTargetIDRequestDto guildTargetIDRequestDto = new GuildTargetIDRequestDto();
            guildTargetIDRequestDto.guildId = userInfoData.guildInfo.id;
            guildTargetIDRequestDto.targetId = _item._data.memberInfo.useridUser;

            bestHttp_GameManager.OnPostGuildMemberOut(guildTargetIDRequestDto, (memberList) =>
            {
                _dataList.Clear();
                for (int i = 0; i < memberList.Count; i++)
                {
                    GuildMemberItemData guildMemberItemData = new GuildMemberItemData();
                    guildMemberItemData.memberInfo = memberList[i];
                    _dataList.Add(guildMemberItemData);
                }
                scrollView.Initialize(this);
                Managers.Instance.GetUIManager().UIGuildHome.Refresh();
                UIManager.ShowCommonToastMessage($"{memberName}님을 연합에서 퇴출하였습니다.");
                    
                // Chat Leave Guild
                #if CHAT
                Managers.Instance.Chat.LeaveGuild(Managers.Instance.UserInfo().userId);
                #endif
            });
        });   

    }
    public void OnClickGuildMember(UIGuildMemberScrollItem _item)
    {
        if(guildMemberDetail == null)
            guildMemberDetail = Managers.Instance.GetUIManager().ShowUISubBase<UIGuildMemberDetail>(Managers.Instance.GetUIManager().UIGuildHome, "UIGuildMemberDetail");
        string serverTimeStr = ServerTime.Instance.CurrentTime().ToString("yyyy-MM-dd");
        string lastAttendanceTime = Convert.ToDateTime(_item._data.memberInfo.attendanceDate).ToString("yyyy-MM-dd");

        int date = Utils.GetDateTimeCompare(lastAttendanceTime, serverTimeStr);
        bool isAttendanceLongTimeAgo = date <= -10;

        guildMemberDetail.OpenToStack(_item,isAttendanceLongTimeAgo, OnClickGuildMasterChange, OnClickGuildSubMasterChange, OnClickGuildSubMasterImpeachment, OnClickGuildMemberOut);

    }
}
