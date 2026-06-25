using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;


public class InventoryScrollviewItem : ICell, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Image _gradeBg;
    [SerializeField] GameObject _eIcon;
    [SerializeField] Image _factionIcon;
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _txt;
    [SerializeField] GameObject _legendayEffect;
    [SerializeField] GameObject _mythicEffect;
   
    public long _id;
    public EInventoryItemType _type;

    // data
    CurrencyData _currencyDBData;
    EquipmentItemData _equipmentItemData;
    Item _itemDBData;

    [Header("Event")]
    public UnityEvent _onLongPress;
    public UnityEvent _onLongPressRelease;

    private bool _isPointerDown = false;
    private float _holdTime = 0f;
    private bool _isPressTringgerd;

    public void SetData(int index, long id, EInventoryItemType type)
    {
        _index = index;
        _id = id;
        _type = type;



        Refresh();
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

    void Refresh()
    {
        switch (_type)
        {
            case EInventoryItemType.Item:
                _itemDBData = ClientLocalDB_Simple.GetData<Item>(DBKey.Item, _id);
                if (_itemDBData == null)
                {
                    Debug.LogError($"item data null!! id = {_id}");
                }

                DrawItem();
                break;
            case EInventoryItemType.Equipment:
                _equipmentItemData = Managers.Instance.UserInfo().GetEquipmentItemData(_id);
                if (_equipmentItemData == null)
                {
                    Debug.LogError($"Equipment data null!! id = {_id} ");
                }
                DrawEquipment();
                break;
            case EInventoryItemType.Currency:
                _currencyDBData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, _id);
                if (_currencyDBData == null)
                {
                    Debug.LogError($"Currency data null!! id = {_id}");
                }
                DrawCurrency();
                break;
            default:
                break;
        }
    }

    private void DrawItem()
    {
        _gradeBg.gameObject.SetActive( false );
        _eIcon.SetActive( false );
        _factionIcon.gameObject.SetActive( false );
        _txt.text = Utils.NumberFormatter.FormatNumber(Managers.Instance.UserInfo().GetItemValue(_itemDBData.ID));

        _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ShopAtlas, _itemDBData.Icon);
        _gradeBg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{_itemDBData.Grade}");
    }

    private void DrawEquipment()
    {
        _gradeBg.gameObject.SetActive(true);
        _gradeBg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{_equipmentItemData.data.Grade}");
        _eIcon.SetActive(_equipmentItemData.isSet);
        _factionIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)_equipmentItemData.data.Faction}");

        _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.EquipmentAtlas, _equipmentItemData.data.Name);
        _txt.gameObject.SetActive(false);

        var grade = _equipmentItemData.data.Grade;
        _legendayEffect.SetActive(grade == EGradeType.Legendary || grade == EGradeType.Legendary_Plus);
        _mythicEffect.SetActive(grade == EGradeType.Mythic);
    }

    private void DrawCurrency()
    {
        ECurrency currencyType = (ECurrency)_currencyDBData.ID;
        _gradeBg.gameObject.SetActive(false);
        _eIcon.SetActive(false);
        _factionIcon.gameObject.SetActive(false);
        _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, _currencyDBData.Icon);

        if (currencyType == ECurrency.Cash_Free || currencyType == ECurrency.MidCash)
            _txt.text = Managers.Instance.UserInfo().GetCurrencyValue(currencyType).ToString("N0");
        else
            _txt.text = Utils.NumberFormatter.FormatNumber(Managers.Instance.UserInfo().GetCurrencyValue(currencyType));

    }

    public void Click()
    {
        switch (_type)
        {
            case EInventoryItemType.Item:
                if(_itemDBData.ItemType == EItemType.Select)
                {
                    UISubOpenItem uISubOpenItem = UIManager.ShowUISubBase<UISubOpenItem>(UIManager.UIInventory, "UISubOpenItem");
                    uISubOpenItem.SetItemDataOpenToStack(_itemDBData);
                }
                else if(_itemDBData.ItemType == EItemType.Random)
                {
                    UISubRandomOpenItem uiSubRandomOpenItem = UIManager.ShowUISubBase<UISubRandomOpenItem>(UIManager.UIInventory, "UISubOpenRandomItem");
                    uiSubRandomOpenItem.SetItemDataOpenToStack(_itemDBData);
                }
                    break;
            case EInventoryItemType.Equipment:

                if(_equipmentItemData.data.Grade < EGradeType.Mythic)
                {
                    UISubEquipmentDetail sub = UIManager.ShowUISubBase<UISubEquipmentDetail>( UIManager.UIInventory,"UISubEquipmentDetail");
                    sub.SetDataOpenToStack(_equipmentItemData);
                }
                else
                {
                    UISubMythicEquipmentDetail sub = UIManager.ShowUISubBase<UISubMythicEquipmentDetail>(UIManager.UIInventory, "UISubMythicEquipmentDetail");
                    sub.SetDataOpenToStack(_equipmentItemData);
                }

                break;
            case EInventoryItemType.Currency:
                break;
            default:
                break;
        }
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
        Vector3 offset = new Vector3(130f, -94f, 0f);
        Vector3 worldPos = transform.position + transform.TransformVector(offset); // 로컬 offset → 월드로 변환

        switch (_type)
        {
            case EInventoryItemType.Item:
                UIManager.UITooltip.Init(_itemDBData.UIName, _itemDBData.Desc, worldPos);
                break;
            case EInventoryItemType.Equipment:
                break;
            case EInventoryItemType.Currency:
                    UIManager.UITooltip.Init(_currencyDBData.UIName, _currencyDBData.Desc, worldPos);
                break;
            default:
                break;
        }


    }

    #endregion
}
