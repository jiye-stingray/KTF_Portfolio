using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class EventQuestItemData : ItemData
{
    public EQuestType _eventQuestType;
    public int _tableID;
    public int _progressValue;

    public EQuestConditionType _conditionType;

    public bool isClear => ReturnClearState();

    public bool isFinish;

    public T GetData<T>() where T : class
    {
        switch (_eventQuestType)
        {
            case EQuestType.Open:
                return ClientLocalDB_Simple.GetData<OpenEventQuest>(DBKey.OpenEventQuest, _tableID) as T;
            default:
                return null;
        }
    }

    private bool ReturnClearState()
    {
        switch (_eventQuestType)
        {
            case EQuestType.Open:
                var data = GetData<OpenEventQuest>();
                return data != null && data.ConditionValue != null && data.ConditionValue.Length > 0
                          && _progressValue >= data.ConditionValue[data.ConditionValue.Length - 1];
                
        }
        return false;
    }
}
