using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISubPVPRanking : UISubBase,IRecyclableScrollRectDataSource
{
    [SerializeField] RecyclableScrollRect _scrollview;

    #region Recycle

    public int GetItemCount()
    {
        throw new System.NotImplementedException();
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as PVPRankingScrollviewItem;
        //item.SetData(index);
    }

    #endregion
}
