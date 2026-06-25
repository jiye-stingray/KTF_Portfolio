using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class AttendanceEventRewardButton : EventRewardButton
{
    [SerializeField] GameObject _gray;
    [SerializeField] Image _todayImg;
    [SerializeField] TMP_Text _countTxt;

    EventRewardItemData _data;
    EAttendanceEventType _type;
    public void SetData(EventRewardItemData data, EAttendanceEventType type)
    {
        _data = data;
        _type = type;

        // reward Data 받기
        AttendanceData dbData = new AttendanceData();
        switch (type)
        {
            case EAttendanceEventType.Weekly:
                dbData = ClientLocalDB_Simple.GetData<AttendanceData>(DBKey.WeeklyAttendance, _data.id);
                break;
            case EAttendanceEventType.Monthly:
                dbData = ClientLocalDB_Simple.GetData<AttendanceData>(DBKey.MonthlyAttendance, _data.id);
                break;

            case EAttendanceEventType.New:
                dbData = ClientLocalDB_Simple.GetData<AttendanceData>(DBKey.NewAttendance, _data.id);
                break;
            default:
                break;
        }
        _rewardData = ClientLocalDB_Simple.GetData<RewardData>(DBKey.AttendanceReward,dbData.RewardID);

        Refresh();
    }

    public override void Refresh()
    {

        base.Refresh();

        _GetImg.SetActive(_data.isGet);
        _gray.SetActive(_data.isGet);
        _countTxt.text = $"{_data.id}일차";

        switch (_type)
        {
            case EAttendanceEventType.Weekly:
            case EAttendanceEventType.New:
                _todayImg.gameObject.SetActive(userinfo._weeklyAttendanceCount == _data.id);
                break;
            case EAttendanceEventType.Monthly:
                _todayImg.gameObject.SetActive(userinfo._monthlyAttendanceCount == _data.id);
                break;
            default:
                break;
        }
    }

    public override void Click()
    {
        
    }
}
