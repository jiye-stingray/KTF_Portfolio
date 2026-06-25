using Assets.SimpleSignIn.Google.Scripts;
using CHAT;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static Define;
using Object = UnityEngine.Object;

public class UIManager
{
    private Stack<Dictionary<string, UIBase>> _uiStack = new Stack<Dictionary<string, UIBase>>();
    public UIBase _currentUI;

    private Stack<UIPopupBase> _popupStack = new Stack<UIPopupBase>();
    private UIPopupBase CurrentPopup => EnablePopupStack ? _popupStack.Peek() : null;
    private bool EnablePopupStack => _popupStack != null && _popupStack.Count > 0;
    private UserInfoData UserInfoData => Managers.Instance.UserInfo();

    private Transform mainCanvas;
    public Transform MainCanvas
    {
        get
        {
            if (mainCanvas == null)
            {
                GameObject canvasObj = GameObject.Find("MainCanvas");
                if (canvasObj == null)
                {
                    MyLogger.LogWarning("[UIManager] UICanvas not found in scene.");
                    return null; // 예외 대신 null 반환
                }

                mainCanvas = canvasObj.transform;
            }
            return mainCanvas;
        }
    }

    private Transform uiCanvas;

    public Transform UICanvas
    {
        get
        {
            if (uiCanvas == null)
            {
                GameObject canvasObj = GameObject.Find("UICanvas");
                if (canvasObj == null)
                {
                    MyLogger.LogWarning("[UIManager] UICanvas not found in scene.");
                    return null; // 예외 대신 null 반환
                }

                uiCanvas = canvasObj.transform;
            }
            return uiCanvas;
        }
    }


    Transform speechCanvas;
    public Transform SpeechCanvas
    {
        get
        {
            if (speechCanvas == null)
                speechCanvas = GameObject.Find("SpeechCanvas").transform;
            return speechCanvas;
        }

    }

    Transform loadingCanvas;
    public Transform LoadingCanvas
    {
        get
        {
            if (loadingCanvas == null)
            {
                GameObject go = GameObject.Find("LoadingCanvas");
                if(go != null)
                    loadingCanvas = go.transform;

            }
            return loadingCanvas;
        }
    }

    private Transform weatherEffectRoot;
    public Transform WeatherEffectRoot
    {
        get
        {
            if (weatherEffectRoot == null)
            {
                GameObject go = GameObject.Find("WeatherEffectRoot");
                if(go != null)
                    weatherEffectRoot = go.transform;

            }
            return weatherEffectRoot;
        }
    }

    private GameObject uiJoystickTrans;
    public GameObject UIJoyStickTrans
    {
        get
        {
            if (uiJoystickTrans == null)
                uiJoystickTrans = GameObject.Find("UIJoystickTrans");
            return uiJoystickTrans;
        }
    }

    private UIJoystick _joystickUI;
    public UIJoystick JoystickUI
    {
        get
        {
            if (_joystickUI == null)
            {
                _joystickUI = GameObject.FindObjectOfType<UIJoystick>();
            }
            return _joystickUI;
        }
    }

    public MainInfoUI _mainInfoUI;

    public MainInfoUI MainInfoUI
    {
        get
        {
            if(_mainInfoUI == null)
            {
                _mainInfoUI = ShowUIBase<MainInfoUI>("MainInfoUI");
            }
            return _mainInfoUI;
        }
    }
    
    private TopCurrencyUI _topCurrencyUI;

    public TopCurrencyUI TopCurrencyUI
    {
        get
        {
            if(_topCurrencyUI == null)
            {
                _topCurrencyUI = ShowUIBase<TopCurrencyUI>("TopCurrencyUI");
            }
            return _topCurrencyUI;
        }
    }

    private UIInventory _uiInventory;

    public UIInventory UIInventory
    {
        get
        {
            if(_uiInventory == null)
            {
                _uiInventory = ShowUIBase<UIInventory>("UIInventory");
                _uiInventory.Close();
            }
            return _uiInventory;
        }
    }


