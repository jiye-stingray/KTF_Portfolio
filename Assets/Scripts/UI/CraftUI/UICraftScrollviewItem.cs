using PolyAndCode.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UICraftScrollviewItem : ICell, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _selectGo;

    CraftData _data;
    CurrencyData _currencyDBData;
    bool _isCurrent;

    [Header("Event")]
    public UnityEvent _onLongPress;
    public UnityEvent _onLongPressRelease;

    private bool _isPointerDown = false;
    private float _holdTime = 0f;
    private bool _isPressTringgerd;

    public void SetData(CraftData craftData, bool isCurrent)
    {
        _isCurrent = isCurrent;
        _data = craftData;
        _currencyDBData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, craftData.RewardID);
        if (_currencyDBData == null) return;

        _selectGo.SetActive(isCurrent);
        _icon.sprite = AtlasManager.GetSprite(EAtlasType.ItemIconAtlas, _currencyDBData.Icon);
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

    public void Click()
    {
        if (_isCurrent) return;
        UIManager.UICraft.SetCraftData(_data);
    }

    #region Event
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
    /// </summary>
    public void ShowToolTipEvent()
    {
        if (_currencyDBData == null) return;

        Vector3 offset = new Vector3(50f, -130f, 0f);
        Vector3 worldPos = transform.position + transform.TransformVector(offset); // 로컬 offset → 월드로 변환

        UIManager.UITooltip.Init(_currencyDBData.UIName, _currencyDBData.Desc, worldPos);
    }

    #endregion
}
