using PolyAndCode.UI;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoutineQuestScrollviewItem : ICell
{
    [SerializeField] TMP_Text _descTxt;
    [SerializeField] TMP_Text _pointValueTxt;
    [SerializeField] TMP_Text _sliderTxt;
    [SerializeField] Slider _slider;
    [SerializeField] GameObject _clear;
    [SerializeField] GameObject _gray;

    [SerializeField] GameObject _redDot;

    RoutineQuestItemData _data;
    UserInfoData UserInfoData => Managers.Instance.UserInfo();

    public override void SetData(ItemData data, int index)
    {
        _data = data as RoutineQuestItemData;
        _index = index;
        Refresh();
    }

    private void Refresh()
    {
        _descTxt.text = DescBuilder.ReturnQuestDesc(_data._conditionType, _data.TableQuestDB().Desc, _data.TableQuestDB().ConditionValue);
        _pointValueTxt.text = _data.TableQuestDB().QuestPoint.ToString();
        _slider.value = (float)_data._progressValue / _data.TableQuestDB().ConditionValue.Last();
        _sliderTxt.text = $"{_data._progressValue} / {_data.TableQuestDB().ConditionValue.Last()}";

        // Button
        _clear.SetActive(_data.isClear && !_data.isFinish);
        _gray.SetActive(_data.isFinish);

        _redDot.SetActive(RedDotManager.RoutineQuestRedDot(_data));
    }

    public void Click()
    {
        if (_gray.activeSelf) return;

        if (_clear.activeSelf)
        {
#if USE_SERVER
            Managers.Instance.GetServerManager().OnPostQuestPointInfo(_data._tableId);
            return;
#else

            // 보상 획득 
            switch (_data._resetType)
            {
                case Define.EResetType.Daily:
                    UserInfoData._dailyQuestPoint += _data.TableQuestDB().QuestPoint;
                    break;
                case Define.EResetType.Weekly:
                    UserInfoData._weeklyQuestPoint += _data.TableQuestDB().QuestPoint;
                    break;
            }

            _data.isFinish = true;
            Refresh();
            Managers.Instance.GetUIManager().UIRoutineQuest.PointRefresh();
            Managers.Instance.GetUIManager().MainInfoUI.Refresh();
            return;

#endif
        }
        // 이동 
            UserInfoData.QuestMoveAction(_data._conditionType, 0);

        }

    }

