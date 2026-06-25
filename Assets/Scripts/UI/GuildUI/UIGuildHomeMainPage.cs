using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class UIGuildHomeMainPage : UITabBase
{
    [SerializeField] private GameObject btn_GuildSetting;
    [SerializeField] private GameObject btn_ExitGuild;
    [SerializeField] private TMP_Text guildNotice;

    [Header("Boss")]
    [SerializeField] TMP_Text _bossRankingTxt;
    [SerializeField] TMP_Text _bossPeriodTxt;
    [SerializeField] TMP_Text _bossInfoTxt;     // gray 상태 일땐 무조건 비활성화 (UI 용)
    [SerializeField] GameObject _endSeasonGray;
    [SerializeField] GameObject _SettlementGray;
    [SerializeField] TMP_Text _grayTxt;
    
    EBossScheduleType _eBossScheduleType;

    public ScheduleDto _guildBossSchedule;

    UserInfoData userInfo => Managers.Instance.UserInfo();
    GuildInfoDto myGuildInfoDto => userInfo.guildInfo;

    UIDefaultPopup confirmPopup;

    UIGuildManaging guildManaging;
    UIGuildBoss guildBoss;

    GuildSettingRequestDto requestDto = new();
    BestHttp_GameManager bestHttp_GameManager => Managers.Instance.GetServerManager();
    /*
     * *
     */

    public override void Open()
    {
        base.Open();
        guildNotice.text = myGuildInfoDto.guildNoti;

        bestHttp_GameManager.OnGetGuildRanking((rankingList, _myGuildRankingData) =>
        {
            BestHttp_GameManager.OnGetGuildBossSchedule((schedule) =>
            {

                if (_myGuildRankingData != null && _myGuildRankingData.ranking != 0)
                    _bossRankingTxt.text = $"연합 랭킹: {_myGuildRankingData.ranking}위";
                else
                    _bossRankingTxt.text  = "연합 랭킹: ---위";

                
                _guildBossSchedule = schedule;
                _eBossScheduleType = CheckBossScheduleType(_guildBossSchedule);
                Refresh();

            });
        });

    }
    public override void Refresh()
    {
        base.Refresh();

        EGuildMemberType memberType = (EGuildMemberType)userInfo.guildUserInfo.memberLevel;
        btn_GuildSetting.SetActive((memberType == EGuildMemberType.MASTER) || (memberType == EGuildMemberType.SUBMASTER));
        btn_ExitGuild.SetActive((EGuildMemberType)userInfo.guildUserInfo.memberLevel != EGuildMemberType.MASTER);

        _eBossScheduleType = CheckBossScheduleType(_guildBossSchedule);
        switch (_eBossScheduleType)
        {
            case EBossScheduleType.OnGoing:
                _bossInfoTxt.gameObject.SetActive(true);


                // Boss
                DateTime startTime = DateTime.Parse(_guildBossSchedule.startTime);
                DateTime endTime = DateTime.Parse(_guildBossSchedule.endTime);
                _bossPeriodTxt.gameObject.SetActive(true);
                _bossPeriodTxt.text = $"{startTime.ToString("yyyy-MM-dd")}~{endTime.ToString("yyyy-MM-dd")}";

                _endSeasonGray.SetActive(false);
                _SettlementGray.SetActive(false);
                break;
            case EBossScheduleType.Calculate:
                _bossPeriodTxt.gameObject.SetActive(false);
                _bossInfoTxt.gameObject.SetActive(false);
                // 정산 중
                _endSeasonGray.SetActive(false);
                _SettlementGray.SetActive(true);
                _grayTxt.text = "정산 중";
                break;
            case EBossScheduleType.End:
                _bossPeriodTxt.gameObject.SetActive(false);
                _bossInfoTxt.gameObject.SetActive(false) ;

                // 시즌 종료
                _endSeasonGray.SetActive(true);
                _SettlementGray.SetActive(false);
                break;
            case EBossScheduleType.Preparing:
                _bossPeriodTxt.gameObject.SetActive(false);
                _bossInfoTxt.gameObject.SetActive(false);

                // 준비 중
                _endSeasonGray.SetActive(false);
                _SettlementGray.SetActive(true);
                _grayTxt.text = "준비 중";
                break;
            default:
                break;
        }


    }
    
    public void OnClickGuildManaging()
    {
        if (guildManaging == null)
            guildManaging = UIManager.ShowUISubBase<UIGuildManaging>(UIManager.UIGuildHome, "UIGuildManaging");
        guildManaging.OpenToStack();
    }

    public void OnClickLeaveGuild()
    {
        if ((EGuildMemberType)userInfo.guildUserInfo.memberLevel == EGuildMemberType.MASTER)
            return;

        BestHttp_GameManager.OnGetGuildBossSchedule((schedule) =>
        {
            _guildBossSchedule = schedule;
            _eBossScheduleType = CheckBossScheduleType(_guildBossSchedule);

            if (_eBossScheduleType == EBossScheduleType.OnGoing)
            {
                UIManager.ShowCommonToastMessage("연합 토벌 기간에는 탈퇴하실 수 없습니다.");
                return;
            }

            UIManager.ShowConfirmPopUp("길드를 탈퇴 하시겠습니까?", "(길드 탈퇴 시 24시간동안 길드 가입 또는 생성이 불가능합니다.)", () =>
            {
                GuildIDRequestDto guildIDRequestDto = new GuildIDRequestDto();
                guildIDRequestDto.guildId = myGuildInfoDto.id;

                BestHttp_GameManager.OnGuildLeave(guildIDRequestDto, () =>
                {
                    
                    #if CHAT
                    // Chat Leave Guild
                    Managers.Instance.Chat.LeaveGuild(Managers.Instance.UserInfo().userId);
                    #endif
                    
                    _mainUI.ClickCloseBtn();
                });
            });
        });

    }

    public void OnClickGuildContent()
    {
        _eBossScheduleType = CheckBossScheduleType(_guildBossSchedule);
        switch (_eBossScheduleType)
        {
            case EBossScheduleType.Calculate:
                UIManager.ShowCommonToastMessage("괴수 토벌 정산 중입니다.\n정산 완료 후 보상은 우편으로 발송됩니다.");
                return;
            case EBossScheduleType.End:
                UIManager.ShowCommonToastMessage("시즌이 종료되었습니다. 다음 시즌에 다시 도전해주세요");
                return;
            default:
                break;
        }

        if(guildBoss == null)
            guildBoss = UIManager.ShowUISubBase<UIGuildBoss>(UIManager.UIGuildHome, "UIGuildBoss");
        guildBoss.Init(_guildBossSchedule);
        guildBoss.OpenToStack();
    }
    public void OnClickGuildShop()
    {
        UIManager.UIShop.SetCashShopOpenToStack(false);
    }
}
