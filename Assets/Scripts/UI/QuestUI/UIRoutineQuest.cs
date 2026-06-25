using PolyAndCode.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIRoutineQuest : UIBase , IRecyclableScrollRectDataSource
{
    #region Tap
    public enum ETAP_TYPE
    {
        Dayily, 
        Weekly
    }
    ETAP_TYPE _currentTap = ETAP_TYPE.Dayily;

    public UITabGroup _group;
    #endregion

    [SerializeField] TMP_Text _currentPointTxt;
    [SerializeField] PointQuestRewardButton[] _pointQuestRewardBtns;
    [SerializeField] Slider _currentPointSlider;
    [SerializeField] RoutineQuestFinishPanel _routineQuestFinishPanel;

    List<RoutineQuestItemData> _routineQuestItemDataList = new List<RoutineQuestItemData>();
    [SerializeField] RecyclableScrollRect _scrollView;

    [Header("RedDot")]
    [SerializeField] GameObject _tabDailyRoutineRedDot;
    [SerializeField] GameObject _tabWeeklyRoutineRedDot;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override void Refresh()
    {

        switch (_currentTap)
        {
            case ETAP_TYPE.Dayily:
                _routineQuestItemDataList = UserInfoData._dicDailyRoutineQuestData.Values.ToList();
                break;
            case ETAP_TYPE.Weekly:
                _routineQuestItemDataList = UserInfoData._dicWeeklyRoutineQuestData.Values.ToList();
                break;
            default:
                break;
        }

        PointRefresh();

        // routine
        if(gameObject.activeSelf)
        {
            _scrollView.Initialize(this);
            _scrollView.ReloadData();
        }
        DrawFinishPanel();

        _tabDailyRoutineRedDot.SetActive(RedDotManager.AllDailyRoutineQuestRedDot());
        _tabWeeklyRoutineRedDot.SetActive(RedDotManager.AllWeeklyRoutineQuestRedDot());

    }

    #region DrawPoint

    public void PointRefresh()
    {
        switch (_currentTap)
        {
            case ETAP_TYPE.Dayily:
                _routineQuestItemDataList = UserInfoData._dicDailyRoutineQuestData.Values.ToList();
                DayilyRefresh();
                break;
            case ETAP_TYPE.Weekly:
                _routineQuestItemDataList = UserInfoData._dicWeeklyRoutineQuestData.Values.ToList();
                WeeklyRefresh();
                break;
            default:
                break;
        }
    }

    private void DayilyRefresh()
    {
        _currentPointTxt.text = UserInfoData._dailyQuestPoint.ToString();
        _currentPointSlider.value = (float)UserInfoData._dailyQuestPoint / QuestPointMax; 

        _pointQuestRewardBtns[0].Init(20,EResetType.Daily);
        _pointQuestRewardBtns[1].Init(40,EResetType.Daily);
        _pointQuestRewardBtns[2].Init(60,EResetType.Daily);
        _pointQuestRewardBtns[3].Init(80,EResetType.Daily);
        _pointQuestRewardBtns[4].Init(100,EResetType.Daily);

    }

    private void WeeklyRefresh()
    {
        _currentPointTxt.text = UserInfoData._weeklyQuestPoint.ToString();
        _currentPointSlider.value = (float)UserInfoData._weeklyQuestPoint / QuestPointMax;

        _pointQuestRewardBtns[0].Init(20, EResetType.Weekly);
        _pointQuestRewardBtns[1].Init(40, EResetType.Weekly);
        _pointQuestRewardBtns[2].Init(60, EResetType.Weekly);
        _pointQuestRewardBtns[3].Init(80, EResetType.Weekly);
        _pointQuestRewardBtns[4].Init(100, EResetType.Weekly);
    }

    #endregion

    private void DrawFinishPanel()
    {
        _routineQuestFinishPanel.gameObject.SetActive(false);
        switch (_currentTap)
        {
            case ETAP_TYPE.Dayily:
                foreach (var item in UserInfoData._dicDailyQuestPointData)
                {
                    if (!item.Value.isClear) return;
                    else if (item.Value.isFinish == false)
                        return;
                }
                break;
            case ETAP_TYPE.Weekly:
                foreach (var item in UserInfoData._dicWeeklyQuestPointData)
                {
                    if (!item.Value.isClear) return;
                    else if (item.Value.isFinish == false)
                        return;
                }
                break;
            default:
                break;
        }

        _routineQuestFinishPanel.gameObject.SetActive(true);
        _routineQuestFinishPanel.Init((EResetType)_currentTap + 1);
    }

    #region Recycle ScrollView
    public int GetItemCount()
    {
        return _routineQuestItemDataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as RoutineQuestScrollviewItem;
        item.SetData(_routineQuestItemDataList[index], index);
    }
    #endregion

    public override void Open()
    {
        _group._currentTapGroupBtn = _group._tapGroupBtns[0];
        _group.Set(0);
        base.Open();
        OnChangeTap();

    }

    /// <summary>
    /// Group에 changeEvent로 연결
    /// </summary>
    public void OnChangeTap()
    {
        _currentTap = (ETAP_TYPE)_group._currentTapGroupBtn._index;
        Refresh();
    }

}
