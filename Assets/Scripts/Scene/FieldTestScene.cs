using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FieldTestScene : FieldBaseScene
{
    [SerializeField] private MapStageMeta _mapStageInfo;
    [SerializeField] private string _bgmName;
    [SerializeField] private BattleTestDeck[] _testDeck;
    [SerializeField] private BattleTestTraining _testTraining;
    [SerializeField] private BattleTestSpawnPointInfo _testSpawnPointInfo;
    [SerializeField] private TMP_Text _timeText;
    
    [Header("UI Elements")]
    [SerializeField] private DamageMeterUI _damageMeterUI;
    [SerializeField] private DamageMeterItemUI[] _damageMeterItemUIs;
    [SerializeField] private CharacterMeterItemUI[] _characterMeterItemUis;
    
    private BattleTestConstellation[] _testConstellation;
    private DeckData _deckData;
    
    private Dictionary<int, UnitCombatStats> _damageMeterDic = new Dictionary<int, UnitCombatStats>();
    private float _refreshTime = 0.2f;
    private string _path = "BattleTestDB/";
    private bool _isPlaying = false;
    private float _time = 0;
    
    IEnumerator Start()
    {
        _isPlaying = false;
        
        Managers.Instance.Init();
        Init();
        Managers.Instance.GetSimpleDBManager().LoadAll();
        Managers.Instance.CreateUserInfo();
        UserInfo.IsSimulation = true;
        UIManager.JoystickUI.Close();

        Squad.Init(UserInfo);
        Squad.SetResurrectionState(false);
        Squad.SetDistanceValue(500, 1000);
        Squad.SetGameOverAction(BattleStop);
        
        yield return null;
        StartCoroutine(RefreshCoroutine());
    }

    private void SpawnCharacter()
    {
        int[] ids = _testDeck.Where(deck => deck.CharacterId != 0).Select(deck => deck.CharacterId).ToArray();
        _deckData = new DeckData { idList = ids };
        Squad.SetDeckData(_deckData);
        Squad.RefreshSquadCharacter();
        Squad._playerUnits.ForEach(unit => unit._unitCombatStats.ResetStats());
    }

    private T[] TestDBLoad<T>(string path, string dbName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>($"{path}{dbName}");
        return JsonConvert.DeserializeObject<T[]>(textAsset.text);
    }

    public async UniTask LoadData()
    {
        _time = 0.0f;
        _timeText.text = $"{_time:0.00}초";
        Managers.Instance.GetObjectUnitManager().DestroyAll();
        foreach (MapStageInfo mapStageInfo in MapManager.MapStageInfos)
        {
            mapStageInfo._spawnPointInfoUnitList.Clear();
        }
        Squad._playerUnits.Clear();
        
        LoadTrainingCenter();
        LoadConstellation();
        
        await CreateDamageText();
        await LoadFieldMap(_mapStageInfo, _testSpawnPointInfo);
        
        Squad._zoneIndex = MapManager.GetZoneIndex(0);
        LoadCharacter();
        Squad.SquadMove(Squad._zoneIndex, Managers.Instance.GetMapManager().GetStartPosition(Squad._zoneIndex)).Forget();
        FollowCamera.LookAtNow();
        _damageMeterUI.SetDamageSnapshot(_damageMeterDic, _time);
        BattleStop();
    }

    public void BattleStart()
    {
        _isPlaying = true;
        UIManager.JoystickUI.Open();
        Squad.BattleStart();
        MapManager.GetMapStageInfo(MapManager.GetZoneIndex(0)).StartUnitState();
    }

    public void BattleStop()
    {
        _isPlaying = false;
        UIManager.JoystickUI.Close();
        Squad.BattleStop();
        MapManager.GetMapStageInfo(MapManager.GetZoneIndex(0)).StopUnitState();
    }

    private void LoadCharacter()
    {
        List<myHerosData> myHeroDatas = new List<myHerosData>();
        foreach (var testDeck in _testDeck)
        {
            if (testDeck.CharacterId == 0)
                continue;
            
            myHerosData heroData = new myHerosData();
            heroData.id = testDeck.ID;
            heroData.heroTableId = testDeck.CharacterId;
            heroData.level = testDeck.Level;
            heroData.awakenStep = (int)testDeck.Grade;
            myHeroDatas.Add(heroData);
        }
        UserInfo.UpdateCharacterItemData(myHeroDatas.ToArray());

        SpawnCharacter();
        SettingUI();
    }

    private void LoadTrainingCenter()
    {
        UserInfo.UnlockBasicIdx = _testTraining.BasicTrainingIndex;
        UserInfo.UnlockHardIdx = _testTraining.HardTrainingIndex;
    }

    private int FindSetting(string key)
    {
        // foreach (var testSetting in _testSetting)
        // {
        //     if (testSetting.Key.Equals(key))
        //         return testSetting.Value;
        // }
        
        return 0;
    }

    private void LoadConstellation()
    {
        _testConstellation = TestDBLoad<BattleTestConstellation>(_path, "BattleTestConstellation");
        foreach (var constellation in _testConstellation)
        {
            constelltionNode node = new constelltionNode();
            node.nodeId = constellation.ID;
            node.open = constellation.Open;
            node.optionGrade = (int)constellation.Grade;
            Managers.Instance.UserInfo().SetConstellationItemData(node);
        }
        UserInfo.CalculateConstellationStatus();
    }

    private void SettingUI()
    {
        if (_deckData == null || _deckData.idList == null)
            return;
        
        int[] ids = _deckData.idList;
        _damageMeterDic.Clear();
        for (int i = 0; i < _characterMeterItemUis.Length; i++)
        {
            CharacterMeterItemUI characterMeterItemUI = _characterMeterItemUis[i];
            characterMeterItemUI.gameObject.SetActive(i < ids.Length);
            if (i < ids.Length)
            {
                BaseUnit baseUnit = Squad._playerUnits.Find(unit => unit._unitId == ids[i]);
                characterMeterItemUI.Init(baseUnit);
                _damageMeterDic.TryAdd(baseUnit._unitId, baseUnit._unitCombatStats);
            }
        }
        
        for (int i = 0; i < _damageMeterItemUIs.Length; i++)
        {
            DamageMeterItemUI damageMeterItemUI = _damageMeterItemUIs[i];
            damageMeterItemUI.gameObject.SetActive(i < ids.Length);
            if (i < ids.Length)
            {
                _damageMeterUI.Register(ids[i], damageMeterItemUI);
            }
        }
    }
    
    private void UpdateUI()
    {
        if (_deckData == null || _deckData.idList == null)
            return;
        
        foreach (CharacterMeterItemUI characterMeterItemUi in _characterMeterItemUis)
        {
            if(!characterMeterItemUi.gameObject.activeSelf)
                continue;
            
            characterMeterItemUi.UpdateUI(_time);
        }

        _damageMeterUI.SetDamageSnapshot(_damageMeterDic, _time);
    }

    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    IEnumerator RefreshCoroutine()
    {
        while (true)
        {
            if (!_isPlaying)
                yield return null;
            
            UpdateUI();
            yield return new WaitForSeconds(_refreshTime);
        }
    }
    
    private bool CheckEnemyAllDeath()
    {
        int unitCount = MapManager.GetLiveUnitCount();
        return unitCount == 0;
    }

    private void Update()
    {
        if (!_isPlaying)
            return;

        if (CheckEnemyAllDeath())
        {
            BattleStop();
            return;
        }
        
        _time += Time.deltaTime;
        _timeText.text = $"{_time:0.00}초";
    }
}
