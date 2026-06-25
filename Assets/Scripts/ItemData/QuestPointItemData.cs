using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class QuestPointItemData : ItemData
{
    public int _point;
    public EResetType _resetType;

    public bool isClear => ReturnClear();
    public bool isFinish;

    bool ReturnClear()
    {
        switch (_resetType)
        {
            case EResetType.Daily:
                return Managers.Instance.UserInfo()._dailyQuestPoint >= _point;
            case EResetType.Weekly:
                return Managers.Instance.UserInfo()._weeklyQuestPoint >= _point;
        }
        return false;
    }

    public QuestPoint _questPointData => ClientLocalDB_Simple.GetData<QuestPoint>(DBKey.QuestPoint, $"{_point}_{_resetType}");
}
