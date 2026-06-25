// using Sirenix.OdinInspector;

using Best.HTTP.SecureProtocol.Org.BouncyCastle.Asn1.Crmf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayStatus : Status
{
    // percent 스텟까지 적용된 최종 status
    // public double ToTalAttack { get { return BigIntegerHelper.CalculateDouble(attackPower, 1 + attackPowerPer); } }
    // public double TotalHp { get { return BigIntegerHelper.CalculateDouble(maxHealthPoint, 1 + maxHealthPointPer); } }
    // public double TotalDefence { get { return BigIntegerHelper.CalculateDouble(defense, 1 + defensePer); } }

    // 인스펙터에서 확인용
    public double _hp;

    public float HpRatio => (float)(Hp / TotalMaxHp);
    
    public double Hp
    {
        get => _hp;
        set => _hp = value;
    }

    public double MaxHp
    {
        get => _maxHp;
        set => _maxHp = value;
    }

    public double Attack
    {
        get => _attack;
        set => _attack = value;
    }

    public double Defense
    {
        get => _defense;
        set => _defense = value;
    }

    public double CriticalChance
    {
        get => _criticalChance;
        set => _criticalChance = value;
    }

    public double CriticalDefense
    {
        get => _criticalDefense;
        set => _criticalDefense = value;
    }

    public double CriticalMultiplier
    {
        get => _criticalMultiplier;
        set => _criticalMultiplier = value;
    }

    public double ReduceDamage
    {
        get => _reduceDamage;
        set => _reduceDamage = value;
    }

    public double RiseDamage
    {
        get => _riseDamage;
        set => _riseDamage = value;
    }

    public double PenetrationDef
    {
        get => _penetrationDef;
        set => _penetrationDef = value;
    }

    public double MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = value;
    }

    public double MoveTargetSearchRange
    {
        get => _moveTargetSearchRange;
        set => _moveTargetSearchRange = value;
    }

    public double TotalAttack => _totalAttack;
    public double TotalDefense => _totalDefense;
    public double TotalMaxHp => _totalMaxHp;
}