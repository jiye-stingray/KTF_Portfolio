using System;
using System.Text;
using UnityEngine.Serialization;
using static Define;

[Serializable]
public class Status
{
    public double _treeAttack;

    public double _maxHp;
    public double _maxHpPercent;
    public double _attack;
    public double _attackPercent;
    public double _defense;
    public double _defensePercent;
    public double _penetrationDef;
    public double _criticalDefense;
    public double _criticalChance;
    public double _criticalMultiplier;
    public double _reduceDamage;
    public double _riseDamage;
    public double _riseSkillDmg;

    #region 종족별 스텟 (랭킹던전용)

    // 받는 데미지
    public double _damageTakenFromHumanRate;
    public double _damageTakenFromGuardianRate;
    public double _damageTakenFromCrusherRate;
    public double _damageTakenFromCelestialRate;
    
    // 주는 데미지
    public double _damageDealtToHumanRate;
    public double _damageDealtToGuardianRate;
    public double _damageDealtToCrusherRate;
    public double _damageDealtToCelestialRate;

    #endregion

    
    public double _moveSpeed;
    public double _moveTargetSearchRange;

    public double _totalAttack;
    public double _totalDefense;
    public double _totalMaxHp;

    public virtual void Set(Status status)
    {
        _treeAttack = status._treeAttack;
        _maxHp = status._maxHp;
        _maxHpPercent = status._maxHpPercent;
        _attack = status._attack;
        _attackPercent = status._attackPercent;
        _defense = status._defense;
        _defensePercent = status._defensePercent;
        _penetrationDef = status._penetrationDef;
        _criticalDefense = status._criticalDefense;
        _criticalChance = status._criticalChance;
        _criticalMultiplier = status._criticalMultiplier;
        _reduceDamage = status._reduceDamage;
        _riseDamage = status._riseDamage;
        _riseSkillDmg = status._riseSkillDmg;
        
        _damageTakenFromHumanRate = status._damageTakenFromHumanRate;
        _damageTakenFromGuardianRate = status._damageTakenFromGuardianRate;
        _damageTakenFromCrusherRate = status._damageTakenFromCrusherRate;
        _damageTakenFromCelestialRate = status._damageTakenFromCelestialRate;
    
        _damageDealtToHumanRate = status._damageDealtToHumanRate;
        _damageDealtToGuardianRate = status._damageDealtToGuardianRate;
        _damageDealtToCrusherRate = status._damageDealtToCrusherRate;
        _damageDealtToCelestialRate = status._damageDealtToCelestialRate;

        _moveSpeed = status._moveSpeed;
        _moveTargetSearchRange = status._moveTargetSearchRange;
    }

    public void Reset()
    {
        _maxHp = 0;
        _maxHpPercent = 0;
        _attack = 0;
        _attackPercent = 0;
        _defense = 0;
        _defensePercent = 0;
        _penetrationDef = 0;
        _treeAttack = 0;
        _criticalChance = 0;
        _criticalDefense = 0;
        _criticalMultiplier = 0;
        _reduceDamage = 0;
        _riseDamage = 0;
        _riseSkillDmg = 0;
        
        _damageTakenFromHumanRate = 0;
        _damageTakenFromGuardianRate = 0;
        _damageTakenFromCrusherRate = 0;
        _damageTakenFromCelestialRate = 0;
    
        _damageDealtToHumanRate = 0;
        _damageDealtToGuardianRate = 0;
        _damageDealtToCrusherRate = 0;
        _damageDealtToCelestialRate = 0;

        _moveSpeed = 0;
        _moveTargetSearchRange = 0;
    }

