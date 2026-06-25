using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class UIMail : UIBase
{
    [SerializeField] private UITabGroup group;

    [Serializable]
    public struct STab
    {
        public UIMailPage nomalMailPage;
        public UIMailPage payMailPage;
    }
    public STab sTab;

    public enum ETab
    {
        Nomal,
        Pay
    }
    ETab eCurrentTab;

    [SerializeField] GameObject _nullGo;

    BestHttp_GameManager bestHttp_GameManager => Managers.Instance.GetServerManager();
    UserInfoData userInfoData => Managers.Instance.UserInfo();

    /*
     * *
     */

    public override void Open()
    {
        base.Open();
        bestHttp_GameManager.OnPostRequestMyMail(() =>
        {
            eCurrentTab = ETab.Nomal;
            group.Set((int)eCurrentTab);
        });
        
    }
    public void OnChangeTab()
    {
        _nullGo.SetActive(false);
        eCurrentTab = (ETab)group._currentTapGroupBtn._index;
        switch (eCurrentTab)
        {
            case ETab.Nomal:
                sTab.payMailPage.Close();
                _nullGo.SetActive(userInfoData.nomalMailItemList.Count <= 0);
                sTab.nomalMailPage.Open(ETab.Nomal, userInfoData.nomalMailItemList);
                break;
            case ETab.Pay:
                sTab.nomalMailPage.Close();
                _nullGo.SetActive(userInfoData.payMailItemList.Count <= 0);
                sTab.payMailPage.Open(ETab.Pay,userInfoData.payMailItemList);
                break;
        }
    }

    public void OnClickReceiveMailAll()
    {
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");

        
        switch (eCurrentTab)
        {
            case ETab.Nomal:
                if (userInfoData.nomalMailItemList.Count <= 0)
                    return;
                break;
            case ETab.Pay:
                if (userInfoData.payMailItemList.Count <= 0)
                    return;
                break;
        }
        bestHttp_GameManager.OnPostReadMailAll((int)eCurrentTab, (RewardData) =>
        {
            // 팝업
            UIManager.ShowRewardPopup(RewardData).Forget();

            OnChangeTab();
        });
    }
}
