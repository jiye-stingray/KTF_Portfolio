using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class SubShopCurrencyTab : UITabBase
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _countTxt;

    [SerializeField] TMP_Text _switchCntTxt;
    [SerializeField] CountSwitchDataButton _leftSwitchBtn;
    [SerializeField] CountSwitchDataButton _rightSwitchBtn;

    IndexWrapper _index = new IndexWrapper();

    [SerializeField] UICostButton _costButton;

    // DB
    TicketShop ticketDB;
    GoldShop goldDB;
    GuildShop guildshopDB;
    HeroPieceShop heroPieceShopDB;

    int goodsId;

    EShopType _shopType;
    ShopItemData _currentShopItemData;

    TimeData _gachaTimeData;

    public void SetDataOpen(ShopItemData data)
    {
        _currentShopItemData = data;
        _shopType = data._type;
        switch (data._type)
        {
            case EShopType.TicketShop:
                ticketDB = data.GetData() as TicketShop;
                goodsId = ticketDB.ID;
                break;
            case EShopType.GuildShop:
                guildshopDB = data.GetData() as GuildShop;
                goodsId = guildshopDB.ID;
                break;
            case EShopType.GoldShop:
                goldDB = data.GetData() as GoldShop;
                goodsId = goldDB.ID;
                break;
            case EShopType.HeroPieceShop:
                heroPieceShopDB = data.GetData() as HeroPieceShop;
                goodsId = heroPieceShopDB.ID;
                Managers.Instance.GetServerManager().OnGetGatchaGetPickUpSchedule(schedule =>
                {

                    DateTime endTime = DateTime.Parse(schedule.endTime);
                    TimeSpan remain = endTime - ServerTime.Instance.CurrentTime();
                    _gachaTimeData = new TimeData();
                    _gachaTimeData.SetByDuration(remain.TotalSeconds);
                });
                break;
            default:
                break;
        }
        Open();
    }

    private int GetMaxWipeCount()
    {
        int costId = 0;
        int costValue = 1;
        int shopPurchaseCount = 0;
        switch (_shopType)
        {
            case EShopType.TicketShop:    costId = ticketDB.CostID;    costValue = ticketDB.CostValue;    break;
            case EShopType.GuildShop:     costId = guildshopDB.CostID; costValue = guildshopDB.CostValue; shopPurchaseCount = guildshopDB.PurchaseCount;    break;
            case EShopType.GoldShop:      costId = goldDB.CostID;      costValue = goldDB.CostValue;      shopPurchaseCount = goldDB.PurchaseCount;          break;
            case EShopType.HeroPieceShop: costId = heroPieceShopDB.CostID; costValue = heroPieceShopDB.CostValue; shopPurchaseCount = heroPieceShopDB.PurchaseCount; break;
        }

        int playerCurrency = Managers.Instance.UserInfo().GetCurrencyValue((ECurrency)costId);
        int affordable = costValue > 0 ? playerCurrency / costValue : 0;

        CurrencyData currencyData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, costId);
        int limitCount = currencyData != null ? currencyData.LimitCount : 0;

        int maxCount = limitCount > 0 ? Math.Min(affordable, limitCount) : affordable;

        if (shopPurchaseCount > 0)
            maxCount = Math.Min(maxCount, shopPurchaseCount);

        int remainingCount = (_currentShopItemData as CurrencyShopItemData)?.count ?? 0;
        if (remainingCount > 0)
            maxCount = Math.Min(maxCount, remainingCount);

        if(_shopType == EShopType.HeroPieceShop && heroPieceShopDB.RewardType == ERewardType.HeroPiece)
        {
            int remainPiece = Managers.Instance.UserInfo().ReturnRemainPiece(heroPieceShopDB.RewardID) - UserInfoData._dicCharacterItemData[heroPieceShopDB.RewardID]._currentCount;
            int rewardCount = heroPieceShopDB.RewardCount;
            int maxPurchaseByPiece = rewardCount > 0 ? Mathf.CeilToInt((float)remainPiece / rewardCount) : 0;
            maxCount = Math.Min(maxCount, maxPurchaseByPiece);
        }

        return Math.Max(1, maxCount);
    }

    public override void Open()
    {
        base.Open();
        _index._index = 1;

        int maxWipeCount = GetMaxWipeCount();
        _leftSwitchBtn.Init(_index, 1, maxWipeCount, SwitchDataBtnClick);
        _rightSwitchBtn.Init(_index, 1, maxWipeCount, SwitchDataBtnClick);
        SwitchDataBtnClick(_index._index);

        Refresh();
    }

    public override void Refresh()
    {
        switch (_shopType)
        {
            case EShopType.TicketShop:
                DrawTicket();
                break;
            case EShopType.GuildShop:
                DrawGuild();
                break;
            case EShopType.GoldShop:
                DrawGold();
                break;
            case EShopType.HeroPieceShop:
                DrawHeroPiece();
                break;
            default:
                break;
        }
        base.Refresh();
    }

    private void DrawTicket()
    {
        _nameTxt.text = ticketDB.ItemTitle;
        _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas,
            ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, ticketDB.RewardID).Icon);
        _countTxt.gameObject.SetActive(false);

        _costButton.Init(new ECurrency[] { (ECurrency)ticketDB.CostID },new int[] { ticketDB.CostValue * _index._index });
        
    }

    private void DrawGuild()
    {
        _nameTxt.text = guildshopDB.ItemTitle;
        _icon.sprite = Utils.ReturnRewardIconSprite(guildshopDB.RewardType, guildshopDB.RewardID);
        _countTxt.text = guildshopDB.RewardCount.ToString();

        _costButton.Init(new ECurrency[] { (ECurrency)guildshopDB.CostID }, new int[] { guildshopDB.CostValue * _index._index });
    }

    private void DrawGold()
    {
        _nameTxt.text = goldDB.ItemTitle;
        _icon.sprite = Utils.ReturnRewardIconSprite(goldDB.RewardType, goldDB.RewardID);
        _countTxt.text = goldDB.RewardCount.ToString();

        _costButton.Init(new ECurrency[] { (ECurrency)goldDB.CostID }, new int[] { goldDB.CostValue * _index._index });
    }

    private void DrawHeroPiece()
    {
        _nameTxt.text = heroPieceShopDB.ItemTitle;
        _icon.sprite = Utils.ReturnRewardIconSprite(heroPieceShopDB.RewardType, heroPieceShopDB.RewardID);
        _countTxt.text = heroPieceShopDB.RewardCount.ToString();

        _costButton.Init(new ECurrency[] { (ECurrency)heroPieceShopDB.CostID }, new int[] { heroPieceShopDB.CostValue * _index._index });
    }

    #region Switch 
    public void MinBtnClick()
    {
        _index._index = 1;
        _switchCntTxt.text = _index._index.ToString();
        Refresh();
    }

    public void MaxBtnClick()
    {
        _index._index = GetMaxWipeCount();
        _switchCntTxt.text = _index._index.ToString();

        Refresh();
    }

    public void SwitchDataBtnClick(int index)
    {
        _index._index = index;
        _switchCntTxt.text = index.ToString();
        Refresh();
    }
    #endregion

    public override void Close()
    {
        base.Close();
    }
    public void CostBtnClick()
    {
        if (_costButton.isGray) return;

        if (_shopType == EShopType.HeroPieceShop)
        {
            // 이미 종료된 상태
            if(_gachaTimeData.GetRemain() <= 0)
            {
                return;
            }
            ConnectShopServer();
            return;
        }
        else 
            ConnectShopServer();
    }

    private void ConnectShopServer()
    {
#if USE_SERVER
        Managers.Instance.GetServerManager().OnPostShopTypePurchase(_shopType, goodsId, _index._index, "", "", (shopDto, RewardBundleDto, response) =>
        {

            UserInfoData.SetShopItemData(_shopType, new ShopDto[] { shopDto });
            if (_shopType != EShopType.TicketShop)
                UserInfoData.UpdateCorrectQuest(EQuestType.Weekly, EQuestConditionType.ShopPurchase, 0, _index._index);
            if (RewardBundleDto != null)
            {
                // rewardPopup
                UIManager.ShowRewardPopup(RewardBundleDto).Forget();
            }

            UIManager.UIShop.OnChangeTab();     // scrollview Update
            _mainUI.GetComponent<UISubBase>().ClickCloseBtn();
        });
#endif
    }
}