    public void Plus(EStatus kind, double value)
    {
        switch (kind)
        {
            case EStatus.MaxHealthPoint:
                _maxHp += value;
                break;

            case EStatus.MaxHealthPointPercent:
                _maxHpPercent += value;
                break;

            case EStatus.Attack:
                _attack += value;
                break;
            case EStatus.AttackPercent:
                _attackPercent += value;
                break;
            case EStatus.Def:
                _defense += value;
                break;
            case EStatus.DefPercent:
                _defensePercent += value;
                break;
            case EStatus.PenetrationDef:
                _penetrationDef += value;
                break;
            case EStatus.CriticalChance:
                _criticalChance += value;
                break;
            case EStatus.CriticalDefense:
                _criticalDefense += value;
                break;
            case EStatus.CriticalMultiplier:
                _criticalMultiplier += value;
                break;
            case EStatus.ReduceDmg:
                _reduceDamage += value;
                break;
            case EStatus.RiseDmg:
                _riseDamage += value;
                break;
            case EStatus.RiseSkillDmg:
                _riseSkillDmg += value;
                break;
            case EStatus.ReduceHumanDamage:
                _damageTakenFromHumanRate += value;
                break;
            case EStatus.ReduceGuardianDamage:
                _damageTakenFromGuardianRate += value;
                break;
            case EStatus.ReduceCrusherDamage:
                _damageTakenFromCrusherRate += value;
                break;
            case EStatus.ReduceCelestialDamage:
                _damageTakenFromCelestialRate += value;
                break;
            case EStatus.RiseHumanDamage:
                _damageDealtToHumanRate += value;
                break;
            case EStatus.RiseGuardianDamage:
                _damageDealtToGuardianRate += value;
                break;
            case EStatus.RiseCrusherDamage:
                _damageDealtToCrusherRate += value;
                break;
            case EStatus.RiseCelestialDamage:
                _damageDealtToCelestialRate += value;
                break;
            default:
                break;
        }
    }

    public void Multiply(EStatus kind, double multiplier)
    {
        switch (kind)
        {
            case EStatus.MaxHealthPoint:
                _maxHp *= multiplier;
                break;

            case EStatus.MaxHealthPointPercent:
                _maxHpPercent *= multiplier;
                break;

            case EStatus.Attack:
                _attack *= multiplier;
                break;

            case EStatus.AttackPercent:
                _attackPercent *= multiplier;
                break;

            case EStatus.Def:
                _defense *= multiplier;
                break;

            case EStatus.DefPercent:
                _defensePercent *= multiplier;
                break;
            case EStatus.PenetrationDef:
                _penetrationDef *= multiplier;
                break;
            case EStatus.CriticalChance:
                _criticalChance *= multiplier;
                break;

            case EStatus.CriticalDefense:
                _criticalDefense *= multiplier;
                break;

            case EStatus.CriticalMultiplier:
                _criticalMultiplier *= multiplier;
                break;

            case EStatus.ReduceDmg:
                _reduceDamage *= multiplier;
                break;

            case EStatus.RiseDmg:
                _riseDamage *= multiplier;
                break;

            case EStatus.RiseSkillDmg:
                _riseSkillDmg *= multiplier;
                break;
            default:
                break;
        }
    }

    public double GetStatus(EStatus kind)
    {
        double status;
        switch (kind)
        {
            case EStatus.TotalAttack:
                status = _totalAttack;
                break;
            case EStatus.TotalDefense:
                status = _totalDefense;
                break;
            case EStatus.TotalMaxHp:
                status = _totalMaxHp;
                break;
            case EStatus.MaxHealthPoint:
                status = _maxHp;
                break;
            case EStatus.MaxHealthPointPercent:
                status = _maxHpPercent;
                break;
            case EStatus.Attack:
                status = _attack;
                break;
            case EStatus.AttackPercent:
                status = _attackPercent;
                break;
            case EStatus.Def:
                status = _defense;
                break;
            case EStatus.DefPercent:
                status = _defensePercent;
                break;
            case EStatus.PenetrationDef:
                status = _penetrationDef;
                break;
            case EStatus.CriticalChance:
                status = _criticalChance;
                break;
            case EStatus.CriticalDefense:
                status = _criticalDefense;
                break;
            case EStatus.CriticalMultiplier:
                status = _criticalMultiplier;
                break;
            case EStatus.ReduceDmg:
                status = _reduceDamage;
                break;
            case EStatus.RiseDmg:
                status = _riseDamage;
                break;
            case EStatus.RiseSkillDmg:
                status = _riseSkillDmg;
                break;
            default:
                status = -1;
                break;
        }

        return status;
    }
    
