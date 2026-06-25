using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static Define;

public class RewardItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RewardIcon _rewardIcon;
    [SerializeField] private TMP_Text _count;
    [SerializeField] private GameObject _deckSynergyIconObj;
    ERewardType _rewardType;
    int _id;

    [Header("Event")]
    public UnityEvent _onLongPress;
    public UnityEvent _onLongPressRelease;

    public TMP_Text CountText => _count;
    private bool _isPointerDown = false;
    private float _holdTime = 0f;
    private bool _isPressTringgerd;

    private UIManager UIManager => Managers.Instance.GetUIManager();

    public void Init(ERewardType rewardType, int id, int count, string synergyCode = null)
    {
        _rewardType = rewardType;
        _id = id;
        if (_deckSynergyIconObj != null)
            _deckSynergyIconObj.SetActive(rewardType == ERewardType.Emblem);

        _rewardIcon.Init(rewardType, id, synergyCode);
        if (_count != null)
        {
            _count.gameObject.SetActive(rewardType != ERewardType.Equipment);
            _count.text = $"x{count.ToString()}";
        }
        _onLongPress.RemoveAllListeners();
        _onLongPress.AddListener(ShowToolTipEvent);
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
        Vector3 offset = new Vector3(130f, 90f, 0f);
        Vector3 worldPos = transform.position + transform.TransformVector(offset); // 로컬 offset → 월드로 변환

        switch (_rewardType)
        {
            case ERewardType.QuestPoint:
                break;
            case ERewardType.PassPoint:
                break;
            case ERewardType.Currency:
                CurrencyData currencydata = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, _id);
                UIManager.UITooltip.Init(currencydata.UIName, currencydata.Desc, worldPos);
                break;
            case ERewardType.CharacterGrade:
                break;
            case ERewardType.Character:
                break;
            case ERewardType.Equipment:
                break;
            case ERewardType.EquipmentBox:
                break;
            case ERewardType.Emblem:
                break;
            case ERewardType.None:
                break;
            case ERewardType.ItemBox:
                Item itemData =  ClientLocalDB_Simple.GetData<Item>(DBKey.Item, _id);
                UIManager.UITooltip.Init(itemData.UIName, itemData.Desc, worldPos);
                break;
            case ERewardType.HeroPiece:
                break;
            case ERewardType.RelicParts:
                break;
        }


    }

    #endregion
}
