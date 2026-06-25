using Cysharp.Threading.Tasks;
using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIOpenEvent : UIBase , IRecyclableScrollRectDataSource
{
    #region Tab
    [SerializeField] UITabGroup _group;
    int _currentTabIndex;
    #endregion

    [SerializeField] Image _illust;
    [SerializeField] RecyclableScrollRect _recycleScrollRect;
    List<EventQuestItemData> _dataList = new List<EventQuestItemData>();

    [SerializeField] EventQuestScrollviewItem _questPointitem;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        SetCharacterImg().Forget();

        return true;
    }
    public override void Open()
    {
        base.Open();
        _currentTabIndex = Mathf.Min(UserInfoData.OpenEventCurrentDay - 1,6);
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();
        _group.Set(_currentTabIndex);
        DrawScrollview();
    }

    private void DrawScrollview()
    {
        _dataList.Clear();
        _dataList = UserInfoData._dicOpenEventQuestItemData[_currentTabIndex + 1].Values.ToList();
        _recycleScrollRect.Initialize(this);
        _questPointitem.SetEventPoint(_currentTabIndex + 1);

    }

    public void OnChangeTab()
    { 
        _currentTabIndex = _group._currentTapGroupBtn._index;
        DrawScrollview();

    }
    private async UniTask SetCharacterImg()
    {
        _illust.sprite = await Managers.Instance.GetResObjectManager().LoadAsync<Sprite>($"Illustration_Cr_001");
    }


    #region Recycle
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as EventQuestScrollviewItem;
        item.SetData(_dataList[index],index);
    }
    #endregion
}
