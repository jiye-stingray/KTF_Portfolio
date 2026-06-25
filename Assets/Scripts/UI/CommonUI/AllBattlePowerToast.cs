using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AllBattlePowerToast : UIToastBase
{
    [SerializeField] TMP_Text _beforeBattlePowerTxt;
    [SerializeField] TMP_Text _beforeATKTxt;
    [SerializeField] TMP_Text _beforeMaxHpTxt;

    [SerializeField] TMP_Text _currentBattlePowerTxt;
    [SerializeField] TMP_Text _currentATKTxt;
    [SerializeField] TMP_Text _currentMaxHpTxt;
        
    public void SetStatus(double beforeBattlePower, double beforeAtk, double beforeMaxHp,
        double currentBattlePower, double currentAtk, double currentmaxHp) 
    {
        // 이전 값 표시
        _beforeBattlePowerTxt.text = beforeBattlePower.ToString("N0");
        _beforeATKTxt.text = beforeAtk.ToString("N0");
        _beforeMaxHpTxt.text = beforeMaxHp.ToString("N0");

        // 현재 텍스트 초기 표시 (시작값)
        _currentBattlePowerTxt.text = beforeBattlePower.ToString("N0");
        _currentATKTxt.text = beforeAtk.ToString("N0");
        _currentMaxHpTxt.text = beforeMaxHp.ToString("N0");

        // 기존 트윈이 있으면 끊기(선택)
        DOTween.Kill(_currentBattlePowerTxt);
        DOTween.Kill(_currentATKTxt);
        DOTween.Kill(_currentMaxHpTxt);

        // 배틀파워 트윈
        double bp = beforeBattlePower;
        DOTween.To(() => bp, v =>
        {
            bp = v;
            _currentBattlePowerTxt.text = ((ulong)bp).ToString("N0");
        }, currentBattlePower, 1.0f)
        .SetEase(Ease.OutCubic)
        .SetId(_currentBattlePowerTxt);

        // 공격력 트윈
        double atk = beforeAtk;
        DOTween.To(() => atk, v =>
        {
            atk = v;
            _currentATKTxt.text = ((ulong)atk).ToString("N0");
        }, currentAtk, 1.0f)
        .SetEase(Ease.OutCubic)
        .SetId(_currentATKTxt);

        // 최대체력 트윈
        double hp = beforeMaxHp;
        DOTween.To(() => hp, v =>
        {
            hp = v;
            _currentMaxHpTxt.text = ((ulong)hp).ToString("N0");
        }, currentmaxHp, 1.0f)
        .SetEase(Ease.OutCubic)
        .SetId(_currentMaxHpTxt);

    }

    
}
