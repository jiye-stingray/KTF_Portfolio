using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ShopItemData : ItemData
{
    public int id;
    public EShopType _type;

    public object GetData()
    {
        return _type switch
        {
            EShopType.PackageShop => ClientLocalDB_Simple.GetData<PackageShop>(DBKey.PackageShop, id),
            EShopType.MonthlyShop   => ClientLocalDB_Simple.GetData<MonthShop>(DBKey.MonthShop, id),
            EShopType.CashShop    => ClientLocalDB_Simple.GetData<CashShop>(DBKey.CashShop, id),
            EShopType.MidCashShop => ClientLocalDB_Simple.GetData<MidCashShop>(DBKey.MidCashShop, id),
            EShopType.TicketShop  => ClientLocalDB_Simple.GetData<TicketShop>(DBKey.TicketShop, id),
            EShopType.GuildShop   => ClientLocalDB_Simple.GetData<GuildShop>(DBKey.GuildShop, id),
            EShopType.GoldShop    => ClientLocalDB_Simple.GetData<GoldShop>(DBKey.GoldShop, id),
            EShopType.HeroPieceShop    => ClientLocalDB_Simple.GetData<HeroPieceShop>(DBKey.HeroPieceShop, id),
            EShopType.LimitShop => ClientLocalDB_Simple.GetData<LimitedShop>(DBKey.LimitedShop,id),
            _ => null
        };
    }
}
