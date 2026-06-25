using UnityEngine;
using static Define;

public class GuildBossUnit : EnemyUnit
{
    public override void Init()
    {
        base.Init();
        _collider.isTrigger = true;
        IsStatusImmune = true;
    }
}