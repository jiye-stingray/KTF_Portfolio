using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;
using Random = UnityEngine.Random;

public class UserInfoData
{
    public long userId = 12345678; // 임시로 setting
    public int serverNum = 0;
    public int _thumbnailID;
    public int _frameID;
    public ReactiveProperty<string> userNickName = new ReactiveProperty<string>("testName");
    public ReactiveProperty<int> userLevel = new ReactiveProperty<int>();
    public ReactiveProperty<int> userExp = new ReactiveProperty<int>();
    public bool levelUp = false; // 레벨업 했는지 여부
    public bool IsSimulation = false; // 시뮬레이션 상태인지
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    #region Setting

    public bool _isEconomyOn = true;
    public bool _isDamageOn = true;


    #endregion

    public bool _isFirstLoginToday;
    public bool _showNoticePopup;

    // 재화
    public Dictionary<int, CharacterClassItemData>
        _dicCharacterItemData = new Dictionary<int, CharacterClassItemData>();

    public Dictionary<ECurrency, ReactiveProperty<int>> _currencyItems =
        new Dictionary<ECurrency, ReactiveProperty<int>>();
    public Dictionary<int, int> _dicitemItems = new Dictionary<int, int>(); // 소지품 
    public Dictionary<ECurrency, ReactiveProperty<int>> _dicCurrencyAcquireNumberData = new Dictionary<ECurrency, ReactiveProperty<int>>();
    public Dictionary<ECurrency, int> _dicCurrencyAcquireLimitData = new Dictionary<ECurrency, int>();


    public ReactiveProperty<int> dialogKey = new ReactiveProperty<int>();

    #region Training

    public List<TrainingItemData> _trainingItemList = new List<TrainingItemData>();

    private int _unlockBasicIdx; // 해금한 basic idx

    public int UnlockBasicIdx
    {
        get => _unlockBasicIdx;
        set
        {
            _unlockBasicIdx = value;
            SetTrainingStatus(UnlockBasicIdx, UnlockHardIdx);
        }
    }

    public int MaxBasicIdx => ClientLocalDB_Simple.GetDB<BasicTraining>(DBKey.BasicTraining).Last().Value.ID;

    private int _unlockHardIdx; // 해금한 hard idx

    public int UnlockHardIdx
    {
        get => _unlockHardIdx;
        set
        {
            _unlockHardIdx = value;
            SetTrainingStatus(UnlockBasicIdx, UnlockHardIdx);
        }
    }

    public int MaxHardIdx => ClientLocalDB_Simple.GetDB<HardTraining>(DBKey.HardTraining).Last().Value.ID;

    #endregion

    #region Status

    public ReactiveProperty<double> AllBattlePower = new ReactiveProperty<double>();

    public double AllAttack => ReturnAllATK();
    public double AllMaxHp => ReturnAllMaxHP();

    public Status _trainingStatus = new Status(); // 공용 Status

    public Status _CelestialEquipmentStatus = new Status();
    public Status _CrusherEquipmentStatus = new Status();
    public Status _GuardianEquipmentStatus = new Status();
    public Status _HumanEquipmentStatus = new Status();

    public Status _DeckSynergyStatus = new Status();

    public Status _constellationStatus = new Status();
    
    public Dictionary<int, Status> _relicStatus = new Dictionary<int, Status>();
    #endregion

    #region Deck

    public Dictionary<EServerContentType, DeckData> _dicDeckData = new Dictionary<EServerContentType, DeckData>();
    public List<CharacterClassItemData> _TopLevelCharacterList = new List<CharacterClassItemData>();
    public Dictionary<int, ResonanceItemData> _dicResonanceItemData = new Dictionary<int, ResonanceItemData>();
    public int _unlockResonanceCount; // 해금 갯수
    public int _resonanceLevel;

    #endregion

    #region Equipment

    public Dictionary<long, EquipmentItemData> _dicEquipmentItemData = new Dictionary<long, EquipmentItemData>();

    public Dictionary<EFactionType, EquipmentItemData[]> _dicEquipment =
        new Dictionary<EFactionType, EquipmentItemData[]>()
        {
            { EFactionType.Celestial, new EquipmentItemData[6] },
            { EFactionType.Crusher, new EquipmentItemData[6] },
            { EFactionType.Guardian, new EquipmentItemData[6] },
            { EFactionType.Human, new EquipmentItemData[6] }
        };

    public Dictionary<EFactionType, int> _dicAltarLevel = new Dictionary<EFactionType, int>()
    {
        { EFactionType.Celestial, 0 },
        { EFactionType.Crusher, 0 },
        { EFactionType.Guardian, 0 },
        { EFactionType.Human, 0 }
    }; // 제단 레벨

    #endregion

    #region Quest

    public ReactiveProperty<int> _currentGuideQuestId => CurrentFieldItemData.guideQuestId;

    public int GuideQuestProgressValue
    {
        get => CurrentFieldItemData.questProgressValue;
        private set => CurrentFieldItemData.questProgressValue = value;
    }

    public GuideQuest GetCurrentGuideQuest => CurrentFieldItemData.GetCurrentQuest();

    public bool isGuideQuestClear => GuideQuestProgressValue >= GetCurrentGuideQuest.ConditionValue.Last(); // 현재 가이드 퀘스트 클리어 상태

    public bool isGuideQuestFinish => CurrentFieldItemData.IsGuideQuestFinish(); // 전체 가이드 퀘스트가 끝났을 때

    // 보상을 받을 수 있는 상태
    public bool isGuideQuestClearable
    { 
        get => CurrentFieldItemData.isQuestClearable;
        set => CurrentFieldItemData.isQuestClearable = value;
    } 
    
    public Dictionary<EQuestConditionType, RoutineQuestItemData> _dicDailyRoutineQuestData =
        new Dictionary<EQuestConditionType, RoutineQuestItemData>();

    public Dictionary<EQuestConditionType, RoutineQuestItemData> _dicWeeklyRoutineQuestData =
        new Dictionary<EQuestConditionType, RoutineQuestItemData>();

    public int _dailyQuestPoint;
    public int _weeklyQuestPoint;

    public Dictionary<int, QuestPointItemData> _dicDailyQuestPointData = new Dictionary<int, QuestPointItemData>();
    public Dictionary<int, QuestPointItemData> _dicWeeklyQuestPointData = new Dictionary<int, QuestPointItemData>();

    public int _guildMissionPoint;

    #endregion

    #region Dungeon

    public int _clearSkillDungeonLevel;
    public int _clearEquipmentDungeonLevel;
    public int _clearGoldDungeonLevel;

    // 타워 종류 마다 많을 예정 (추후 추가) 
    public int _clearAllTowerDungeonLevel;
    public int _clearHumanTowerDungeonLevel;
    public int _clearCelestialTowerDungeonLevel;
    public int _clearCrusherTowerDungeonLevel;
    public int _clearGuardianTowerDungeonLevel;

    public int _humanDungeonDailyCount;
    public int _guardianDungeonDailyCount;
    public int _crusherDungeonDailyCount;
    public int _celestialDungeonDailyCount;

    // 랭킹 던전
    public RankingItemData _myRankingItemData = new RankingItemData();
    public int _maxConstellationDungeonMonsterCnt = 0; // 별자리 던전 최대 몬스터 처치 수

    //길드 보스 던전
    public double _currentGuildBossBestDamage; //현재 길드 보스에게 가한 최고 데미지

    #endregion

    #region Pass

    public Dictionary<int, PassItemData> _dicPassItem = new Dictionary<int, PassItemData>();

    #endregion

    #region Emblem

    //2025-08-28 by RimGa : 컨텐츠별 장착한 엠블럼 데이터 dic
    public Dictionary<EContent, EmblemItemData> _dicEquipEmblemitemData = new Dictionary<EContent, EmblemItemData>();

    //2025-08-28 by RimGa : 보유한 엠블럼 데이터 dic
    public Dictionary<long, EmblemItemData> _dicEmblemItemData = new Dictionary<long, EmblemItemData>();

    #endregion

    #region Gacha

    public Dictionary<EGachaType, GachaItemData> _dicGachaItemData = new Dictionary<EGachaType, GachaItemData>();

    #endregion

    #region Shop

    public Dictionary<EShopType, Dictionary<int, ShopItemData>> _dicShopItemData = new Dictionary<EShopType, Dictionary<int, ShopItemData>>();

    public int _currentLimitShopDataID;     // 현재 진행 중인 limit Shop ID

    #endregion

    #region Constellation

    public Dictionary<int, ConstellationItemData> _dicConstellationItemData =
        new Dictionary<int, ConstellationItemData>();

    #endregion

    #region Field

    public ReactiveProperty<bool> _enablePortal = new ReactiveProperty<bool>(false);
    public MyPortalData _portalData = new MyPortalData();
    public int _fieldId;            // 현재 어떤 field 인지
    public int _previousFieldID;
    public Vector2 squadPosition = Vector2.zero;
    public int zoneId = 0;
    public TreasureBoxData[] _treasureBoxList;
    public Dictionary<int, BuildingData> _dicInstallationBuildingData = new Dictionary<int, BuildingData>();

    public Dictionary<int, FieldItemData> _dicFieldItemData = new Dictionary<int, FieldItemData>();
    
    public FieldItemData CurrentFieldItemData => _dicFieldItemData[_fieldId];
    public int CurrentDifficultyLevel => CurrentFieldItemData.currentDifficultyLevel;      // 현재 난이도

    #endregion

    #region AD

    public bool _isAdsRemoved = false;
    public myADInfo _myADInfo = new myADInfo();
    public TimeData _adBuffTimeData = new TimeData();
    public bool EnableAdBuff => _adBuffTimeData.GetRemain() > 0;


    #endregion

    #region Event

    public bool _isNewb;

    public int _weeklyAttendanceCount;
    public int _monthlyAttendanceCount;

    public Dictionary<int, EventRewardItemData> _dicWeeklyAttendanceRewardItemData =
        new Dictionary<int, EventRewardItemData>();

    public Dictionary<int, EventRewardItemData> _dicMonthlyAttendanceRewardItemData =
        new Dictionary<int, EventRewardItemData>();

    public Dictionary<int, EventRewardItemData> _dicNewAttendanceItemData = new Dictionary<int, EventRewardItemData>();


    public DateTime _openEventStartDate;
    public int OpenEventCurrentDay => (ServerTime.Instance.CurrentTime() - _openEventStartDate).Days + 1;

    public bool _openEventCompleted;
    public bool[] _openEventDayCompleted = new bool[7];

    public Dictionary<int, Dictionary<EQuestConditionType,EventQuestItemData>> _dicOpenEventQuestItemData = new Dictionary<int, Dictionary<EQuestConditionType, EventQuestItemData>>();

    #endregion

    #region Mail

    public List<MailItemData> nomalMailItemList = new List<MailItemData>();
    public List<MailItemData> payMailItemList = new List<MailItemData>();

    #endregion

    #region Relic

    private Dictionary<int, RelicItemData> _dicRelicItemData = new Dictionary<int, RelicItemData>();
    private Dictionary<int, Dictionary<ERelicPartsType, List<RelicPartsItemData>>> _dicRelicPartsItemData = new Dictionary<int, Dictionary<ERelicPartsType, List<RelicPartsItemData>>>();

    #endregion

    #region Setting
    public Dictionary<ToggleSettingType, bool> _isToggleOnDic = new Dictionary<ToggleSettingType, bool>();
    public static System.Action<bool> OnChattingToggleChanged;
    #endregion

    UIManager UIManager => Managers.Instance.GetUIManager();

    // Function ==============================================

    //User가 만들어질때 초기화 해주는 함수
    public void Init()
    {
        LoadBuilding();
        LoadDeck();
        LoadCharacter();
        LoadConstellation();
        LoadCurrency();
        LoadItem();
        LoadQuest();
        LoadResonance();
        LoadTraining();
        LoadRelicData();
        LoadGachaData();
        LoadAttendanceItemData();
        LoadFieldItemData();
        LoadShopItemData();
        LoadCurrencyAcquireLimitData();
        LoadEventQuestData();
        
        userLevel.DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
            .Subscribe(UserLevelChanged).AddTo(_disposables);
    }

    #region LoadData

