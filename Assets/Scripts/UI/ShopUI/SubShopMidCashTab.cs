using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using static Define;

public class SubShopMidCashTab : UITabBase
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _countTxt;
    [SerializeField] UIShopCostButton _costBtn;
    MidCashShop _db;
    AtlasManager atlas => Managers.Instance.GetAtlasManager();
    int _multi;


    public void SetDataOpen(MidCashShopItemData data)
    {
        Open();

        _db = data.GetData() as MidCashShop;
        _multi = data._isfirstBuy ? 2 : 1;
        Refresh();
    }

    public override void Refresh()
    {
        _nameTxt.text = _db.ItemTitle;
        _icon.sprite = atlas.GetSprite(EAtlasType.ShopAtlas, _db.Resource);
        _countTxt.text = "x" + (_db.RewardCount * _multi) .ToString();
#if IAP
        _costBtn.Init(_db);
#endif
    }

#if IAP

    public override void Open()
    {
        base.Open();
        SubscribeShop(true);
    }

    public override void Close()
    {
        SubscribeShop(false);
        base.Close();
    }

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
        Managers.Instance?.IAP?.RepublishPendingOrders(
            pid => ShopManager.Instance.IsProductOfKind(pid, ShopManager.EShopKind.MidCash));
        
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
        Managers.Instance.GetServerManager().OnPostShopTypePurchase(EShopType.MidCashShop,int.Parse(goodsId), 1, shop, payLoad, (shopDto, reward,response) =>
        {
            onSuccess.Invoke(response);
            UIManager.UIShop.OnChangeTab();     // scrollview Update
            Managers.Instance.GetServerManager().OnPostRequestMyMail(() =>
            {
                UIManager.MainInfoUI.Refresh();
                UIManager.ShowCommonToastMessage("구매하신 상품은 우편함으로 발송되었습니다.");
                _mainUI.GetComponent<UISubBase>().ClickCloseBtn();
            });
        });
    }
#endif
}