    private TrainingCenterUI _trainingCenterUI;
    public TrainingCenterUI TrainingUI
    {
        get
        {
            if(_trainingCenterUI == null)
            {
                _trainingCenterUI = ShowUIBase<TrainingCenterUI>("UITrainingCenter");
                _trainingCenterUI.Close();
            }
            return _trainingCenterUI;
        }
    }


    private UICharacterInventory _uicharacterInventory;
    public UICharacterInventory UICharacterInventory
    {
        get
        {
            if(_uicharacterInventory == null)
            {
                _uicharacterInventory = ShowUIBase<UICharacterInventory>("UICharacterInventory");
                _uicharacterInventory.Close();
            }
            return _uicharacterInventory;
        }
    }

    private UICharacterDetail _uICharacterDetail;
    public UICharacterDetail UICharacterDetail
    {
        get
        {
            if(_uICharacterDetail == null)
            {
                _uICharacterDetail = ShowUIBase<UICharacterDetail>("UICharacterDetail");
                _uICharacterDetail.Close();
            }
            return _uICharacterDetail;
        }
    }

    private UIDeckSetting _uIDeckSetting;
    public UIDeckSetting UIDeckSetting
    {
        get
        {
            if(_uIDeckSetting == null)
            {
                _uIDeckSetting = ShowUIBase<UIDeckSetting>("UIDeckSetting");
                _uIDeckSetting.Close();
            }
            return _uIDeckSetting;
        }
    }

    private UIEquipmentSetting _uiEquipmentSetting;
    public UIEquipmentSetting UIEquipmentSetting
    {
        get
        {
            if(_uiEquipmentSetting == null)
            {
                _uiEquipmentSetting = ShowUIBase<UIEquipmentSetting>("UIEquipmentSetting");
                _uiEquipmentSetting.Close();
            }
            return _uiEquipmentSetting;
        }
    }

    public UIGuideQuest UIGuideQuest
    {
        get
        {
            if(_mainInfoUI == null)
                return null;
                
            return _mainInfoUI?.GetGuideQuest();
        }
    }

    private UIDungeonEntranceList _uIDungeonEntranceList;
    public UIDungeonEntranceList UIDungeonEntranceList
    {
        get
        {
            if(_uIDungeonEntranceList == null)
            {
                _uIDungeonEntranceList = ShowUIBase<UIDungeonEntranceList>("UIDungeonEntranceList");
                _uIDungeonEntranceList.Close();
            }
            return _uIDungeonEntranceList;
        }
    }

    private UIDungeonEntrance _uIDungeonEnterance;
    public UIDungeonEntrance UIDungeonEntrance
    {
        get 
        { 
            if(_uIDungeonEnterance == null)
            {
                _uIDungeonEnterance = ShowUIBase<UIDungeonEntrance>("UIDungeonEntrance");
                _uIDungeonEnterance.Close();
            }
            return _uIDungeonEnterance;
        
        }

    }

    private UITowerDungeonEntranceList _uITowerDungeonEntranceList;
    public UITowerDungeonEntranceList UITowerDungeonEntranceList
    {
        get
        {
            if(_uITowerDungeonEntranceList == null)
            {
                _uITowerDungeonEntranceList = ShowUIBase<UITowerDungeonEntranceList>("UITowerDungeonEntranceList");
                _uITowerDungeonEntranceList.Close();
            }
            return _uITowerDungeonEntranceList;
        }
    }

    private UITowerDungeonEntrance _uITowerDungeonEntrance;
    public UITowerDungeonEntrance UITowerDungeonEntrance
    {
        get
        {
            if(_uITowerDungeonEntrance == null)
            {
                _uITowerDungeonEntrance = ShowUIBase<UITowerDungeonEntrance>("UITowerDungeonEntrance");
                _uITowerDungeonEntrance.Close();
            }
            return _uITowerDungeonEntrance;
        }
    }

    private UIRankingDungeonEntrance _uIRankingDungeonEntrance;
    public UIRankingDungeonEntrance UIRankingDungeonEntrance
    {
        get
        {
            if(_uIRankingDungeonEntrance == null)
            {
                _uIRankingDungeonEntrance = ShowUIBase<UIRankingDungeonEntrance>("UIRankingDungeonEntrance");
                _uIRankingDungeonEntrance.Close();
            }
            return _uIRankingDungeonEntrance;
        }
    }

