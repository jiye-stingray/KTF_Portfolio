using UnityEngine;

public class UISubShop : UISubBase
{
    [SerializeField] SubShopPackageTab _packageTab;
    [SerializeField] SubShopCurrencyTab _currencyTab;
    [SerializeField] SubShopMidCashTab _midCashTab;
    [SerializeField] SubShopMonthTab _monthTab;
    [SerializeField] SubCashTab _cashTab;

    ShopItemData _shopItemData;

    public void SetShopTypeOpenToStack(ShopItemData shopItemData)
    {
        _shopItemData = shopItemData;
        OpenToStack();
    }

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public override void Refresh()
    {
        // 항상 close 로 shopmanager 구독 제거 한 뒤에 tab 별로 open 할 것 
        _currencyTab.Close();
        _monthTab.Close();
        _midCashTab.Close();
        _cashTab.Close();
        _packageTab.Close();

        if (_shopItemData == null) return;
        switch (_shopItemData._type)
        {
            case Define.EShopType.PackageShop:
                _packageTab.SetDataOpen(_shopItemData as PackageShopItemData);
                break;
            case Define.EShopType.MonthlyShop:
                _monthTab.SetDataOpen(_shopItemData as MonthShopItemData);
                break;
            case Define.EShopType.MidCashShop:
                _midCashTab.SetDataOpen(_shopItemData as MidCashShopItemData);
                break;
            case Define.EShopType.CashShop:
                _cashTab.SetDataOpen(_shopItemData);
                break;
            case Define.EShopType.TicketShop:
            case Define.EShopType.GuildShop:
            case Define.EShopType.GoldShop:
            case Define.EShopType.HeroPieceShop:
                _currencyTab.SetDataOpen(_shopItemData);
                break;
            default:
                break;
        }
    }
}
