using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class MainInfoUI : UIBase
{
    [SerializeField] private UIGuideQuest _uiGuideQuest;
    
    [Header("Level")]
    public TMP_Text _levelTxt;
    public Slider _expSlider;
    public TMP_Text _expTxt;

    public TMP_Text _nickNameTxt;

    public TMP_Text _battlePowerTxt;
    public GameObject _townPortalButton;
    public GameObject _townPortalGray;

    public Image[] _currencyIcon;
    public TMP_Text[] _currencyCountTxt;
    public TMP_Text[] _currencyLimitTxt;
    public Slider[] _currencySliders;
    public Image[] _sliderFill;
    
    [SerializeField] private Image thumbnail;
    [SerializeField] private Image frameImage;
    
    [Header("RedDot")]
    [SerializeField] private GameObject _characterListRedDot;
    [SerializeField] private GameObject _gachaRedDot;
    [SerializeField] private GameObject _trainingRedDot;
    [SerializeField] private GameObject _inventoryRedDot;
    [SerializeField] private GameObject _questRedDot;
    [SerializeField] private GameObject _equipmentRedDot;
    [SerializeField] private GameObject _dungeonRedDot;
    [SerializeField] private GameObject _constellationRedDot;
    [SerializeField] private GameObject _guildRedDot;
    [SerializeField] private GameObject _mailRedDot;
    [SerializeField] private GameObject _passRedDot;
    [SerializeField] private GameObject _openEventRedDot;

    [Header("SideMenu")]
    [SerializeField] private SideMenuUI sideMenuUI;
    [SerializeField] private ContentItemUI deckSettingContentsItem;
    [SerializeField] private ContentItemUI characterInventoryContentsItem;
    [SerializeField] private ContentItemUI gachaContentsItem;
    [SerializeField] private ContentItemUI eventContentsItem;
    [SerializeField] private ContentItemUI shopContentsItem;
    [SerializeField] private ContentItemUI passContentsItem;

    [Header("LimitShop")]
    [SerializeField] private GameObject _limitShopBtn;
    [SerializeField] UITimer _limitShopUITimer;

    
    private Dictionary<ECurrency, RectTransform> _currencyPos = new Dictionary<ECurrency, RectTransform>();

    private TimeData _portalDelayTimeData = new TimeData();
    private float _portalDelayTime = 1.0f;
    
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    
    public Button btnZoom; 
    public TextMeshProUGUI txtZoomLabel;

    [Header("AD")]
    [SerializeField] Image _adIcon;
    [SerializeField] private GameObject _EnableAdBuffImg;
    [SerializeField] private UITimer _ADBuffUiTimer;
    
    [Header("ONE Store Hide Buttons")]
    [SerializeField] private GameObject _btnShop;  
    [SerializeField] private GameObject _btnPass;
    
    FieldInfo FieldInfo => ClientLocalDB_Simple.GetData<FieldInfo>(DBKey.FieldInfo, Managers.Instance.GetMapManager()._contentsId);
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        FollowCamera followCam = Managers.Instance.GetCameraManager().FollowCam;
         
        if (followCam != null)
        {
            followCam.txtCameraScaling = txtZoomLabel;
            
            btnZoom.onClick.RemoveAllListeners(); // 중복 방지
            btnZoom.onClick.AddListener(followCam.OnClickChangeZoom);
            followCam.RefreshUI();
        }

        _portalDelayTime = ClientLocalDB.GetFieldConfigInt("PlayerPortalCoolTime");

        _currencyPos.Clear();
        _disposables.Clear();
        
        for (int i = 0; i < FieldInfo.MainUICurrency.Length; i++)
        {
            int currencyId = FieldInfo.MainUICurrency[i];
            ECurrency currencyType = (ECurrency)currencyId;
            CurrencyData currencyData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, currencyId);
            _currencyIcon[i].sprite = AtlasManager.GetSprite(EAtlasType.ItemIconAtlas, currencyData.Icon);
            var currencyTxt = _currencyCountTxt[i];
            UserInfoData.GetCurrencyProperty(currencyType)
                .Subscribe(value => currencyTxt.text = Utils.NumberFormatter.FormatNumber(value))
                .AddTo(_disposables);
            
            TMP_Text currencyLimit = _currencyLimitTxt[i];
            Slider currencySlider = _currencySliders[i];
            Image fillImage = _sliderFill[i];
            ReactiveProperty<int> currencyProperty = UserInfoData.GetCurrencyAcquireNumber(currencyType);
            currencyProperty
                .StartWith(currencyProperty.Value)  // 처음 Init 시 현재값 바로 반영
                .Subscribe(newValue =>
                {
                    int limitCount = UserInfoData.GetCurrencyAcquireLimit(currencyType);
                    currencyLimit.text = $"({newValue}/{limitCount})";
                    currencySlider.value = limitCount > 0 ? (float)newValue / limitCount : 0f;
                    bool enable = limitCount > newValue;
                    fillImage.color = enable ? Utils.HexToColor("00FF00") : Utils.HexToColor("B15046");
                }).AddTo(_disposables);
            
            _currencyPos.Add(currencyType, _currencyIcon[i].rectTransform);
        }
        
        Utils.BindText(UserInfoData.userNickName,_nickNameTxt, this);
        Utils.BindText(UserInfoData.userLevel,_levelTxt, this);
        
        ReactiveProperty<double> battlePowerProperty = UserInfoData.AllBattlePower;
        battlePowerProperty
            .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
            .StartWith(battlePowerProperty.Value)  // 처음 Init 시 현재값 바로 반영 
            .Subscribe(newValue =>
            {
                _battlePowerTxt.text = Utils.NumberFormatter.FormatNumber(newValue);
            }).AddTo(_disposables);
        
        ReactiveProperty<bool> isTownProperty = Managers.Instance.GetObjectUnitManager().playerSquad.IsTownZone;
        isTownProperty
            .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
            .StartWith(isTownProperty.Value)  // 처음 Init 시 현재값 바로 반영 
            .Subscribe(newValue =>
            {
                _townPortalGray.SetActive(newValue);
            }).AddTo(_disposables);
        
        ReactiveProperty<bool> isPortalProperty = UserInfoData._enablePortal;
        isPortalProperty
            .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
            .StartWith(isPortalProperty.Value)  // 처음 Init 시 현재값 바로 반영 
            .Subscribe(newValue =>
            {
                _townPortalButton.SetActive(newValue);
            }).AddTo(_disposables);
        
        ReactiveProperty<int> isLevelProperty = UserInfoData.userLevel;
        isLevelProperty
            .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
            .StartWith(isLevelProperty.Value)  // 처음 Init 시 현재값 바로 반영 
            .Subscribe(newValue =>
            {
                _levelTxt.text = newValue.ToString();
                Refresh();
                RefreshCurrencyProgressBar();
            }).AddTo(_disposables);
        
        ReactiveProperty<int> isExpProperty = UserInfoData.userExp;
        isExpProperty
            .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
            .StartWith(isExpProperty.Value)  // 처음 Init 시 현재값 바로 반영 
            .Subscribe(newValue =>
            {
                SetExpProgress();
            }).AddTo(_disposables);

        ReactiveProperty<int> isDialogueProperty = UserInfoData.dialogKey;
        isDialogueProperty
            .DistinctUntilChanged()
            .StartWith(isDialogueProperty.Value)
            .Subscribe(newValue =>
            {
                Refresh();
            }).AddTo(_disposables);


        thumbnail.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.CharacterIconAtlas,
                $"Thum_SD_Cr_{UserInfoData._thumbnailID.ToString("000")}");
        if (frameImage)
        {
            var frameSprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.FrameAtlas,
                $"FrameImg_{UserInfoData._frameID.ToString("000")}");
            frameImage.sprite = frameSprite;
            frameImage.gameObject.SetActive(frameSprite != null);
        }

        sideMenuUI.Init();
        _uiGuideQuest.Init();
        BindCurrency();
        Refresh();

        _adIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, "Advert_Icon");

