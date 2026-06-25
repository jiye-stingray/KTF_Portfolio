using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIRelicStatusText : StatusText
{
    public void SetData(EStatus statusType, Status status, EOptionGradeType gradeType)
    {
        base.SetDataGrade(statusType, status, gradeType);   
    }
}
