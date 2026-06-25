using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Define;

public class UIGuildMemberDetail : UISubBase
{
    [SerializeField] private TMP_Text txtUserName;
    [SerializeField] private TMP_Text txtUserLevel;
    [SerializeField] private TMP_Text txtUserPoint;
    [SerializeField] private Image thumbnailSprite;
    [SerializeField] private Image frameImage;

    [SerializeField] GameObject _btn_masterChange;
    [SerializeField] GameObject _btn_masterChange_off;
    [SerializeField] GameObject _btn_submasterChange;
    [SerializeField] GameObject _btn_submasterchange_off;
    [SerializeField] GameObject _btn_submasterImpeachment;
    [SerializeField] private GameObject btn_MemberOut;
    [SerializeField] private GameObject btn_MemberOut_Off;

    UIGuildMemberScrollItem selectedItem;
    UnityAction<UIGuildMemberScrollItem> guildMasterChangeAction;
    UnityAction<UIGuildMemberScrollItem> guildSubMasterChangeAction;
    UnityAction<UIGuildMemberScrollItem> guildSubMasterImpeachmentAction;
    UnityAction<UIGuildMemberScrollItem> guildMemberOutAction;

    bool isAttendanceLongTimeAgo;

    UserInfoData userinfoData => Managers.Instance.UserInfo();

    /*
     * *
     */
    
    public void OpenToStack(UIGuildMemberScrollItem _memberInfo, bool _isAttendanceLongTimeAgo,
                            UnityAction<UIGuildMemberScrollItem> _guildMasterChangeAction, UnityAction<UIGuildMemberScrollItem> _guildSubMasterChangeAction,
                            UnityAction<UIGuildMemberScrollItem> _guildSubMasterImpeachmentAction,
                            UnityAction<UIGuildMemberScrollItem> _guildMemberOutAction)
    {
        base.OpenToStack();
        selectedItem = _memberInfo;
        isAttendanceLongTimeAgo = _isAttendanceLongTimeAgo;
        guildMasterChangeAction = _guildMasterChangeAction;
        guildSubMasterChangeAction = _guildSubMasterChangeAction;
        guildSubMasterImpeachmentAction = _guildSubMasterImpeachmentAction;
        guildMemberOutAction = _guildMemberOutAction;
        Refresh();
    }
    public override void Refresh()
    {
        base.Refresh();
        txtUserName.text = selectedItem._data.memberInfo.userGameName;
        txtUserLevel.text = "Lv." + selectedItem._data.memberInfo.level.ToString();
        txtUserPoint.text = $"{selectedItem._data.memberInfo.contribution}";
        thumbnailSprite.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.CharacterIconAtlas,
                $"Thum_SD_Cr_{selectedItem._data.memberInfo.thumbnail.ToString("000")}");
        if (frameImage)
        {
            var frameSprite = selectedItem._data.memberInfo.frame <= 0 ? null
                : Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.FrameAtlas, $"FrameImg_{selectedItem._data.memberInfo.frame.ToString("000")}");
            frameImage.sprite = frameSprite;
            frameImage.gameObject.SetActive(frameSprite != null);
        }
        EGuildMemberType itemMemberType = (EGuildMemberType)selectedItem._data.memberInfo.memberLevel;
        EGuildMemberType myMemberType = (EGuildMemberType)userinfoData.guildUserInfo.memberLevel;

        UpdateGuildButtons(myMemberType, itemMemberType, isAttendanceLongTimeAgo);

    }
    void UpdateGuildButtons(EGuildMemberType myMemberType, EGuildMemberType itemMemberType, bool isAttendanceLongTimeAgo)
    {
        // 모든 버튼 기본값 off
        _btn_submasterImpeachment.SetActive(false);
        _btn_submasterChange.SetActive(false);
        _btn_masterChange_off.SetActive(false);
        _btn_masterChange.SetActive(false);
        _btn_submasterchange_off.SetActive(false);
        btn_MemberOut.SetActive(false);
        btn_MemberOut_Off.SetActive(false);

        // 나와 같은 등급이거나 NONE → 모든 버튼 off
        if (myMemberType == itemMemberType || itemMemberType == EGuildMemberType.NONE)
        {
            _btn_submasterchange_off.SetActive(true);
            _btn_masterChange_off.SetActive(true);
            btn_MemberOut_Off.SetActive(true);
            return;
        }


        // 내 등급별 처리
        switch (myMemberType)
        {
            case EGuildMemberType.MASTER:
                btn_MemberOut.SetActive(true);

                if (itemMemberType == EGuildMemberType.SUBMASTER)
                {
                    // 부길드원 → 강퇴만 가능
                    _btn_masterChange_off.SetActive(true);
                    _btn_submasterImpeachment.SetActive(true);
                }
                else
                {
                    // 일반 길드원 → 강퇴, 교체 다 가능
                    btn_MemberOut.SetActive(true);
                    _btn_masterChange.SetActive(true);
                    _btn_submasterChange.SetActive(true);
                }
                break;

            case EGuildMemberType.SUBMASTER:
                if (itemMemberType == EGuildMemberType.MASTER)
                {
                    _btn_submasterchange_off.SetActive(true);
                    _btn_masterChange_off.SetActive(true);
                    btn_MemberOut_Off.SetActive(true);
                }
                else
                {
                    _btn_submasterchange_off.SetActive(true);
                    _btn_masterChange_off.SetActive(true);
                    btn_MemberOut.SetActive(true);
                }
                break;

            case EGuildMemberType.MEMBER:
                _btn_submasterchange_off.SetActive(true);
                _btn_masterChange_off.SetActive(true);
                btn_MemberOut_Off.SetActive(true);
                break;
        }
    }

    public void OnClickGuildMasterChangeAction()
    {
        guildMasterChangeAction?.Invoke(selectedItem);
        ClickCloseBtn();
    }
    public void OnClickGuildSubMasterChangeAction()
    {
        guildSubMasterChangeAction?.Invoke(selectedItem);
        ClickCloseBtn();
    }
    public void OnClickGuildSubMasterImpeachment()
    {
        guildSubMasterImpeachmentAction?.Invoke(selectedItem);
        ClickCloseBtn();
    }
    public void OnClickGuildMemberOut()
    {
        guildMemberOutAction?.Invoke(selectedItem);
        ClickCloseBtn();
    }
}
