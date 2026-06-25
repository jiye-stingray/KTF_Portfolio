using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Define;

[System.Serializable]
public class ConstellationItemData : ItemData
{
    public int _id;
    public EConstellationGrade _grade;
    public bool _isOpen;

    public Constellation data => ClientLocalDB_Simple.GetData<Constellation>(DBKey.Constellation, _id);

    public float ReturnStatusValue()
    {
        if (!_isOpen) return 0;

        float value = 1;
        if (data.StatusType == EStatus.Attack || data.StatusType == EStatus.Def ||
            data.StatusType == EStatus.MaxHealthPoint)
            value = 100;
            
        switch (_grade)
        {
            case EConstellationGrade.Normal:
                return data.StatusValue_Normal / value;
            case EConstellationGrade.Rare:
                return data.StatusValue_Rare / value;
            case EConstellationGrade.Epic:
                return data.StatusValue_Epic / value;
            case EConstellationGrade.Legendary:
                return data.StatusValue_Legendary / value;
        }
        return -1;
    }
    
    public string ReturnStatusDescriptionValue()
    {
        if (!_isOpen) return "";
        
        double statusValue = 0;
        float value = 100f;
        bool intStatus = data.StatusType == EStatus.Attack || data.StatusType == EStatus.Def ||
                         data.StatusType == EStatus.MaxHealthPoint;
            
        switch (_grade)
        {
            case EConstellationGrade.Normal:
                statusValue = data.StatusValue_Normal / value;
                break;
            case EConstellationGrade.Rare:
                statusValue = data.StatusValue_Rare / value;
                break;
            case EConstellationGrade.Epic:
                statusValue = data.StatusValue_Epic / value;
                break;
            case EConstellationGrade.Legendary:
                statusValue = data.StatusValue_Legendary / value;
                break;
        }
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
}
