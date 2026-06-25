using Cysharp.Threading.Tasks;
using System;
using static Define;

public class GuildBossDungeonManager : DungeonFieldBase
{
    private GuildDungeon _dungeon;
    private GuildBossUnit _guildBossUnit;
    private double _currentGuildBossHp;
    private double _prevTotalPoint;

    public override async UniTask Init(BattleData battleData)
    {
        _currentGuildBossHp = UserInfo.currentGuildBossDto.hp;
        await base.Init(battleData);
    }

    private void GuildBossSetting()
    {
        SpawnPointInfoData spawnPointInfo =
            ClientLocalDB_Simple.GetData<SpawnPointInfoData>(DBKey.SpawnPointInfo, _dungeon.SpawnPointInfo);
        SpawnObjectGroupData groupData =
            ClientLocalDB_Simple.GetData<SpawnObjectGroupData>(DBKey.SpawnObjectGroup, spawnPointInfo.SpawnObjectGroup);
        _guildBossUnit = MapManager.GetEnemyUnit(groupData.ObjectList[0]) as GuildBossUnit;
        if (_guildBossUnit == null)
        {
            GuildBossGameDone().Forget();
            return;
        }
        
        _guildBossUnit._playStatus._hp = _currentGuildBossHp;
        _guildBossUnit.IsImmortal = _dungeon.Rule == 2;
    }
    
    protected override void DungeonDataSetting()
    { 
        _dungeonType = EDungeonType.GuildBoss;
        _dungeon = ClientLocalDB_Simple.GetData<GuildDungeon>(DBKey.GuildDungeon, _battleData._index);
        base.DungeonDataSetting();
    }

    protected override void DungeonUISetting()
    {
        base.DungeonUISetting();
        GuildBossSetting();
        _dungeonInfoUI.InitGuildBossDungeon(UserInfo.currentGuildBossDto);
        _dungeonInfoUI.UpdateBossHp(_guildBossUnit._playStatus._hp, _guildBossUnit._playStatus._totalMaxHp, _totalPoint);
    }
    
    protected override void BattleSetting()
    {
        base.BattleSetting();
        _dungeonInfoUI.UpdateTime((int)_gameTime);
    }
    
    private void Update()
    {
        if (!_isPlaying)
            return;

        CalculationPoint();
        
        if (TimeOverCheck())
            GuildBossGameDone().Forget();
        
        if (CheckEnemyAllDeath())
            GuildBossGameDone().Forget();
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
        
        if(_dungeon.Rule == 1)
            _totalPoint = Math.Min(_totalPoint, _currentGuildBossHp);
        
        _dungeonInfoUI.UpdateBossHp(_guildBossUnit._playStatus._hp, _guildBossUnit._playStatus._totalMaxHp, _totalPoint);
        if(_currentGuildBossHp == 0)
            GuildBossGameDone().Forget();
    }
    public async UniTaskVoid GuildBossGameDone()
    {
        if (!_isPlaying)
            return;

        StopGame();
        await UniTask.WaitForSeconds(1);
        GuildBossClearDto guildBossClearDto = new GuildBossClearDto();
        guildBossClearDto.guildId = UserInfo.guildInfo.id;
        guildBossClearDto.point = (int)_totalPoint;
        guildBossClearDto.step = _battleData._index; 
        BestHttpGameManager.OnPostGuildBossClear(guildBossClearDto,() => 
        {
            double guildBossTotalDamage = Utils.GetDungeonClearValue(EDungeonType.GuildBoss, EFactionType.None);
            bool isBestScore = false;
            if (_totalPoint > guildBossTotalDamage)
            {
                isBestScore = true;
                Utils.SetDungeonClearValue(EDungeonType.GuildBoss, EFactionType.None, _totalPoint);
            }
            Managers.Instance._dungeonFieldBase._dungeonInfoUI.OpenBattleWinPopup(EContent.GuildBoss, _totalPoint, isBestScore);

            //클리어 UI
        });
    }
}
