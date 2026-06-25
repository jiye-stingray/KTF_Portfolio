using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class SubShopPackageTab : UITabBase
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] Image _icon;
    [SerializeField] ScrollRectDynamicPopulator _scrollview;
    [SerializeField] UICostButton _costBtn;

    List<ItemData> _dataList = new List<ItemData>();

    PackageShop _db;
    PackageShopItemData _data;
    AtlasManager atlas => Managers.Instance.GetAtlasManager();

    public void SetDataOpen(PackageShopItemData data)
    {
        Open();
        _data = data;
        _db = data.GetData() as PackageShop;
        Refresh();

    }

    public override void Refresh()
    {
        _costBtn.Init(new ECurrency[] { ECurrency.MidCash }, new int[] { _db.Price });
        _nameTxt.text = _db.ItemTitle;
        _icon.sprite = atlas.GetSprite(Define.EAtlasType.ShopAtlas, _db.Resource);

        _dataList.Clear();

        for (int i = 0; i < _db.RewardType.Length; i++)
        {

            RewardItemData itemData = new RewardItemData
            {
                _rewardType = _db.RewardType[i],
                _index = _db.RewardID[i],
                _count = _db.RewardCount[i]

            };
            _dataList.Add(itemData);
        }

        _scrollview.Init((cell, data, index) =>
        {
            cell.SetData(data, index);
        });
        _scrollview.Populate(_dataList);


    }

    public void ClickCostBtn()
    {
        if (_costBtn.isGray)
        {
            // dummy hide
            UIManager.ShowConfirmPopUp("", "여우구슬이 부족합니다.\n여우 구슬 구매 탭으로 이동 하시겠습니까?", () =>
            {

                UIManager.UIShop._group._currentTapGroupBtn = UIManager.UIShop._group._tapGroupBtns[3];
                UIManager.UIShop.OnChangeTab();
                UIManager.UIShop.Refresh();

                _mainUI.GetComponent<UISubBase>().ClickCloseBtn();
            });
            return;
        }

        if (_db.Limited && _data.endTime <= ServerTime.Instance.CurrentTime())
        {
            UIManager.ShowCommonToastMessage("판매 기간이 종료되었습니다.");
            return;
        }

        Managers.Instance.GetServerManager().OnPostShopTypePurchase(EShopType.PackageShop, _db.ID,1, "", "", (shopDto,reward,response) =>
        {
            if(_db.ID == 1) //광고 제거 구매
                UserInfoData._isAdsRemoved = true;
            
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