    private UIAwakeDungeonEntrance _uIAwakeDungeonEntrance;
    public UIAwakeDungeonEntrance UIAwakeDungeonEntrance
    {
        get
        {
            if(_uIAwakeDungeonEntrance==null)
            {
                _uIAwakeDungeonEntrance = ShowUIBase<UIAwakeDungeonEntrance>("UIAwakeDungeonEntrance");
                _uIAwakeDungeonEntrance.Close();
            }
            return (_uIAwakeDungeonEntrance);
        }
    }

    private UIRoutineQuest _uIRoutineQuest;
    public UIRoutineQuest UIRoutineQuest
    {
        get
        {
            if(_uIRoutineQuest == null)
            {
                _uIRoutineQuest = ShowUIBase<UIRoutineQuest>("UIRoutineQuest");
                _uIRoutineQuest.Close();
            }
            return _uIRoutineQuest;
        }
    }

    private UIConstellation _uiConstellation;
    public UIConstellation UIConstellation
    {
        get
        {
            if(_uiConstellation==null)
            {
                _uiConstellation = ShowUIBase<UIConstellation>("UIConstellation");
                _uiConstellation.Close();
            }
            return _uiConstellation;
        }
    }

    private UIPassBanner _uiPassBanner;
    public UIPassBanner UIPassBanner
    {
        get
        {
            if(_uiPassBanner==null)
            {
                _uiPassBanner = ShowUIBase<UIPassBanner>("UIPassBanner");
                _uiPassBanner.Close();
            }
            return _uiPassBanner;
        }
    }

    private UIBattlePass _uIBattlePass;
    public UIBattlePass UIBattlePass
    {
        get
        {
            if(_uIBattlePass == null)
            {
                _uIBattlePass = ShowUIBase<UIBattlePass>("UIBattlePass");
                _uIBattlePass.Close();
            }
            return _uIBattlePass;
        }
    }

    private UIGacha _uiGacha;
    public UIGacha UIGacha
    {
        get
        {
            if(_uiGacha == null)
            {
                _uiGacha = ShowUIBase<UIGacha>("UIGacha");
                _uiGacha.Close();
            }
            return _uiGacha;
        }
    }

    private UIMinimap _uIMinimap;
    public UIMinimap UIMinimap
    {
        get
        {
            if(_uIMinimap == null)
            {
                _uIMinimap = ShowUIBase<UIMinimap>("UIMinimap");
                _uIMinimap.Close();
            }
            return _uIMinimap;
        }
    }

    private UIWarpPointPopup _uIWarpPointPopup;
    public UIWarpPointPopup UIWarpPointPopup
    {
        get
        {
            if (_uIWarpPointPopup == null)
            {
                _uIWarpPointPopup = ShowUIBase<UIWarpPointPopup>("UIWarpPointPopup");
            }
            return _uIWarpPointPopup;
        }
    }

    private UIStorageBuildingPopup _uIStorageBuildingPopup;
    public UIStorageBuildingPopup UIStorageBuildingPopup
    {
        get
        {
            if (_uIStorageBuildingPopup == null)
            {
                _uIStorageBuildingPopup = ShowUIBase<UIStorageBuildingPopup>("UIStorageBuildingPopup");
            }
            return _uIStorageBuildingPopup;
        }
    }

    private EconomyMainPopup _economyMainPopup;
    public EconomyMainPopup EconomyMainPopup
    {
        get
        {
            if (_economyMainPopup == null)
            {
                _economyMainPopup = ShowUIBase<EconomyMainPopup>("EconomyMainPopup");
            }
            return _economyMainPopup;
        }
    }
    
    public EconomyMainPopup GetEconomyMainPopup()
    {
        return _economyMainPopup;
    }

    private DailyRewardUI _dailyRewardPopup;
    public DailyRewardUI DailyRewardPopup
    {
        get
        {
            if (_dailyRewardPopup == null)
                _dailyRewardPopup = ShowUIBase<DailyRewardUI>("DailyRewardUI");
            return _dailyRewardPopup;
        }
    }

