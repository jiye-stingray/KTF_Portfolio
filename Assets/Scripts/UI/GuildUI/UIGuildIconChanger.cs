using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIGuildIconChanger : UISubBase
{
    public UIGuildIconChangerIcon[] _guildIcons;

    UnityAction<Sprite,int> _iconClickAction;
    UIGuildIconChangerIcon _selectedIcon;
    /*
     * *
     */

    public void SetData(int guildPattern, UnityAction<Sprite,int> iconClickAction)
    {
        _iconClickAction = iconClickAction;
        for (int i = 0; i < _guildIcons.Length; i++)
        {
            _guildIcons[i].Init(i, OnClickIcon);
        }
        OnClickIcon(_guildIcons[guildPattern - 1]);
    }
    public void OnClickIcon(UIGuildIconChangerIcon _icon)
    {
        if (_selectedIcon != null)
        {
            _selectedIcon._isSelect = false;
            _selectedIcon.Refresh();
        }

        _selectedIcon = _icon;
        _selectedIcon._isSelect = true;
        _selectedIcon.Refresh();
    }
    
    public void OnClickConfirm()
    {
        if (_selectedIcon != null)
            _iconClickAction?.Invoke(_selectedIcon._icon.sprite, _selectedIcon._index);
        ClickCloseBtn();
    }
}
