using Assets.SimpleSignIn.Google.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UIGuildInfoJoin : UISubBase
{
    [SerializeField] private UIGuildInfo guildInfo;
    [SerializeField] private GameObject btn_GuildJoin;

    GuildInfoDto _selectedGuildDto;
    BestHttp_GameManager bestHttp_GameManager => Managers.Instance.GetServerManager();
    UserInfoData userInfo => Managers.Instance.UserInfo();
    /*
     * *
     */
    public void SetGuildInfoDto(GuildInfoDto guildInfoDto)
    {
        _selectedGuildDto = guildInfoDto;
        Refresh();
    }
    public override void Refresh()
    {
        base.Refresh();
        guildInfo.Refresh(_selectedGuildDto);
    }

    // 길드 가입 신청 버튼 클릭
    public void OnGuildApplyButtonClick()
    {
        EGuildApprovalType approvalType = (EGuildApprovalType)_selectedGuildDto.approvalType;
        switch (approvalType)
        {
            case EGuildApprovalType.NONE:
            case EGuildApprovalType.APPROVAL:
                CheckGuildJoin(approvalType);
                break;
            case EGuildApprovalType.UNABLE:
                Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("가입 불가한 연합입니다.", "ToastMessage");
                break;
        }
    }

    private void CheckGuildJoin(EGuildApprovalType type)
    {
        if (!CanJoinGuldTime(userInfo.guildUserInfo.secessionDate))
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("연합 탈퇴 후 24시간동안\n연합가입이 제한됩니다.", "ToastMessage");
            return;
        }
        if (_selectedGuildDto.minLevel> userInfo.userLevel.Value)
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("가입할 수 없는 연합입니다 .", "ToastMessage");
            return;
        }

        switch (type)
        {
            case EGuildApprovalType.NONE:
                GuildJoin();
                break;
            case EGuildApprovalType.APPROVAL:
                GuildJoinRequest();
                break;
        }
    }


    ////길드 가입 신청 - 승인 가입

    public void GuildJoinRequest()
    {
        if (GuildApplyCheck(_selectedGuildDto.id) == true)
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("가입 대기 중 입니다.", "ToastMessage");
            return;
        }
        GuildIDRequestDto applyRequestDto = new GuildIDRequestDto();
        applyRequestDto.guildId = _selectedGuildDto.id;

        bestHttp_GameManager.OnPostRequestJoinGuild(applyRequestDto, (a) =>
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("연합 가입이 신청되었습니다.", "ToastMessage");
            ClickCloseBtn();
            mainUI.ClickCloseBtn();
        });

    }
    ////길드 가입 - 자유 가입
    public void GuildJoin()
    {
        if (_selectedGuildDto.joinNum >= _selectedGuildDto.limNum)
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("인원이 가득 찬 연합입니다.", "ToastMessage");
            return;
        }

        GuildIDRequestDto applyRequestDto = new GuildIDRequestDto();
        applyRequestDto.guildId = _selectedGuildDto.id;

        bestHttp_GameManager.OnPostJoinGuild(applyRequestDto, (a) =>
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("연합 가입이 완료되었습니다.", "ToastMessage");
            ClickCloseBtn();
            mainUI.ClickCloseBtn();
        });
    }

    // 이미 가입 신청한 길드인지 체크
    private bool GuildApplyCheck(int id)
    {
        foreach (var guildId in userInfo.guildRequestList)
        {
            if (guildId == id)
                return true;
        }

        return false;
    }

    private bool CanJoinGuldTime(string secessionDate)
    {
        // 탈퇴 기록이 없으면 바로 가입 가능
        if (string.IsNullOrEmpty(secessionDate))
            return true;

        DateTime secessionTime;
        if (!DateTime.TryParse(secessionDate, out secessionTime))
        {
            Debug.LogError($"Invalid secessionDate format: {secessionDate}");
            return true; // 파싱 실패 시 막아버리면 UX 최악이라 보통 허용
        }

        DateTime now = ServerTime.Instance.CurrentTime();

        TimeSpan elapsed = now - secessionTime;

        return elapsed.TotalHours >= 24;
    }
}
