using PolyAndCode.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MailScrollviewItem : ICell
{
    [SerializeField] private RewardItem rewardIcon;
    [SerializeField] private TMP_Text txtTitle;
    [SerializeField] private TMP_Text txtItemInfo;
    [SerializeField] private TMP_Text txtRemainTime;
    [SerializeField] private TMP_Text txtReceiveTxt;
    [SerializeField] GameObject _nullImg;

    public MailItemData _data;

    UnityAction<MailScrollviewItem> _mailReceiveAction;
    UnityAction<MailScrollviewItem> _mailRemoveAction;

    /*
     * *
     */

    public void SetData(ItemData data, int index, UnityAction<MailScrollviewItem> mailReceiveAction, UnityAction<MailScrollviewItem> mailRemoveAction)
    {
        base.SetData(data, index);
        _data = data as MailItemData;
        _index = index;

        _mailReceiveAction = mailReceiveAction;
        _mailRemoveAction = mailRemoveAction;
        _nullImg.SetActive(_data.rewardType == Define.ERewardType.None);
        if (_data.rewardType != Define.ERewardType.None)
        {
            rewardIcon.gameObject.SetActive(true);
            if(_data.rewardType == Define.ERewardType.Equipment)
                rewardIcon.Init(_data.rewardType, _data.rewardTableID, _data.rewardCount, _data.synergyCode);
            else
                rewardIcon.Init(_data.rewardType,_data.rewardTableID, _data.rewardCount, _data.synergyCode);

            txtReceiveTxt.text = "받기";
        }
        else
        {
            rewardIcon.gameObject.SetActive(false);
            txtReceiveTxt.text = "확인";
        }

            Refresh();

    }
    public void Refresh()
    {
        txtTitle.text = _data.mailTitle;
    }
    public void Update()
    {
        if (_data.mailType == 2)
            return;

        DateTime ServerCurrentTime = ServerTime.Instance.CurrentTime();
        TimeSpan timeCal;
        timeCal = _data.expireDate - ServerCurrentTime;
        if (0 > timeCal.TotalSeconds)
        {
            _mailRemoveAction.Invoke(this);
            return;
        }

        txtRemainTime.text = $"{timeCal.Days} : {timeCal.Hours} : {timeCal.Minutes}";
    }

    public void OnClickReceiveMail()
    {
        _mailReceiveAction?.Invoke(this);
    }
}
