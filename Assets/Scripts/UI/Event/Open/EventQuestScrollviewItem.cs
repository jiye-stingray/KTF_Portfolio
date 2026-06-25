using PolyAndCode.UI;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class EventQuestScrollviewItem : ICell
{
    [SerializeField] RewardItem _rewardItem;
    [SerializeField] TMP_Text _descTxt;
    [SerializeField] Slider _slider;

    [SerializeField] GameObject _clear;
    [SerializeField] GameObject _finish;

    EventQuestItemData _eventQuestItemData;
    int _conditionID;

    [Header("Point")]
    [SerializeField] bool _isPoint;     // UI Prefab 에서 셋팅
    private int _day;
    OpenEventPoint _openEventPointDB;

    public override void SetData(ItemData data, int index)
    {
        _eventQuestItemData = data as EventQuestItemData;
        _index = index;
        base.SetData(data, index);

        Refresh();
    }

    /// <summary>
    /// _isPoint가 true일때만 호출 
    /// UI 에서 변수로 호출
    /// </summary>
    /// <param name="day"></param>
    public void SetEventPoint(int day)
    {
        _day = day;
        _openEventPointDB = ClientLocalDB_Simple.GetData<OpenEventPoint>(DBKey.OpenEventPoint, _day);
        Refresh();
    }

    private void Refresh()
    {
        if(!_isPoint)
        {
            var data = _eventQuestItemData.GetData<OpenEventQuest>();
            _conditionID = data.ConditionValue.Last();
            EQuestConditionType conditionType = data.ConditionType;

            _descTxt.text = DescBuilder.ReturnQuestDesc(conditionType, data.Desc, data.ConditionValue);
            _slider.value = (float)_eventQuestItemData._progressValue / data.ConditionValue.Last();

            var rewardData = ClientLocalDB_Simple.GetData<OpenEventReward>(DBKey.OpenEventReward, data.RewardID);
            _rewardItem.Init(rewardData.RewardType, rewardData.RewardID, rewardData.RewardValue);

            _clear.SetActive(_eventQuestItemData.isClear && !_eventQuestItemData.isFinish);
            _finish.SetActive(_eventQuestItemData.isFinish);

        }
        else
        {
            _descTxt.text = $"{_day}일차 임무 모두 완료!";

            var dicQuests = Managers.Instance.UserInfo()._dicOpenEventQuestItemData;
            int finishedCount = dicQuests.TryGetValue(_day, out var dayQuests)
                ? dayQuests.Values.Count(q => q.isFinish)
                : 0;
            _slider.value = (float)finishedCount / _openEventPointDB.Count;

            var rewardData = ClientLocalDB_Simple.GetData<QuestReward>(DBKey.QuestReward, _openEventPointDB.RewardID);
            _rewardItem.Init(rewardData.RewardType.First(), rewardData.RewardID.First(), rewardData.RewardValue.First());

            _clear.SetActive(finishedCount >= _openEventPointDB.Count);
            _finish.SetActive(UserInfo._openEventDayCompleted[_day - 1]);       // index
        }
    }

    public void Click()
    {
        if(_finish.activeSelf)
        {
            UIManager.ShowCommonToastMessage("이미 완료한 임무입니다.");
            return;
        }
        else if(_clear.activeSelf)
        {
            // 서버 통신 
            if(!_isPoint)
            {
                Managers.Instance.GetServerManager().OnPostNewbQuestGetQuestReward(_eventQuestItemData._tableID);
                return;

            }
            else
            {
                Managers.Instance.GetServerManager().OnPostNewbQuestGetQuestPointReward(_day);
                return;
            }
        }


        if (!_isPoint)
            Managers.Instance.UserInfo().QuestMoveAction(_eventQuestItemData._conditionType, _conditionID);
        else
            UIManager.ShowCommonToastMessage("임무를 모두 완료하세요.");

    }
}
