using PolyAndCode.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIWarpPointPopup : UIBase, IRecyclableScrollRectDataSource
{
    private List<WarpPointItemData> _dataList = new List<WarpPointItemData>();

    [SerializeField] RecyclableScrollRect _scrollView;
    [SerializeField] WarpPointScrollviewItem _cellviewPrefab;

    MapManager MapManager => Managers.Instance.GetMapManager();
    Squad Squad => Managers.Instance.GetObjectUnitManager().playerSquad;

    Dictionary<string, WarpPointInfo> WarpPointInfoDB => ClientLocalDB_Simple.GetDB<WarpPointInfo>(DBKey.WarpPointInfo).Where(e => e.Value.FieldID == UserInfoData._fieldId).ToDictionary(e => e.Key, e => e.Value);
    
    public override void Open()
    {
        base.Open();
        SetData();
        _scrollView.Initialize(this);
    }

    private void SetData()
    {
        if(_dataList.Count == 0)
        {
            foreach (var data in WarpPointInfoDB)
            {
                WarpPointItemData warpPointItemData = new WarpPointItemData();
                warpPointItemData._warpPointInfo = data.Value;
                _dataList.Add(warpPointItemData);
            }
        }
        for (int i = 0; i < _dataList.Count; i++)
        {
            WarpPointItemData warpPointItemData = _dataList[i];
            BaseBuilding warpPointBuilding = MapManager.FindWarpPoint(warpPointItemData._warpPointInfo.ID);

            if (warpPointBuilding == null)
            {
                warpPointItemData.IsLock = true;
                continue;
            }

            warpPointItemData.IsLock = !warpPointBuilding.BuildingData.isOpen;
            warpPointItemData.IsSquadField = Squad._zoneIndex == warpPointItemData._warpPointInfo.ZoneID;
            warpPointItemData._warpPointPosition = warpPointBuilding.transform.position;
        }
    }

    #region Recycle ScrollView
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as WarpPointScrollviewItem;
        item.SetData(_dataList[index], index);
        item.OnClick = ScrollItemClick;
    }
    #endregion

    private void ScrollItemClick(int index)
    {
        WarpPointItemData warpPointItemData = _dataList[index];

        if (warpPointItemData.IsLock)
        {
            UIManager.ShowCommonToastMessage("이동할 수 없는 지역입니다.");
            return;
        }
        
        if (warpPointItemData.IsSquadField)
            return;

        Managers.Instance.GetObjectUnitManager().playerSquad.TeleportHeroes(warpPointItemData._warpPointInfo.ZoneID, warpPointItemData._warpPointPosition);
        Close();
    }
}
