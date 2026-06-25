using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISubPVPReward : UISubBase , IRecyclableScrollRectDataSource
{
    [SerializeField] RecyclableScrollRect _scroll;

    #region Recycle
    public int GetItemCount()
    {
        throw new System.NotImplementedException();
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as PVPRewardScrollviewItem;
        //item.SetData(index);
    }
    #endregion
}
