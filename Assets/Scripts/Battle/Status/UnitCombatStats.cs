public class UnitCombatStats
{
    public int _id; // 유닛 이름 또는 ID

    public double _damageDealt { get; private set; } // 딜량
    public double _damageTaken { get; private set; } // 받은 피해
    public double _healingDone { get; private set; } // 힐량

    public int _killCount { get; private set; } // 킬 횟수


    public UnitCombatStats(int id)
    {
        _id = id;
        ResetStats();
    }

    public void AddDamageDealt(double amount)
    {
        _damageDealt += amount;
    }

    public void AddDamageTaken(double amount)
    {
        _damageTaken += amount;
    }

    public void AddHealingDone(double amount)
    {
        _healingDone += amount;
    }

    public void AddKillCount(int amount)
    {
        _killCount += amount;
    }

    public void ResetStats()
    {
        _damageDealt = 0;
        _damageTaken = 0;
        _healingDone = 0;
        _killCount = 0;
    }
}