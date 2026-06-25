using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResurrectionPopup : UIBase
{
    [SerializeField] private GameObject _resurrectionRoot;
    [SerializeField] private GameObject _battleLoseRoot;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private GameObject _adResurrectionButton;
    [SerializeField] private GameObject _resurrectionButton;
    [SerializeField] private Image _costIcon;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private UITimer _timer;
    
    private const int CountDownTime = 10;
    private int _cost;
    private int _adMaxCount;
    private bool _resurrection;

    private Squad Squad => Managers.Instance.GetObjectUnitManager().playerSquad;
    private MapManager MapManager => Managers.Instance.GetMapManager();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        _adMaxCount = ClientLocalDB.GetFieldConfigInt("AdResurrectionMaxCount");
        _cost = ClientLocalDB.GetFieldConfigInt("ResurrectionCost");
        _costText.text = _cost.ToString();

        int costId = ClientLocalDB.GetFieldConfigInt("ResurrectionCostID");
        CurrencyData currencyData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, costId);
        _costIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.ItemIconAtlas, currencyData.Icon);
        return true;
    }

    public void InitData()
    {
        _resurrection = false;
        _resurrectionRoot.SetActive(true);
        _battleLoseRoot.SetActive(false);
        
        Refresh();
        SetTimer();
    }

    public override void Refresh()
    {
        int enableResurrectionCnt = UserInfoData._myADInfo.resurrection;
        _descriptionText.text = $"무료 부활 가능횟수 ({enableResurrectionCnt}/{_adMaxCount})";

    #if ONE
        _adResurrectionButton.SetActive(false);
    #else
        bool enableAdResurrection = enableResurrectionCnt > 0;
        _adResurrectionButton.SetActive(enableAdResurrection);
    #endif
        
    }

    public void OpenBattleLosePopup()
    {
        _resurrectionRoot.SetActive(false);
        _battleLoseRoot.SetActive(true);
    }

    private void SetTimer()
    {
        TimeData timeData = TimeData.Create();
        timeData.SetByDuration(CountDownTime);
        
        _timer.Set(timeData);
        _timer.SetFinishString("0");
        _timer.RegisterOnFinished(delegate { OnGiveUpClicked(); });
    }
    
    public void OnAdResurrectionClicked()
    {
        _timer.StopTimer();
        
        #if ADMOB
        
        if (UserInfoData._isAdsRemoved)
        {
            Resurrection(true);
            return;
        }
        
        if (AdmobManager.Instance.ShowRewarded(Resurrection))
            MyLogger.Log("Enable Admob");
        else
        {
            UIManager.ShowCommonToastMessage("광고가 준비 되지 않았습니다.");
            OpenBattleLosePopup();
        }
        #else
            Resurrection(true);
        #endif
    }

    private void Resurrection(bool earned)
    {
        if (earned)
        {
            _resurrection = true;
            Managers.Instance.GetServerManager().OnGetADResurrection();
        }
        else
        {
            UIManager.ShowCommonToastMessage("광고가 중단 되었습니다.");
            OpenBattleLosePopup();
        }
    }
    
    public void OnCashResurrectionClicked()
    {
        int userCount = UserInfoData.GetCurrencyValue(Define.ECurrency.Cash_Free);
        if (userCount < _cost)
        {
            UIManager.ShowCommonToastMessage("재화가 부족합니다.");
            return;
        }
        
        _timer.StopTimer();
        _resurrection = true;
        BestHttp_GameManager.OnGetResurrection();
    }

    public void OnGiveUpClicked()
    {
        _timer.StopTimer();
        OpenBattleLosePopup();
    }

    public void OnGoVillageClicked()
    {
        _battleLoseRoot.SetActive(false);
        Squad._zoneIndex = MapManager._townPortal.GetZoneIndex();
        Squad.PlayerResurrection();
        Squad.TeleportHeroes(MapManager._townPortal.GetZoneIndex(), MapManager._townPortal.transform.position);
        ClickCloseBtn();
    }

    public override void ClickCloseBtn()
    {
        if (_resurrectionRoot.activeSelf)
        {
            if (!_resurrection)
            {
                OnGiveUpClicked();
                return;
            }
        }
        else if (_battleLoseRoot.activeSelf)
        {
            OnGoVillageClicked();
            return;
        }
        
        base.ClickCloseBtn();
    }
}
