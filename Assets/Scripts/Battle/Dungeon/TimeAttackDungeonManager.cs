using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class TimeAttackDungeonManager : DungeonFieldBase
{
    private DungeonBase _dungeonBase;
    private int _step = 0;
    private int FieldCount => MapManager.ZonesCount;

    protected override void DungeonDataSetting()
    {
        switch(_battleData._contentType)
        {
            case EContent.Gold:
            case EContent.Equipment:
                _dungeonType = _battleData._contentType == EContent.Gold ? EDungeonType.Gold : EDungeonType.Equipment;
                _dungeonBase = ClientLocalDB_Simple.GetData<Dungeon>(DBKey.Dungeon,$"{_dungeonType}_{_battleData._index}");
                _mapIndex = _dungeonBase.ZoneFlow;
                break;
            case EContent.Tower:
                _dungeonBase = ClientLocalDB_Simple.GetData<TowerDungeon>(DBKey.TowerDungeon, $"{_battleData._factionType}_{_battleData._index}");
                _mapIndex = _dungeonBase.ZoneFlow;
                _dungeonType = EDungeonType.Tower;
                break;
            case EContent.Fog:
                _dungeonBase = ClientLocalDB_Simple.GetData<FogDungeon>(DBKey.FogDungeon, _battleData._index);
                _mapIndex = _dungeonBase.ZoneFlow;
                _dungeonType = EDungeonType.Fog;
                break;
        }
        base.DungeonDataSetting();
    }
    
    protected override void DungeonUISetting()
    {
        base.DungeonUISetting();
        if(_dungeonType == EDungeonType.Tower)
            _dungeonInfoUI.InitDungeon(_dungeonBase as TowerDungeon);
        else if (_dungeonType == EDungeonType.Fog)
            _dungeonInfoUI.InitDungeon(_dungeonBase as FogDungeon);
        else
            _dungeonInfoUI.InitDungeon(_dungeonBase as Dungeon);
    }

    protected override void BattleSetting()
    {
        base.BattleSetting();
        _dungeonInfoUI.UpdateCount(MapManager.GetUnitCount());
        _dungeonInfoUI.UpdateTime((int)_gameTime);
    }

    private void Update()
    {
        if (!_isPlaying)
            return;

        if(TimeOverCheck())
        {
            GameOver().Forget();
            return;
        }

        if (CheckEnemyAllDeath())
            StartCoroutine(NextFieldCoroutine());
    }

    private void NextStep()
    {
        _step++;
        _zoneIndex = MapManager.GetZoneIndex(_step);
        BattleSetting();
        StartCountDown();
    }

    private IEnumerator NextFieldCoroutine()
    {
        if (_step + 1 < FieldCount) // 다음 필드
        {
            StopGame();
            yield return new WaitForSeconds(2);
            NextStep();
        }
        else // 게임 승리
            GameWin().Forget();
    }

    public override void OpenStartDialogue()
    {
        if (_dungeonType == EDungeonType.Fog)
        {
            if (_dungeonBase is FogDungeon fogDungeon)
            {
                List<DialogueData> data = Managers.Instance.GetSimpleDBManager().GetDialogueDataList("Chapter_1", fogDungeon.DialogStart);
                if (data != null && data.Count > 0)
                {
                    UIManager.DialogueUI.StartDialogue(data, StartDialogueCallBack);
                    return;
                }
            }
        }

        base.OpenStartDialogue();
    }

    protected override void OpenEndDialogue()
    {
        if (_dungeonType == EDungeonType.Fog)
        {
            if (_dungeonBase is FogDungeon fogDungeon)
            {
                List<DialogueData> data = Managers.Instance.GetSimpleDBManager().GetDialogueDataList("Chapter_1", fogDungeon.DialogEnd);
                if (data != null && data.Count > 0)
                {
                    UIManager.DialogueUI.StartDialogue(data, EndDialogueCallBack);
                    return;
                }
            }
        }
        
        base.OpenEndDialogue();
    }

    private void StartDialogueCallBack(int key)
    {
        StartCountDown();
    }
    
    private void EndDialogueCallBack(int key)
    {
        OnDungeonClear();
    }
}
