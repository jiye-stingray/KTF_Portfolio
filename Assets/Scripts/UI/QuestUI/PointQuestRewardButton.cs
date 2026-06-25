using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class PointQuestRewardButton : MonoBehaviour , IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Image _icon;
    [SerializeField] RewardTooltip _rewardTooltip;
    [SerializeField] Animator _anim;

    [Header("RedDot")]
    [SerializeField] GameObject _redDot;

    QuestPointItemData _questPoint = null;

    public UnityEvent<QuestPoint> _onLongPress;
    public UnityEvent _onLongPressRelease;

    private bool _isPointerDown = false;
    private float _holdTime = 0f;
    private float _longPressTime = 0.5f;
    private bool _isPressTringgerd;

    UserInfoData userinfo => Managers.Instance.UserInfo();
    UIManager UIManager => Managers.Instance.GetUIManager();
    AtlasManager atlas => Managers.Instance.GetAtlasManager();

    public void Init(int point , EResetType resetType)
    {
        _questPoint = userinfo.GetQuestPoint(resetType,point);    
        Refresh();
    }

    void Refresh()
    {
        _anim.SetBool("isFinish", _questPoint.isFinish);
        _anim.SetBool("isClear", _questPoint.isClear);


        if(_questPoint._point == 100)
        {
            _icon.sprite = _questPoint.isFinish ? atlas.GetSprite(EAtlasType.RoutineQuestAtlas, "Open_Box2") : atlas.GetSprite(EAtlasType.RoutineQuestAtlas, "Closedbox2");
        }
        else
        {
            _icon.sprite = _questPoint.isFinish ? atlas.GetSprite(EAtlasType.RoutineQuestAtlas, "Open_Box") : atlas.GetSprite(EAtlasType.RoutineQuestAtlas, "Closedbox");
        }


        // RedDot
        _redDot.SetActive(RedDotManager.QuestPointRedDot(_questPoint));
        
    }

    private void Update()
    {
        if(_isPointerDown)
        {
            _holdTime += Time.deltaTime;
            if(!_isPressTringgerd && _holdTime > _longPressTime)
            {
                _isPressTringgerd = true;
                _onLongPress?.Invoke(_questPoint._questPointData);
            }
        }
    }

    public void Click()
    {
        if (!_questPoint.isClear || _questPoint.isFinish) return;
        else if (!_anim.GetCurrentAnimatorStateInfo(0).IsName("PointQuestReward_Ready")) return;      //  장비 ready 중 아니면 다 반환

#if USE_SERVER
        BoxClickEvent();
#else

        MyLogger.Log(" 퀘스트 포인트 보상 획득");

        // UI Popup -----
        RewardBundleDto rewardDto = new RewardBundleDto() {
            characterRewardDtoList = new List<RewardDto>(),
            currencyRewardDtoList = new List<RewardDto>(),
            equipmentRewardDtoList = new List<EquipmentDto>()
        };

        QuestReward questReward = ClientLocalDB_Simple.GetData<QuestReward>(DBKey.QuestReward, _questPoint._questPointData.RewardID);
        if (questReward == null) Debug.LogError("data null!!");
        for (int i = 0; i < questReward.RewardType.Length; i++)
        {
            switch (questReward.RewardType[i])
            {
                case ERewardType.None:
                    break;
                case ERewardType.QuestPoint:
                    break;
                case ERewardType.PassPoint:
                    break;
                case ERewardType.Currency:
                    rewardDto.currencyRewardDtoList.Add(new RewardDto { tableId = questReward.RewardID[i], count = questReward.RewardValue[i] });
                    
                    break;
                case ERewardType.CharacterGrade:
                    break;
                case ERewardType.Character:
                    break;
                case ERewardType.Equipment:
                    break;
                default:
                    break;
            }
        }

        UISubRewards subUI = UIManager.ShowPopup<UISubRewards>("UIRewardPopup");
        subUI.SetRewardData(rewardDto);
        subUI.OpenToStack();
        
        // pass point 추가 ---
        userinfo.SetPassXp(EPassType.Season, userinfo.GetPassXp(EPassType.Season) + questReward.PassPoint);

        _questPoint.isFinish = true;

        Refresh();

#endif
    }

    public void BoxClickEvent()
    {
        if (_questPoint._resetType == EResetType.Daily)
        {
            Managers.Instance.GetServerManager().OnGetReceiveDayReward();
        }
        else
            Managers.Instance.GetServerManager().OnGetReceiveWeekReward();

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        _holdTime = 0f;
        _isPressTringgerd = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(_isPointerDown && _isPressTringgerd)
            _onLongPressRelease?.Invoke();

        _isPointerDown = false;
    }

    public void ShowRewardTooltip()
    {
        _rewardTooltip.InitQuestReward(_questPoint._questPointData.RewardID);
    }

    public void CloseRewardTooltip()
    {
        _rewardTooltip.gameObject.SetActive(false);
    }
}
