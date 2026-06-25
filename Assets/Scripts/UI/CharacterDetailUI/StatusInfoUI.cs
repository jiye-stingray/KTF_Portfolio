using TMPro;
using UnityEngine;

public class StatusInfoUI : MonoBehaviour
{
    [SerializeField] TMP_Text _battlePowerTxt;
    //공격 속성
    [SerializeField] TMP_Text _totalTxt;
    [SerializeField] TMP_Text _atkTxt;
    [SerializeField] TMP_Text _atkPercentTxt;
    [SerializeField] TMP_Text _criticalChanceTxt;
    [SerializeField] TMP_Text _criticalMultiplierTxt;

    //방어 속성
    [SerializeField] TMP_Text _totalMaxHpTxt;
    [SerializeField] TMP_Text _maxHpTxt;
    [SerializeField] TMP_Text _maxHpPercentTxt;

    [SerializeField] TMP_Text _totalDefenseTxt;
    [SerializeField] TMP_Text _defenseTxt;
    [SerializeField] TMP_Text _defensePercentTxt;
    

    [SerializeField] TMP_Text _reduceDamageTxt;
    [SerializeField] TMP_Text _criticalDesfanseTxt;

    public void Init(PlayerStatusInfo statusInfo)
    {
        _battlePowerTxt.text = statusInfo._battlePower.ToString(); 
        Status characterStatus = statusInfo.GetCharacterStatus();
        
        // 공격 속성
        _totalTxt.text = characterStatus.GetStatusText(EStatus.TotalAttack);
        _atkTxt.text = characterStatus.GetStatusText(EStatus.Attack);
        _atkPercentTxt.text = characterStatus.GetStatusText(EStatus.AttackPercent);
        _criticalChanceTxt.text = characterStatus.GetStatusText(EStatus.CriticalChance);
        _criticalMultiplierTxt.text = characterStatus.GetStatusText(EStatus.CriticalMultiplier);

        // 방어 속성 - HP
        _totalMaxHpTxt.text = characterStatus.GetStatusText(EStatus.TotalMaxHp);
        _maxHpTxt.text = characterStatus.GetStatusText(EStatus.MaxHealthPoint);
        _maxHpPercentTxt.text = characterStatus.GetStatusText(EStatus.MaxHealthPointPercent);

        // 방어 속성 - 물리 방어
        _totalDefenseTxt.text = characterStatus.GetStatusText(EStatus.TotalDefense);
        _defenseTxt.text = characterStatus.GetStatusText(EStatus.Def);
        _defensePercentTxt.text = characterStatus.GetStatusText(EStatus.DefPercent);

        // 기타 방어 속성
        _reduceDamageTxt.text = characterStatus.GetStatusText(EStatus.ReduceDmg);
        _criticalDesfanseTxt.text = characterStatus.GetStatusText(EStatus.CriticalDefense);
    }

}