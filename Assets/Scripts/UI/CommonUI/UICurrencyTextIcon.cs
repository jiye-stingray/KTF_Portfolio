using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UICurrencyTextIcon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] TMP_Text _valueTxt;
    [SerializeField] Image _icon;
    [SerializeField] GameObject _plusImg;
    
    private ECurrency _currencyType;
    UserInfoData UserInfoData => Managers.Instance.UserInfo();
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    [Header("Tooltip")]
    [SerializeField] protected UnityEvent _onLongPress = new UnityEvent();

    protected bool _isPointerDown = false;
    protected float _holdTime = 0f;
    protected bool _isPressTringgerd;

    [SerializeField] Vector3 _tooltipOffset = new Vector3(0, -100, 0);

    protected UIManager uimanager => Managers.Instance.GetUIManager();


    public void Init(ECurrency currencyType)
    {
        // 이전 구독 정리(중복 구독 방지)
        _disposables.Clear();
        
        _currencyType = currencyType;

        CurrencyData currencyData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, currencyType);
        if (currencyData != null)
        {
            _icon.sprite  = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, currencyData.Icon);
        }
        
        ReactiveProperty<int> currencyValue = UserInfoData.GetCurrencyProperty(currencyType);

        // Cash_Free(4)는 Cash_Purchase(5)와 합산하여 표시
        if (currencyType == ECurrency.Cash_Free)
        {
            ReactiveProperty<int> cashPurchaseValue = UserInfoData.GetCurrencyProperty(ECurrency.Cash_Purchase);
            currencyValue.CombineLatest(cashPurchaseValue, (free, purchase) => free + purchase)
                .DistinctUntilChanged()
                .StartWith(currencyValue.Value + cashPurchaseValue.Value)
                .Subscribe(combinedValue =>
                {
                    _valueTxt.text = combinedValue.ToString("N0");
                }).AddTo(_disposables);
        }
        else if (currencyType == ECurrency.MidCash)
        {
            currencyValue
                .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
                .StartWith(currencyValue.Value)  // 처음 Init 시 현재값 바로 반영
                .Subscribe(newValue =>
                {
                    _valueTxt.text = newValue.ToString("N0");
                }).AddTo(_disposables);
        }
        else
        {
            currencyValue
                .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
                .StartWith(currencyValue.Value)  // 처음 Init 시 현재값 바로 반영
                .Subscribe(newValue =>
                {
                    _valueTxt.text = Utils.NumberFormatter.FormatNumber(newValue);
                }).AddTo(_disposables);
        }

        // --- 수정된 부분 시작 ---
        if (_plusImg != null)
        {
            bool isPlusCurrency = (_currencyType == ECurrency.Cash_Free || _currencyType == ECurrency.MidCash);
        
        #if ONE
                // ONE 빌드에서는 플러스 버튼을 무조건 숨김
                isPlusCurrency = false;
        #endif
            
            _plusImg.SetActive(isPlusCurrency);
        }
        
    }

    public void Update()
    {
        if (_isPointerDown)
        {
            _holdTime += Time.deltaTime;
            if (!_isPressTringgerd && _holdTime > Utils.LongPressTime)
            {
                _isPressTringgerd = true;
                _onLongPress?.Invoke();
            }
        }
    }

    public void OnDestroy()
    {
        _disposables.Dispose();     // 완전 해제
    }


    /// 필요 시 외부에서 수동 해제할 수 있게 제공(옵션)
    public void Unbind()
    {
        _disposables.Clear();
    }

    /// <summary>
    /// 버튼 이벤트로 연결
    /// </summary>
    public void ShowToolTipEvent()
    {
        Vector3 worldPos = transform.position + transform.TransformVector(_tooltipOffset); // 로컬 offset → 월드로 변환
        CurrencyData currencyData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, _currencyType);
        uimanager.UITooltip.Init(currencyData.UIName,currencyData.Desc,worldPos);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        _holdTime = 0f;
        _isPressTringgerd = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
    }

    /// <summary>
    /// 버튼 이벤트로 연결
    /// 상점 이동
    /// </summary>
    public void Click()
    {
    #if ONE
        // ONE 빌드에서는 클릭 시 상점 이동 기능을 차단 (심사 리젝 방지)
        return;
    #endif
        
        if(_currencyType == ECurrency.Cash_Free || _currencyType == ECurrency.MidCash)
        {
            uimanager.UIShop.SetCashShopOpenToStack(true);
        }
    }

}
