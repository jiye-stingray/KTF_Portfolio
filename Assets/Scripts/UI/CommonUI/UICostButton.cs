using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Define;

public class UICostButton : MonoBehaviour
{
    [SerializeField] protected GameObject _text_ValueTxt;
    [SerializeField] protected Image[] _currencyIcon;
    [SerializeField] protected TMP_Text[] _valueTxt;
    [SerializeField] protected GameObject _gray;
    [SerializeField] protected TMP_Text _descTxt;

    Button _btn;

    protected ECurrency[] _currency;
    protected int[] _value;

    public bool isGray => _gray.activeSelf;

    [SerializeField] UnityEvent _successEvent;

    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    protected bool _isIAP;

    protected BestHttp_GameManager ServerManager => Managers.Instance.GetServerManager();

    protected UserInfoData UserInfoData => Managers.Instance.UserInfo();
    protected UIManager UIManager => Managers.Instance.GetUIManager();

    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(Click);
    }

    public void Init(int[] currencyId, int[] value)
    {
        ECurrency[] enums = currencyId.Select(i => (ECurrency)i).ToArray();
        Init(enums, value);
    }

    public virtual void Init(ECurrency[] currency, int[] value)
    {
        _currency = currency;
        _value = value;

        // 구독 (sync currency 를 위해)
        _disposables.Clear();       // 초기화
        for (int i = 0; i < _currency.Length; i++)
        {
            ECurrency currencyType = _currency[i];
            if(currencyType == ECurrency.None)
                continue;
            
            ReactiveProperty<int> currencyValue = UserInfoData.GetCurrencyProperty(currencyType);
            currencyValue
                .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
                .StartWith(currencyValue.Value)  // 처음 Init 시 현재값 바로 반영 
                .Subscribe(newValue =>
                {
                    Refresh();
                }).AddTo(_disposables);
        }

        for (int i = 0; i < _currency.Length; i++)
        {
            if(_currency[i] == ECurrency.None)
                continue;
            
            CurrencyData currencyData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, _currency[i]);
            _currencyIcon[i].sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, currencyData.Icon);
        }

        _isIAP = false;
        
        Refresh();
    }

    public virtual void Init(int[] value)
    {
        _currency = new ECurrency[1] { ECurrency.None};
        _value = value;

        _isIAP = false;

        Refresh();
    }

    /// <summary>
    /// 세팅한 재화에 맞춰 보여줌 
    /// Init 할때 호출
    /// </summary>
    public virtual void Refresh()
    {


        // add code sizz _currency 재화 길이만큼만 처리 나중에 DB 체크 필요
        if (_currency.Length != _value.Length || _currency.Length != _valueTxt.Length)
        {
            MyLogger.LogWarning($"[CurrencyUI] 배열 길이 불일치! _currency={_currency.Length}, _value={_value.Length}, _valueTxt={_valueTxt.Length}");
        }

        // currency 부분 비활성화 시키고 활성화 시키기
        for (int i = 0; i < _valueTxt.Length; i++)
        {
            _valueTxt[i].gameObject.SetActive(false);
            _currencyIcon[i].gameObject.SetActive(false);
        }

        int len = Mathf.Min(_currency.Length, _value.Length, _valueTxt.Length);
        for (int i = 0; i < len; i++)
        {
            if (_valueTxt[i] == null) continue; // 혹시 null인 Text 참조도 방어

            _valueTxt[i].gameObject.SetActive(true);
            _currencyIcon[i].gameObject.SetActive(true);
            _valueTxt[i].text = _value[i].ToString();

            int userCurrency = Managers.Instance.UserInfo().GetCurrencyValue(_currency[i]);
            _valueTxt[i].color = (_value[i] > userCurrency)
                ? Utils.HexToColor("FF767A")
                : Color.white;
        }
        
        if(_gray != null)
        {
            for (int i = 0; i < _currency.Length; i++)
            {
                _gray?.SetActive((_value[i] > Managers.Instance.UserInfo().GetCurrencyValue(_currency[i])));
                if (_gray.activeSelf) break;
            }
        }
    }

    /// <summary>
    /// Sound
    /// </summary>
    public virtual void Click()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

    }

    public virtual void SuccessAction()
    {
        _successEvent?.Invoke();
    }

    /// 필요 시 외부에서 수동 해제할 수 있게 제공(옵션)
    public void Unbind()
    {
        _disposables.Clear();
    }

    private void OnDestroy()
    {
        _disposables.Dispose(); // 완전 해제
    }
}
