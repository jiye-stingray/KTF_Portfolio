using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISubPVPRecord : UISubBase , IRecyclableScrollRectDataSource
{
    [SerializeField] RecyclableScrollRect _scroll;

    #region Recycle
    public int GetItemCount()
    {
        throw new System.NotImplementedException();
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as PVPRecordScrollviewItem;
        //item.SetData(index);
    }
    #endregion
}
