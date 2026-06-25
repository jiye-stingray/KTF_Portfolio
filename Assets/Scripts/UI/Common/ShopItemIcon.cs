using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class ShopItemIcon : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _txtCount;
    ECurrency _currencyType;
    CurrencyData _currencyData;
    int _count;
    /*
     * *
     */
    public void Init(ECurrency currency,int count)
    {
        _currencyType = currency;
        _currencyData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, currency);
        _count = count;
        Refresh();
    }

    public void Refresh()
    {
        _txtCount.text = $"x{_count}";
    }

}
