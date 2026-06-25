using Cysharp.Threading.Tasks;
using MarchingBytes;
using UnityEngine;
using UnityEngine.Pool;
using static Define;

public class AwakeDungeonManager : DungeonFieldBase
{
    private Dungeon _dungeon;
    
    private const int CountTime = 3;
    private float _secondPerKill; // 몬스터 잡을때마다 증가되는 시간(초)
    private int _maxMonsterKill; // 최대로 잡을수 있는 몬스터 수량

    public override async UniTask Init(BattleData battleData)
    {
        _secondPerKill = ClientLocalDB_Simple.GetData<DungeonSetting>(DBKey.DungeonSetting, "SecondPerKill").Value / 100f;
        _maxMonsterKill = ClientLocalDB_Simple.GetData<DungeonSetting>(DBKey.DungeonSetting, "MaxMonsterDungeon").Value;

        // DungeonTimeCount 
        await EasyObjectPool.instance.CreatePoolInfo(EPoolType.UI, "AddDungeonCountTimeText");

        await base.Init(battleData);
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
        _dungeonType = EDungeonType.Constellation;
        _dungeon = ClientLocalDB_Simple.GetData<Dungeon>(DBKey.Dungeon, $"{_dungeonType}_{_battleData._index}");
        base.DungeonDataSetting();
    }

    protected override void DungeonUISetting()
    {
        base.DungeonUISetting();
        _dungeonInfoUI.InitDungeon(_dungeon);
    }
    
    protected override void BattleSetting()
    {
        base.BattleSetting();
        _dungeonInfoUI.UpdateCount(0);
        _dungeonInfoUI.UpdateTime((int)_gameTime);
    }

    private void Update()
    {
        if (!_isPlaying)
            return;

        if(TimeOverCheck())
        {
            GameWin().Forget();
            return;
        }
        
        CalculationCount();

        if (GameOverCheck())
            GameWin().Forget();
    }
    
    private void CalculationCount()
    {
        int count = 0;
        
        for (int i = 0; i < Squad._playerUnits.Count; i++)
        {
            BaseUnit unit = Squad._playerUnits[i];
            count += unit._unitCombatStats._killCount;
        }

        if ((int)_totalPoint == count)
            return;

        int addCnt = count - (int)_totalPoint;
        _gameTime += _secondPerKill * addCnt;
        _totalPoint = count;
        // Effect 
        SpawnTimeEffect(_secondPerKill * addCnt).Forget();

        _dungeonInfoUI.UpdateCount((int)_totalPoint);
    }

    private const float EffectDepth = 10f; // 월드 카메라로부터의 z 거리 (이펙트가 놓일 평면)

    private async UniTask<GameObject> SpawnTimeEffect(double count)
    {
        // UI 텍스트의 스크린 좌표만 받아온다. (UI 카메라 기준)
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(
            Managers.Instance.GetCameraManager().UICam, _dungeonInfoUI._timeTxtTrans.position);

        // 스크린 좌표 -> 월드 좌표 변환 (메인 카메라 기준)
        screenPos.z = EffectDepth;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        GameObject effect = await EasyObjectPool.instance.GetObjectFromPool(
            EPoolType.UI, "AddDungeonCountTimeText", worldPos);

        AddDungeonCountTimeText txt = effect.GetComponent<AddDungeonCountTimeText>();
        txt.Init(count); // Init 내부에서 DOTween 연출 + 종료 시 pool 반환

        return effect;
    }
    
    private bool GameOverCheck()
    {
        return _maxMonsterKill <= (int)_totalPoint;
    }
}
