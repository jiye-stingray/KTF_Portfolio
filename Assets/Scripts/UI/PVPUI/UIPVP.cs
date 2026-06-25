using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIPVP : UIBase
{
    [SerializeField] UITimer _uiTimer;
    [SerializeField] ScrollRectDynamicPopulator _scrollview;
    [SerializeField] PVPInfo _myPVPInfo;

    /// <summary>
    /// 대전 기록 버튼 클릭 
    /// </summary>
    public void PVPRecordBtnClick()
    {
        UIManager.ShowUISubBase<UISubPVPRecord>(this, "UISubPVPRecord").OpenToStack();

    }

    public void ShopBtnClick()
    {

    }

    /// <summary>
    /// 새로고침 버튼 클릭 
    /// </summary>
    public void RefreshBtnClick()
    {
        
    }

    public void PVPRewardBtnClick()
    {
        UIManager.ShowUISubBase<UISubPVPReward>(this, "UISubPVPReward").OpenToStack();
    }

    public void PVPRankingBtnClick()
    {
        UIManager.ShowUISubBase<UISubPVPRecord>(this, "UISubPVPRecord").OpenToStack();
    }
}