#if ONE
            if (_btnShop != null) _btnShop.SetActive(false);
            if (_btnPass != null) _btnPass.SetActive(false);
#endif
        _limitShopBtn.gameObject.SetActive(false);
        if (UserInfoData._currentLimitShopDataID > 0)
        {
            LimitShopItemData limitShopItemData = UserInfoData._dicShopItemData[EShopType.LimitShop][UserInfoData._currentLimitShopDataID] as LimitShopItemData;

            _limitShopBtn.SetActive(limitShopItemData.count > 0);
            // limit shop 이 있을 때
            if (limitShopItemData.count > 0)
            {
                DateTime endTime = 
                        (UserInfoData._dicShopItemData[EShopType.LimitShop][UserInfoData._currentLimitShopDataID] as LimitShopItemData).endTime;
                TimeSpan remaining = endTime - ServerTime.Instance.CurrentTime();

                TimeData timeData = new TimeData();
                timeData.SetByDuration(remaining.TotalSeconds);
                _limitShopUITimer.Set(timeData);
                _limitShopUITimer.RegisterOnFinished(delegate { _limitShopBtn.SetActive(false); });
                StartLimitShopTimerShakeLoop();

            }

        }


        return true;
    }

    private void RefreshCurrencyProgressBar()
    {
        for (int i = 0; i < FieldInfo.MainUICurrency.Length; i++)
        {
            int currencyId = FieldInfo.MainUICurrency[i];
            ECurrency currencyType = (ECurrency)currencyId;

            bool enable = UserInfoData.CanCurrencyAcquireNumber(currencyType);
            Image fillImage = _sliderFill[i];
            fillImage.color = enable ? Utils.HexToColor("00FF00") : Utils.HexToColor("B15046");
        }
    }

    public override void Refresh()
    {
        SetRedDot();
        sideMenuUI.Refresh();
        deckSettingContentsItem.Refresh();
        characterInventoryContentsItem.Refresh();
        gachaContentsItem.Refresh();

        //openEventBtn (전체 완료시에 버튼 비활성화)
        eventContentsItem.gameObject.SetActive(!UserInfoData._openEventCompleted);
        if (!UserInfoData._openEventCompleted)
        {
            eventContentsItem.Refresh();
        }

        shopContentsItem.Refresh();
        passContentsItem.Refresh();

        // AD 
        _EnableAdBuffImg.SetActive(UserInfoData.EnableAdBuff);
        if (UserInfoData.EnableAdBuff)
        {
            _ADBuffUiTimer.Set(UserInfoData._adBuffTimeData);
            _ADBuffUiTimer.RegisterOnFinished(delegate { Refresh(); });

        }

        // limit shop 
        _limitShopBtn.SetActive(UserInfoData._currentLimitShopDataID > 0);
        if (UserInfoData._currentLimitShopDataID > 0 )
        {
            LimitShopItemData limitShopItemData = UserInfoData._dicShopItemData[EShopType.LimitShop][UserInfoData._currentLimitShopDataID] as LimitShopItemData;

            _limitShopBtn.SetActive(limitShopItemData.count > 0);
        }
        
    }

    private void SetExpProgress()
    {
        int level = UserInfoData.userLevel.Value;
        UserLevelData userLevel = ClientLocalDB_Simple.GetData<UserLevelData>(DBKey.UserLevel, level);
        int maxExp = userLevel.Exp;
        int exp = UserInfoData.userExp.Value;
        float gauge = exp / (float)maxExp;
        
        _expSlider.value = gauge;
        _expTxt.text = $"{exp}/{maxExp}";
    }

    public void BindCurrency()
    {
        UIManager.TopCurrencyUI.SetCurrency(this.transform, ECurrency.Gold, ECurrency.Cash_Free, ECurrency.MidCash);
    }

    public void SetRedDot()
    {
        _characterListRedDot.SetActive(RedDotManager.CharacterListRedDot());
        _gachaRedDot.SetActive(RedDotManager.GachaRedDot());
        _trainingRedDot.SetActive(RedDotManager.TrainingRedDot());
        _inventoryRedDot.SetActive(RedDotManager.InventoryAllDecompositionBtnRedDot());     // 추후 제작은 or 문으로 넣어두기
        _questRedDot.SetActive(RedDotManager.AllDailyRoutineQuestRedDot() || RedDotManager.AllWeeklyRoutineQuestRedDot());
        _equipmentRedDot.SetActive(RedDotManager.AllAlterLevelRedDot() || RedDotManager.AllSettingEquipmentRedDot());
        _dungeonRedDot.SetActive(RedDotManager.AllDungeonRedDot());
        _constellationRedDot.SetActive(RedDotManager.AllConstellationRedDot());
        _guildRedDot.SetActive(RedDotManager.AllGuildRedDot());
        _mailRedDot.SetActive(RedDotManager.AllMailRedDot());
        _passRedDot.SetActive(RedDotManager.AllPassRedDot());
        _openEventRedDot.SetActive(RedDotManager.AllOpenEventQuestRedDot());
    }

    public void SpawnPortal()
    {
        Squad squad = Managers.Instance.GetObjectUnitManager().playerSquad;

        if (squad.IsTownPortalZone)
        {
            UIManager.ShowCommonToastMessage("마을에서는 포탈을 사용할 수 없습니다.");
            return;
        }

        if (_portalDelayTimeData.GetRemain() > 0)
        {
            UIManager.ShowCommonToastMessage($"{_portalDelayTimeData.GetRemain()}초 후 사용 가능합니다.");
            return;
        }

        if (squad.IsBattleCheck())
        {
            UIManager.ShowCommonToastMessage("전투중에는 포탈을 사용할 수 없습니다.");
            return;
        }
        
        Managers.Instance.GetServerManager().PortalOpen(UserInfoData._fieldId, squad._zoneIndex, squad.circleRigidbody.position);
        SetPortalDelay();
    }

    public void SetPortalDelay()
    {
        _portalDelayTimeData.SetByDuration(_portalDelayTime);
    }

    public RectTransform GetIconTransform(ECurrency currencyType)
    {
        return _currencyPos.GetValueOrDefault(currencyType, null);
    }

    [ContextMenu("Dungeon")]
    public void OpenDungeonUI()
    {
        Managers.Instance.GetUIManager().UIDungeonEntranceList.OpenToStack();
    }

    public void OnClickShopUI()
    {

        if(shopContentsItem.IsLock)
        {
            UIManager.ShowCommonToastMessage("점검중 입니다.");
            return;
        }

        MyLogger.Log("OnClickShop");
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        //Managers.Instance.GetUIManager().UIShop_Dummy.OpenToStack();
        Managers.Instance.GetUIManager().UIShop.SetCashShopOpenToStack(true);

    }
    public void OnClickCurrencyShopUI()
    {

        MyLogger.Log("OnClickShop");
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        Managers.Instance.GetServerManager().OnRequestMyGuildInfo(() =>
        {
            //Managers.Instance.GetUIManager().UIShop_Dummy.OpenToStack();
            Managers.Instance.GetUIManager().UIShop.SetCashShopOpenToStack(false);

        });


    }

    public void OnClickPassUI()
    {
        MyLogger.Log("OnClickPassUI");
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        if (passContentsItem.IsLock)
        {            
            UIManager.ShowCommonToastMessage("점검중 입니다.");
            return;
        }
        UIManager.UIPassBanner.OpenToStack();

    }

    public void OnClickGachaUI()
    {
        if(gachaContentsItem.IsLock)
        {
            UIManager.ShowCommonToastMessage("점검중 입니다.");
            return;
        }

        MyLogger.Log("OnClickGachaUI");
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        EGachaType gachaType = Managers.Instance.GetTutorialManager().isTutorialActive
            ? EGachaType.General
            : EGachaType.PickUp;
        UIManager.OpenGachaUI(gachaType);
    }

    public void OnClickEventUI()
    {
        if (eventContentsItem.IsLock)
        {
            UIManager.ShowCommonToastMessage("점검중 입니다.");
            return;
        }

        MyLogger.Log("OnClickEventUI");
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        UIManager.UIOpenEvent.OpenToStack();

    }

    public void OnClickThumbnailUI()
    {
        MyLogger.Log("OnClickThumbnailUI");
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        
        var profile = Managers.Instance.GetUIManager().ProfileUI;

        // 1) 콜백만 세팅 (일회성)
        profile.SetOnAvatarSelectedOnce(sp =>
        {
            if (!thumbnail || !sp) return;
            thumbnail.overrideSprite = null;
            thumbnail.preserveAspect = true;
            thumbnail.sprite = sp;
        });

        profile.SetOnFrameSelectedOnce(sp =>
        {
            if (!frameImage) return;
            frameImage.sprite = sp;
            frameImage.gameObject.SetActive(sp != null);
        });
        
        Managers.Instance.GetUIManager().ProfileUI.OpenToStack();
        
    }
    public void OnClickBagUI()
    {
        MyLogger.Log("OnClickBagUI");
        Managers.Instance.GetUIManager().UIInventory.OpenToStack();
    }
    
    public void OnClickWorldMapUI()
    {
        MyLogger.Log("OnClickWorldMapUI");
        Managers.Instance.GetUIManager().UIMinimap.SetZoneOpenToStack(UserInfoData._fieldId);
        
        #if ENABLE_ADDRESSABLES
        Addressables.LoadAssetAsync<GameObject>("UI/MainMenu").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                Instantiate(handle.Result);
        };
        #endif
    }
    
    public void OnClickQuestUI()
    {
        Managers.Instance.GetUIManager().UIRoutineQuest.OpenToStack();
    }
    
    public void OnClickEquipmentUI()
    {
        Managers.Instance.GetUIManager().UIEquipmentSetting.OpenToStack();
    }
    
    public void OnClickChallenge_DungeonUI()
    {
        OpenDungeonUI();
    }
    
    public void OnClickTrainingUI()
    {
        Managers.Instance.GetUIManager().TrainingUI.OpenToStack();
    }
    
    public void OnClickMailUI()
    {
        Managers.Instance.GetUIManager().UIMail.OpenToStack();
    }

    public void OnClickGuildUI()
    {
        // 길드 정보 받아오기
        Managers.Instance.GetServerManager().OnRequestMyGuildInfo(() => 
        {

            if (UserInfoData.ExistGuild == false)
                Managers.Instance.GetUIManager().GuildInfoListPage.OpenToStack();
            else
                Managers.Instance.GetUIManager().UIGuildHome.OpenToStack();

        });
    }
    
    public void OnClickFishingUI()
    {
        Managers.Instance.GetUIManager().FishingUI.OpenToStack();
    }

    public void OnClickSpineUI()
    {
        Managers.Instance.GetUIManager().SpinUI.OpenToStack();
    }

    public void OnClickArenaUI()
    {
        MyLogger.Log("OnClickArenaUI");
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("차기 버전에서 확인 가능 합니다.", "ToastMessage");
    }

    public void OnClickDeckSettingUI()
    {
        if(deckSettingContentsItem.IsLock)
        {
            UIManager.ShowCommonToastMessage("점검중 입니다.");
            return;
        }
        

        MyLogger.Log("OnClickDeckSettingUI");
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        //by rainful 2025-05-15 덱셋팅 기능 연결
        Managers.Instance.GetUIManager().UIDeckSetting.InitContentType(EContent.Field);
        Managers.Instance.GetUIManager().UIDeckSetting.OpenToStack();
        //Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("차기 버전에서 확인 가능 합니다.", "ToastMessage");
    }

    public void OnClickCharacterInventory()
    {
        if (characterInventoryContentsItem.IsLock)
        {
            UIManager.ShowCommonToastMessage("점검중 입니다.");
            return;
        }

        MyLogger.Log("OnClickCharacterInventory");
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        Managers.Instance.GetUIManager().UICharacterInventory.OpenToStack();
        
    }

    public void OnClickConstellation()
    {
        Managers.Instance.GetUIManager() .UIConstellation.OpenToStack();
    }
    public void OnClickCraft()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        Managers.Instance.GetUIManager().UICraft.OpenToStack();
    }

    public void OnClickPVP()
    {
        Managers.Instance.GetUIManager().ShowCommonToastMessage("출시 예정입니다. 기대해주세요!");
    }

    public void OnClickSettingUI()
    {
        UIManager.SettingUIPopup.Init();
    }

    public void OnClickADBuff()
    {
        if (UserInfoData._adBuffTimeData.GetRemain() > 0)
        {
            //UIManager.ShowCommonToastMessage($"현재 버프가 적용중입니다. {UserInfoData._adBuffTimeData.GetRemain()}초 남음");
            return;
        }

#if ADMOB
            if(UserInfoData._isAdsRemoved)
            {
                // 보상 지급
                BestHttp_GameManager.OnPostADSpeed();
            }
            else
            {
                AdmobManager.Instance.ShowRewarded(earned =>
                {
                    if (earned)
                    {
                        // 보상 지급
                        BestHttp_GameManager.OnPostADSpeed();


                    }
                });
            }
#else
        BestHttp_GameManager.OnPostADSpeed();
#endif
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
        _disposables.Clear();
    }

    // public void SetThumbnail(Sprite sp)
    // {
    //     if (!thumbnail || !sp) return;
    //     thumbnail.overrideSprite = null;
    //     thumbnail.preserveAspect = true;
    //     thumbnail.sprite = sp;
    // }

    public void OnClickPlaySpeed(int n)
    {
        Time.timeScale = n;
        Debug.LogError("TimeScale - " + n);
    }

    public void OpenToggleSideMenu()
    {
        sideMenuUI.ToggleMenu(true);
    }

    public void GoNavigation(EContent content, string param = "")
    {
        switch (content)
        {
            case EContent.Equipment:
            case EContent.Gold:
                EDungeonType dungeonType = Utils.ParseEnum<EDungeonType>(content.ToString());
                UIManager.UIDungeonEntrance.InitType(dungeonType);
                UIManager.UIDungeonEntrance.OpenToStack();
                break;
            case EContent.Tower:
                if (!string.IsNullOrEmpty(param))
                {
                    EFactionType factionType = Utils.ParseEnum<EFactionType>(param);
                    UIManager.UITowerDungeonEntrance.InitDataOpenToStack(factionType);
                }
                else
                    UIManager.UITowerDungeonEntranceList.OpenToStack();
                break;
            case EContent.Ranking:

                BestHttp_GameManager.OnGetRankingDungeonRankingList((myRanking, RankingList) =>
                {

                    BestHttp_GameManager.OnGetRankingDungeonSchedule(schedule =>
                    {
                        UIManager.UIRankingDungeonEntrance.SetRankingDataOpenToStack(myRanking,RankingList,schedule);   
                    });

                });

                break;
            case EContent.Constellation:
                UIManager.UIAwakeDungeonEntrance.OpenToStack();
                break;
            case EContent.GuildBoss:
                UIManager.UIGuildHome.OpenToStack();
                break;
        }
    }

    public UIGuideQuest GetGuideQuest()
    {
        return _uiGuideQuest;
    }

    public void OnClickDailCurrencyLimit()
    {
        UIManager.ShowPopup<UIDailyCurrencyLimitPopup>("UIDailyCurrencyLimitPopup").OpenToStack();
    }

    public void OnClickLimitShopBtnClick()
    {
        UILimitShopPopup popup = UIManager.ShowPopup<UILimitShopPopup>($"LimitPopup/LimitShopPopup_{UserInfoData._currentLimitShopDataID}");
        popup.SetIDOpenToStack(UserInfoData._currentLimitShopDataID);
    }

    private void StartLimitShopTimerShakeLoop()
    {
        RectTransform rt = _limitShopUITimer.GetComponent<RectTransform>();

        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOPunchRotation(new Vector3(0f, 0f, 10f), 0.6f, 8, 0.5f));
        seq.AppendInterval(0.3f);
        seq.Append(rt.DOPunchRotation(new Vector3(0f, 0f, 10f), 0.6f, 8, 0.5f));
        seq.AppendInterval(1f);
        seq.SetLoops(-1, LoopType.Restart);
    }
}
