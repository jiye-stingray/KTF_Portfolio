using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public enum ETextPrefixType
{
    None = 0,
    X
}

public class CurrencyIcon : MonoBehaviour , IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _countTxt;
    [SerializeField] ETextPrefixType _prefixType = ETextPrefixType.X;
    
    CurrencyData _data;
    int _count;
    private StringBuilder _builder = new StringBuilder();

    [Header("Event")]
    public UnityEvent _onLongPress;
    public UnityEvent _onLongPressRelease;

    private bool _isPointerDown = false;
    private float _holdTime = 0f;
    private bool _isPressTringgerd;

    private AtlasManager AtlasManager => Managers.Instance.GetAtlasManager();
    
    public void Init(ECurrency currency, int count)
    {
        _data = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency,currency);    
        _count = count;
        _countTxt.gameObject.SetActive(true);

        _onLongPress.RemoveAllListeners();
        _onLongPress.AddListener(ShowToolTipEvent);

        Refresh();
    }

    public void Init(ECurrency currency)
    {
        _data = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, currency);
        _countTxt.gameObject.SetActive(false);

        _onLongPress.RemoveAllListeners();
        _onLongPress.AddListener(ShowToolTipEvent);

        Refresh();
    }

    public void SetTextColor(Color color)
    {
        _countTxt.color = color;
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

    private void Refresh()
    {
        
        _icon.sprite = AtlasManager.GetSprite(EAtlasType.ItemIconAtlas, _data.Icon);
        _builder.Clear();
        if (_prefixType == ETextPrefixType.X)
            _builder.Append("x");
        _builder.Append(_count.ToString());
        _countTxt.text = _builder.ToString();
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

    UIManager uimanager => Managers.Instance.GetUIManager();
    /// <summary>
    /// 버튼 이벤트로 연결
    /// </summary>
    public void ShowToolTipEvent()
    {
        Vector3 offset = new Vector3(-130f, 130f, 0f);
        Vector3 worldPos = transform.position + transform.TransformVector(offset); // 로컬 offset → 월드로 변환

        uimanager.UITooltip.Init(_data.UIName, _data.Desc, worldPos);
    }

    #endregion
}
