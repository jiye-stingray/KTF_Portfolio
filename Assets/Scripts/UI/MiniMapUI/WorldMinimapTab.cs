using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMinimapTab : UITabBase
{
    [SerializeField] MinimapWorldButton[] worldBtns;

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public override void Refresh()
    {
        foreach (var b in worldBtns)
        {
            b.Init();
        }
    }
}
