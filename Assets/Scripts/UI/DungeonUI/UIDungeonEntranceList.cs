using UnityEngine;
using static Define;

public class UIDungeonEntranceList : UIBase
{
    [SerializeField] DungeonEntranceScrollviewItem _equipmentEnteranceScrollviewItem;
    [SerializeField] DungeonEntranceScrollviewItem _goldEnteranceScrollviewItem;
    [SerializeField] DungeonEntranceScrollviewItem _towerEnteranceScrollviewItem;
    [SerializeField] DungeonEntranceScrollviewItem _rankingEnteranceScrollviewItem;
    [SerializeField] DungeonEntranceScrollviewItem _awakeEnteranceScrollviewItem;

    public override void Open()
    {
        base.Open();
        _equipmentEnteranceScrollviewItem.Init();
        _goldEnteranceScrollviewItem.Init();
        _towerEnteranceScrollviewItem.Init();
        _rankingEnteranceScrollviewItem.Init();
        _awakeEnteranceScrollviewItem.Init();
    }

    public void OpenUI(EDungeonType dungeonType)
    {
        switch (dungeonType)
        {
            case EDungeonType.Gold:
                _goldEnteranceScrollviewItem.OpenUI();
                break;
            case EDungeonType.Equipment:
                _equipmentEnteranceScrollviewItem.OpenUI();
                break;
            case EDungeonType.Tower:
                _towerEnteranceScrollviewItem.OpenUI();
                break;
            case EDungeonType.Ranking:
                _rankingEnteranceScrollviewItem.OpenUI();
                break;
            case EDungeonType.Constellation:
                _awakeEnteranceScrollviewItem.OpenUI();
                break;
        }
    }

}
