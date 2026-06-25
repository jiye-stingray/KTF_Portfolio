using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class RoutineQuestFinishPanel : MonoBehaviour
{
    [SerializeField] TMP_Text txtInitTime;
    EResetType resetType;
    public void Init(EResetType _resetType)
    {
        resetType = _resetType;
    }
    public void Update()
    {
        DateTime now = ServerTime.Instance.CurrentTime();

        switch (resetType)
        {
            case EResetType.Daily:
                DateTime midnight = now.Date.AddDays(1);
                TimeSpan remain = midnight - now;
                if(txtInitTime != null) txtInitTime.text = $"초기화 까지 {remain.Hours}h {remain.Minutes}m";
                break;
            case EResetType.Weekly:
                int daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
                if (daysUntilNextMonday == 0)
                    daysUntilNextMonday = 7; // 이미 월요일이면 다음 주 월요일로 설정

                DateTime nextMondayMidnight = now.Date.AddDays(daysUntilNextMonday);
                TimeSpan weeklyRemain = nextMondayMidnight - now;
                if (txtInitTime != null) txtInitTime.text = $"초기화 까지 {weeklyRemain.Days}d {weeklyRemain.Hours}h {weeklyRemain.Minutes}m";
                break;
        }
    }
}