    private UITooltip _uiTooltip;
    public UITooltip UITooltip
    {
        get
        {
            if(_uiTooltip == null)
            {
                GameObject canvas = GameObject.Find("TooltipCanvas");
               if(canvas != null)
                    _uiTooltip = canvas.GetComponentInChildren<UITooltip>(true);
            }
            return _uiTooltip;
        }
        set
        {
            _uiTooltip = value;
        }
    }
    
    private UITreasureNavigation _uiTreasureNavigation;
    public UITreasureNavigation UITreasureNavigation
    {
        get
        {
            if (_uiTreasureNavigation == null)
                _uiTreasureNavigation = ShowUIBase<UITreasureNavigation>("UITreasureNavigation", SpeechCanvas);
            
            return _uiTreasureNavigation;
        }
    }
    
    private FishingUI _fishingUI;
    public FishingUI FishingUI
    {
        get
        {
            if (_fishingUI == null)
            {
                _fishingUI = ShowUIBase<FishingUI>("UIFishing");
                _fishingUI.Close();
            }
            return _fishingUI;
        }
    }
    
    private LuckySpinUI _spinUI;
    public LuckySpinUI SpinUI
    {
        get
        {
            if (_spinUI == null)
            {
                _spinUI = ShowUIBase<LuckySpinUI>("LuckySpinUI"); // Resources/UI/Popup/WheelMenu.prefab
            }
            return _spinUI;
        }
    }
    
    private UIEmergencyPopup _emergencyPopup;
    public UIEmergencyPopup EmergencyPopup
    {
        get
        {
            if (_emergencyPopup == null)
            {
                _emergencyPopup = ShowPopup<UIEmergencyPopup>("UIEmergencyPopup");
                // 긴급팝업은 항상 최상단
                _emergencyPopup.transform.SetAsLastSibling();
            }
            return _emergencyPopup;
        }
    }
    // // EmergencyPopup 호출
    // Managers.Instance.GetUIManager().EmergencyPopup.Show(
    //     "현재 긴급 점검 중입니다.\n잠시 후 다시 시도해 주세요.",
    //     true
    // );

    private PlayerProfileUI _profileUI;
    public PlayerProfileUI ProfileUI
    {
        get
        {
            if (_profileUI == null)
            {
                _profileUI = ShowUIBase<PlayerProfileUI>("PlayerProfileUI");
            }
            return _profileUI;
        }
    }

    private UIShop _uiShop;
    public UIShop UIShop
    {
        get
        {
            if( _uiShop == null)
            {
                _uiShop = ShowUIBase<UIShop>("UIShop");
                _uiShop.Close();
            }
            return _uiShop;
        }
    }

    #region 길드

    private GuildInfoListPage _guildInfoListPage;
    public GuildInfoListPage GuildInfoListPage
    {
        get
        {
            if (_guildInfoListPage == null)
            {
                _guildInfoListPage = ShowUIBase<GuildInfoListPage>("GuildUI/UIGuildList");
                _guildInfoListPage.Close();
            }
            return _guildInfoListPage;
        }
    }

    #endregion

    private UICraft _uICraft;
    public UICraft UICraft
    {
        get
        {
            if (_uICraft == null)
            {
                _uICraft = ShowUIBase<UICraft>("UICraft");
                _uICraft.Close();
            }
            return _uICraft;
        }
    }

    private UIGuildHome _uIGuildHome;
    public UIGuildHome UIGuildHome
    {
        get
        {
            if (_uIGuildHome == null)
            {
                _uIGuildHome = ShowUIBase<UIGuildHome>("GuildUI/UIGuildHome");
                _uIGuildHome.Close();
            }
            return _uIGuildHome;
        }
    }

    // private SettingUI _settingUI;
    // public SettingUI SettingUI
    // {
    //     get
    //     {
    //         if (_settingUI == null)
    //         {
    //             _settingUI = ShowUIBase<SettingUI>("SettingUI");
    //         }
    //         return _settingUI;
    //     }
    // }

    private SettingUI _settingUIPopup;
    public SettingUI SettingUIPopup
    {
        get
        {
            if (_settingUIPopup == null)
            {
                _settingUIPopup = ShowPopup<SettingUI>("UISettingPopup");
            }
            return _settingUIPopup;
        }
        set
        {
            _settingUIPopup = value;
        }
    }
    
