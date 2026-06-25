using Only1Games.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using static Define;

public class UIGuildCreate : UISubBase
{
    [SerializeField] private Image guildMark;
    [SerializeField] private TMP_InputField guildNameInputField;
    [SerializeField] private UIDropDownMenu guildLevelDropDownMenu;
    [SerializeField] private UIDropDownMenu guildApprovalTypeDropDownMenu;

    [SerializeField] private UICostButton btn_CreateGuild;

    int guildPattern = 1;
    UIGuildIconChanger guildInfoPopup;
    BestHttp_GameManager bestHttp_GameManager => Managers.Instance.GetServerManager();
    UserInfoData userInfo => Managers.Instance.UserInfo();

    public enum ELevelCondition
    {
        NONE,
        Ten,
        twenty,
        thirty,
        Forty,
        Fifty,
    }
    ELevelCondition eCurrentLevelType;
    EGuildApprovalType eCurrentGuildApprovalType;
    /*
    * *
    */
    public override void OpenToStack()
    {
        base.OpenToStack();
        eCurrentLevelType = ELevelCondition.NONE;
        eCurrentGuildApprovalType = EGuildApprovalType.NONE;

        guildLevelDropDownMenu.Set((int)eCurrentLevelType);
        guildApprovalTypeDropDownMenu.Set((int)eCurrentGuildApprovalType - 1);
        guildNameInputField.characterLimit = 10;
        btn_CreateGuild.Init(new ECurrency[] { ECurrency.Cash_Free }, new int[] { 300 });
    }
    
    public void OnClickGuildMark()
    {
        if (guildInfoPopup == null)
            guildInfoPopup = Managers.Instance.GetUIManager().ShowUISubBase<UIGuildIconChanger>(mainUI, "UIGuildIconChanger");
        guildInfoPopup.OpenToStack();
        guildInfoPopup.SetData(1,(sprite,index) =>
        {
            guildMark.sprite = sprite;
            guildPattern = index + 1;
        });
    }
    public void OnClickGuildLevelCondition()
    {
        eCurrentLevelType = (ELevelCondition)guildLevelDropDownMenu.CurrentIndex;
    }
    public void OnClickGuildApproval()
    {
        eCurrentGuildApprovalType = (EGuildApprovalType)(guildApprovalTypeDropDownMenu.CurrentIndex + 1);
    }
    public void OnClickCreateGuild()
    {
        string inputGuildName = guildNameInputField.text;
        
        // 길이 체크 (빈 값 포함)
        if (string.IsNullOrEmpty(inputGuildName) || inputGuildName.Length < 2)
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("길드 이름은 2글자 이상이어야 합니다", "ToastMessage");
            return;
        }

        // 공백 체크
        if (inputGuildName.Contains(" "))
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("길드 이름은 공백을 포함할 수 없습니다", "ToastMessage");
            return;
        }

        // 금칙어 체크
        if (Utils.CheckBanText(inputGuildName))
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("길드 이름에 사용할 수 없는 단어가 포함되어 있습니다", "ToastMessage");
            return;
        }

        // 특수문자 및 이모지 체크 (정규식)
        if (!Regex.IsMatch(inputGuildName, @"^[\p{L}\p{N}]+$"))
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("길드 이름에는 특수문자나 이모지를 포함할 수 없습니다", "ToastMessage");
            return;
        }
        
        
        if (userInfo.GetCurrencyValue(ECurrency.Cash_Free) < 300)
            return;
        GuildInfoRequestDto guildInfoRequestDto = new GuildInfoRequestDto();
        guildInfoRequestDto.approvalType = (int)eCurrentGuildApprovalType;
        guildInfoRequestDto.name = guildNameInputField.text;
        guildInfoRequestDto.guildPattern = guildPattern;
        guildInfoRequestDto.minLevel = (int)eCurrentLevelType;

        bestHttp_GameManager.OnPostCreateGuild(guildInfoRequestDto, () =>
        {
            mainUI.ClickCloseBtn();
        });
    }
}
