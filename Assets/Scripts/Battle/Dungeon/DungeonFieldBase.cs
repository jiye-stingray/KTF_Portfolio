using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using static Define;

public class DungeonFieldBase : FieldBaseScene
{
    public float _gameTime;
    protected int MaxTime;
    protected bool IsTimeOverRule => MaxTime > 0;
    public float GetGameTime { get { return _gameTime; } }
    public bool _isPlaying;
    public bool _battleEnd;
    
    private const int CountTime = 3;
    private int _countTime;

    protected int _mapIndex;
    protected int _zoneIndex;
    protected int _resurrectionCount;
    protected int MaxResurrectionCount => _dungeonRule.Resurrection;
    protected DungeonRule _dungeonRule;
    protected BattleData _battleData;
    public DungeonInfoUI _dungeonInfoUI;
    protected double _totalPoint;
    protected int _phase;
    protected EDungeonType _dungeonType; // 던전 타입
    protected TMP_Text _countDownText;

    private bool _delay = false;
    
    public virtual async UniTask Init(BattleData battleData)
    {
        var loadingUI = GameObject.FindObjectOfType<LoadingUI>();
        _delay = false;
        _battleData = battleData;
        _countTime = CountTime;
        if (_countDownText == null)
        {
            GameObject countDownTextObj = GameObject.Find("CountDown");
            if (countDownTextObj != null)
            {
                _countDownText = countDownTextObj.GetComponent<TMP_Text>();
                _countDownText.text = _countTime.ToString();
                _countDownText.gameObject.SetActive(false);
            }
        }
        
        BattleData.Set(_battleData);
        Init();
        InitValue();
        DungeonDataSetting();
        await LoadDungeonMap(_battleData._contentType, _battleData._factionType, _mapIndex, _battleData._index, _dungeonRule.Joystick);
        DungeonUISetting();
        SquadSetting();
        await SpawnCharacter(_battleData._contentType, _battleData._factionType);
        BattleSetting();
        await CreateDamageText();
        Managers.Instance.GetEconomySystem().gameObject.SetActive(false);

        Managers.Instance.Sound.PlayBGMAsync(_dungeonRule.BGM).Forget();
        if (loadingUI != null)
            await loadingUI.FadeOutAndHideAsync();
        
        if (_battleData._contentType == EContent.Fog || _battleData._contentType == EContent.FieldDungeon)
            OpenStartDialogue();
        else
            BestHttpGameManager.OnPostDungeonRun(_battleData._contentType, _battleData._factionType,
                _battleData._index);
    }

    //변수 초기화
    protected virtual void InitValue()
    {
        _resurrectionCount = 0;
        _isPlaying = false;
        _battleEnd = false;
        _mapIndex = 1;
    }

    //던전용 데이터 세팅
    protected virtual void DungeonDataSetting()
    {
        _dungeonRule = ClientLocalDB_Simple.GetData<DungeonRule>(DBKey.DungeonRule, _dungeonType.ToString());
        MaxTime = _dungeonRule.TimeLimit / 100;
    }

    //Squad 세팅
    protected virtual void SquadSetting()
    {
        Squad.Init(Managers.Instance.UserInfo());
        _zoneIndex = MapManager.GetZoneIndex(0);
        Squad._zoneIndex = _zoneIndex;
        Squad.ForceSquadMove(_zoneIndex, MapManager.GetStartPosition(_zoneIndex));
        Squad.SetResurrectionState(false);
        Squad.SetDistanceValue(_dungeonRule.BasicDistance, _dungeonRule.LimitDistance);
        Squad.BattleStop();
        Squad.SetGameOverAction(()=> GameOver().Forget());
    }

    protected virtual void DungeonUISetting()
    {
        _dungeonInfoUI = UIManager.ShowUIBase<DungeonInfoUI>("DungeonInfoUI");
        _dungeonInfoUI.EnableTime(IsTimeOverRule);
        _dungeonInfoUI.SetAutoAction(SquadAutoMode);
        _dungeonInfoUI.OpenToStack();
    }

