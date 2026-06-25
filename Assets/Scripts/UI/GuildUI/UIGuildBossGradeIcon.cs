using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIGuildBossGradeIcon : MonoBehaviour
{
    [SerializeField] private GameObject goClearIcon;
    [SerializeField] private TMP_Text txtGrade;

    /*
     * *
     */
    
    public void Refresh(int grade,bool isClear)
    {
        if(grade <= 0)
            txtGrade.text = "";
        else
            txtGrade.text = grade.ToString();
        goClearIcon.SetActive(isClear);
    }
}
