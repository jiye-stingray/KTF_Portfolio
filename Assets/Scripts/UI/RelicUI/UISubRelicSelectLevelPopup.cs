using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISubRelicSelectLevelPopup : UISubBase
{
    // [SerializeField] private ScrollRectDynamicPopulator _scrollView;
    [SerializeField] private UIRelicSelectLevelItem[] _selectLevelItems;

    private Action<int> _selectAction;
    public void Init(int fieldId, int selectLevel, Action<int> selectAction)
    {
        FieldItemData fieldItemData = UserInfoData._dicFieldItemData[fieldId];
        _selectAction = selectAction;

        for (int i = 0; i < _selectLevelItems.Length; i++)
        {
            int level = i + 1;
            UIRelicSelectLevelItem selectLevelItem = _selectLevelItems[i];
            selectLevelItem.Init(fieldId, level, SelectLevel);
            selectLevelItem.SetSelected(selectLevel == level);
            selectLevelItem.SetLock(fieldItemData.difficultyLevel < level);
        }
    }

    private void SelectLevel(int level)
    {
        if(_selectAction != null)
            _selectAction.Invoke(level);
        
        ClickCloseBtn();
    }
}
