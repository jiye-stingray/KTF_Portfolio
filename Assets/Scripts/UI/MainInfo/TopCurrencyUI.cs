using UnityEngine;
using static Define;

public class TopCurrencyUI : UIBase
{
    [SerializeField] private UICurrencyTextIcon[] _currencyTextIcons;

    public void SetCurrency(Transform parent, params ECurrency[] currencies)
    {
        transform.SetParent(parent);

        for (int i = 0; i < _currencyTextIcons.Length; i++)
        {
            UICurrencyTextIcon currencyTextIcon = _currencyTextIcons[i];
            currencyTextIcon.gameObject.SetActive(i < currencies.Length);
            
            if (i >= currencies.Length)
                continue;
            
            currencyTextIcon.Init(currencies[i]);
        }
    }
}
