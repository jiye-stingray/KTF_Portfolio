using UnityEngine;
using UnityEngine.UI;

public class UIDailyCurrencyLimitPopup : UIPopupBase
{
    [SerializeField] LimitCurrencyText[] _items;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        Refresh();
        return true;
    }

    public override void Refresh()
    {
        foreach (var item in _items)
            item.Init();
    }
}
