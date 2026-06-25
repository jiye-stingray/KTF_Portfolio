using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class EquipmentOptionStausText : StatusText
{
    int _index;


    [SerializeField] Image _lockImg;
    [SerializeField] Button _lockBtn;


    public void SetDataGrade(EStatus statusType, Status status, EOptionGradeType gradeType,int index,bool isLock, Action<int> clickAction = null)
    {
        base.SetDataGrade(statusType, status, gradeType);
        _index = index;

        _lockImg.gameObject.SetActive(statusType != EStatus.NULL);
        if(statusType != EStatus.NULL)
        {
            string spriteName = isLock ? "Lock" : "lock2";
            _lockImg.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.PictogramAtlas, spriteName);

            if(_lockBtn != null  && clickAction != null)
            {
                _lockBtn.onClick.RemoveAllListeners();
                _lockBtn.onClick.AddListener(() => clickAction(_index));

            }
        }
    }
}
