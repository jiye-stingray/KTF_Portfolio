using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIGuildApprovalManagePage : UITabBase, IRecyclableScrollRectDataSource
{
    [SerializeField] private RecyclableScrollRect scrollView;
    
    [SerializeField] private TMP_Text txtMemberCount;
    [SerializeField] private GameObject _nullImg;

    UserInfoData userInfoData => Managers.Instance.UserInfo();
    
    List<GuildApprovalItemData> _dataList = new List<GuildApprovalItemData>();

    #region ScrollView
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as UIGuildApprovalScrollItem;
        item.SetData(_dataList[index], index, OnClickAcceptMember, OnClickRefuseMember);
    }
    #endregion

    public override void Open()
    {
        base.Open();


        BestHttp_GameManager.OnRequestGuildApprovalMemberList((approvalList) =>
        {
            _dataList.Clear();
            for (int i = 0; i < approvalList.Count; i++)
            {
                GuildApprovalItemData guildApprovalItemData = new GuildApprovalItemData();
                guildApprovalItemData.memberInfo = approvalList[i];
                _dataList.Add(guildApprovalItemData);
            }
            scrollView.Initialize(this);
            txtMemberCount.text = $"{_dataList.Count}/{100}";

            _nullImg.SetActive(_dataList.Count <= 0);
        });

    }

    public void OnClickAcceptMember(UIGuildApprovalScrollItem _item)
    {
        GuildRequesterIDRequestDto guildRequesterIDRequestDto = new GuildRequesterIDRequestDto();
        guildRequesterIDRequestDto.guildId = userInfoData.guildInfo.id;
        guildRequesterIDRequestDto.requester = _item._data.memberInfo.useridUser;

        BestHttp_GameManager.OnRequestApprovalConfirm(guildRequesterIDRequestDto, () =>
        {
            _dataList.RemoveAt(_item._index);
            scrollView.Initialize(this);

            UIManager.UIGuildHome.Refresh();
        });

    }
    public void OnClickRefuseMember(UIGuildApprovalScrollItem _item)
    {
        GuildRequesterIDRequestDto guildRequesterIDRequestDto = new GuildRequesterIDRequestDto();
        guildRequesterIDRequestDto.guildId = userInfoData.guildInfo.id;
        guildRequesterIDRequestDto.requester = _item._data.memberInfo.useridUser;

        BestHttp_GameManager.OnRequestApprovalReject(guildRequesterIDRequestDto, () =>
        {
            _dataList.RemoveAt(_item._index);
            scrollView.Initialize(this);
        });
    }
}
