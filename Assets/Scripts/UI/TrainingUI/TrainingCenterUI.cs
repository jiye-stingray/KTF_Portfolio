using PolyAndCode.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;


public class TrainingCenterUI : UIBase , IRecyclableScrollRectDataSource
{
    List<TrainingItemData> _dataList = new List<TrainingItemData>();    
    UITrainingUnlock _curentUITrainingUnlock;
    [SerializeField] RecyclableScrollRect _scrollview;

    [Header("Anim")]
    public bool _isAnim;     // animation 실행 필요 
    ETrainingType _trainingType;
    int _trainingId;

    public override void Open()
    {
        base.Open();
        _isAnim = false;
        UIManager.TopCurrencyUI.SetCurrency(this.transform, ECurrency.Ingot, ECurrency.Special_Ingot);
        RefreshItemList();
        _scrollview.Initialize(this); 
        Refresh();
        SetStartJumpDataIndex();
    }

    #region Recycle ScrollView
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as TrainingScrollviewItem;
        item.SetData(_dataList[index], index);
    }
    
    #endregion

    public override void Refresh()
    {
        // 연출 위해 항상 Reload 
        _scrollview.ReloadData();
    }

    /// <summary>
    /// 시작할 떄 스크롤뷰 포커싱 하는 조건
    /// 1 - 진행 가능한 HardTraing이 있을 경우 해당 위치로 포커스
    /// 2 – 진행 가능한 BasicTraing  q이 있을 경우 해당 위치로 포커스
    /// 3 – 진행 가능한 트레이닝이 없을 경우 최후의 HardTraing 위치로 포커스
    /// </summary>
    private void SetStartJumpDataIndex()
    {
        int id = _dataList.Count;

        if(UserInfoData.UnlockHardIdx < UserInfoData.MaxHardIdx)
        {
            id = ReturnBasicIdxUsingHardIdx(UserInfoData.UnlockHardIdx + 1);
        }
        else
        {
            if(UserInfoData.UnlockBasicIdx < UserInfoData.MaxBasicIdx)
            {
                id = UserInfoData.UnlockBasicIdx;
            }
            else
            {
                id = ReturnBasicIdxUsingHardIdx(UserInfoData.UnlockHardIdx + 1);
            }
        }

        JumpToDataIndex(id);
    }

    private void RefreshItemList()
    {
        _dataList = UserInfoData._trainingItemList;
    }

    public void SetCurrentUITrainingUnlock(TrainingItemData data, ETrainingType type, RectTransform trans)
    {
        _curentUITrainingUnlock = Managers.Instance.GetUIManager().ShowUISubBase<UITrainingUnlock>(this, "UITrainingUnlock");
        _curentUITrainingUnlock.InitData(data, type, trans);
        _curentUITrainingUnlock.OpenToStack();
    }

    /// <summary>
    /// Basic 인덱스를 넣어야함
    /// </summary>
    /// <param name="id"></param>
    private void JumpToDataIndex(int id)
    {
        for (int i = 0; i < _dataList.Count; i++)
        {
            if (_dataList[i]._trainingBasicData.ID == id)
            {
                _scrollview.ScrollToIndex(i);
                return;
            }
        }

        _scrollview.ScrollToIndex(_dataList.Count);
    }

    private int ReturnBasicIdxUsingHardIdx(int hardId)
    {
        int basicId = -1;

        foreach (TrainingItemData item in _dataList)
        {
            if (item._trainingBasicData.ID == ClientLocalDB_Simple.GetData<HardTraining>(DBKey.HardTraining, hardId).BasicTrainingLimit)
                basicId = item._trainingBasicData.ID;
        }

        return basicId;
    }

    public void SettingAnimation(ETrainingType type, int id)
    {
        _isAnim = true;
        _trainingType = type;
        _trainingId = id;
    }

    public bool ShowAnim(ETrainingType type, int id)
    { 
        return _isAnim && _trainingType == type && _trainingId == id;
    }

    public void HelpBtnClick()
    {
        UIManager.ShowUISubBase<UISubHelp>(UIManager.TrainingUI, "UISubHelpPopup").SetType(EHelpType.Training);
    }
}
