using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class StatusText : MonoBehaviour
{
    [SerializeField] protected TMP_Text _statusTxt;
    [SerializeField] protected TMP_Text _statusValue;
    [SerializeField] protected Image _gradeBg;
    [SerializeField] private GameObject _empty;

    protected AtlasManager AtlasManager => Managers.Instance.GetAtlasManager();

    public void SetData(EStatus statusType, Status status)
    {
        _statusTxt.text = Status.ReturnStatusString(statusType);
        _statusValue.text = status.GetStatusText(statusType);
    }

    public void SetDataGrade(EStatus statusType, Status status, EOptionGradeType gradeType)
    {
        if (_gradeBg != null)
        {
            if (gradeType == EOptionGradeType.None)
            {
                _gradeBg.type = Image.Type.Sliced;
                _gradeBg.sprite = AtlasManager.GetSprite(EAtlasType.CommonAtlas,
                    $"BG_TextBox_Chat");
            }
            else
            {
                _gradeBg.type = Image.Type.Simple;
                _gradeBg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas,
                    $"BG_Textbox_grade_{gradeType.ToString()}");
            }
        }

        if (_empty != null)
            _empty.SetActive(statusType == EStatus.NULL);

        _statusTxt.gameObject.SetActive(statusType != EStatus.NULL);
        _statusValue.gameObject.SetActive(statusType != EStatus.NULL);

        if (statusType != EStatus.NULL)
            SetData(statusType, status);
    }
}