    private UIResurrectionPopup _uiResurrectionPopup;
    public UIResurrectionPopup UIResurrectionPopup
    {
        get
        {
            if (_uiResurrectionPopup == null)
            {
                _uiResurrectionPopup = ShowUIBase<UIResurrectionPopup>("UIResurrectionPopup");
            }
            return _uiResurrectionPopup;
        }
    }

    private UIMail _uIMail;
    public UIMail UIMail
    {
        get
        {
            if (_uIMail == null)
            {
                _uIMail = ShowUIBase<UIMail>("UIMail");
                _uIMail.Close();
            }
            return _uIMail;
        }
    }
    
    #if CHAT
    private ChatMainUI _chattingMainUI;
    public ChatMainUI ChattingMainUI
    {
        get
        {
            if (_chattingMainUI == null)
            {
                _chattingMainUI = ShowUIBase<ChatMainUI>("ChatMainUI");
                _chattingMainUI.Close(); // 기본은 닫아두기(비활성)
            }
            return _chattingMainUI;
        }
    }
    #endif
    
    private TriggerDialogue _dialogueUI;
    public TriggerDialogue DialogueUI
    {
        get
        {
            if (_dialogueUI == null)
            {
                _dialogueUI = ShowUIBase<TriggerDialogue>("DialogueUI");
            }
            return _dialogueUI;
        }
    }
    
    private UIRelic _uiRelic;
    public UIRelic UIRelic
    {
        get
        {
            if(_uiRelic == null)
            {
                _uiRelic = ShowUIBase<UIRelic>("UIRelic");
                _uiRelic.Close();
            }
            return _uiRelic;
        }
    }
    
    private UIRelicManagement _uiRelicManagement;
    public UIRelicManagement UIRelicManagement
    {
        get
        {
            if(_uiRelicManagement == null)
            {
                _uiRelicManagement = ShowUIBase<UIRelicManagement>("UIRelicManagement");
                _uiRelicManagement.Close();
            }
            return _uiRelicManagement;
        }
    }

    Tween[] _gameMoneyTxtTween = new Tween[Enum.GetValues(typeof(Define.EGameMoneyType)).Length];

    private UIOpenEvent _uiOpenEvent;
    public UIOpenEvent UIOpenEvent
    {
        get
        {
            if( _uiOpenEvent == null)
            {
                _uiOpenEvent = ShowUIBase<UIOpenEvent>("UIOpenEvent");
                _uiOpenEvent.Close();
            }
            return _uiOpenEvent;
        }
    }

    public T ShowUIBase<T>(string _name, Transform parent = null) where T : UIBase
    {
        // UI
        GameObject tPrefab = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/UI/MainUI/{_name}");
        T baseUI = Utils.GetOrAddComponent<T>(tPrefab);
        if (parent == null)
            parent = UICanvas;
        tPrefab.transform.SetParent(parent, false);
        //tPrefab.transform.localScale = Vector3.one;
        //tPrefab.transform.localPosition = Vector3.zero;
        baseUI.Init();
      
        return baseUI;
    }

    public T ShowUIToast<T>(string txt,  string _name = null, string text2 = null) where T : UIToastBase
    {
        if (UICanvas == null)
        {
            MyLogger.LogWarning($"[UIManager] Cannot show toast '{_name}' because UICanvas is null.");
            return null;
        }
        
        if (string.IsNullOrEmpty(_name))
        {
            _name = typeof(T).Name;
        }

        GameObject tPrefab = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/UI/ToastMessage/{_name}");
        T baseUI = Utils.GetOrAddComponent<T>(tPrefab);
        tPrefab.transform.SetParent(UICanvas, false);
        baseUI.Init();

        UIToastBase uIToastBase = baseUI;
        if(string.IsNullOrEmpty(text2))
        {
            uIToastBase.SetText(txt);
        }
        else
        {
            uIToastBase.SetText(txt, text2);
        }

        return uIToastBase as T;
    }

