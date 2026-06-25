using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class UIShop : UIBase
{
    #region Tab
    [Serializable]
    public struct EPages
    {
        public PackageShopTab PackageShopTab;
        public MonthShopTab MonthShopTab;
        public CashShopTab CashShopTab;
        public MidCashShopTab MidCashShopTab;
        public TicketShopTab TicketShopTab;
        public GuildShopTab GuildShopTab;
        public GoldShopTab GoldShopTab;
        public HeroPieceShopTab HeroPieceShopTab;
    }
    public EPages _pages;

    public enum ETAB_TYPE
    {
        PackageTab,
        MonthShop,
        CashShop,
        MidCashShop,
        TicketShop,
        GuildShop,
        GoldShop,
        HeroPieceShop,
    }
    public ETAB_TYPE _currentTab;

    public UITabGroup _group;
    #endregion

    bool _isCashShop;
    [SerializeField] GameObject _cashTab;
    [SerializeField] GameObject _currencyTab;

    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] GameObject _LimitTabObject;

    [SerializeField] UITimer _uiTimer;

    EShopType _shopType;

    public override void Refresh()
    {
        _group.Set((int)_currentTab);
        DrawShop();
    }

    private void DrawShop()
    {
        _shopType = (EShopType)_currentTab;

        _nameTxt.text = Utils.ReturnShopTypeString(_shopType);

    }

    public void SetCashShopOpenToStack(bool isCashShop)
    {
        _isCashShop = isCashShop;
        OpenToStack();
    }

    public override void Open()
    {
        base.Open();
        _group._currentTapGroupBtn = _group._tapGroupBtns[_isCashShop ? 0 : 5];
        _cashTab.SetActive(_isCashShop);
        _currencyTab.SetActive(!_isCashShop);
        OnChangeTab();

        Refresh();
    }
    public void OnChangeTab()
    {
        _currentTab = (ETAB_TYPE)_group._currentTapGroupBtn._index;
        _LimitTabObject.SetActive(false);
        _uiTimer.gameObject.SetActive(false);
        UIManager.TopCurrencyUI.SetCurrency(this.transform, ECurrency.Cash_Free, ECurrency.MidCash);
        DrawShop();

        switch (_currentTab)
        {
            case ETAB_TYPE.PackageTab:
                _LimitTabObject.SetActive(true);
                _pages.PackageShopTab.Open();
                _pages.MonthShopTab.Close();
                _pages.CashShopTab.Close();
                _pages.MidCashShopTab.Close();
                _pages.TicketShopTab.Close();
                _pages.GuildShopTab.Close();
                _pages.GoldShopTab.Close();
                _pages.HeroPieceShopTab.Close();
                break;
            case ETAB_TYPE.MonthShop:
                _pages.MonthShopTab.Open();
                _pages.PackageShopTab.Close();
                _pages.CashShopTab.Close();
                _pages.MidCashShopTab.Close();
                _pages.TicketShopTab.Close();
                _pages.GuildShopTab.Close();
                _pages.GoldShopTab.Close();
                _pages.HeroPieceShopTab.Close();
                break;
            case ETAB_TYPE.CashShop:
                _pages.CashShopTab.Open();
                _pages.MidCashShopTab.Close();
                _pages.TicketShopTab.Close();
                _pages.PackageShopTab.Close();
                _pages.MonthShopTab.Close();
                _pages.GuildShopTab.Close();
                _pages.GoldShopTab.Close();
                _pages.HeroPieceShopTab.Close();
                break;
            case ETAB_TYPE.MidCashShop:
                _pages.MidCashShopTab.Open();
                _pages.CashShopTab.Close();
                _pages.TicketShopTab.Close();
                _pages.PackageShopTab.Close();
                _pages.MonthShopTab.Close();
                _pages.GuildShopTab.Close();
                _pages.GoldShopTab.Close();
                _pages.HeroPieceShopTab.Close();
                break;
            case ETAB_TYPE.TicketShop:
                _pages.TicketShopTab.Open();
                _pages.PackageShopTab.Close();
                _pages.MonthShopTab.Close();
                _pages.CashShopTab.Close();
                _pages.MidCashShopTab.Close();
                _pages.GuildShopTab.Close();
                _pages.GoldShopTab.Close();
                _pages.HeroPieceShopTab.Close();
                break;
            case ETAB_TYPE.GuildShop:
                _pages.GuildShopTab.Open();
                _pages.TicketShopTab.Close();
                _pages.PackageShopTab.Close();
                _pages.MonthShopTab.Close();
                _pages.CashShopTab.Close();
                _pages.MidCashShopTab.Close();
                _pages.GoldShopTab.Close();
                _pages.HeroPieceShopTab.Close();
                SetGuildTimer();
                UIManager.TopCurrencyUI.SetCurrency(this.transform, ECurrency.GuildBossCoin);
                break;
            case ETAB_TYPE.GoldShop:
                _pages.GoldShopTab.Open();
                _pages.TicketShopTab.Close();
                _pages.PackageShopTab.Close();
                _pages.MonthShopTab.Close();
                _pages.CashShopTab.Close();
                _pages.MidCashShopTab.Close();
                _pages.GuildShopTab.Close();
                _pages.HeroPieceShopTab.Close();
                UIManager.TopCurrencyUI.SetCurrency(this.transform, ECurrency.Cash_Free, ECurrency.Gold);
                SetGoldTimer();
                break;
            case ETAB_TYPE.HeroPieceShop:
                _pages.HeroPieceShopTab.Open();
                _pages.GoldShopTab.Close();
                _pages.GuildShopTab.Close();
                _pages.TicketShopTab.Close();
                _pages.PackageShopTab.Close();
                _pages.MonthShopTab.Close();
                _pages.CashShopTab.Close();
                _pages.MidCashShopTab.Close();
                UIManager.TopCurrencyUI.SetCurrency(this.transform, ECurrency.GachaSwapCurrency);
                SetHeroPieceTimer();
                break;
            default:
                break;
        }
    }

    public void OnChangeLimit()
    {
        switch (_currentTab)
        {
            case ETAB_TYPE.PackageTab:
                _pages.PackageShopTab.OnChangeTab();
                break;
            default:
                break;
        }

    }

    private void SetGoldTimer()
    {
        _uiTimer.gameObject.SetActive(true);

        DateTime now = ServerTime.Instance.CurrentTime();
        DateTime nextMidnight = now.Date.AddDays(1);
        TimeSpan remaining = nextMidnight - now;

        TimeData timeData = new TimeData();
        timeData.SetByDuration(remaining.TotalSeconds);
        _uiTimer.Set(timeData, "초기화까지: {0}");
    }

    private void SetGuildTimer()
    {
        _uiTimer.gameObject.SetActive(true);

        Managers.Instance.GetServerManager().OnGetGuildBossSchedule(schedule =>
        {
            DateTime startTime = DateTime.Parse(schedule.startTime);
            DateTime initTime = startTime.AddDays(21);
            TimeSpan remaining = initTime - ServerTime.Instance.CurrentTime();

            TimeData timeData = new TimeData();
            timeData.SetByDuration(remaining.TotalSeconds);
            _uiTimer.Set(timeData, "초기화까지: {0}");

            _uiTimer.SetFinishString("다음 토벌 기간에 초기화 됩니다.");
        });
    }

    private void SetHeroPieceTimer()
    {
        _uiTimer.gameObject.SetActive(true);
        Managers.Instance.GetServerManager().OnGetGatchaGetPickUpSchedule(schedule =>
        {
            DateTime endTime = DateTime.Parse(schedule.endTime);
            TimeSpan remaining = endTime - ServerTime.Instance.CurrentTime();

            TimeData timeData = new TimeData();
            timeData.SetByDuration(remaining.TotalSeconds);
            _uiTimer.Set(timeData, "종료까지: {0}");

            _uiTimer.SetFinishString("점검 후 초기화 됩니다.");
        });

    }
}
