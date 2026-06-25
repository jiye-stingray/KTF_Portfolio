using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Define;

public class WorldLevelSwitchDataButton : SwitchDataButton
{
    public int _fieldId;

    public void Init(int fieldId, IndexWrapper _index,int startIndex, int maxIndex, UnityAction<int> action)
    {
        _fieldId = fieldId;
        base.Init(_index, startIndex, maxIndex, action);
        Refresh();
    }

    public override void Click()
    {
        if (_gray.activeSelf) return;

        base.Click();
        // 2026.05.28 click 에서 click aciton 이내에 gray 를 이미 refresh 하였으니  그 후에 refresh 추가 
        Refresh();      
    }
    public void Refresh()
    {
        UserInfoData userInfo = Managers.Instance.UserInfo();

        switch (switchType)
        {
            case SwitchType.Right:
                if (userInfo != null && userInfo._dicFieldItemData != null &&
                    userInfo._dicFieldItemData.TryGetValue(_fieldId, out FieldItemData fieldItemData))
                {
                    bool isGrayOn = fieldItemData.difficultyLevel <= indexClass._index;
                    _gray.SetActive(isGrayOn);
                }
                else
                {
                    _gray.SetActive(false);
                }
                break;

            case SwitchType.Left:
                _gray.SetActive(indexClass._index == 1);

                break;
        }


    }

    
}
