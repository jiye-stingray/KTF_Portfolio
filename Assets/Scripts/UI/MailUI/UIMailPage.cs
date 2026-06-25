using Cysharp.Threading.Tasks;
using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMailPage : UIBase, IRecyclableScrollRectDataSource
{
    [SerializeField] private RecyclableScrollRect scrollView;
    [SerializeField] private TMP_Text _countTxt;

    UIMail.ETab eCurrentTab;

    BestHttp_GameManager bestHttp_GameManager => Managers.Instance.GetServerManager();
    
    UserInfoData userInfo => Managers.Instance.UserInfo();

    List<MailItemData> _dataList = new List<MailItemData>();

    /*
     * *
     */

    #region Recycle Scrollview
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as MailScrollviewItem;
        item.SetData(_dataList[index], index, OnClickReceiveMail, OnClickRemoveMail);
    }
    #endregion

    public void Open(UIMail.ETab _eCurrentTab, List<MailItemData> _mailList)
    {
        eCurrentTab = _eCurrentTab;
        _dataList =  _mailList;
        scrollView.Initialize(this);
        Refresh();
        base.Open();
    }
    public override void Refresh()
    {
        scrollView.ReloadData();
        _countTxt.text = $"{_dataList.Count}/100";
        base.Refresh();
    }

    void OnClickReceiveMail(MailScrollviewItem _item)
    {
        bestHttp_GameManager.OnPostReadMail(_item._data.mailId, (RewardData) => 
        {
            // 팝업
            if(RewardData != null)
            {
                UIManager.ShowRewardPopup(RewardData).Forget();

            }


            switch (eCurrentTab)
            {
                case UIMail.ETab.Nomal:
                    _dataList = userInfo.nomalMailItemList;
                    break;
                case UIMail.ETab.Pay:
                    _dataList = userInfo.payMailItemList;
                    break;
            }
            scrollView.Initialize(this);
            Refresh();

            Managers.Instance.GetUIManager().MainInfoUI.Refresh();
        });
    }
    void OnClickRemoveMail(MailScrollviewItem _item)
    {
        _dataList.Remove(_item._data);
        scrollView.Initialize(this);
        Refresh();
    }
}
