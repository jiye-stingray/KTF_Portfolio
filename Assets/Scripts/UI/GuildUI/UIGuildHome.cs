using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using static Define;

public class UIGuildHome : UIBase
{

    public enum ETab
    {
        Main,
        Member
    }
    ETab eCurrentTab;

    [Serializable]
    public struct STab
    {
        public UIGuildHomeMainPage mainPage;
        public UIGuildHomeMemberPage memberPage;
    }
    public STab sTab;

    
    [SerializeField] private UITabGroup group;
    [SerializeField] private UIButtonBase btn_Attendance;
    [SerializeField] private UIGuildInfo guildInfo;

    bool IsTodayJoin = false;
    bool EnableAttendance = false;
    
    GuildInfoDto myGuildInfoDto => userInfo.guildInfo;
    UserInfoData userInfo => Managers.Instance.UserInfo();
    /*
     * *
     */

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        sTab.mainPage.Init();
        sTab.memberPage.Init();
        return true;
    }
    public override void OpenToStack()
    {
        eCurrentTab = ETab.Main;

        base.OpenToStack();
        Refresh();
    }
    public override void Refresh()
    {
        BestHttp_GameManager.OnRequestMyGuildInfo(() =>
        {
            if (userInfo.guildInfo == null)
            {
                Debug.LogError("GuildInfo null");
                return;
            }
            group.Set((int)eCurrentTab);
            DrawGuild();
            base.Refresh();

        });



    }

    private void DrawGuild()
    {

        guildInfo.Refresh(myGuildInfoDto);

        string serverTimeStr = ServerTime.Instance.CurrentTime().ToString("yyyy-MM-dd");
        string joinTime = Convert.ToDateTime(userInfo.guildUserInfo.joinDate).ToString("yyyy-MM-dd");

        IsTodayJoin = Utils.GetDateTimeCompare(joinTime, serverTimeStr) == 0;
        EnableAttendance = Utils.GetDateTimeCompare(userInfo.guildUserInfo.attendanceDate, serverTimeStr) < 0;
        btn_Attendance.SetGray(IsTodayJoin || !EnableAttendance);
    }

    public void OnClickAttendence()
    {
        GuildIDRequestDto requestDto = new GuildIDRequestDto();
        requestDto.guildId = myGuildInfoDto.id;
        BestHttp_GameManager.OnGuildAttendance(requestDto, (RewardDto) =>
        {
            // 팝업
            UIManager.ShowRewardPopup(RewardDto).Forget();

            UserInfoData.UpdateCorrectQuest(EQuestType.Open, EQuestConditionType.GuildAttend, 1);
            UserInfoData.UpdateCorrectQuest(EQuestType.Weekly, EQuestConditionType.GuildAttend, 1);
            Refresh();
        });
    }
    public void OnChangeTab()
    {
        eCurrentTab = (ETab)group._currentTapGroupBtn._index;
        switch (eCurrentTab)
        {
            case ETab.Main:
                sTab.memberPage.Close();
                sTab.mainPage.Open();
                break;
            case ETab.Member:
                sTab.mainPage.Close();
                sTab.memberPage.Open();
                break;
        }
    }
    
}