    protected virtual void BattleSetting()
    {
        Squad.ForceSquadMove(_zoneIndex, MapManager.GetStartPosition(_zoneIndex));
        _dungeonInfoUI.ActiveAutoButton(_dungeonRule.Joystick && _dungeonRule.EnableAuto);
        _dungeonInfoUI.SetAutoMode(!_dungeonRule.Joystick || (_dungeonRule.EnableAuto && MapManager.IsDungeonAutoMode));
        FollowCamera.transform.position = Squad.transform.position;
        FollowCamera.LookAtNow();
        _gameTime = MaxTime;
    }

    private void SquadAutoMode(bool isAuto)
    {
        Squad.SetAutoMode(isAuto);
    }

    public virtual void OpenStartDialogue()
    {
        StartCountDown();
    }
    
    protected virtual void OpenEndDialogue()
    {
        OnDungeonClear();
    }
    
    protected void StartCountDown()
    {
        StartCoroutine(CountDown());
    }
    private IEnumerator CountDown()
    {
        _countDownText?.gameObject.SetActive(true);
        for(int i = CountTime; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            _countTime--;
            if(_countDownText != null)
                _countDownText.text = _countTime.ToString();
        }

        _countDownText?.gameObject.SetActive(false);
        StartGame();
    }

    protected virtual void StartGame()
    {
        FollowCamera.smoothSpeed = 0.1f;
        _isPlaying = true;
        Squad.BattleStart();
        MapManager.CheckMapData(_zoneIndex);
    }
    
    public void StopGame()
    {
        _isPlaying = false;
        Squad.BattleStop();
    }

    protected virtual async UniTaskVoid GameWin()
    {
        if (!_isPlaying)
            return;
        
        StopGame();
        await UniTask.WaitForSeconds(1);
        OpenEndDialogue();
    }

    protected void OnDungeonClear()
    {
        if (_dungeonRule.DungeonType == EDungeonType.Fog || _dungeonRule.DungeonType == EDungeonType.Field)
        {
            BuildingInfo buildingInfo = ClientLocalDB_Simple.GetDB<BuildingInfo>(DBKey.BuildingInfo).Values.ToList().Find(building => 
                building.BuildingConditionType == EBuildingConditionType.DungeonClear &&
                building.CurrencyList[0] == _battleData._index);
            
            BestHttpGameManager.ActiveBuilding(buildingInfo.ID);
        }
        else
        {
            BestHttpGameManager.OnPostDungeonClear(_battleData._contentType, _battleData._factionType, _battleData._index, _totalPoint, _phase, 1);
        }
    }
    
    public async UniTaskVoid GameOver()
    {
        if (!_isPlaying)
            return;
        
        StopGame();
        await UniTask.WaitForSeconds(1);
        _dungeonInfoUI.OpenBattleLosePopup(_battleData._contentType, _battleData._factionType, _battleData._index);
    }
    
    protected bool TimeOverCheck()
    {
        if (!IsTimeOverRule)
            return false;
        
        _gameTime -= Time.deltaTime;
        _dungeonInfoUI.UpdateTime((int)_gameTime);

        if (_gameTime < 0)
        {
            _gameTime = 0;
            _dungeonInfoUI.UpdateTime(0);
            UIManager.AllClosePopupUI();
            return true;
        }

        return false;
    }
    
    protected bool CheckEnemyAllDeath()
    {
        int unitCount = MapManager.GetLiveUnitCount();
        _dungeonInfoUI.UpdateCount(unitCount);

        return unitCount == 0;
    }

    public virtual void GoField()
    {
        if (_delay)
            return;
        
        _delay = true;
        // Managers.Instance.GetMapManager().UnLoadMap();
        Loading.Load(Loading.Field);
    }
    
    public virtual void NextStage()
    {
        if (_delay)
            return;
        
        _delay = true;
        int level = _battleData._index + 1;
        BestHttpGameManager.OnPostDungeonCurrencyCheck(_battleData._contentType, _battleData._factionType, level);
    }
    
    public virtual void ReTryStage()
    {
        if (_delay)
            return;
        
        _delay = true;
        if(_battleData._contentType != EContent.Fog && _battleData._contentType != EContent.FieldDungeon)
            BestHttpGameManager.OnPostDungeonCurrencyCheck(_battleData._contentType, _battleData._factionType, _battleData._index);
        else
        {
            BattleData.Set(_battleData);
            Loading.Load(Loading.Dungeon);
        }
            
    }

    public void GameWinClicked()
    {
        GameWin().Forget();
    }

    public void GameOverClicked()
    {
        GameOver().Forget();
    }
}
