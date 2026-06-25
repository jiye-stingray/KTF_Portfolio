using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIRelicSelectLevelItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private GameObject _lock;
    [SerializeField] private GameObject _selected;

    private UserInfoData UserInfoData => Managers.Instance.UserInfo();
    private UIManager UIManager => Managers.Instance.GetUIManager();
    
    private Action<int> _selectAction;
    private int _level;
    private bool _isLocked;
    private bool _isSelected;
    public void Init(int fieldId, int level, Action<int> selectAction)
    {
        _level = level;
        _levelText.text = $"{_level}단계";
        FieldItemData fieldItemData = UserInfoData._dicFieldItemData[fieldId];
        _selectAction = selectAction;
    }
    
    public void SetSelected(bool state)
    {
        _isSelected = state;
        _selected.SetActive(state);
    }

    public void SetLock(bool state)
    {
        _isLocked = state;
        _lock.SetActive(state);
    }

    public void OnSelectLevelClicked()
    {
        if (_isSelected)
        {
            UIManager.ShowCommonToastMessage("현재 선택된 난이도 입니다.");
            return;
        }
        
        if (_isLocked)
        {
            UIManager.ShowCommonToastMessage("해금되지 않은 난이도 입니다.");
            return;
        }
            
        if(_selectAction != null)
            _selectAction.Invoke(_level);
    }
}
