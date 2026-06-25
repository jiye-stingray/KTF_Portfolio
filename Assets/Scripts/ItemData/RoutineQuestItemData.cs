using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class RoutineQuestItemData : ItemData
{
    public EQuestConditionType _conditionType;
    public int _tableId;
    public EResetType _resetType;
    public int _progressValue;      // 진행도

    public bool isClear => TableQuestDB().ConditionValue.Last() <= _progressValue;
    public bool isFinish;

    public RoutineQuest TableQuestDB()
    {
        RoutineQuest quest = new RoutineQuest();
        quest = ClientLocalDB_Simple.GetData<RoutineQuest>(DBKey.RoutineQuest, _tableId);
        return quest;
    }

}