    public string GetStatusText(EStatus kind)
    {
        double statusValue = GetStatus(kind);
        float value = 100f;
        bool intStatus = kind == EStatus.Attack || kind == EStatus.Def ||
                         kind == EStatus.MaxHealthPoint || kind == EStatus.TotalAttack || 
                         kind == EStatus.TotalMaxHp || kind == EStatus.TotalDefense;
        if (intStatus)
            value = 1;

        statusValue /= value;

        StringBuilder descriptionBuilder = new StringBuilder();

        if (intStatus)
            descriptionBuilder.Append(((int)statusValue).ToString());
        else
        {
            descriptionBuilder.Append(statusValue.ToString("F2"));
            descriptionBuilder.Append("%");
        }
        
        return descriptionBuilder.ToString();
    }

    public static Status operator +(Status a, Status b)
    {
        if (b == null) return a;
        Status status = new Status();

        status._maxHp = a._maxHp + b._maxHp;
        status._maxHpPercent = a._maxHpPercent + b._maxHpPercent;
        status._attack = a._attack + b._attack;
        status._attackPercent = a._attackPercent + b._attackPercent;
        status._defense = a._defense + b._defense;
        status._defensePercent = a._defensePercent + b._defensePercent;
        status._penetrationDef = a._penetrationDef + b._penetrationDef;
        status._criticalChance = a._criticalChance + b._criticalChance;
        status._criticalDefense = a._criticalDefense + b._criticalDefense;
        status._criticalMultiplier = a._criticalMultiplier + b._criticalMultiplier;
        status._reduceDamage = a._reduceDamage + b._reduceDamage;
        status._riseDamage = a._riseDamage + b._riseDamage;
        status._riseSkillDmg = a._riseSkillDmg + b._riseSkillDmg;

        status._damageTakenFromHumanRate = a._damageTakenFromHumanRate + b._damageTakenFromHumanRate;
        status._damageTakenFromGuardianRate = a._damageTakenFromGuardianRate + b._damageTakenFromGuardianRate;
        status._damageTakenFromCrusherRate = a._damageTakenFromCrusherRate + b._damageTakenFromCrusherRate;
        status._damageTakenFromCelestialRate = a._damageTakenFromCelestialRate + b._damageTakenFromCelestialRate;
        
        status._damageDealtToHumanRate = a._damageDealtToHumanRate + b._damageDealtToHumanRate;
        status._damageDealtToGuardianRate = a._damageDealtToGuardianRate + b._damageDealtToGuardianRate;
        status._damageDealtToCrusherRate = a._damageDealtToCrusherRate + b._damageDealtToCrusherRate;
        status._damageDealtToCelestialRate = a._damageDealtToCelestialRate + b._damageDealtToCelestialRate;
        
        status._moveSpeed = a._moveSpeed + b._moveSpeed;
        status._moveTargetSearchRange = a._moveTargetSearchRange + b._moveTargetSearchRange;

        return status;
    }

