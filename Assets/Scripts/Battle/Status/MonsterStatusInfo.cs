

using System;

[Serializable]
public class MonsterStatusInfo : StatusInfo
{
    /// <summary>
    /// Level로 올라가는 스테이터스 보정값 적용
    /// </summary>
    public void SetLevelStatus(int level, StatusData statusData, GrowStatusData growStatusData, Status addStatus)
    {
        _level = level;
        _growthStatus.Reset();
        _statusData = statusData;
        _growStatusData = growStatusData;
        
        _levelStatus = CalculateStatus.GetMonsterLevelStatus(level, _statusData, _growStatusData);
        SetLevelStatus();
        _growthStatus += addStatus;
        ResetPlayStatus();
        ApplyPlayHealth();
    }
}