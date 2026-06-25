using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class UIGuildIconChangerIcon : MonoBehaviour
{
    public int _index;
    public bool _isSelect;
    public Image _icon;
    [SerializeField] private GameObject _goSelect;
    AtlasManager _atlasManager => Managers.Instance.GetAtlasManager();

    UnityAction<UIGuildIconChangerIcon> _clickAction;
    /*
     * *
     */
    public void Init(int index, UnityAction<UIGuildIconChangerIcon> clickAction)
    {
        _index = index;
        _clickAction = clickAction;
        _isSelect = false;

        if (_index + 1 > 0)
        {
            string markSpriteName = $"GuildMark{(_index + 1).ToString("00")}";
            _icon.sprite = _atlasManager.GetSprite(Define.EAtlasType.GuildAtlas, markSpriteName);
        }

        Refresh();
    }
    public void Refresh()
    {
        _goSelect.SetActive(_isSelect);
    }
    public void OnClick()
    {
        _clickAction.Invoke(this);
        Refresh();
    }
    
}
