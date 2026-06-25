using Only1Games.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIGuildSettingPage : UITabBase
{
    [SerializeField] private Image guildMark;
    [SerializeField] private TMP_Text _guildName;

    [SerializeField] private TMP_InputField guildNotice;

    [SerializeField] private UIDropDownMenu guildLevelDropDownMenu;
    [SerializeField] private UIDropDownMenu guildApprovalTypeDropDownMenu;

    UIGuildIconChanger guildInfoPopup;
    UIDefaultPopup confirmPopup;
    GuildInfoDto guildInfoDto => Managers.Instance.UserInfo().guildInfo;

    AtlasManager _atlasManager => Managers.Instance.GetAtlasManager();
    BestHttp_GameManager bestHttp_GameManager => Managers.Instance.GetServerManager();
    GuildSettingRequestDto requestDto = new();

    TimeData _guildScheduleTimedata = new TimeData();
    public enum ELevelCondition
    {
        NONE,
        twenty,
        thirty,
        Forty,
        Fifty,
    }
    int guildPattern;
    ELevelCondition eCurrentLevelType;
    EGuildApprovalType eCurrentGuildApprovalType;

    /*
     * *
     */
    public override void Open()
    {
        base.Open();
        ELevelCondition eLevelConditionType = ELevelCondition.NONE;
        switch (guildInfoDto.minLevel)
        {
            case 0:
                eLevelConditionType = ELevelCondition.NONE;
                break;
            case 20:
                eLevelConditionType = ELevelCondition.twenty;
                break;
            case 30:
                eLevelConditionType = ELevelCondition.twenty;
                break;
            case 40:
                eLevelConditionType = ELevelCondition.Forty;
                break;
            case 50:
                eLevelConditionType = ELevelCondition.Fifty;
                break;

            default:
                break;
        }

        eCurrentLevelType = eLevelConditionType;
        eCurrentGuildApprovalType = (EGuildApprovalType)guildInfoDto.approvalType;

        guildLevelDropDownMenu.Set((int)eCurrentLevelType);
        guildApprovalTypeDropDownMenu.Set((int)eCurrentGuildApprovalType - 1);
        _guildName.text = guildInfoDto.name;
        guildNotice.text = guildInfoDto.guildNoti;
        if (guildInfoDto.guildPattern > 0 && guildMark != null)
        {
            string markSpriteName = $"GuildMark{guildInfoDto.guildPattern.ToString("00")}";
            guildMark.sprite = _atlasManager.GetSprite(EAtlasType.GuildAtlas, markSpriteName);
        }

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

    public void OnClickGuildMark()
    {
        if (guildInfoPopup == null)
            guildInfoPopup = Managers.Instance.GetUIManager().ShowUISubBase<UIGuildIconChanger>(Managers.Instance.GetUIManager().UIGuildHome, "UIGuildIconChanger");

        guildInfoPopup.OpenToStack();
        guildInfoPopup.SetData(guildInfoDto.guildPattern, (sprite, index) =>
        {
            guildMark.sprite = sprite;
            guildPattern = index + 1;
        });
    }
    public void OnClickGuildLevelCondition()
    {
        eCurrentLevelType = (ELevelCondition)guildLevelDropDownMenu.currentIndex;
    }
    public void OnClickGuildApproval()
    {
        eCurrentGuildApprovalType = (EGuildApprovalType)(guildApprovalTypeDropDownMenu.currentIndex + 1);
    }
    public void OnClickGuildDisband()
    {

        if(_guildScheduleTimedata.GetRemain() > 0)
        {
            UIManager.ShowCommonToastMessage("연합 괴수 토벌 기간에는 해산할 수 없습니다.");
            return;

        }
        if (UserInfoData.guildInfo.joinNum > 1)
        {
            UIManager.ShowUIToast<UIToastBase>("모든 길드원을 방출해야 해산이 가능합니다.", "ToastMessage");
            return;
        }

        if (confirmPopup == null)
            confirmPopup = Managers.Instance.GetUIManager().ShowPopup<UIDefaultPopup>("ConfirmPopup");
        UIManager.ShowConfirmPopUp("길드 해산", "길드를 해산 하시겠습니까?", () =>
        {
            GuildIDRequestDto guildIDRequestDto = new GuildIDRequestDto();
            guildIDRequestDto.guildId = guildInfoDto.id;

            bestHttp_GameManager.OnPostGuildDisband(guildIDRequestDto, () =>
            {
                Managers.Instance.GetLoadingUI().Hide();
                UIManager.AllCloseStackUI();
            });

            confirmPopup.ClickCloseBtn();
        });

    }
    public void OnClickChangeGuildSetting()
    {

        if(Utils.CheckBanText(guildNotice.text))
        {
            UIManager.ShowCommonToastMessage("공지에 사용할 수 없는 단어가 포함되어 있습니다.");
            return;
        }
        requestDto.guildId = guildInfoDto.id;
        requestDto.approvalType = (int)eCurrentGuildApprovalType;
        requestDto.minLevel = (int)eCurrentLevelType;
        requestDto.guildPattern = guildPattern;
        requestDto.guildNoti = guildNotice.text;

        bestHttp_GameManager.OnPostChangeGuildInfo(requestDto, () =>
        {
            Managers.Instance.GetLoadingUI().Hide();
            Managers.Instance.GetUIManager().UIGuildHome.Refresh();
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("길드 정보가 변경되었습니다.", "ToastMessage");
        });
    }
}
