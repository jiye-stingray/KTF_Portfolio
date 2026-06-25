using UnityEngine;
using static Define;

public class RankingBossUnit : EnemyUnit
{
    public override void Init()
    {
        base.Init();
        _collider.isTrigger = true;
        IsInvincible = true;
        IsStatusImmune = true;
    }
}