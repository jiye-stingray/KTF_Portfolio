using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RelicManagermentTab : UITabBase
{
    [SerializeField] private UIManagermentRelicBaseItem[] _relicBaseItems;
    public override void Open()
    {
        base.Open();
        
        Refresh();
    }

    public override void Refresh()
    {
        RefreshRelicItems();
    }

    private void RefreshRelicItems()
    {
        Dictionary<string, RelicBase> dicRelicBase = ClientLocalDB_Simple.GetDB<RelicBase>(DBKey.RelicBase);
        string[] keys = dicRelicBase.Keys.ToArray();
        for (int i = 0; i < _relicBaseItems.Length; i++)
        {
            UIManagermentRelicBaseItem relicBaseItem = _relicBaseItems[i];
            bool isEmpty = i > keys.Length - 1;
            relicBaseItem.SetEmpty(isEmpty);

            if (isEmpty)
                continue;
            
            relicBaseItem.Init(keys[i]);
        }
    }
}