    private void LoadGachaData()
    {
        if (_dicGachaItemData.Count > 0)
            return;

        Dictionary<string, UnitData> playerUnits = ClientLocalDB_Simple.GetDB<UnitData>(DBKey.PlayerCharacter);
        List<UnitData> playerUnitList = playerUnits.Values.Where(row => row.Live).ToList();
        List<UnitData> defaultGeneralGachaWishList = playerUnitList.FindAll(row => row.WishList).Take(5).ToList();
        List<UnitData> defaultCelestialGachaWishList =
            playerUnitList.FindAll(row => row.Faction == EFactionType.Celestial).Take(1).ToList();

        int gachaCount = (int)EGachaType.Celestial;
        for (int i = 0; i <= gachaCount; i++)
        {
            EGachaType gachaType = (EGachaType)i;
            GachaGroup gachaGroup = ClientLocalDB_Simple.GetData<GachaGroup>(DBKey.GachaGroup, i);
            GachaItemData gachaItemData = new GachaItemData();
            gachaItemData._id = gachaGroup.GroupID;
            gachaItemData._count = 0;
            if (gachaType == EGachaType.General)
                gachaItemData._wishList = defaultGeneralGachaWishList.Select(unit => unit.ID).ToArray();
            else if (gachaType == EGachaType.Celestial)
                gachaItemData._wishList = defaultCelestialGachaWishList.Select(unit => unit.ID).ToArray();

            _dicGachaItemData.Add((EGachaType)i, gachaItemData);
        }
    }

    private void LoadBuilding()
    {
        if (_dicInstallationBuildingData.Count > 0)
            return;

        Dictionary<string, BuildingInfo> buildingInfoDB = ClientLocalDB_Simple.GetDB<BuildingInfo>(DBKey.BuildingInfo);
        List<string> buildingDBKeys = buildingInfoDB.Keys.ToList();

        foreach (var id in buildingDBKeys)
        {
            BuildingData data = new BuildingData();
            data._data = buildingInfoDB[id];
            data._level = 1;
            data._isBuild.Value = data._data.InitialState;
            _dicInstallationBuildingData.Add(int.Parse(id), data);
        }
    }

    private void LoadDeck()
    {
        if (_dicDeckData.Count > 0)
            return;

        foreach (EServerContentType content in Enum.GetValues(typeof(EServerContentType)))
        {
            _dicDeckData.Add(content, new DeckData(content));
        }
    }

