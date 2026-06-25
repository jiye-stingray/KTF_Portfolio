using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonthShopItemData : ShopItemData
{
    public bool isPurchased => day > 0;
    public int day;
}
