public class GatherResUnitStatusInfo : StatusInfo
{
    /// <summary>
    /// Level로 올라가는 스테이터스 보정값 적용
    /// </summary>
    public void SetLevelStatus(double hp)
    {
        _levelStatus._maxHp = hp;

        SetLevelStatus();
    }
}