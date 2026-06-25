using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UIElements;
using static Define;

public class UILimitShopPopup : UIPopupBase
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] ScrollRectDynamicPopulator _scrollview;
    [SerializeField] UIShopCostButton _shopCostBtn;

    int _currentID;

    LimitShopItemData _data;
    LimitedShop _db;

    public void SetIDOpenToStack(int id)
    {
        _currentID = id;
        _data = UserInfoData._dicShopItemData[EShopType.LimitShop][_currentID] as LimitShopItemData;
        _db = _data.GetData() as LimitedShop;


#if IAP
        _shopCostBtn.Init(_db);
#endif

        OpenToStack();
        Refresh();
    }

    public override void OpenToStack()
    {
        base.OpenToStack();
#if IAP
        SubscribeShop(true);
#endif
        _nameTxt.text = _db.ItemTitle;
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
    }

    public override void Refresh()
    {
        var dataList = new List<ItemData>();
        for (int i = 0; i < _db.RewardType.Length; i++)
        {
            dataList.Add(new RewardItemData
            {
                _rewardType = _db.RewardType[i],
                _index      = _db.RewardID[i],
                _count      = _db.RewardCount[i]
            });
        }

        _scrollview.Init((cell, data, index) =>
        {
            cell.SetData(data, index);
        });

        _scrollview.Populate(dataList);
        base.Refresh();
    }

#if IAP

    private void OnDestroy()
    {
        SubscribeShop(false);
    }

    private void SubscribeShop(bool sub)
    {
        var shop = ShopManager.Instance;
        if (shop == null) return;
        shop.OnPurchaseCompleted -= OnPassPurchaseCompleted;
        shop.OnPurchaseFailed   -= OnPassPurchaseFailed;
        shop.OnPurchaseStarted  -= OnPassPurchaseStarted;
        shop.OnPurchaseAction   -= OnPurchaseAction;
        if (!sub) return;
        shop.OnPurchaseCompleted += OnPassPurchaseCompleted;
        shop.OnPurchaseFailed   += OnPassPurchaseFailed;
        shop.OnPurchaseStarted  += OnPassPurchaseStarted;
        shop.OnPurchaseAction   += OnPurchaseAction;
        
        // 구독 직후, 보류돼 있던 미완료 거래 복구 시도
        // UILimitShopPopup.SubscribeShop - 한정 pending만 복구
        Managers.Instance?.IAP?.RepublishPendingOrders(
            pid => ShopManager.Instance.IsProductOfKind(pid, ShopManager.EShopKind.Limit));
    }

    private void OnPassPurchaseStarted(ProductShopData item) { }

    private void OnPassPurchaseCompleted(ProductShopData item)
    {
        //Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("구매 완료!", "ToastMessage");
    }

    private void OnPassPurchaseFailed(ProductShopData item, ShopFailReason reason, string detail)
    {
        Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>($"구매 실패: {reason}", "ToastMessage");
        Debug.LogError(detail);
    }

    public void OnPurchaseAction(string goodsId, string payLoad, string shop, System.Action<string> onSuccess) 
    {
        Managers.Instance.GetServerManager().OnPostShopTypePurchase(EShopType.LimitShop,int.Parse(goodsId), 1, shop, payLoad, (shopDto, reward,response) =>
        {
            onSuccess.Invoke(response);
            Managers.Instance.GetServerManager().OnPostRequestMyMail(() =>
            {
                UIManager.MainInfoUI.Refresh();
                UIManager.ShowCommonToastMessage("구매하신 상품은 우편함으로 발송되었습니다.");
                ClickCloseBtn();
            });
        });
    }
#endif
}
