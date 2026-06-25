#if IAP
using UnityEngine.Purchasing;
#endif

public class UIShopCostButton : UICostButton
{
#if IAP
    Product _product;
    IapManager iap => Managers.Instance.IAP;
#endif

    ProductShopData _data;

    public void Init(ProductShopData data)
    {

        _isIAP = true;
        _data = data;
#if IAP
        _product = iap.GetProduct(data.ProductID);
#endif
        Refresh();
    }


    public override void Refresh()
    {
        if(_isIAP == false)
        {
            base.Refresh();
            return;
        }
        for (int i = 0; i < _valueTxt.Length; i++)
        {
            _valueTxt[i]?.gameObject.SetActive(false);
            _currencyIcon[i]?.gameObject.SetActive(false);
        }
#if IAP
        if(_product == null) return;
        _valueTxt[0].gameObject.SetActive(true);
        _valueTxt[0].text = $"{_product.metadata.localizedPriceString}";
#endif
    }

    public override void Click()
    {
        base.Click();

#if IAP
        ShopManager.Instance.TryPurchase(_data);
#endif
    }

}
