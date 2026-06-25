using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class CraftCurrencyItem :MonoBehaviour
{
    public TMP_Text _name;
    public Image _icon;
    public TMP_Text _count;

    AtlasManager atlas => Managers.Instance.GetAtlasManager();

    public void SetCurrency(ECurrency currency, int count)
    {
        CurrencyData data = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, (int)currency);
        if (data == null) return;

        _name.text = data.UIName;
        _icon.sprite = atlas.GetSprite(EAtlasType.ItemIconAtlas, data.Icon);

        int owned = Managers.Instance.UserInfo().GetCurrencyValue(currency);
        _count.text = count.ToString();
        _count.color = owned < count ? Utils.HexToColor("FF5959") : Utils.HexToColor("#FFF4E8");
    }

}
