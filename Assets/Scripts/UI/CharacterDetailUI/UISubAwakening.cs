using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UISubAwakening : UISubBase
{
    [Header("Grade")]
    [SerializeField] TMP_Text _beforeRankTxt;
    [SerializeField] TMP_Text _currentRankTxt;



    [SerializeField] AllBattlePowerToast _battlePower;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public void InitData(int id,EGradeType gradeType,
        double beforeBattlePower, double beforeAtk, double beforeMaxHp,
        double currentBattlePower, double currentAtk, double currentmaxHp)
    {

        DrawGrade(gradeType - 1, _beforeRankTxt);
        DrawGrade(gradeType, _currentRankTxt);
        _currentRankTxt.color =  Utils.HexToColor(Define.GradeColorHex[gradeType]);
        _battlePower.SetStatus(beforeBattlePower,beforeAtk, beforeMaxHp, currentBattlePower, currentAtk, currentmaxHp);
    }

    private void DrawGrade(EGradeType grade, TMP_Text txt)
    {
        txt.text = ReturnGradeString(grade);
        
    }
}
