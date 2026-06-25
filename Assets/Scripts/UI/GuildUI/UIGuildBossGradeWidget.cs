using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UIGuildBossGradeWidget : MonoBehaviour
{
    [SerializeField] private UIGuildBossGradeIcon[] icons;

    UIGuildBossGradeIcon centerIcon;
    int currentBossGrade;

    /*
     * *
     */
    
    public void SetData(int _currentBossGrade)
    {
        currentBossGrade = _currentBossGrade;
    }

    public void Refresh()
    {
        for (int i = 0; i < icons.Length; i++)
        {
            int gradeToShow = (i + 1);

            if (gradeToShow >= BOSS_MIN_GRADE && gradeToShow <= BOSS_MAX_GRADE)
            {
                if (gradeToShow < currentBossGrade)
                {
                    
                    icons[i].Refresh(gradeToShow, true);
                }
                else
                {
                    icons[i].Refresh(gradeToShow, false);
                }
            }

        }
    }
}
