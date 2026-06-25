using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UITabGroup : MonoBehaviour
{
    public UITabGroupButton[] _tapGroupBtns;
    public UITabGroupButton _currentTapGroupBtn;

    public UnityEvent _changeEvent;
    private void Awake()
    {
        for (int i = 0; i < _tapGroupBtns.Length; i++)
        {
            _tapGroupBtns[i].Init(this,i);
        }
    }

    public void Set(int index)
    {
        _currentTapGroupBtn = _tapGroupBtns[index];
        for (int i = 0; i < _tapGroupBtns.Length; i++)
        {
            _tapGroupBtns[i].Set(i == index);
        }
        _changeEvent.Invoke();
    }
    public void Set(int index,bool useChangeEvent)
    {
        _currentTapGroupBtn = _tapGroupBtns[index];
        for (int i = 0; i < _tapGroupBtns.Length; i++)
        {
            _tapGroupBtns[i].Set(i == index);
        }

        if(useChangeEvent == true)
            _changeEvent.Invoke();
    }

}
