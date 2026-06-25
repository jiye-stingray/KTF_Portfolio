using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ConstellationTab : UITabBase
{
 
    [SerializeField] ConstellationNode[] _nodes;
    [SerializeField] ConstellationNodeBar[] _bars;

    EConstellationBoardType _boardType;
    public void SetBoardType(EConstellationBoardType type)
    {
        _boardType = type;
    }

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public override void Refresh()
    {
        for (int i = 0; i < _nodes.Length; i++)
        {
            _nodes[i].SetData();
        }
        for (int i = 0; i < _bars.Length; i++)
        {
            _bars[i].Refresh();
        }
    }
}
