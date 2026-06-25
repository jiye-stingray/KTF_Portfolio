using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITowerDungeonEntranceList : UIBase
{

    [SerializeField] RectTransform _nameTxtImg;

    [SerializeField] TowerDungeonEntranceButton _allTowerDungeonBtn;
    [SerializeField] TowerDungeonEntranceButton _humanTowerDungeonBtn;
    [SerializeField] TowerDungeonEntranceButton _guardianTowerDungeonBtn;
    [SerializeField] TowerDungeonEntranceButton _crusherTowerDungeonBtn;
    [SerializeField] TowerDungeonEntranceButton _celestialTowerDungeonBtn;


    public override bool Init()
    {
        if(base.Init() == false) 
            return false;

        _allTowerDungeonBtn.Init();
        _humanTowerDungeonBtn.Init();
        _guardianTowerDungeonBtn.Init();
        _crusherTowerDungeonBtn.Init();
        _celestialTowerDungeonBtn.Init();

        LayoutRebuilder.ForceRebuildLayoutImmediate(_nameTxtImg);

        return true;
    }

    public override void Refresh()
    {

    }

    public override void Open()
    {
        base.Open();
        Refresh();
    }
}