    public static Status operator -(Status a, Status b)
    {
        if (b == null) return a;
        Status status = new Status();
        status._maxHp = a._maxHp - b._maxHp;
        status._maxHpPercent = a._maxHpPercent - b._maxHpPercent;
        status._attack = a._attack - b._attack;
        status._attackPercent = a._attackPercent - b._attackPercent;
        status._defense = a._defense - b._defense;
        status._defensePercent = a._defensePercent - b._defensePercent;
        status._penetrationDef = a._penetrationDef - b._penetrationDef;
        status._criticalChance = a._criticalChance - b._criticalChance;
        status._criticalDefense = a._criticalDefense - b._criticalDefense;
        status._criticalMultiplier = a._criticalMultiplier - b._criticalMultiplier;
        status._reduceDamage = a._reduceDamage - b._reduceDamage;
        status._riseDamage = a._riseDamage - b._riseDamage;
        status._riseSkillDmg = a._riseSkillDmg - b._riseSkillDmg;

        status._damageTakenFromHumanRate = a._damageTakenFromHumanRate - b._damageTakenFromHumanRate;
        status._damageTakenFromGuardianRate = a._damageTakenFromGuardianRate - b._damageTakenFromGuardianRate;
        status._damageTakenFromCrusherRate = a._damageTakenFromCrusherRate - b._damageTakenFromCrusherRate;
        status._damageTakenFromCelestialRate = a._damageTakenFromCelestialRate - b._damageTakenFromCelestialRate;
        
        status._damageDealtToHumanRate = a._damageDealtToHumanRate - b._damageDealtToHumanRate;
        status._damageDealtToGuardianRate = a._damageDealtToGuardianRate - b._damageDealtToGuardianRate;
        status._damageDealtToCrusherRate = a._damageDealtToCrusherRate - b._damageDealtToCrusherRate;
        status._damageDealtToCelestialRate = a._damageDealtToCelestialRate - b._damageDealtToCelestialRate;
        
        status._moveSpeed = a._moveSpeed - b._moveSpeed;
        status._moveTargetSearchRange = a._moveTargetSearchRange - b._moveTargetSearchRange;

        return status;
    }

    public static string ReturnStatusString(EStatus type)
    {
        switch (type)
        {
            case EStatus.MaxHealthPoint:
            case EStatus.MaxHealthPointPercent:
                return "최대 생명력";
            case EStatus.Attack:
            case EStatus.AttackPercent:
                return "공격력";
            case EStatus.CriticalChance:
                return "치명타 확률";
            case EStatus.CriticalDefense:
                return "치명타 저항";
            case EStatus.CriticalMultiplier:
                return "치명 데미지";
            case EStatus.Def:
            case EStatus.DefPercent:
                return "방어력";
            case EStatus.PenetrationDef:
                return "방어력 관통";
            case EStatus.ReduceDmg:
                return "피해 감소";
            case EStatus.RiseDmg:
                return "피해 증폭";
            case EStatus.RiseSkillDmg:
                return "스킬 피해 증가";
            default:
                return "";
        }
    }
}

public enum EStatus
{
    NULL, // only data Parsing. Do not use

    MaxHealthPoint, //HP
    MaxHealthPointPercent, // 체력 %
    Attack, //공격력
    AttackPercent, // 공격력 %
    CriticalChance, //치명타 확률
    CriticalDefense, //치명타 저항
    CriticalMultiplier, //치명 데미지
    Def, //물리방어력
    DefPercent, // 물리 방어력 %
    PenetrationDef, //물리방어관통

    ReduceDmg, //적에게 받는 피해 감소
    RiseDmg, //적에게 주는 피해 증가
    RiseSkillDmg, //스킬피해증가
    
    ReduceHumanDamage, //인간 유닛에게 받는 피해 감소
    ReduceGuardianDamage, //수인 유닛에게 받는 피해 감소
    ReduceCrusherDamage, //요괴 유닛에게 받는 피해 감소
    ReduceCelestialDamage, //천계 유닛에게 받는 피해 감소
    
    RiseHumanDamage, //인간 유닛에게 주는 피해 증가
    RiseGuardianDamage, //수인 유닛에게 주는 피해 증가
    RiseCrusherDamage, //요괴 유닛에게 주는 피해 증가
    RiseCelestialDamage, //천계 유닛에게 주는 피해 증가
    
    TotalAttack,
    TotalMaxHp,
    TotalDefense
}