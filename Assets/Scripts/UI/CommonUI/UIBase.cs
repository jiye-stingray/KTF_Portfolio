using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UIBase : MonoBehaviour
{
    protected bool _init = false;

    public Transform _subCanvas;

    public Stack<UISubBase> _subUIStack = new Stack<UISubBase>();
    public UISubBase CurrentSubUI => _subUIStack != null && _subUIStack.Count > 0 ? _subUIStack.Peek() : null;

    protected UIManager UIManager => Managers.Instance.GetUIManager();
    protected UserInfoData UserInfoData => Managers.Instance.UserInfo();
    protected ClientLocalDB_Simple ClientLocalDB => Managers.Instance.GetSimpleDBManager();
    
    protected BestHttp_GameManager BestHttp_GameManager => Managers.Instance.GetServerManager();
    protected AtlasManager AtlasManager => Managers.Instance.GetAtlasManager();

    public virtual bool Init()
    {
        if (_init) return false;         

        _init = true;
        return true;
    }

    public virtual void Refresh()
    {
        if(CurrentSubUI != null)
            CurrentSubUI.Refresh();
    }


    public virtual void OpenToStack()
    {
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");

        // 내 생각엔 main UI 에 있어야 할 듯 ? (정확히는 재사용이 필요한 UI 들)

        // UI Stack
        if (UIManager._currentUI != null && this != UIManager._currentUI)
            UIManager.PushUIStack(UIManager._currentUI);
        UIManager._currentUI = this;

        Open();
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void ClickCloseBtn()
    {
        // Sound 
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuClose");

        UIManager.PopUIStack();
        Close();
    }

    public virtual void Close()
    {
        while (_subUIStack.Count > 0)
        {
            CurrentSubUI.ClickCloseBtn();
        }


        if(gameObject!= null)
            gameObject.SetActive(false);
    }

}
