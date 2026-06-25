using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Define;

public class UIGuildApprovalScrollItem : ICell
{
    [SerializeField] private TMP_Text txtUserName;
    [SerializeField] private TMP_Text txtUserLv;
    [SerializeField] private Image thumbnail;
    [SerializeField] private Image frameImage;

    public GuildApprovalItemData _data;
    UnityAction<UIGuildApprovalScrollItem> _acceptAction;
    UnityAction<UIGuildApprovalScrollItem> _refuseAction;

    /*
     * *
     */

    public void SetData(ItemData data, int index, UnityAction<UIGuildApprovalScrollItem> acceptAction, UnityAction<UIGuildApprovalScrollItem> refuseAction)
    {
        base.SetData(data, index);
        _data = data as GuildApprovalItemData;
        _acceptAction = acceptAction;
        _refuseAction = refuseAction;
        Refresh();
    }
    public void Refresh()
    {
        txtUserName.text = _data.memberInfo.userGameName;
        txtUserLv.text = $"Lv.{_data.memberInfo.level}";
        thumbnail.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.CharacterIconAtlas,
                $"Thum_SD_Cr_{_data.memberInfo.thumbnail.ToString("000")}");
        if (frameImage)
        {
            var frameSprite = _data.memberInfo.frame <= 0 ? null
                : Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.FrameAtlas, $"FrameImg_{_data.memberInfo.frame.ToString("000")}");
            frameImage.sprite = frameSprite;
            frameImage.gameObject.SetActive(frameSprite != null);
        }

    }
    public void OnClickAccept()
    {
        _acceptAction?.Invoke(this);
    }
    public void OnClickRefuse()
    {
        _refuseAction?.Invoke(this);
    }
}
