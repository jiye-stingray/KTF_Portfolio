using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConstellationDetail : MonoBehaviour
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] TMP_Text _descriptionTxt;
    [SerializeField] GameObject _gradeObj;
    [SerializeField] TMP_Text _gradeTxt;
    [SerializeField] ConstellationCostButton _costBtn;

    ConstellationItemData _data;

    public void Init(ConstellationItemData data)
    {
        _data = data;
        if (_data == null)
        {
            gameObject.SetActive(false);
            _costBtn.gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
            _costBtn.gameObject.SetActive(true);
        }
        Refresh();
    }

    private void Refresh()
    {
        _nameTxt.text = Define.ReturnConstellationSize(_data.data.StarSize);

        if (_data._isOpen)
            _descriptionTxt.text = $"{Status.ReturnStatusString(_data.data.StatusType)} : {_data.ReturnStatusDescriptionValue()}";
        else
            _descriptionTxt.text = $"{Status.ReturnStatusString(_data.data.StatusType)} 증가";

        _costBtn.gameObject.SetActive( !_data._isOpen || (_data._isOpen &&_data.data.StarSize == Define.EConstellationSize.Small));
        if (_costBtn.gameObject.activeSelf)
            _costBtn.Init(_data);

        _gradeObj.SetActive(_data._isOpen);
        if (_data._isOpen)
            _gradeTxt.text = $"현재등급:{Define.ReturnConstellationGrade(_data._grade)}";
    }
}
