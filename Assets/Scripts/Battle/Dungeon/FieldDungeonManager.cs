using Cysharp.Threading.Tasks;
using static Define;

public class FieldDungeonManager : DungeonFieldBase
{
    private DungeonBase _dungeonBase;
    private int FieldCount => MapManager.ZonesCount;
    private RewardBoxBuilding _rewardBoxBuilding;
    private UITreasureNavigation _uiTreasureNavigation;

    public override async UniTask Init(BattleData battleData)
    {
        await base.Init(battleData);

        _rewardBoxBuilding = MapManager.FindRewardBoxBuilding();
        _rewardBoxBuilding._enemyCount = 1;
        _uiTreasureNavigation = MapManager.CreateTreasureNavigation(_rewardBoxBuilding.transform);
        _uiTreasureNavigation.Close();
    }

    protected override void DungeonDataSetting()
    {
        _dungeonBase = ClientLocalDB_Simple.GetData<FieldDungeon>(DBKey.FieldDungeon, _battleData._index);
        _mapIndex = _dungeonBase.ZoneFlow;
        _dungeonType = EDungeonType.Field;
        base.DungeonDataSetting();
    }
    
    protected override void DungeonUISetting()
    {
        base.DungeonUISetting();
        _dungeonInfoUI.InitDungeon(_dungeonBase as FieldDungeon);
    }

    protected override void BattleSetting()
    {
        base.BattleSetting();
        _dungeonInfoUI.UpdateCount(MapManager.GetUnitCount());
        _dungeonInfoUI.UpdateTime((int)_gameTime);
    }

    protected override void StartGame()
    {
        base.StartGame();
        _uiTreasureNavigation.Open();
    }

    private void Update()
    {
        if (!_isPlaying)
            return;

        if(TimeOverCheck())
            GameOver().Forget();
        
        if(CheckEnemyAllDeath())
            _rewardBoxBuilding._enemyCount = 0;
    }
}
