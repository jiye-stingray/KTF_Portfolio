using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapProgressRewardButton : EventRewardButton
{
    [SerializeField] GameObject _grayImg;
    [SerializeField] GameObject _redDot;
    FieldQuestReward _data;
    int _progress;
    public void SetData(int progress,FieldQuestReward itemdata)
    {
        _progress = progress;
        RewardData rewardData = new RewardData()
        {
            ID = itemdata.ID,      // not use
            RewardType = itemdata.RewardType,
            RewardId = itemdata.RewardID,
            RewardValue = itemdata.RewardValue,

        };
        _rewardData = rewardData;

        _data = itemdata;
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();

        // lock Check 하기
        _grayImg.SetActive(_progress > userinfo._dicFieldItemData[_data.FieldID].progress);
        _GetImg.SetActive(userinfo._dicFieldItemData[_data.FieldID].isGet[_data.Index]);

        _redDot.SetActive(RedDotManager.FieldQuestRewardRedDot(_data));

    }

    public override void Click()
    {
        if (_grayImg.activeSelf || _GetImg.activeSelf) return;

        // 획득 서버 통신 & toastMessage

#if USE_SERVER
        Managers.Instance.GetServerManager().OnPostGetProgressReward(_data.Index);
#else

        uimanager.ShowCommonToastMessage("우편을 통해 보상이 지급되었습니다.");
        userinfo._dicFieldItemData[_data.FieldID].isGet[_data.Index] = true;
        uimanager.UIMinimap._pages.AreaMinimapTab.RefreshMapProgress();
#endif

    }
}