    public T ShowPopup<T>(string name = null) where T : UIPopupBase
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        GameObject tPrefab = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/UI/Popup/{name}");
        T popup = Utils.GetOrAddComponent<T>(tPrefab);
        tPrefab.transform.SetParent(UICanvas, false);
        _popupStack.Push(popup);
        popup.Init();

        return popup;
    }

    public T ShowUISubBase<T>(UIBase mainUI, string _name = null) where T : UISubBase
    {
        if (string.IsNullOrEmpty(_name))
        {
            _name = typeof(T).Name;
        }

        GameObject tPrefab = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/UI/MainUI/Sub/{_name}");
        T subBaseUI = Utils.GetOrAddComponent<T>(tPrefab);
        subBaseUI.SetMainUI(mainUI);

        tPrefab.transform.SetParent(mainUI._subCanvas, false);

        subBaseUI.Init();
        subBaseUI.Close();
        return subBaseUI;

    }

    public void SetGameMoneyTxt(Define.EGameMoneyType eGameMoneyType, int value, TMP_Text textUI)
    {

        _gameMoneyTxtTween[(int)eGameMoneyType].Kill();

        int tempValue = int.Parse(textUI.text);
        int currentValue = value;

        _gameMoneyTxtTween[(int)eGameMoneyType] = DOTween.To(() => tempValue, x => tempValue = x, value, 1f)
            .SetDelay(0.3f)
            .SetEase(Ease.Linear)
            .OnUpdate(() => textUI.text = tempValue.ToString())
            .OnComplete(() =>
            {
                textUI.text = value.ToString();
            });

    }

    public void ClosePopupUI(UIPopupBase popup)
    {
        if (popup == null)
            return;

        if (EnablePopupStack)
        {
            if (_popupStack.Peek() == popup)
                _popupStack.Pop();
        }

        Managers.Instance.GetResObjectManager().Destroy(popup.gameObject);
    }

    public void ClosePopupUI()
    {
        if (_popupStack == null || _popupStack.Count == 0)
            return;
        
        UIPopupBase popup = _popupStack.Peek();
        _popupStack.Pop();
        if(popup != null)
            Managers.Instance.GetResObjectManager().Destroy(popup.gameObject);
    }

    #region UIStack
    public void PushUIStack(UIBase uibase)
    {
        Dictionary<string, UIBase> dic = new Dictionary<string, UIBase>() { { uibase.gameObject.name, uibase } };

        if (!_uiStack.Contains(dic))
        {
            _uiStack.Push(dic);
            _uiStack.Peek().First().Value.Close();
        }
    }

    public void PopUIStack()
    {
        if(_mainInfoUI != null)
        {
            MainInfoUI.BindCurrency();
            MainInfoUI.SetRedDot();
        }
        if (_uiStack.Count <= 0)
        {
            _currentUI = null;
            return;
        }
        Dictionary<string, UIBase> dic = _uiStack.Pop();
        _currentUI = dic.First().Value;
        _currentUI.Open();


    }

    private void CloseCurrentUI()
    {
        if(EnablePopupStack)
        {
            CurrentPopup.ClickCloseBtn();
            return;
        }

        if(_currentUI == null) return;
        UIBase currentUI = _currentUI;
        if(currentUI._subUIStack.Count > 0)
        {
            _currentUI.CurrentSubUI.ClickCloseBtn();
            return;
        }

        currentUI.ClickCloseBtn();
    }

    private void AllCloseBaseUI()
    {
        if (_currentUI == null)
        {
            _uiStack.Clear();
            return;
        }
        
        while (_uiStack.Count > 0)
        {
            CloseCurrentUI();
        }

        _currentUI.ClickCloseBtn();
    }
    
    public void AllClosePopupUI()
    {
        if (!EnablePopupStack)
            return;
        
        while (_popupStack.Count > 0)
        {
            ClosePopupUI();
        }
    }

    public void AllCloseStackUI()
    {
        AllClosePopupUI();
        AllCloseBaseUI();
    }

    #endregion

    public void HandleAndroidBackButton()
    {
        if (_currentUI != null || EnablePopupStack)
        {
            CloseCurrentUI();
        }
        else
        {
            MapManager mapManager = Managers.Instance.GetMapManager();
            if (mapManager != null)
            {
                if (mapManager._contentType == Define.EContent.Field)
                {
                    ShowConfirmPopUp(
                        "게임 종료",
                        "게임을 종료하시겠습니까?",
                        () => Application.Quit(),
                        null
                    );
                }
            }
           
        }
    }

    public void ShowConfirmPopUp(string description, string subDescription, UnityAction action, UnityAction cancelAction = null)
    {
        UIDefaultPopup confirmPopup = ShowPopup<UIDefaultPopup>("ConfirmPopup");
        confirmPopup.Init(description, subDescription, action, cancelAction);
    }
    
    public void ShowErrorPopUp(EErrorCloseType type, string description, string subDescription, UnityAction action)
    {
        UIErrorPopup errorPopup = ShowPopup<UIErrorPopup>("ErrorPopup");
        errorPopup.Init(type, description, subDescription, action);
    }
    
    public void ShowServerMaintenancePopUp()
    {
        UIErrorPopup errorPopup = ShowPopup<UIErrorPopup>("ServerMaintenancePopup");
        errorPopup.Init(EErrorCloseType.ApplicationQuit, "서버 점검중 입니다.", "게임을 종료합니다.", null);
    }
    
     
    public void ShowCommonToastMessage(string description)
    {
        ShowUIToast<UIToastBase>(description,"ToastMessage", null);
    }
    
    public void ShowUserLevelUpToastMessage(int expGap)
    {
        if (expGap <= 0)
            return;
        
        ShowUIToast<UIToastBase>($"경험치를 {expGap}획득 했습니다.", "ToastMessage");
    }

    public async UniTask ShowRewardPopup(RewardBundleDto rewardDto, bool isRewardPopup = true)
    {
        if (isRewardPopup)
        {
            UISubRewards subUI = ShowPopup<UISubRewards>("UIRewardPopup");
            subUI.SetRewardData(rewardDto);
            subUI.OpenToStack();

            await subUI.WaitUntilClosedAsync();
        }

        await UniTask.Yield();
        if (UserInfoData.levelUp)
        {
            UserInfoData.levelUp = false;

            int level = UserInfoData.userLevel.Value;

#if ANALYTICS
            // 레벨 4,5,6,7 콘텐츠 해금 로그
            AnalyticsLogEventHelp.AnalyticsLogEventHelper.CheckUnlockByLevel(level);

            // 계정 레벨 30,50,100 달성 로그
            AnalyticsLogEventHelp.AnalyticsLogEventHelper.CheckUserLevelMilestone(level);
#endif

            UIAccountLevelUpPopup levelUpPopup = ShowPopup<UIAccountLevelUpPopup>("UIAccountLevelUpPopup");
            levelUpPopup.SetData(UserInfoData.userLevel.Value);
            levelUpPopup.OpenToStack();
            
            if(level == 15)      // open event start level
            {
                MainInfoUI.Refresh();
            }
        }
    }

    public async UniTask AttendancePopupCoroutine()
    {
        if (Managers.Instance.UserInfo()._isNewb)
        {
            var newPopup = ShowPopup<UIAttendanceEventPopup>("UINewAttendanceEventPopup");
            newPopup.InitType(EAttendanceEventType.New);

            await newPopup.WaitUntilClosedAsync();
            await UniTask.Yield();
        }

        var weeklyPopup = ShowPopup<UIAttendanceEventPopup>("UIWeeklyAttendanceEventPopup");
        weeklyPopup.InitType(EAttendanceEventType.Weekly);

        await weeklyPopup.WaitUntilClosedAsync();
        await UniTask.Yield();

        var monthlyPopup = ShowPopup<UIAttendanceEventPopup>("UIMonthlyAttendanceEventPopup");
        monthlyPopup.InitType(EAttendanceEventType.Monthly);

        await monthlyPopup.WaitUntilClosedAsync();
        await UniTask.Yield();
    }

    public void OpenGachaUI(EGachaType gachaType)
    {
        Managers.Instance.GetServerManager().OnGetGatchaGetPickUpSchedule(schedule =>
        {
            UIGacha.Init(gachaType, schedule);
        });
    }
}