    private void LoadResonance()
    {
        if (_dicResonanceItemData.Count > 0)
            return;

        for (int i = 0; i < ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "MaxSlot").Value; i++)
        {
            ResonanceItemData resonanceItemData = new ResonanceItemData()
            {
                _index = i,
                _characterId = 0,
                _isLock = true,
                _expireDate = DateTime.Now,
            };
            _dicResonanceItemData.Add(resonanceItemData._index, resonanceItemData);
        }
    }

    private void LoadCurrency()
    {
        if (_currencyItems.Count > 0)
            return;

        foreach (ECurrency currency in Enum.GetValues(typeof(ECurrency)))
        {
            _currencyItems.Add(currency, new ReactiveProperty<int>(0));
        }
    }

    private void LoadItem()
    {
        if (_dicitemItems.Count > 0)
            return;

        foreach (var item in ClientLocalDB_Simple.GetDB<Item>(DBKey.Item).Values)
        {
            _dicitemItems.Add(item.ID, 0);
        }
    }

    public void SetItemDto(ItemBoxDto[] itemBoxDtos)
    {
        foreach (var item in itemBoxDtos)
        {

            _dicitemItems[item.boxId] = item.count;
        }
    }


    private void LoadTraining()
    {
        if (_trainingItemList.Count > 0)
            return;

        int count = ClientLocalDB_Simple.GetDB<BasicTraining>(DBKey.BasicTraining).Count;
        for (int i = 1; i <= count; i++)
        {
            TrainingItemData data = new TrainingItemData();

            // statusData
            data._trainingBasicData = ClientLocalDB_Simple.GetData<BasicTraining>(DBKey.BasicTraining, i);

            if (i < count && (data._trainingBasicData.AccountLevelLimit + 1 == ClientLocalDB_Simple
                    .GetData<BasicTraining>(DBKey.BasicTraining, i + 1).AccountLevelLimit))
                data.islevelShow = true; // 기존 레벨보다 한 칸 높으면 표기하기

            // 마지막 (userlevel 기준 만랩 )은 무조건 level txt 띄우기
            if (data._trainingBasicData.AccountLevelLimit == ClientLocalDB_Simple.GetDB<UserLevelData>(DBKey.UserLevel).Last().Value.Level)
            {
                data.islevelShow = true;
            }


            // hardTraining
            // 없으면 null 들어감 
            data._trainingHardData = ClientLocalDB_Simple.GetDB<HardTraining>(DBKey.HardTraining)
                .FirstOrDefault(item => item.Value.BasicTrainingLimit == data._trainingBasicData.ID).Value;
            // level
            data.level =
                data._trainingBasicData.AccountLevelLimit; // data 안에 들어 있는 레벨은 이 다음 레벨을 UI 로 표시하기 위해 -1 해서 표시한다.
            _trainingItemList.Add(data);
        }
    }

    private void LoadCharacter()
    {
        if (_dicCharacterItemData.Count > 0)
            return;

        Dictionary<string, UnitData> playerUnitDB = ClientLocalDB_Simple.GetDB<UnitData>(DBKey.PlayerCharacter);
        List<string> unitKeys = playerUnitDB.Keys.ToList();
        int count = unitKeys.Count;
        for (int i = 0; i < count; i++)
        {
            UnitData unitData = playerUnitDB[unitKeys[i]];
            
            CharacterClassItemData item = new CharacterClassItemData();
            item.id = unitData.ID;
            item.InitStatus(this);
            item._grade = unitData.StartGrade;
            item.activeSkillLevel = 1;
            item.RefreshStatus();

            _dicCharacterItemData.Add(item.id, item);
        }
    }
    private void LoadQuest()
    {
        if (_dicDailyQuestPointData.Count == 0)
        {
            for (int i = 20; i <= QuestPointMax; i += 20)
            {
                QuestPoint data = ClientLocalDB_Simple.GetData<QuestPoint>(DBKey.QuestPoint, $"{i}_{EResetType.Daily}");
                QuestPointItemData item = new QuestPointItemData() { _point = i, _resetType = EResetType.Daily };
                _dicDailyQuestPointData.Add(i, item);
            }
        }

        if (_dicWeeklyQuestPointData.Count == 0)
        {
            for (int i = 20; i <= QuestPointMax; i += 20)
            {
                QuestPoint data =
                    ClientLocalDB_Simple.GetData<QuestPoint>(DBKey.QuestPoint, $"{i}_{EResetType.Weekly}");
                QuestPointItemData item = new QuestPointItemData() { _point = i, _resetType = EResetType.Weekly };
                _dicWeeklyQuestPointData.Add(i, item);
            }
        }

        if (_dicDailyRoutineQuestData.Count == 0)
        {
            Dictionary<string, RoutineQuest> dic = ClientLocalDB_Simple.GetDB<RoutineQuest>(DBKey.RoutineQuest);
            foreach (var quest in dic)
            {
                RoutineQuest data = quest.Value;

                RoutineQuestItemData item = new RoutineQuestItemData()
                {
                    _conditionType = data.ConditionType,
                    _tableId = data.ID,
                    _resetType = data.ResetType,
                    _progressValue = 0
                };

                if (data.ResetType == EResetType.Daily)
                    _dicDailyRoutineQuestData.TryAdd(item._conditionType, item);
                else
                    _dicWeeklyRoutineQuestData.TryAdd(item._conditionType, item);
            }
        }
    }

    public void LoadConstellation()
    {
        if (_dicConstellationItemData.Count > 0)
            return;

        Dictionary<string, Constellation> constellationDB =
            ClientLocalDB_Simple.GetDB<Constellation>(DBKey.Constellation);
        List<string> unitKeys = constellationDB.Keys.ToList();
        int count = unitKeys.Count;

        for (int i = 0; i < count; i++)
        {
            ConstellationItemData itemData = new ConstellationItemData()
            {
                _id = constellationDB[unitKeys[i]].ID,
                _grade = EConstellationGrade.Normal,
                _isOpen = false
            };

            _dicConstellationItemData.Add(itemData._id, itemData);
        }
    }

    public void LoadAttendanceItemData()
    {
        if (_dicWeeklyAttendanceRewardItemData.Count > 0)
            return;

        // 임시 (추후 로그인 서버 연결로 setting)
        Dictionary<string, AttendanceData> weDic = ClientLocalDB_Simple.GetDB<AttendanceData>(DBKey.WeeklyAttendance);
        foreach (var item in weDic)
        {
            if (_dicWeeklyAttendanceRewardItemData.ContainsKey(item.Value.ID)) continue;
            EventRewardItemData itemData = new EventRewardItemData() { id = item.Value.ID, isGet = false, };
            _dicWeeklyAttendanceRewardItemData.Add(item.Value.ID, itemData);
        }

        Dictionary<string, AttendanceData> monDic = ClientLocalDB_Simple.GetDB<AttendanceData>(DBKey.MonthlyAttendance);
        foreach (var item in monDic)
        {
            if (_dicMonthlyAttendanceRewardItemData.ContainsKey(item.Value.ID)) continue;
            EventRewardItemData itemData = new EventRewardItemData() { id = item.Value.ID, isGet = false, };
            _dicMonthlyAttendanceRewardItemData.Add(item.Value.ID, itemData);
        }

        Dictionary<string, AttendanceData> newDic = ClientLocalDB_Simple.GetDB<AttendanceData>(DBKey.NewAttendance);
        foreach (var item in newDic)
        {
            if (_dicNewAttendanceItemData.ContainsKey(item.Value.ID)) continue;
            EventRewardItemData itemData = new EventRewardItemData() { id = item.Value.ID, isGet = false, };
            _dicNewAttendanceItemData.Add(item.Value.ID, itemData);
        }
    }

    /// <summary>
    /// 추후 서버로 받기
    /// </summary>
    public void LoadFieldItemData()
    {
        FieldInfo[] fieldInfos = ClientLocalDB_Simple.GetDB<FieldInfo>(DBKey.FieldInfo).Values.ToArray();

        foreach (var item in fieldInfos)
        {
            FieldItemData fieldItemData = new FieldItemData()
            {
                ID = item.ID,
                isOpen = false,
                progress = 0,
            };
            // max 계산 (이건 클라에서) 
            int maxCount = ClientLocalDB_Simple.GetDB<FieldQuestReward>(DBKey.FieldQuestReward).Values
                .Where(r => r.FieldID == fieldItemData.ID)
                .Select(r => r.ClearCount)
                .DefaultIfEmpty(0)
                .Max();
            fieldItemData.MaxProgress = maxCount;
            // max Progress Count 만큼 isGet 배열 할당
            fieldItemData.isGet = new bool[maxCount];
            // --- 
            _dicFieldItemData[fieldItemData.ID] = fieldItemData;
        }

    }

    //하루 획득 제한 데이터
    private void LoadCurrencyAcquireLimitData()
    {
        if (_dicCurrencyAcquireNumberData.Count > 0)
            return;
        
        UserLevelData userLevelData = ClientLocalDB_Simple.GetData<UserLevelData>(DBKey.UserLevel, 1);
        foreach (var currencyId in userLevelData.MaxCurrencyId)
        {
            _dicCurrencyAcquireNumberData.Add((ECurrency)currencyId, new ReactiveProperty<int>());
        }
    }

    private void LoadEventQuestData()
    {
        var db = ClientLocalDB_Simple.GetDB<OpenEventQuest>(DBKey.OpenEventQuest);
        foreach (var pair in db)
        {
            var quest = pair.Value;
            var itemData = new EventQuestItemData
            {
                _eventQuestType = Define.EQuestType.Open,
                _tableID = quest.ID,
                _progressValue = 0,
                _conditionType = quest.ConditionType,
            };

            if (!_dicOpenEventQuestItemData.ContainsKey(quest.Date))
                _dicOpenEventQuestItemData[quest.Date] = new Dictionary<EQuestConditionType, EventQuestItemData>();

            _dicOpenEventQuestItemData[quest.Date][itemData._conditionType] = itemData;
        }
    }

    #endregion

    public void SetUserInfoData(UserBaseDto userBaseDto)
    {
        if (userBaseDto == null)
            return;

        userId = userBaseDto.id;
        userNickName.Value = userBaseDto.userGameName;
        serverNum = userBaseDto.serverNum;
        _thumbnailID = userBaseDto.thumbnail;
        _frameID = userBaseDto.frame;
        _isAdsRemoved = userBaseDto.advertisement;
    }

    public void SetUserLevelData(UserLevelDto userLevelDto)
    {
        if (userLevelDto == null)
            return;

        levelUp = userLevel.Value != 0 && userLevel.Value != userLevelDto.level;
        userLevel.Value = userLevelDto.level;
        userExp.Value = userLevelDto.exp;
    }

    public int RefreshUserLevelData(UserLevelDto userLevelDto)
    {
        if (userLevelDto == null)
            return 0;

        int expGap = Utils.GetExpGap(userLevel.Value, userExp.Value, userLevelDto.level, userLevelDto.exp);
        SetUserLevelData(userLevelDto);
        return expGap;
    }
    
    private void UserLevelChanged(int value)
    {
        if (value == 0) return;
        UserLevelData userLevelData = ClientLocalDB_Simple.GetData<UserLevelData>(DBKey.UserLevel, value);
        if (userLevelData == null)
            return;
        
        for (int i = 0; i < userLevelData.MaxCurrencyId.Length; i++)
        {
            ECurrency currencyType = (ECurrency)userLevelData.MaxCurrencyId[i];
            _dicCurrencyAcquireLimitData[currencyType] = userLevelData.MaxCurrencyCount[i];
        }
    }

    #region Currency

    public ReactiveProperty<int> GetCurrencyProperty(ECurrency currency)
    {
        return _currencyItems[currency];
    }

    public void SetCurrencyValue(myCurrencyData[] currencyData, RequestContext requestContext)
    {
        if (currencyData == null || currencyData.Length == 0)
            return;
        
        foreach (var currency in currencyData)
        {
            ECurrency currencyType = (ECurrency)currency.currencyId;
            ReactiveProperty<int> item = _currencyItems[currencyType];

            //sync되는 타입은 따로 처리
            if (requestContext != null && requestContext.IsSync)
            {
                if (Managers.Instance.GetSyncCurrencyManager()._dicSyncCurrency.ContainsKey(currencyType))
                {
                    myCurrencyData data = requestContext.SyncCurrencyDto.syncCurrencyList.Find(row => row.currencyId == currency.currencyId);
                    if (data != null)
                    {
                        CalculateSyncCurrencyValue(currency, data);
                        continue;
                    }
                }
            }
            item.Value = currency.currentCount;
            SetCurrencyAcquireNumber(currencyType, currency.dayLimit);
        }
        
        //sync되는 타입은 따로 처리
        if (requestContext != null && requestContext.IsSync)
            Managers.Instance.GetSyncCurrencyManager().ResetSyncCurrency(); 
    }

    public int GetCurrencyValue(ECurrency currency)
    {
        return _currencyItems[currency].Value;
    }
    
    private void CalculateSyncCurrencyValue(myCurrencyData currencyData, myCurrencyData syncCurrencyData)
    {
        int addCount = currencyData.currentCount - syncCurrencyData.currentCount;
        AddCurrencyValue((ECurrency)currencyData.currencyId, addCount);
    }

    public void AddCurrencyValue(ECurrency currency, int value)
    {
        if (_currencyItems.TryGetValue(currency, out ReactiveProperty<int> item))
            item.Value += value;
    }
    
    public int CanCurrencyAcquireNumber(ECurrency currency, int defaultValue)
    {
        if (_dicCurrencyAcquireNumberData.TryGetValue(currency, out ReactiveProperty<int> number))
        {
            int limit = _dicCurrencyAcquireLimitData[currency];
            return limit - number.Value;
        }
        
        return defaultValue;
    }
    
    public bool CanCurrencyAcquireNumber(ECurrency currency)
    {
        if (_dicCurrencyAcquireNumberData.TryGetValue(currency, out ReactiveProperty<int> number))
        {
            int limit = _dicCurrencyAcquireLimitData[currency];
            return limit > number.Value;
        }
        
        return true;
    }
    
    public void AddCurrencyAcquireNumber(ECurrency currency, int value)
    {
        if(_dicCurrencyAcquireNumberData.ContainsKey(currency))
            _dicCurrencyAcquireNumberData[currency].Value += value;
    }

    private void SetCurrencyAcquireNumber(ECurrency currency, int value)
    {
        if(_dicCurrencyAcquireNumberData.ContainsKey(currency))
            _dicCurrencyAcquireNumberData[currency].Value = value;
    }

    public ReactiveProperty<int> GetCurrencyAcquireNumber(ECurrency currency)
    {
        if(_dicCurrencyAcquireNumberData.ContainsKey(currency))
            return _dicCurrencyAcquireNumberData[currency];
        
        return null;
    }
    
    public int GetCurrencyAcquireLimit(ECurrency currency)
    {
        if(_dicCurrencyAcquireLimitData.ContainsKey(currency))
            return _dicCurrencyAcquireLimitData[currency];
        
        return 0;
    }

    #endregion

    #region Item(소지품)

    public int GetItemValue(int id)
    {
        return _dicitemItems[id];
    }

    #endregion

    #region Deck

    public DeckData GetDeckData(EServerContentType content)
    {
        return _dicDeckData[content];
    }

    public void SetDeckData(EServerContentType content, DeckData data)
    {
        for (int i = 0; i < data.idList.Length; i++)
        {
            int id = data.idList[i];
            _dicDeckData[content].idList[i] = id;
        }
    }

    public void SaveDungeonDeck()
    {
        Dictionary<EServerContentType, DeckData> deckData = new Dictionary<EServerContentType, DeckData>(_dicDeckData);
        deckData.Remove(EServerContentType.Field); // 필드 덱은 제외

        string json = JsonConvert.SerializeObject(deckData);
        MyLogger.Log("json : " + json);
        SaveManager.SaveData($"DungeonDeck_{userId}", json);
    }

    public void LoadDungeonDeck()
    {
        string json = SaveManager.LodeData($"DungeonDeck_{userId}");

        if (string.IsNullOrEmpty(json))
            return;
        try
        {
            Dictionary<EServerContentType, DeckData> deckData = JsonConvert.DeserializeObject<Dictionary<EServerContentType, DeckData>>(json);

            foreach (var deck in deckData)
            {
                SetDeckData(deck.Key, deck.Value);
            }
        }
        catch
        {
            SaveManager.RemoveData($"DungeonDeck_{userId}");
        }
    }

    public bool CheckEquipDeckCharacter(EServerContentType content, int id)
    {
        return _dicDeckData[content].idList.Contains(id);
    }

    /// <summary>
    /// Deck Update 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="index"></param>
    /// <param name="contentType"></param>
    public void ChangeDeck(int id, int index, EServerContentType contentType)
    {
#if USE_SERVER

        DeckData deckData = new DeckData(contentType);
        Array.Copy(_dicDeckData[contentType].idList, deckData.idList, deckData.idList.Length);
        ChangeDeck(deckData, id, index);

        if (contentType == EServerContentType.Field)
        {
            Managers.Instance.GetServerManager().OnPostSetMyHeroDeck(deckData.idList.ToList());
        }
        else
        {
            // 필드 & 공명 덱 제외 클라 저장
            _dicDeckData[contentType].idList = deckData.idList;
            UIManager.UIDeckSetting.stab.deckSettingPage.InitScrollItemData();
            UIManager.UIDeckSetting.Refresh();
        }

#else
        int deckSize = ReturnDeckSize(contentType);
        //by rainful 2025-05-15 필드덱에 한하여 5월 빌드에서는 아래 코드로 대체 합니다.         
        if (contentType == EContent.Field) {
            ChangeDeck(_dicDeckData[contentType], id, index);
        }
        else
        {
            DeckData deckData = new DeckData(contentType, deckSize);
            Array.Copy(_dicDeckData[contentType].idList, deckData.idList, deckSize);
            ChangeDeck(deckData, id, index);
            _dicDeckData[contentType].idList = deckData.idList;
        }

        if (contentType == EContent.Resonance)
            Managers.Instance.GetUIManager().UICharacterInventory.Refresh();
        else
        {
            Managers.Instance.GetUIManager().UIDeckSetting.InitContentType(contentType);
            Managers.Instance.GetUIManager().UIDeckSetting.Refresh();
        }
#endif
    }

    public void SwapDeck(int idx1, int idx2, EServerContentType contentType)
    {
        DeckData deckData = new DeckData(contentType);
        Array.Copy(_dicDeckData[contentType].idList, deckData.idList, deckData.idList.Length);
        SwapDeck(deckData, idx1, idx2);

#if USE_SERVER

        if (contentType == EServerContentType.Field)
            Managers.Instance.GetServerManager().OnPostSetMyHeroDeck(deckData.idList.ToList());

#else
        // 추후 서버 연결 하면 삭제 (덱 세팅이 되기 때문에 필요 없음
        _dicDeckData[contentType].idList = deckData.idList;

        if (contentType == EContent.Field)
        {
            //서버 통신 작업 예정.
        }
        else if (contentType == EContent.Resonance)
            Managers.Instance.GetUIManager().UICharacterInventory.Refresh();
#endif
    }

    private void ChangeDeck(DeckData deckData, int id, int idx)
    {
        if (idx < 0) return;
        deckData.idList[idx] = id;
    }

    private void SwapDeck(DeckData deckData, int idx1, int idx2)
    {
        if (idx1 < 0 || idx2 < 0) return;
        deckData.SwapID(idx1, idx2);
    }

    public bool CheckContainDeck(EServerContentType type, int id)
    {
        return _dicDeckData[type].idList.Contains(id);
    }

    #endregion

    #region Slot Function

    public double ReturnAllBattlePower()
    {
        double battlepower = 0;

        DeckData deckData = _dicDeckData[EServerContentType.Field];
        for (int i = 0; i < deckData.idList.Length; i++)
        {
            CharacterClassItemData data = GetCharacterItemData(deckData.idList[i]);
            if (data == null)
                continue;
            battlepower += data._statusInfo._battlePower;
        }

        return battlepower;
    }

    double ReturnAllATK()
    {
        double atk = 0;
        for (int i = 0; i < _dicDeckData[EServerContentType.Field].idList.Length; i++)
        {
            CharacterClassItemData data = GetCharacterItemData(_dicDeckData[EServerContentType.Field].idList[i]);
            if (data == null) continue;
            Status status = data._statusInfo.GetCharacterStatus();
            atk += status._attack;
        }

        return atk;
    }

    double ReturnAllMaxHP()
    {
        double maxHp = 0;
        for (int i = 0; i < _dicDeckData[EServerContentType.Field].idList.Length; i++)
        {
            CharacterClassItemData data = GetCharacterItemData(_dicDeckData[EServerContentType.Field].idList[i]);
            if (data == null) continue;
            Status status = data._statusInfo.GetCharacterStatus();
            maxHp += status._maxHp;
        }

        return maxHp;
    }


    // new (2025.12.02)
    public void SetResonanceSlot(ResonanceDto[] resonanceDtos)
    {
        if (resonanceDtos == null)
            return;

        foreach (var item in resonanceDtos)
        {
            ResonanceItemData resonanceItemData = new ResonanceItemData()
            {
                _index = item.index,
                _characterId = item.heroTableId,
                _isLock = false,
                _expireDate = DateTime.Parse(item.releaseTime)
            };

            _dicResonanceItemData[resonanceItemData._index] = resonanceItemData;
        }

        _unlockResonanceCount = _dicResonanceItemData.Values.Count(s => !s._isLock);

        GetTopLevelCharacterList();
    }

    /// <summary>
    /// 상위 레벨 5명 캐릭터 구하기 (공명 슬롯 캐릭터는 제외)
    /// 캐릭터 레벨과 공명 슬롯이 업데이트되었을 때 다시 셋팅
    /// </summary>
    public void GetTopLevelCharacterList()
    {
        // 공명 슬롯에 포함되어 있는
        HashSet<int> resonanceCharacterIds = Enumerable.ToHashSet(_dicResonanceItemData.Values.Select(c => c._characterId));

        // 가장 높은 레벨 캐릭터 5명 
        List<CharacterClassItemData> characterList =
            _dicCharacterItemData.Values.Where(c => c.isOpen && !resonanceCharacterIds.Contains(c.id))
                .OrderByDescending(c => c.Level)
                .ThenBy(c => c.id)
                .Take(5).ToList();

        _TopLevelCharacterList = characterList;
        _resonanceLevel = _TopLevelCharacterList.Min(c => c.Level);
    }

    /// <summary>
    /// 공명 슬롯에 포함되어 있는 캐릭터 인가
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool ReturnResonanceCharacter(int id)
    {
        return _dicResonanceItemData.Any(c => c.Value._characterId == id);
    }


    /// <summary>
    /// 공명 슬롯 해금 (클라이언트 계산 용)
    /// </summary>
    public void UnlockResonance()
    {
        // 재화 소모

        _unlockResonanceCount++;
        _dicResonanceItemData[_unlockResonanceCount - 1]._isLock = false;
    }

    /// <summary>
    /// 공명 슬롯 셋팅 (클라이언트 계산용)
    /// </summary>
    /// <param name="index"></param>
    /// <param name="id"></param>
    public void RegistrationResonanceCharacter(int index, int id)
    {
        _dicResonanceItemData[index]._characterId = id;
        _dicResonanceItemData[index]._expireDate = default;
    }

    /// <summary>
    /// 캐릭터 슬롯 해제 (클라이언트 계산 용)
    /// </summary>
    /// <param name="index"></param>
    public void ReleaseResonanceCharacter(int index)
    {
        _dicResonanceItemData[index]._characterId = 0;
        _dicResonanceItemData[index]._expireDate = DateTime.Now.AddHours(24); // 시간 임시(완료 시간)
    }

    #endregion

    #region Character Player

    public CharacterClassItemData GetCharacterItemData(int id)
    {
        if (!_dicCharacterItemData.ContainsKey(id)) return null;
        return _dicCharacterItemData[id];
    }

    public void UpdateCharacterItemData(myHerosData[] myHerosDatas)
    {
        foreach (var data in myHerosDatas)
        {
            if (!_dicCharacterItemData.TryGetValue(data.heroTableId, out CharacterClassItemData item))
                continue;

            item.isOpen = true;
            if(data.level > 0)
                item.Level = data.level;
            item._currentCount = data.currentCount;
            if(data.awakenStep > 0)
                item._grade = (EGradeType)data.awakenStep;
            if (data.activeSkillLevel > 0)
                item.activeSkillLevel = data.activeSkillLevel;

            item.RefreshStatus();
        }

        GetTopLevelCharacterList();
    }

    public void AddCharacterItemData(int id, int count)
    {
        if (!_dicCharacterItemData.ContainsKey(id))
            return;
        CharacterClassItemData item = _dicCharacterItemData[id];
        if (item.isOpen == false)
            item._currentCount = count;
        else
            item._currentCount += count;
        item.isOpen = true;
        item.RefreshStatus();
    }

    public void RefreshStatusCharacterItem()
    {
        foreach (var data in _dicCharacterItemData.Values)
        {
            data.RefreshStatus();
        }
    }

    public void SaveCharacter()
    {
        //SaveCharacterData data = new SaveCharacterData();
        //Dictionary<int, PlayerUnitData> playerUnitDB = Managers.Instance.GetDBManager().GetPlayerUnitDB();
        //List<int> unitDBKeys = playerUnitDB.Keys.ToList();
        //for (int i = 0; i < _dicCharacterItemData.Count; i++)
        //{
        //    SaveCharacterDto dto = new SaveCharacterDto();
        //    CharacterClassItemData itemData = _dicCharacterItemData[unitDBKeys[i]];
        //    dto.id = itemData.id;
        //    dto.level = itemData.level;
        //    dto.isOpen = itemData.isOpen;

        //    data.dicCharacterItemData.Add(dto.id,dto);
        //}
        //Managers.Instance.GetSaveManager().SaveCharacter(data);
    }

    public void AllCharacterStatusCacluate()
    {
        foreach (var itemData in _dicCharacterItemData.Values)
        {
            itemData.RefreshStatus();
        }
    }

    public int OpenCharacterCount()
    {
        int cnt = 0;
        foreach (var item in _dicCharacterItemData)
        {
            if (item.Value.isOpen) cnt++;
        }

        return cnt;
    }

    #endregion

    #region Training Function

    /// <summary>
    /// 초기에 Training Status 세팅
    /// </summary>
    /// <param name="basicId"></param>
    /// <param name="hardId"></param>
    public void SetTrainingStatus(int basicId, int hardId)
    {
        Managers.Instance.UserInfo()._trainingStatus.Reset();
        for (int i = ClientLocalDB_Simple.GetDB<BasicTraining>(DBKey.BasicTraining).First().Value.ID; i <= basicId; i++)
        {
            BasicTraining basicData = ClientLocalDB_Simple.GetData<BasicTraining>(DBKey.BasicTraining, i);
            Managers.Instance.UserInfo()._trainingStatus.Plus(basicData.StatusType, basicData.StatusValue);
        }

        for (int i = ClientLocalDB_Simple.GetDB<HardTraining>(DBKey.HardTraining).FirstOrDefault().Value.ID;
             i <= hardId;
             i++)
        {
            HardTraining hardData = ClientLocalDB_Simple.GetData<HardTraining>(DBKey.HardTraining, i);
            if (hardData == null)
                continue;

            if (hardData.RewardType == EHardTrainingType.Status)
                Managers.Instance.UserInfo()._trainingStatus.Plus(hardData.StatusType, hardData.StatusValue);
            else if (hardData.RewardType == EHardTrainingType.PortalEnable)
                _enablePortal.Value = true;
        }

        Managers.Instance.UserInfo().AllCharacterStatusCacluate();
    }

    #endregion

    #region Building

    public BuildingData GetInstallationBuilding(int id)
    {
        if (!_dicInstallationBuildingData.TryGetValue(id, out BuildingData building))
            return null;

        return building;
    }

    public void UpdateInstallationBuilding(MyFieldDto[] fieldList)
    {
        foreach (var field in fieldList)
        {
            UpdateInstallationBuilding(field);
        }
    }

    public void UpdateProductBuilding(DateTime syncTime)
    {
        List<BuildingData> buildingDataList = _dicInstallationBuildingData.Values.ToList().FindAll(building => building._isBuild.Value && building._data.BuildingType == EBuildingType.Storage);

        if (buildingDataList.Count == 0)
            return;

        foreach (var buildingData in buildingDataList)
        {
            buildingData.UpdateSyncTime(syncTime);
        }
    }

    public void UpdateInstallationBuilding(MyFieldDto fieldDto)
    {
        _dicInstallationBuildingData.TryGetValue(fieldDto.buildingInfoId, out BuildingData buildingData);
        if (buildingData == null)
            return;

        buildingData._level = fieldDto.level;
        buildingData._isBuild.Value = fieldDto.active;
        buildingData._currencyCount.Value = fieldDto.buildingCurrencyCurrentCount;
    }

    public void SaveBuilding()
    {
        // Dictionary<string, BuildingInfo> buildingInfoDB = ClientLocalDB_Simple.GetDB<BuildingInfo>(DBKey.BuildingInfo);
        // List<string> buildingDBKeys = buildingInfoDB.Keys.ToList();
        // SaveBuildingData data = new SaveBuildingData();
        // for (int i = 0; i < buildingDBKeys.Count; i++)
        // {
        //     data.dicInstallBuildingData.Add(int.Parse(buildingDBKeys[i]), _dicInstallationBuildingData[int.Parse(buildingDBKeys[i])].isOpen);
        // }

        //Managers.Instance.GetSaveManager().SaveBuilding(data);
    }

    #endregion

    #region Equipment

    public EquipmentItemData GetEquipmentItemData(long id)
    {
        if (!_dicEquipmentItemData.ContainsKey(id)) return null;
        return _dicEquipmentItemData[id];
    }

    public EquipmentItemData[] GetEquipmentIDList(EFactionType actionType)
    {
        return _dicEquipment[actionType];
    }

    public void EquipEquipmentList(EFactionType type, MyEquipStatusList equipmentList)
    {
        int[] equipmentIds = new int[]
        {
            equipmentList.weapon, equipmentList.subWeapon, equipmentList.helmet, equipmentList.armor,
            equipmentList.gloves, equipmentList.shoes
        };

        for (int i = 0; i < equipmentIds.Length; i++)
        {
            _dicEquipment[type][i] = GetEquipmentItemData(equipmentIds[i]);
        }

        // 능력치 계산 
        CalculateEquipmentStatus(type);
    }

    public bool IsEquipped(EFactionType faction, long equipmentId)
    {
        if (!_dicEquipment.ContainsKey(faction)) return false;

        foreach (var slot in _dicEquipment[faction])
        {
            if (slot != null && slot.id == equipmentId) return true;
        }
        return false;
    }

    public void RefreshEquipEquipmentItemData(EFactionType faction)
    {
        if (!_dicEquipment.ContainsKey(faction)) return;

        var slots = _dicEquipment[faction];
        for (int i = 0; i < slots.Count(); i++)
        {
            if (slots[i] == null) continue;
            slots[i] = GetEquipmentItemData(slots[i].id);
        }
        CalculateEquipmentStatus(faction);
    }

    public void AddEquipmentItemData(EquipmentDto equipment)
    {
        EquipmentItemData equipmentItem = new EquipmentItemData() 
        {
            id = equipment.id, 
            tableId = equipment.tableId 
        };
        for (int i = 0; i < MaxEquipmentOptionCount; i++)
        {
            equipmentItem._equipmentOption[i] = equipment.GetEquipmentOption(i);
        }
        equipmentItem.SetStatus();

        if(_dicEquipmentItemData.ContainsKey(equipment.id))
            _dicEquipmentItemData[equipment.id] = equipmentItem;
        else
            _dicEquipmentItemData.Add(equipmentItem.id, equipmentItem);
    }

    public void AddEquipmentItemData(EquipmentDto[] equipmentData)
    {
        foreach (var data in equipmentData)
        {
            AddEquipmentItemData(data);
        }
    }

    public void SetEquipmentItemData(EquipmentDto[] equipments)
    {
        _dicEquipmentItemData.Clear();
        foreach (EquipmentDto equipment in equipments)
        {
            EquipmentItemData equipmentItem = new EquipmentItemData()
            {
                id = equipment.id,
                tableId = equipment.tableId,
                isLock = equipment.isLock,

            };
            // 옵션 
            for (int i = 0; i < MaxEquipmentOptionCount; i++)
            {
                equipmentItem._equipmentOption[i] = equipment.GetEquipmentOption(i);
            }
            equipmentItem.SetStatus();

            if (_dicEquipmentItemData.ContainsKey(equipment.id))
                _dicEquipmentItemData[equipment.id] = equipmentItem;
            else
                _dicEquipmentItemData.Add(equipmentItem.id, equipmentItem);
        }
    }

    /// <summary>
    /// 장비 전투력 반환
    /// </summary>
    /// <returns></returns>
    public double EquipmentFactionBattlePower(EFactionType factionType)
    {
 
        Status status = new Status();
        switch (factionType)
        {
            case EFactionType.Celestial:
                status = _CelestialEquipmentStatus;
                break;
            case EFactionType.Crusher:
                status = _CrusherEquipmentStatus;
                break;
            case EFactionType.Guardian:
                status = _GuardianEquipmentStatus;
                break;
            case EFactionType.Human:
                status = _HumanEquipmentStatus;
                break;

            default:
                break;
        }

        return CalculateStatus.CalculateBattlePoint(status, EGradeType.Common, 0);
    }

    public void CalculateEquipmentStatus(EFactionType _currentFactionType)
    {
        // 능력치 계산 

        int maxlevel = ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "EquipmentMaxLevel").Value;

        Status newStatus = new Status();
        for (int i = 0; i < _dicEquipment[_currentFactionType].Length; i++)
        {
            EquipmentItemData data = _dicEquipment[_currentFactionType][i];
            if (data == null) continue;

            Status dummyStatus = new Status();
            dummyStatus.Set(data.allStauts);
            foreach (EStatus item in Enum.GetValues(typeof(EStatus)))
            {
                dummyStatus.Multiply(item, 1 + 0.4f * Mathf.Log(_dicAltarLevel[_currentFactionType], maxlevel));
            }

            newStatus += dummyStatus;
        }

        switch (_currentFactionType)
        {
            case EFactionType.Celestial:
                _CelestialEquipmentStatus.Reset();
                _CelestialEquipmentStatus.Set(newStatus);
                break;
            case EFactionType.Crusher:
                _CrusherEquipmentStatus.Reset();
                _CrusherEquipmentStatus.Set(newStatus);
                break;
            case EFactionType.Guardian:
                _GuardianEquipmentStatus.Reset();
                _GuardianEquipmentStatus.Set(newStatus);
                break;
            case EFactionType.Human:
                _HumanEquipmentStatus.Reset();
                _HumanEquipmentStatus.Set(newStatus);
                break;
            default:
                break;
        }

        AllCharacterStatusCacluate();
    }

    #endregion

    #region Quest

    public void SetMyQuestInfo(myQuestInfo questInfo)
    {
        _dailyQuestPoint = questInfo.currentDayQuestPoint;
        foreach (var item in _dicDailyQuestPointData.Values)
        {
            item.isFinish = false;
        }
        for (int i = 20; i <= questInfo.receivedDayRewardIndex * 20; i += 20)
        {
            _dicDailyQuestPointData[i].isFinish = true;
        }

        _weeklyQuestPoint = questInfo.currentWeekQuestPoint;
        foreach (var item in _dicWeeklyQuestPointData.Values)
        {
            item.isFinish = false;
        }
        for (int i = 20; i <= questInfo.receivedWeekRewardIndex * 20; i += 20)
        {
            _dicWeeklyQuestPointData[i].isFinish = true;
        }
    }

    public QuestPointItemData GetQuestPoint(EResetType type, int point)
    {
        switch (type)
        {
            case EResetType.Daily:
                return _dicDailyQuestPointData[point];
            case EResetType.Weekly:
                return _dicWeeklyQuestPointData[point];
        }
        return null;
    }

    public void SetCurrentGuideQuest(MyGuideInfo guideInfo)
    {
        FieldItemData fieldItemData = _dicFieldItemData[FieldItemData.GetFirstFieldId()];
        fieldItemData.isQuestClearable = guideInfo.clearable;
        fieldItemData.isQuestFinish = guideInfo.allClear;

        fieldItemData.guideQuestId.Value = guideInfo.currentQuestId;

        // 모든 퀘스트 완료 상태면 이후 처리 불필요
        if (guideInfo.allClear)
            return;

        GuideQuest currentQuest = fieldItemData.GetCurrentQuest();
        fieldItemData.questProgressValue = (guideInfo.clearable && currentQuest != null)
            ? currentQuest.ConditionValue.Last()
            : 0;
        CheckAlreadyGuideQuestClear();
    }

    public void CheckAlreadyGuideQuestClear()
    {
        // 가이드 퀘스트는 항상 첫 번째 필드 기준 (SetCurrentGuideQuest와 동일한 필드)
        FieldItemData firstFieldItemData = _dicFieldItemData[FieldItemData.GetFirstFieldId()];
        GuideQuest currentQuest = firstFieldItemData.GetCurrentQuest();
        if (currentQuest == null)
            return;

        // 건물이 open상태라면
        BuildingData buildingData = GetInstallationBuilding(currentQuest.ConditionValue.First());
        if (currentQuest.ConditionType == EQuestConditionType.BuildingTarget &&
            buildingData != null && buildingData.isOpen)
        {
            // 클리어 처리
            UpdateProgressValue(1);
            return;
        }

        // Training
        if (currentQuest.ConditionType == EQuestConditionType.TrainingBasicLevel)
            UpdateProgressValue(UnlockBasicIdx);
        else if (currentQuest.ConditionType == EQuestConditionType.TrainingHardLevel)
            UpdateProgressValue(UnlockHardIdx);
    }


    public void UpdateProgressValue(int value)
    {
        GuideQuestProgressValue = value;
        UIManager.UIGuideQuest?.Refresh();

        if (isGuideQuestClear)
        {
            if (!isGuideQuestClearable)
            {
                isGuideQuestClearable = true;
                if (CurrentFieldItemData.IsFirstFieldID)
                    Managers.Instance.GetServerManager().OnGetClearableGuideQuest();
                else
                    Managers.Instance.GetServerManager().OnGetDungeonQuestClearable();
            }   
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="questType"></param>
    /// <param name="type"></param>
    /// <param name="conditionId">특정 퀘스트 타입에서 체크해야 하는 컨디션 ID 값</param>
    /// <param name="count"></param>
    public void UpdateCorrectQuest(EQuestType questType, EQuestConditionType type, int conditionId, int count = 1)
    {
        int conditionValue = 0;
        int conditionCount = 0;
        bool state = false;
        if (questType == EQuestType.Guide)
        {
            if (isGuideQuestFinish || isGuideQuestClear) return;
            EQuestConditionType conditionType = CurrentFieldItemData.GetCurrentQuest().ConditionType;
            if (conditionType != type) return;
            conditionValue = CurrentFieldItemData.GetCurrentQuest().ConditionValue.First();
            conditionCount = GuideQuestProgressValue;
        }
        else if (questType == EQuestType.Daily)
        {
            if (!_dicDailyRoutineQuestData.TryGetValue(type, out RoutineQuestItemData itemData))
                return;

            conditionValue = itemData.TableQuestDB().ConditionValue.FirstOrDefault();
            conditionCount = itemData._progressValue;
        }
        else if (questType == EQuestType.Weekly)
        {
            if (!_dicWeeklyRoutineQuestData.TryGetValue(type, out RoutineQuestItemData itemData))
                return;

            conditionValue = itemData.TableQuestDB().ConditionValue.FirstOrDefault();
            conditionCount = itemData._progressValue;
        }
        else if(questType == EQuestType.Open)
        {
            EventQuestItemData openItemData = null;
            for (int day = OpenEventCurrentDay; day >= 1; day--)
            {
                if (_dicOpenEventQuestItemData.TryGetValue(day, out var dayDict) &&
                    dayDict.TryGetValue(type, out openItemData))
                    break;
            }

            if (openItemData == null) return;

            var dbData = openItemData.GetData<OpenEventQuest>();
            conditionValue = dbData?.ConditionValue?.FirstOrDefault() ?? 0;
            conditionCount = openItemData._progressValue;
        }

        switch (type)
        {
            case EQuestConditionType.MonsterKillAnyone:
            case EQuestConditionType.MercenaryLevelup:
            case EQuestConditionType.GatherGetAnyone:
            case EQuestConditionType.DungeonClearAll:
            case EQuestConditionType.TowerEnter:
            case EQuestConditionType.TraningCount:
            case EQuestConditionType.AllMercenaryAwakenCount:
            case EQuestConditionType.Login:
            case EQuestConditionType.ConstellationOpen:
            case EQuestConditionType.GachaCount:
            case EQuestConditionType.GuildAttend:
            case EQuestConditionType.ShopPurchase:
            case EQuestConditionType.EquipmentEquip:
            case EQuestConditionType.EquipmentDismiss:
            case EQuestConditionType.TrasureBoxOpen:
                state = true;
                conditionCount += count;
                break;

            case EQuestConditionType.MonsterKillType:
            case EQuestConditionType.MonsterKillTarget:
            case EQuestConditionType.BuildingTarget:
            case EQuestConditionType.GatherTarget:
            case EQuestConditionType.MonsterKillTargetSpawn:
            case EQuestConditionType.DungeonClearTarget:
                state = conditionId == conditionValue;
                conditionCount += count;
                break;

            // 기존 값에서 추가 되는게 아니고 세팅되야함
            case EQuestConditionType.TrainingBasicLevel:
            case EQuestConditionType.TrainingHardLevel:
            case EQuestConditionType.GuideQuestClearID:
                state = conditionId <= conditionValue;
                conditionCount = count;
                break;

            // 기존 값에서 추가 되는게 아니고 세팅되야함
            case EQuestConditionType.BuildingTargetLevel:
                state = conditionId <= conditionValue;
                conditionCount = count;
                break;

            default:
                break;
        }

        if (state)
        {
            if (questType == EQuestType.Guide)
            {
                UpdateProgressValue(conditionCount);
            }
            else if(questType == EQuestType.Daily || questType == EQuestType.Weekly)
            {
                EResetType resetType = (EResetType)questType;
                UpdateQuestProgressValue(type, resetType, conditionCount);
            }
            else if(questType == EQuestType.Open)
            {
                EventQuestProgressValue(questType, type, conditionCount, count);
            }
        }
    }

    public void UpdateQuestListInfo(myQuestListInfo[] myQuestListInfoData)
    {
        if (myQuestListInfoData == null || myQuestListInfoData.Length == 0)
            return;

        foreach (myQuestListInfo item in myQuestListInfoData)
        {
            RoutineQuest data = ClientLocalDB_Simple.GetData<RoutineQuest>(DBKey.RoutineQuest, item.questId);

            if (data.ResetType == EResetType.Daily)
            {
                RoutineQuestItemData questItem = _dicDailyRoutineQuestData[data.ConditionType];
                questItem._progressValue = item.currentCount;
                questItem.isFinish = item.receivedPoint;
            }
            else if (data.ResetType == EResetType.Weekly)
            {
                RoutineQuestItemData questItem = _dicWeeklyRoutineQuestData[data.ConditionType];
                questItem._progressValue = item.currentCount;
                questItem.isFinish = item.receivedPoint;
            }
        }
    }

    public void UpdateQuestProgressValue(EQuestConditionType conditionType, EResetType resetType, int value)
    {
        RoutineQuestItemData itemData = new RoutineQuestItemData();
        switch (resetType)
        {
            case EResetType.Daily:
                itemData = _dicDailyRoutineQuestData[conditionType];
                break;
            case EResetType.Weekly:
                itemData = _dicWeeklyRoutineQuestData[conditionType];
                break;
        }

        if (itemData.isClear || itemData.isFinish) return; // 이미 끝

        itemData._progressValue = value;


        if (UIManager._mainInfoUI != null && itemData.isClear) UIManager.MainInfoUI.Refresh();
    }

    public void EventQuestProgressValue(EQuestType eventQuestType, EQuestConditionType conditionType, int conditionCount, int count)
    {
        // 기존값에서 추가가 아닌 셋팅인 type
        bool isSet = conditionType is EQuestConditionType.TrainingBasicLevel
                                   or EQuestConditionType.TrainingHardLevel
                                   or EQuestConditionType.GuideQuestClearID
                                   or EQuestConditionType.BuildingTargetLevel;

        bool needRefresh = false;
        foreach (var (day, dayDict) in _dicOpenEventQuestItemData)
        {
            if (day > OpenEventCurrentDay) continue;
            if (!dayDict.TryGetValue(conditionType, out EventQuestItemData itemData)) continue;
            if (itemData.isClear || itemData.isFinish) continue;

            itemData._progressValue = isSet ? conditionCount : itemData._progressValue + count;
            if (itemData.isClear) needRefresh = true;
        }
        if (UIManager._mainInfoUI != null && needRefresh) UIManager.MainInfoUI.Refresh();
    }

    public void QuestMoveAction(EQuestConditionType conditionType, int id)
    {
        switch (conditionType)
        {
            case EQuestConditionType.Login:
                UIManager.ShowCommonToastMessage("출석 하세요");
                break;

            case EQuestConditionType.MonsterKillAnyone:
                UIManager.ShowCommonToastMessage("몬스터를 처치하세요");
                break;
            case EQuestConditionType.MonsterKillType:
            case EQuestConditionType.MonsterKillTargetSpawn:
            case EQuestConditionType.MonsterKillTarget:
            case EQuestConditionType.GatherTarget:
                Managers.Instance.GetMapManager().SpawnPointCameraMove(id);
                break;
            case EQuestConditionType.GatherGetAnyone:
                UIManager.ShowCommonToastMessage("자원을 채집하세요");
                break;
            case EQuestConditionType.MercenaryLevelup:
                UIManager.UICharacterInventory.OpenToStack();
                break;

            case EQuestConditionType.AllMercenaryAwakenCount:
                UIManager.UICharacterInventory.OpenToStack();

                break;


            case EQuestConditionType.TrainingBasicLevel:
            case EQuestConditionType.TrainingHardLevel:
            case EQuestConditionType.TraningCount:
                UIManager.TrainingUI.OpenToStack();
                break;

            case EQuestConditionType.BuildingTarget:
            case EQuestConditionType.BuildingTargetLevel:
                if (id == 20032)
                {
                    BuildingInfo buildingInfo = ClientLocalDB_Simple.GetData<BuildingInfo>(DBKey.BuildingInfo, id);
                    ECurrency currencyType = (ECurrency)buildingInfo.CurrencyList[0];
                    int value = GetCurrencyValue(currencyType);
                    if (value < buildingInfo.CountList[0])
                    {
                        UIManager.ShowCommonToastMessage("봉인의 열쇠가 부족합니다.\n산적 소굴을 클리어하면 획득할 수 있습니다.");
                        return;
                    }
                }
                
                Managers.Instance.GetMapManager().BuildingCameraMove(id);
                break;

            case EQuestConditionType.DungeonClearTarget:
            case EQuestConditionType.DungeonClearAll:
            case EQuestConditionType.TowerEnter:
                Managers.Instance.GetUIManager().UIDungeonEntranceList.OpenToStack();
                break;
            case EQuestConditionType.EquipmentEquip:
            case EQuestConditionType.EquipmentDismiss:
                UIManager.UIEquipmentSetting.OpenToStack();
                break;

            case EQuestConditionType.GachaCount:
                UIManager.OpenGachaUI(EGachaType.PickUp);
                break;

            case EQuestConditionType.TrasureBoxOpen:
                UIManager.ShowUIToast<UIToastBase>("필드에서 보물 상자를 획득하세요", "ToastMessage");
                break;
            case EQuestConditionType.ConstellationOpen:
                UIManager.UIConstellation.OpenToStack();
                break;
            case EQuestConditionType.ShopPurchase:
                UIManager.UIShop.SetCashShopOpenToStack(false);
                break;
            case EQuestConditionType.GuildAttend:
                Managers.Instance.GetServerManager().OnRequestMyGuildInfo(() =>
                {

                    if (ExistGuild == false)
                        Managers.Instance.GetUIManager().GuildInfoListPage.OpenToStack();
                    else
                        Managers.Instance.GetUIManager().UIGuildHome.OpenToStack();

                });
                break;
            case EQuestConditionType.GuideQuestClearID:
                UIManager.ShowCommonToastMessage("길잡이를 클리어하세요.");
                break;

            default:
                UIManager.ShowCommonToastMessage("임무를 진행하세요");
                break;
        }
    }

    public bool RoutineQuestFinish(EResetType resetType)
    {
        if(resetType == EResetType.Daily)
        {
            foreach (var item in _dicDailyQuestPointData)
            {
                if (!item.Value.isClear) return false;
                else if (item.Value.isFinish == false)
                    return false;
            }
        }
        else if(resetType ==  EResetType.Weekly)
        {
            foreach (var item in _dicWeeklyQuestPointData)
            {
                if (!item.Value.isClear) return false;
                else if (item.Value.isFinish == false)
                    return false;
            }
        }

        return true;
    }
    

    #endregion

    #region Pass

    public void SetPassDto(PassDto[] passDtos)
    {
        foreach (PassDto pass in passDtos)
        {
            PassItemData passItemData = new PassItemData
            {
                passType = pass.passTemplateId,
                passXp = pass.exp,
                passLevel = pass.passLevel,
                isPremium = pass.premium,
                freeGetLevel = pass.freeRewardCount,
                premiumGetLevel = pass.premiumRewardCount,
                startTime = DateTime.Parse(pass.startTime),
                endTime = DateTime.Parse(pass.endTime),
            };
            _dicPassItem[passItemData.passType] = passItemData;
        }
    }


    public PassItemData GetPassItemData(int passType)
    {
        return _dicPassItem[passType];
    }


    #endregion

    #region Gacha

    public void UpdateGachaData(GachaDataDto gachaData)
    {
        if (gachaData == null)
            return;

        _dicGachaItemData[EGachaType.General]._count = gachaData.normalCeilingCount;
        if (gachaData.wishList.Length > 0)
            _dicGachaItemData[EGachaType.General]._wishList = gachaData.wishList;
        _dicGachaItemData[EGachaType.PickUp]._count = gachaData.pickUpCeilingCount;
        _dicGachaItemData[EGachaType.Celestial]._count = gachaData.celestialCeilingCount;
        if (gachaData.celestialWishList != 0)
            _dicGachaItemData[EGachaType.Celestial]._wishList = new int[] { gachaData.celestialWishList };
    }

    public bool CheckGachaCharacterMaxGrade(int id)
    {
        CharacterClassItemData itemData = _dicCharacterItemData[id];
        int remainCost = ReturnRemainPiece(id);
        // 각성까지 필요한 조각
        return remainCost <= itemData._currentCount;
    }

    public int ReturnRemainPiece(int id)
    {
        CharacterClassItemData itemData = _dicCharacterItemData[id];
        UnitData db = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, id);

        EGradeType maxGrade = ClientLocalDB_Simple.GetData<GradeSetting>(DBKey.GradeSetting, db.StartGrade.ToString()).MaxGrade;
        int remainCost = 0;
        for (int i = (int)itemData._grade; i < (int)maxGrade; i++)
        {
            GradeAwaken gradeAwake = ClientLocalDB_Simple.GetData<GradeAwaken>(DBKey.GradeAwaken, i);
            switch (db.StartGrade)
            {
                case EGradeType.Common:
                    remainCost += gradeAwake.Common_PieceCost;
                    break;
                case EGradeType.Rare:
                    remainCost += gradeAwake.Rare_PieceCost;
                    break;
                case EGradeType.Epic:
                    remainCost += gradeAwake.Epic_PieceCost;
                    break;
                case EGradeType.Legendary:
                    remainCost += gradeAwake.Legendary_PieceCost;
                    break;

                default:
                    break;
            }
        }

        return remainCost;
    }

    #endregion

    #region Constellation

    public void SetConstellationItemData(constelltionNode node)
    {
        if (!_dicConstellationItemData.ContainsKey(node.nodeId))
        {
            ConstellationItemData itemData = new ConstellationItemData()
            {
                _id = node.nodeId,
                _grade = (EConstellationGrade)node.optionGrade,
                _isOpen = node.open
            };

            _dicConstellationItemData.Add(itemData._id, itemData);
            return;
        }

        _dicConstellationItemData[node.nodeId]._grade = (EConstellationGrade)node.optionGrade;
        _dicConstellationItemData[node.nodeId]._isOpen = node.open;
    }


    public ConstellationItemData GetConstellationItemData(int id)
    {
        if (!_dicConstellationItemData.ContainsKey(id))
        {
            return null;
        }

        return _dicConstellationItemData[id];
    }

    public void ConstellationReactivation(int id)
    {
        double beforeBattlePower = AllBattlePower.Value;
#if USE_SERVER
        if (_dicConstellationItemData[id]._isOpen)
            Managers.Instance.GetServerManager()
                .OnPostConstellationRerollGrade(_dicConstellationItemData[id].data.BoardID, id);
        else
            Managers.Instance.GetServerManager()
                .OnPostConstellationOpenNode(_dicConstellationItemData[id].data.BoardID, id);
#else
        if (!_dicConstellationItemData[id]._isOpen)
        {
            _dicConstellationItemData[id]._isOpen = true;       // 해금
        
            // Guide Quest Check 
            if (Managers.Instance.UserInfo().CheckCorrectCurrentGuideQuest(EQuestConditionType.ConstellationOpen, 1))
            Managers.Instance.UserInfo().UpdateProgressValue(1);    
        }

        EConstellationGrade grade = EConstellationGrade.Normal;

        // 등급 셋팅 
        if (_dicConstellationItemData[id].data.StarSize == EConstellationSize.Small)
        {
            grade = ReturnConstellationGrade();

            _dicConstellationItemData[id]._grade = grade;

            // 중대형 노드 등급 재설정 코드
            if (_dicConstellationItemData[_dicConstellationItemData[id].data.PreviousNode].data.StarSize != EConstellationSize.Small)
            {
                // 전체 등급 재설정
                foreach (var item in _dicConstellationItemData.Values)
                {
                    if (item.data.StarSize == EConstellationSize.Small) continue;
                    if(item.data.Rank.Contains(0)) continue;
                    EConstellationGrade g = 
                        _dicConstellationItemData.Values.Where(c => item.data.Rank.Contains(c._id))
                        .Min(c => c._grade);
                    item._grade = g;
                }
            }
        }
        else
        {
            grade = _dicConstellationItemData.Values
                    .Where(x => x.data.PreviousNode == id)
                    .Min(x => x._grade);

            _dicConstellationItemData[id]._grade = grade;
        }



        // Status
        _constellationStatus.Reset();
        foreach (var item in _dicConstellationItemData)
        {
            _constellationStatus.Plus(item.Value.data.StatusType, item.Value.ReturnStatusValue());
        }
        AllCharacterStatusCacluate();
        double currentBattlePower = AllBattlePower.Value;

        // Currency
        int value =
 ClientLocalDB_Simple.GetData<ConstellationPrice>(DBKey.ConstellationPrice, _dicConstellationItemData[id].data.StarSize).CostValue;
        SetCurrencyValue(ECurrency.StarPiece, GetCurrencyValue(ECurrency.StarPiece) - value);

        // UI Update
        UIManager.UIConstellation.OnChangeTab();
        UIManager.UIConstellation.Refresh();

        UIManager.ShowUIToast<UIBattlePowerToast>((currentBattlePower - beforeBattlePower).ToString() , "ChangeBattlePowerToastMessage");

#endif
    }

    private EConstellationGrade ReturnConstellationGrade()
    {
        EConstellationGrade grade = EConstellationGrade.Normal;

        float r = Random.Range(0f, 100f);
        foreach (var g in ClientLocalDB_Simple.GetDB<ConstellationRankPercent>(DBKey.ConstellationRankPercent))
        {
            if (r < g.Value.Percent / 100f)
            {
                grade = g.Value.StarRank;
                break;
            }

            r -= g.Value.Percent / 100f;
        }

        return grade;
    }

    public void CalculateConstellationStatus()
    {
        _constellationStatus.Reset();
        foreach (var item in _dicConstellationItemData)
        {
            _constellationStatus.Plus(item.Value.data.StatusType, item.Value.ReturnStatusValue());
        }

        AllCharacterStatusCacluate();
    }

    #endregion

    #region Emblem

    public void SetEquipEmblem(EContent content, EmblemItemData data)
    {
        _dicEquipEmblemitemData[content] = data;
    }

    public void SetEmblem(EContent content, int id)
    {
        EmblemItemData data = null;
        if (_dicEmblemItemData.TryGetValue(id, out data))
        {
            data = _dicEmblemItemData[id];
            SetEquipEmblem(content, data);
        }
    }

    public void SetEmblemItemData(EmblemDto[] emblemDtos)
    {
        _dicEmblemItemData.Clear();
        for (int i = 0; i < emblemDtos.Length; i++)
        {
            AddEmblemItemData(emblemDtos[i]);
        }
    }

    public void SetEmblemItemData(EmblemDto emblemDto, EContent content)
    {
        EmblemItemData data = null;
        // if (_dicEmblemItemData.TryGetValue(emblemDto.id, out data))
        // {
        //     data = _dicEmblemItemData[emblemDto.id];
        //     data.id = emblemDto.id;
        //     data.grade = (EGradeType)emblemDto.grade;
        //     data.isLock = emblemDto.islock;
        //     for (int i = 0; i < emblemDto.synergyList.Count; i++)
        //     {
        //         data.emblemOptions[i].emblemOption = emblemDto.synergyList[i];
        //         data.emblemOptions[i].SynergyCategory =
        //             GetDeckSynergyTypeToDeckSynergyCategory(data.emblemOptions[i].emblemOption);
        //     }
        // }
    }

    public void AddEmblemItemData(EmblemDto[] emblemDtos)
    {
        for (int i = 0; i < emblemDtos.Length; i++)
        {
            AddEmblemItemData(emblemDtos[i]);
        }
    }

    public void AddEmblemItemData(EmblemDto emblemDto)
    {
        // EmblemItemData item = new EmblemItemData();
        //
        // item.id = emblemDto.id;
        // item.grade = (EGradeType)emblemDto.grade;
        // item.emblemOptions = new EmblemItemOption[3];
        // item.isLock = emblemDto.islock;
        // for (int i = 0; i < emblemDto.synergyList.Count; i++)
        // {
        //     item.emblemOptions[i] = new EmblemItemOption();
        //     item.emblemOptions[i].emblemOption = emblemDto.synergyList[i];
        //     item.emblemOptions[i].SynergyCategory =
        //         GetDeckSynergyTypeToDeckSynergyCategory(item.emblemOptions[i].emblemOption);
        // }
        //
        // _dicEmblemItemData[item.id] = item;
    }

    public EmblemItemData GetEquipEmblem(EContent content)
    {
        EmblemItemData data;
        if (_dicEquipEmblemitemData.TryGetValue(content, out data))
            return data;
        return null;
    }

    public EmblemItemData GetEmblemItemData(int id)
    {
        EmblemItemData data;
        if (_dicEmblemItemData.TryGetValue(id, out data))
            return data;
        return null;
    }

    //private myEmblemData CreateEmblem(int id, EGradeType grade, int optionCount)
    //{
    //    var emblemDto = new EmblemDto
    //    {
    //        id = id,
    //        grade = (int)grade,
    //        cartegoty = new int[optionCount],
    //        options = new string[optionCount]
    //    };

    //    for (int i = 0; i < optionCount; i++)
    //    {
    //        // 카테고리 랜덤 선택
    //        var category = (EDeckSynergeyCategory)UnityEngine.Random.Range((int)EDeckSynergeyCategory.Faction,(int)EDeckSynergeyCategory.JobType + 1);

    //        emblemDto.cartegoty[i] = (int)category;

    //        // 카테고리에 맞는 옵션 랜덤 선택
    //        var options = DeckSynergyCategoryOptions[category];
    //        emblemDto.options[i] = options[UnityEngine.Random.Range(0, options.Count)];
    //    }

    //    return emblemDto;
    //} 

    //public void TestSetEmblemItemData()
    //{
    //    AddEmblemItemData(CreateEmblem(1, EGradeType.Rare, 1));
    //    AddEmblemItemData(CreateEmblem(2, EGradeType.Epic, 2));
    //    AddEmblemItemData(CreateEmblem(3, EGradeType.Legendary, 3));
    //}

    #endregion

    #region Shop

    public void LoadShopItemData()
    {
        _dicShopItemData.Clear();

        // Package → PackageShopItemData
        var packageDB = ClientLocalDB_Simple.GetDB<PackageShop>(DBKey.PackageShop);
        if (packageDB != null)
        {
            var dic = new Dictionary<int, ShopItemData>();
            foreach (var data in packageDB.Values)
                dic[data.ID] = new PackageShopItemData { id = data.ID, _type = EShopType.PackageShop };
            _dicShopItemData[EShopType.PackageShop] = dic;
        }

        // Month → MonthShopItemData
        var monthDB = ClientLocalDB_Simple.GetDB<MonthShop>(DBKey.MonthShop);
        if (monthDB != null)
        {
            var dic = new Dictionary<int, ShopItemData>();
            foreach (var data in monthDB.Values)
                dic[data.ID] = new MonthShopItemData { id = data.ID, _type = EShopType.MonthlyShop };
            _dicShopItemData[EShopType.MonthlyShop] = dic;
        }

        // Cash → ShopItemData
        var cashDB = ClientLocalDB_Simple.GetDB<CashShop>(DBKey.CashShop);
        if (cashDB != null)
        {
            var dic = new Dictionary<int, ShopItemData>();
            foreach (var data in cashDB.Values)
                dic[data.ID] = new ShopItemData { id = data.ID, _type = EShopType.CashShop };
            _dicShopItemData[EShopType.CashShop] = dic;
        }

        // MidCash → ShopItemData
        var midCashDB = ClientLocalDB_Simple.GetDB<MidCashShop>(DBKey.MidCashShop);
        if (midCashDB != null)
        {
            var dic = new Dictionary<int, ShopItemData>();
            foreach (var data in midCashDB.Values)
                dic[data.ID] = new MidCashShopItemData { id = data.ID, _type = EShopType.MidCashShop };
            _dicShopItemData[EShopType.MidCashShop] = dic;
        }

        // Ticket → ShopItemData
        var ticketDB = ClientLocalDB_Simple.GetDB<TicketShop>(DBKey.TicketShop);
        if (ticketDB != null)
        {
            var dic = new Dictionary<int, ShopItemData>();
            foreach (var data in ticketDB.Values)
                dic[data.ID] = new ShopItemData { id = data.ID, _type = EShopType.TicketShop };
            _dicShopItemData[EShopType.TicketShop] = dic;
        }

        // Guild → ShopItemData
        var guildDB = ClientLocalDB_Simple.GetDB<GuildShop>(DBKey.GuildShop);
        if (guildDB != null)
        {
            var dic = new Dictionary<int, ShopItemData>();
            foreach (var data in guildDB.Values)
                dic[data.ID] = new CurrencyShopItemData { id = data.ID, _type = EShopType.GuildShop };
            _dicShopItemData[EShopType.GuildShop] = dic;
        }

        // Gold → ShopItemData
        var goldDB = ClientLocalDB_Simple.GetDB<GoldShop>(DBKey.GoldShop);
        if (goldDB != null)
        {
            var dic = new Dictionary<int, ShopItemData>();
            foreach (var data in goldDB.Values)
                dic[data.ID] = new CurrencyShopItemData { id = data.ID, _type = EShopType.GoldShop };
            _dicShopItemData[EShopType.GoldShop] = dic;
        }

        // HeroPiece → ShopItemData
        var heroPieceDB = ClientLocalDB_Simple.GetDB<HeroPieceShop>(DBKey.HeroPieceShop);
        if (heroPieceDB != null)
        {
            var dic = new Dictionary<int, ShopItemData>();
            foreach (var data in heroPieceDB.Values)
                dic[data.ID] = new CurrencyShopItemData { id = data.ID, _type = EShopType.HeroPieceShop };
            _dicShopItemData[EShopType.HeroPieceShop] = dic;
        }

        var limitedShopDB = ClientLocalDB_Simple.GetDB<LimitedShop>(DBKey.LimitedShop);
        if (limitedShopDB != null)
        {
            var dic = new Dictionary<int, ShopItemData>();
            foreach (var data in limitedShopDB.Values)
                dic[data.ID] = new LimitShopItemData { id = data.ID, _type = EShopType.LimitShop };
            _dicShopItemData[EShopType.LimitShop] = dic;
        }
    }

    public void SetShopItemData(EShopType shopType, ShopDto[] shopDto)
    {
        if (shopDto == null)
            return;

        // 서버는 package에서 묶어서 내려오기 떄문에 limit 는 받기 전에 초기화 진행 필요
        if (shopType == EShopType.LimitShop)
        {
            _dicShopItemData[EShopType.LimitShop].Clear();
            _currentLimitShopDataID = 0;
        }

        for (int i = 0; i < shopDto.Length; i++)
        {
            ShopDto shop = shopDto[i];
            ShopItemData item = null;
            switch (shopType)
            {
                case EShopType.PackageShop:
                    if(ClientLocalDB_Simple.GetDB<PackageShop>(DBKey.PackageShop).ContainsKey(shop.goodsId.ToString()))
                        item = new PackageShopItemData() { id = shop.goodsId, _type = EShopType.PackageShop, count = shop.count };
                    else
                    {
                        // limitShop일 때 (서버에서는 합쳐서 내려옴)
                        item = new LimitShopItemData() { id = shop.goodsId, _type = EShopType.LimitShop , count = shop.count};
                        _currentLimitShopDataID = shop.goodsId;
                        _dicShopItemData[EShopType.LimitShop][shop.goodsId] = item;
                        continue;
                    }
                    break;
                case EShopType.MonthlyShop:
                    item = new MonthShopItemData() { id = shop.goodsId, _type = EShopType.MonthlyShop, day = shop.count };
                    break;
                case EShopType.CashShop:
                    item = new MidCashShopItemData() { id = shop.goodsId, _type = EShopType.CashShop, _isfirstBuy = shop.firstBuy };
                    break;
                case EShopType.MidCashShop:
                    item = new MidCashShopItemData() { id = shop.goodsId, _type = EShopType.MidCashShop, _isfirstBuy = shop.firstBuy };
                    break;
                case EShopType.TicketShop:
                    item = new ShopItemData() { id = shop.goodsId, _type = EShopType.TicketShop };
                    break;
                case EShopType.GuildShop:
                    item = new CurrencyShopItemData() { id = shop.goodsId, _type = EShopType.GuildShop, count = shop.count };
                    break;
                case EShopType.GoldShop:
                    item = new CurrencyShopItemData() { id = shop.goodsId, _type = EShopType.GoldShop, count = shop.count };
                    break;
                case EShopType.HeroPieceShop:
                    item = new CurrencyShopItemData() { id = shop.goodsId, _type = EShopType.HeroPieceShop, count = shop.count };
                    break;
                default:
                    break;

            }
            _dicShopItemData[shopType][shop.goodsId] = item;

        }
    }

    public void SetPackageScheduleData(PackageSchedulerDto[] packageSchedulerDtos)
    {
        for (int i = 0; i < packageSchedulerDtos.Length; i++)
        {
            if (_dicShopItemData.ContainsKey(EShopType.LimitShop) &&
                _dicShopItemData[EShopType.LimitShop].ContainsKey(packageSchedulerDtos[i].id))
            {
                (_dicShopItemData[EShopType.LimitShop][packageSchedulerDtos[i].id] as LimitShopItemData).endTime
                                = DateTime.Parse(packageSchedulerDtos[i].endTime);
            }
            else
                (_dicShopItemData[EShopType.PackageShop][packageSchedulerDtos[i].id] as PackageShopItemData).endTime
                    = DateTime.Parse(packageSchedulerDtos[i].endTime);
        }
    }



    #endregion

    #region Guild

    //길드 유저 정보 : 직위 등
    public GuildUserInfoDto guildUserInfo = new GuildUserInfoDto();

    //길드 정보 : 공지, 가이드, 멤버 수, 세팅
    public GuildInfoDto guildInfo = new GuildInfoDto();

    //가입 신청한 길드
    public int[] guildRequestList;

    //길드 가입 여부
    public bool ExistGuild => guildUserInfo != null && guildUserInfo.guildId != 0;

    public bool IsGuildMemberMax => guildInfo.joinNum == guildInfo.limNum;

    //길드 보스 정보
    public GuildBossDto currentGuildBossDto;


    public void GuildMissionMoveAction(EQuestConditionType conditionType, int id)
    {
        switch (conditionType)
        {
            case EQuestConditionType.DungeonClearTarget:
                UIManager.UIGuildHome.OpenToStack();
                break;
            case EQuestConditionType.DungeonClearAll:
                UIManager.UIGuildHome.ClickCloseBtn();
                UIManager.UIDungeonEntranceList.OpenToStack();
                break;
            case EQuestConditionType.GuildAttend:
                UIManager.UIGuildHome.OpenToStack();
                break;
            case EQuestConditionType.ShopPurchase:
                UIManager.UIGuildHome.ClickCloseBtn();
                UIManager.UIShop.SetCashShopOpenToStack(false);
                break;
        }
    }

    #endregion

    #region TreasureBox

    public void SetTreasureBoxItemData(TreasureBoxData[] treasureBoxDatas)
    {
        _treasureBoxList = treasureBoxDatas;
    }

    public TreasureBoxData GetTreasureBoxItemData()
    {
        if (_treasureBoxList == null || _treasureBoxList.Length == 0)
            return null;

        foreach (var treasureBoxData in _treasureBoxList)
        {
            if (!treasureBoxData.received)
                return treasureBoxData;
        }

        return null;
    }

    public TreasureBoxData GetTreasureBoxItemData(int id)
    {
        return _treasureBoxList[id];
    }

    #endregion

    #region Event

    public EventRewardItemData GetAttendanceData(EAttendanceEventType type, int id)
    {
        switch (type)
        {
            case EAttendanceEventType.Weekly:
                return _dicWeeklyAttendanceRewardItemData[id];
            case EAttendanceEventType.Monthly:
                return _dicMonthlyAttendanceRewardItemData[id];
            case EAttendanceEventType.New:
                return _dicNewAttendanceItemData[id];
        }

        return null;
    }

    public void SetMyOpenQuestInfoDto(MyNewbQuestInfoDto myNewbQuestInfo)
    {
        _openEventCompleted = myNewbQuestInfo.completed;

        _openEventDayCompleted[0] = myNewbQuestInfo._1stDayCompleted;
        _openEventDayCompleted[1] = myNewbQuestInfo._2ndDayCompleted;
        _openEventDayCompleted[2] = myNewbQuestInfo._3rdDayCompleted;
        _openEventDayCompleted[3] = myNewbQuestInfo._4thDayCompleted;
        _openEventDayCompleted[4] = myNewbQuestInfo._5thDayCompleted;
        _openEventDayCompleted[5] = myNewbQuestInfo._6thDayCompleted;
        _openEventDayCompleted[6] = myNewbQuestInfo._7thDayCompleted;

        _openEventStartDate = DateTime.Parse(myNewbQuestInfo.startDate);
    }

    public void SetOpenQuestListInfoDto(NewbQuestListInfoDto[] newbQuestListInfoDto)
    {
        foreach (NewbQuestListInfoDto dto in newbQuestListInfoDto)
        {
            if (!_dicOpenEventQuestItemData.ContainsKey(dto.date))
                continue;

            var itemDataList = _dicOpenEventQuestItemData[dto.date].Values;
            var itemData = itemDataList.FirstOrDefault(x => x._tableID == dto.questId);
            if (itemData == null)
                continue;

            itemData.isFinish = dto.receivedReward;
            // 완료 시에는 항상 slider max  (서버와 싱크 맞출 수 X) 
            itemData._progressValue = dto.receivedReward ? dto.count : dto.currentCount;  
            

        }

    }

    #endregion

    #region AD
    public void SetADInfo(myADInfo myADInfo)
    {
        _myADInfo = myADInfo;
    }
    #endregion

    #region Field
    public void SetMyFieldDiffStatus(myFieldDiffStatus myFieldDiffStatus)
    {
        _fieldId = myFieldDiffStatus.currentMapId;
        _previousFieldID = myFieldDiffStatus.previousMapId;
    }

    public void SetFieldItemData(myFieldDiffInfo[] myFieldDiffInfos)
    {
        foreach (var item in myFieldDiffInfos)
        {
            if (!_dicFieldItemData.ContainsKey(item.fieldId))
                continue;
            
            FieldItemData fieldItemData = _dicFieldItemData[item.fieldId];

            fieldItemData.isOpen = item.open;
            fieldItemData.progress = item.progress;
            fieldItemData.isGet = item.isGet;
            fieldItemData.isFirstClearRewardGet = item.isFirstClearRewardGet;

            if (fieldItemData.IsFirstFieldID)
                continue;
            
            // 서버에서는 all clear 시 11로 셋팅
            fieldItemData.difficultyLevel = Mathf.Min(item.difficultyLevel,10);
            fieldItemData.currentDifficultyLevel = item.currentDifficultyLevel;
            fieldItemData.dungeonQuestClearCount = item.dungeonQuestClearCount;
            fieldItemData.isQuestClearable = item.clearable;
 
            fieldItemData.RefreshCurrentQuestId();
            fieldItemData.questProgressValue = item.clearable ? fieldItemData.GetCurrentQuest().ConditionValue.Last() : 0;
        }

        // dummy setting (초원은 무조건 해금)
/*        _dicFieldItemData[1].isOpen = true;
        _dicFieldItemData[2].isOpen = true;
        _dicFieldItemData[2].difficultyLevel = 11;
        _dicFieldItemData[2].currentDifficultyLevel = 10;*/
    }


    public FieldItemData GetFieldItemData(int fieldId)
    {
        return _dicFieldItemData.GetValueOrDefault(fieldId);
    }
    
    #endregion

    #region Relic

    private void LoadRelicData()
    {
        if (_dicRelicItemData.Count != 0)
            return;

        Dictionary<string, RelicBase> dicRelicBase = ClientLocalDB_Simple.GetDB<RelicBase>(DBKey.RelicBase);
        foreach (var relicBase in dicRelicBase)
        {
            int relicId = int.Parse(relicBase.Key);
            RelicItemData relicItemData = new RelicItemData();
            relicItemData._level = 1;
            relicItemData._baseId = relicId;
            _dicRelicItemData.Add(relicId, relicItemData);
            
            Dictionary<ERelicPartsType, List<RelicPartsItemData>> dicParts = new Dictionary<ERelicPartsType, List<RelicPartsItemData>>();
            _dicRelicPartsItemData.Add(relicId, dicParts);
            
            for (int i = 0; i <= (int)ERelicPartsType.NORTH; i++)
            {
                _dicRelicPartsItemData[relicId].Add((ERelicPartsType)i, new List<RelicPartsItemData>());
            }
            
            _relicStatus.Add(relicId, new Status());
        }
    }

    public void RefreshRelicStatus(int relicId)
    {
        Status relicStatus = _relicStatus[relicId];
        relicStatus.Reset();
        RelicItemData relicItemData = _dicRelicItemData[relicId];
        relicStatus += relicItemData.GetMainStatus();

        for (int i = 0; i <= (int)ERelicPartsType.NORTH; i++)
        {
            ERelicPartsType partType = (ERelicPartsType)i;
            int partsId = relicItemData.GetPartsId(partType);
            
            if(partsId == 0)
                continue;
            
            RelicPartsItemData relicPartsItemData = GetRelicPartsItemData(relicId, partType, partsId);
            relicStatus += relicPartsItemData.GetMainPartsStatus();

            List<Status> subStatuses = relicPartsItemData.GetSubPartsStatusList();
            foreach (var subStatus in subStatuses)
            {
                relicStatus += subStatus;
            }
        }
        
        _relicStatus[relicId] = relicStatus;

        if (relicItemData._equipHeroId != 0)
        {
            CharacterClassItemData characterClassItemData = GetCharacterItemData(relicItemData._equipHeroId);
            characterClassItemData.equipRelicId = relicId;
            characterClassItemData.RefreshStatus();
        }
    }
    
    public void SetRelicPartListDto(RelicPartsGroup[] relicPartsGroups)
    {
        foreach (var partsGroup in relicPartsGroups)
        {
            foreach (var parts in partsGroup.parts)
            {
                foreach (var part in parts.Value)
                {
                    AddRelicPartsItemData(part);
                }
            }
        }
    }
    
    //일괄분해는 리스트 다시 세팅.
    public void RefreshRelicPartListDto(RelicPartsGroup[] relicPartsGroups)
    {
        foreach (var partsGroup in relicPartsGroups)
        {
            foreach (var parts in partsGroup.parts)
            {
                _dicRelicPartsItemData[partsGroup.relicId][parts.Key].Clear();
                foreach (var part in parts.Value)
                {
                    AddRelicPartsItemData(part);
                }
            }
        }
    }

    public void SetRelicInfoDto(RelicInfoDto[] relicInfoDto)
    {
        foreach (var relicInfo in relicInfoDto)
        {
            RelicItemData relicItemData = _dicRelicItemData[relicInfo.relicId];
            relicItemData._level = relicInfo.level;
            
            for (int i = 0; i <= (int)ERelicPartsType.NORTH; i++)
            {
                ERelicPartsType relicPartsType = (ERelicPartsType)i;
                relicItemData.SetEquipParts(relicPartsType, relicInfo.GetPartsId(relicPartsType));
            }
            
            int beforeHeroId = relicItemData._equipHeroId;
            relicItemData._equipHeroId = relicInfo.equippedHeroTableId;
            if (beforeHeroId != 0)
                UnEquipRelicBase(beforeHeroId);
            
            RefreshRelicStatus(relicInfo.relicId);
        }
    }

    public void UnEquipRelicBase(int heroTableId)
    {
        CharacterClassItemData itemData = GetCharacterItemData(heroTableId);
        if (itemData != null)
        {
            itemData.equipRelicId = 0;
            itemData.RefreshStatus();
        }
    }

    public bool EnableRelicPartsEquip(int baseId, ERelicPartsType relicPartsType)
    {
        return _dicRelicPartsItemData[baseId][relicPartsType].Count > 0;
    }
    
    public RelicPartsItemData GetRelicPartsItemData(int baseId, ERelicPartsType relicPartsType, int id)
    {
        foreach (var relicPartsItemData in _dicRelicPartsItemData[baseId][relicPartsType])
        {
            if (relicPartsItemData._id == id)
                return relicPartsItemData;
        }
        
        return null;
    }
    
    public List<RelicPartsItemData> GetRelicPartsItemList(int baseId, ERelicPartsType relicPartsType)
    {
        return _dicRelicPartsItemData[baseId][relicPartsType];
    }

    public RelicItemData GetRelicItemData(int relicId)
    {
        return _dicRelicItemData[relicId];
    }
    
    public RelicPartsItemData AddRelicPartsItemData(RelicPartsDto relicPartsDto)
    {
        int id = relicPartsDto.id;
        int relicId = relicPartsDto.relicId;
        ERelicPartsType partType = relicPartsDto.relicPartsDirection;
        RelicPartsItemData relicPartItemData = _dicRelicPartsItemData[relicId][partType].Find(row => row._id == id);
        if (relicPartItemData == null)
        {
            relicPartItemData = new RelicPartsItemData();
            _dicRelicPartsItemData[relicId][partType].Add(relicPartItemData);
        }

        RelicParts relicParts = ClientLocalDB_Simple.GetData<RelicParts>(DBKey.RelicParts,
            $"{relicId}_{(int)partType}");
        relicPartItemData._id = id;
        relicPartItemData._relicBaseId = relicId;
        relicPartItemData._relicPartsId = relicParts.RelicPartsId;
        relicPartItemData._grade = (EOptionGradeType)relicPartsDto.grade;
        relicPartItemData._partsType = partType;
        relicPartItemData._isLock = relicPartsDto.isLocked;
        for (int i = 0; i < MaxRelicPartsOptionCount; i++)
        {
            relicPartItemData._relicPartsOptions[i] = relicPartsDto.GetRelicPartsOption(i);
        }
        
        return relicPartItemData;
    }

    public void RemoveRelicPartsItemData(int id)
    {
        foreach (var partsGroup in _dicRelicPartsItemData.Values)
        {
            foreach (var parts in partsGroup.Values)
            {
                for (int i = parts.Count - 1; i >= 0; i--)
                {
                    RelicPartsItemData relicPartsItemData = parts[i];
                    if (relicPartsItemData._id == id)
                    {
                        parts.Remove(relicPartsItemData);
                        return;
                    }
                }
            }
        }
    }
    
    public bool IsEquipCheck(int heroTableId)
    {
        foreach (var relicItem in _dicRelicItemData.Values)
        {
            if(relicItem._equipHeroId == heroTableId)
                return true;
        }
        
        return false;
    }
    
    public bool EnableRelicPartsDisMiss(int baseId, ERelicPartsType relicPartsType)
    {
        int equipId = _dicRelicItemData[baseId].GetPartsId(relicPartsType);
        foreach (var partsItemData in _dicRelicPartsItemData[baseId][relicPartsType])
        {
            if(partsItemData._id != equipId && !partsItemData._isLock)
                return true;
        }
        
        return false;
    }
    
    public bool EnableRelicPartsCraft(int baseId)
    {
        foreach (var partsList in _dicRelicPartsItemData[baseId].Values)
        {
            if(partsList.Count >= MaxRelicPartsCount)
                return false;
        }
        
        return true;
    }

    #endregion

    #region Setting
    public  void InitSetting()
    {
        _isToggleOnDic[ToggleSettingType.BGM] = PlayerPrefs.GetInt(ToggleSettingType.BGM.ToString(), 1) == 1;
        _isToggleOnDic[ToggleSettingType.SFX] = PlayerPrefs.GetInt(ToggleSettingType.SFX.ToString(), 1) == 1;
        _isToggleOnDic[ToggleSettingType.Joystick] = PlayerPrefs.GetInt(ToggleSettingType.Joystick.ToString(), 0) == 1;
        _isToggleOnDic[ToggleSettingType.Damage] = PlayerPrefs.GetInt(ToggleSettingType.Damage.ToString(), 1) == 1;
        _isToggleOnDic[ToggleSettingType.Chatting] = PlayerPrefs.GetInt(ToggleSettingType.Chatting.ToString(), 1) == 1;
        _isToggleOnDic[ToggleSettingType.Economy] = PlayerPrefs.GetInt(ToggleSettingType.Economy.ToString(), 1) == 1;


        // 초기 실행
        foreach (ToggleSettingType e in Enum.GetValues(typeof(ToggleSettingType)))
        {
            SettingToggleEvent(e);
        }
    }

    public void SettingToggleEvent(ToggleSettingType key)
    {
        switch (key)
        {
            case ToggleSettingType.BGM:
                Managers.Instance.Sound.ToggleBGMMute(!_isToggleOnDic[key]);
                break;
            case ToggleSettingType.SFX:
                Managers.Instance.Sound.ToggleSFXMute(!_isToggleOnDic[key]);
                break;
/*            case ToggleSettingType.Vibration:
                if (VibrationManager.Instance != null)
                {
                    VibrationManager.Instance.isVibrationEnabled = toggleVal;

                    if (!toggleVal)
                    {
                        VibrationManager.Instance.Stop();
                    }
                    // 초기화 중이 아닐 때만(유저가 직접 눌렀을 때만) 진동 테스트
                    else if (!_isInitializing)
                    {
                        VibrationManager.Instance.Vibrate(VibrationType.Light);
                    }
                }
                break;*/
            case ToggleSettingType.Joystick:
                Managers.Instance.GetJoystick().joystickType = _isToggleOnDic[key] ? Define.EJoystickType.Fixed : Define.EJoystickType.Flexible;
                break;
            case ToggleSettingType.Damage:
                _isDamageOn = _isToggleOnDic[key];
                break;
            case ToggleSettingType.Chatting:
                OnChattingToggleChanged?.Invoke(_isToggleOnDic[key]);
                break;
            case ToggleSettingType.Economy:
                _isEconomyOn = _isToggleOnDic[key];
                break;
            default:
                break;
        }
    }
    #endregion

    public void SetADBuff(string endTimeString)
    {
        DateTime endTime = DateTime.Parse(endTimeString);
        DateTime now = ServerTime.Instance.CurrentTime();
        TimeSpan durationTimeSpan = endTime - now;

        _adBuffTimeData.SetByDuration(durationTimeSpan.TotalSeconds);
    }

    public void InitializationDay()
    {
        UIManager.ShowCommonToastMessage("유저 데이터를 초기화 중입니다.");
        BestHttp_GameManager server = Managers.Instance.GetServerManager();
        server.OnGetQuestMyInfo();
        server.OnGetPassInfo(() => UIManager.MainInfoUI.Refresh());
        server.OnGetShopInfo();
        server.OnPostRequestMyMail(() => UIManager.MainInfoUI.Refresh());
        server.OnGetMyDungeonInfo();
        server.OnGetMyADInfo();
        server.OnGetMyCurrency();
        server.OnGetAttendanceInfo();
    }
}