using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UISubFactionSynergyDetail : UISubBase
{
    [SerializeField] private FactionSynergyDetailItem[] _factionSynergyDetailItems;

    public void Init(int[] deckIds)
    {
        Dictionary<EFactionType, int> factionDic = ClientLocalDB_Simple.CalculateFactionCount(deckIds);

        foreach (var item in _factionSynergyDetailItems)
        {
            int count = 0;
            
            if(factionDic.ContainsKey(item.GetFactionType()))
                count = factionDic[item.GetFactionType()];
            
            item.SetData(count);
        }
    }
}
