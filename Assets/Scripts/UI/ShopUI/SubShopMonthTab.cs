using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class SubShopMonthTab : UITabBase
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] Image _icon;

    [SerializeField] ScrollRectDynamicPopulator _purchaseScrollview;
    [SerializeField] ScrollRectDynamicPopulator _dailyScrollview;

    [SerializeField] UICostButton _costBtn;

    MonthShop _db;
    AtlasManager atlas => Managers.Instance.GetAtlasManager();

    public void SetDataOpen(MonthShopItemData itemData)
    {
        Open();
        _db = itemData.GetData() as MonthShop;
        Refresh();
    }

    public override void Refresh()
    {
        _costBtn.Init(new ECurrency[] { ECurrency.MidCash }, new int[] { _db.Price });
        _nameTxt.text = _db.ItemTitle;
        _icon.sprite = atlas.GetSprite(Define.EAtlasType.ShopAtlas, _db.Resource2);

        List<ItemData> purchaseData = new List<ItemData>();
        for (int i = 0; i < _db.RewardID_1.Length; i++)
        {
            RewardItemData rewardItem = new RewardItemData
            {
                _rewardType = Define.ERewardType.Currency,
                _index = (int)_db.RewardID_1[i],
                _count = _db.RewardCount_1[i]
            };
            purchaseData.Add(rewardItem);
        }
        _purchaseScrollview.Init((cell, data, index) =>
        {
            cell.SetData(data, index);
        });
        _purchaseScrollview.Populate(purchaseData);

        List<ItemData> dailyData = new List<ItemData>();
        for (int i = 0; i < _db.RewardID_2.Length; i++)
        {
            RewardItemData rewardItem = new RewardItemData
            {
                _rewardType = Define.ERewardType.Currency,
                _index = (int)_db.RewardID_2[i],
                _count = _db.RewardCount_2[i]
            };
            dailyData.Add(rewardItem);
        }
        _dailyScrollview.Init((cell, data, index) =>
        {
            cell.SetData(data, index);
        });
        _dailyScrollview.Populate(dailyData);
    }

    public void ClickCostBtn()
    {
        if (_costBtn.isGray)
        {
            // dummy setting 
/*            UIManager.ShowConfirmPopUp("", "여우구슬이 부족합니다.\n여우 구슬 구매 탭으로 이동 하시겠습니까?","",() =>  {

                UIManager.UIShop._group._currentTapGroupBtn = UIManager.UIShop._group._tapGroupBtns[3];
                UIManager.UIShop.OnChangeTab();
                UIManager.UIShop.Refresh();

                _mainUI.GetComponent<UISubBase>().ClickCloseBtn();
            });*/
            return;
        }
        
        Managers.Instance.GetServerManager().OnPostShopTypePurchase(EShopType.MonthlyShop, _db.ID, 1, "", "", (shopDto, reward,response) =>
        {
            UIManager.UIShop.OnChangeTab();     // scrollview Update
            Managers.Instance.GetServerManager().OnPostRequestMyMail(() =>
            {
                UIManager.MainInfoUI.Refresh();
                UIManager.ShowCommonToastMessage("구매하신 상품은 우편함으로 발송되었습니다.");
                _mainUI.GetComponent<UISubBase>().ClickCloseBtn();
            });
        });
    }
}
