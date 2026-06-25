using System.Linq;
using UnityEngine;
using static Define;

public class RankingDungeonManager : DungeonFieldBase
{
    private RankingDungeon _dungeon;
    private double _prevPoint;

    protected override void InitValue()
    {
        _phase = 1;
        _prevPoint = 0;
        base.InitValue();
    }
    
    protected override void SquadSetting()
    {
        Squad.Init(Managers.Instance.UserInfo());
        _zoneIndex = MapManager.GetZoneIndex(0);
        Squad._zoneIndex = _zoneIndex;
        Squad.SetResurrectionState(false);
        Squad.SetDistanceValue(_dungeonRule.BasicDistance, _dungeonRule.LimitDistance);
        Squad.BattleStop();
        Squad.SetGameOverAction(()=> GameWin().Forget());
    }
    
    protected override void DungeonDataSetting()
    {
        _dungeonType = EDungeonType.Ranking;
        _dungeon = ClientLocalDB_Simple.GetData<RankingDungeon>(DBKey.RankingDungeon, _phase);
        base.DungeonDataSetting();
    }

    protected override void DungeonUISetting()
    {
        base.DungeonUISetting();
        _dungeonInfoUI.InitRankingDungeon(_dungeon);
        _dungeonInfoUI.UpdateDamagePoint(_prevPoint, _totalPoint);
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

        CalculationPoint();

        if (TimeOverCheck())
            GameWin().Forget();
    }

    private void CalculationPoint()
    {
        double point = 0;
        
        for (int i = 0; i < Squad._playerUnits.Count; i++)
        {
            BaseUnit unit = Squad._playerUnits[i];
            point += unit._unitCombatStats._damageDealt;
        }
        _totalPoint = point;
        _dungeonInfoUI.UpdateDamagePoint(_totalPoint - _prevPoint, _totalPoint);

        if (_dungeon == null)
            return;
        if (_dungeon.DamageScale <= _totalPoint - _prevPoint)
        {
            _prevPoint += _dungeon.DamageScale;

            // 마지막 단계 체크
            _phase = Mathf.Min(_phase + 1, ClientLocalDB_Simple.GetDB<RankingDungeon>(DBKey.RankingDungeon).Last().Value.Phase);

            _dungeon = ClientLocalDB_Simple.GetData<RankingDungeon>(DBKey.RankingDungeon, _phase);
            //by rainful 2025-09-13 최대치 처리. 
            if (_dungeon == null)
                return;

            _dungeonInfoUI.UpdateRankingDungeonData(_dungeon);
        }
    }
}
