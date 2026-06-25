using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class LimitCurrencyText : MonoBehaviour
{
    [SerializeField] ECurrency _currency;
    [SerializeField] Image _iconImage;
    [SerializeField] TMP_Text _acquireText;

    CompositeDisposable _disposables = new CompositeDisposable();

    public void Init()
    {
        _disposables.Clear();

        var userInfo = Managers.Instance.UserInfo();

        CurrencyData currencyData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, (int)_currency);
        if (currencyData != null)
            _iconImage.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, currencyData.Icon);

        if (!userInfo._dicCurrencyAcquireNumberData.TryGetValue(_currency, out var acquireNumber))
            return;

        if (!userInfo._dicCurrencyAcquireLimitData.TryGetValue(_currency, out int limit))
            return;

        acquireNumber
            .Subscribe(value =>
            {
                _acquireText.text = $"{value} / {limit}";
                _acquireText.color = value >= limit
                    ? Utils.HexToColor("B15046")
                    : Utils.HexToColor("261E17");
            })
            .AddTo(_disposables);
    }

    void OnDestroy()
    {
        _disposables.Clear();
    }
}
