using System;

[Serializable]
public class StatusInfo
{
    // 레벨이 적용된 능력치
    public Status _levelStatus = new Status();

    // 계산 완료된 능력치
    public Status _growthStatus = new Status();

    // 플레이 능력치
    public PlayStatus _playStatus = new PlayStatus();

    // 플레이 중 버프, 디버프 와 같이 변동 값의 보정을 위함
    public Status _sumStatus = new Status();

    public int _level;
    public int _grade;
    public double _battlePower;

    public StatusData _statusData;
    protected GrowStatusData _growStatusData;
    protected GrowStatusData _gradeGrowStatusData;
    protected UnitData _unitData;

    public virtual void SetLevelStatus()
    {
        _growthStatus.Set(_levelStatus);
    }
    
    public virtual void ResetPlayStatus()
    {
        _playStatus.Set(_growthStatus + _sumStatus);
        CalculatePercentStatus(_playStatus);
    }

    public void ApplyPlayHealth()
    {
        _playStatus.Hp = _playStatus.TotalMaxHp; // 체력까지 초기화
    }

    public void ResetSumStatus()
    {
        _sumStatus.Reset();
    }

    protected void CalculatePercentStatus(Status status)
    {
        status._totalAttack = Math.Floor(status._attack * (1 + CalculateStatus.ToRate(status._attackPercent)));
        status._totalMaxHp = Math.Floor(status._maxHp * (1 + CalculateStatus.ToRate(status._maxHpPercent)));
        status._totalDefense = Math.Floor(status._defense * (1 + CalculateStatus.ToRate(status._defensePercent)));
    }
}