using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SideMenuItemUI : MonoBehaviour
{
    public Image img;
    public RectTransform rectTrans;
    public bool isOpen = true; 

    SideMenuUI settingsMenu;
    Button button;
    [SerializeField] GameObject _grayImg;
    [SerializeField] UnityEvent clickAction;

    int index;

    ContentsOpen openContentBase = null;

    UserInfoData userInfoData => Managers.Instance.UserInfo();

    void Awake()
    {
        img = GetComponent<Image>();
        rectTrans = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        button.onClick.AddListener(Click);
        //-1 to ignore the main button
        index = rectTrans.GetSiblingIndex() - 1;

        if(ClientLocalDB_Simple.GetDB<ContentsOpen>(DBKey.ContentsOpen).ContainsKey(gameObject.name))
            openContentBase = ClientLocalDB_Simple.GetData<ContentsOpen>(DBKey.ContentsOpen, gameObject.name);
    }

    public void Refresh()
    {
        #if TUTO

        if (openContentBase != null)
        {
            isOpen = userInfoData.userLevel.Value >= openContentBase.ConditionValue || openContentBase.Lock;
        }
        else
        {
            isOpen = true;
        }

        if(_grayImg != null) 
            _grayImg.SetActive(!isOpen);
        #endif

    }

    public void Click()
    {
        if (!isOpen)
        {
            if(openContentBase.Lock)
            {
                Managers.Instance.GetUIManager().ShowCommonToastMessage("점검중입니다");
            }
            else
                Managers.Instance.GetUIManager().ShowCommonToastMessage($"{openContentBase.ConditionValue}레벨에 해금됩니다");
            return;
        }
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        clickAction?.Invoke();
    }


}
