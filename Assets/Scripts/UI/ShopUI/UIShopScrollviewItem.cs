using PolyAndCode.UI;
using System;
using TMPro;
using UnityEngine;
#if IAP
using UnityEngine.Purchasing;
#endif
using UnityEngine.UI;
using static Define;

public class UIShopScrollviewItem : ICell
{
    [SerializeField] TMP_Text _nameText;
    [SerializeField] Image _icon;

    [SerializeField] TMP_Text _valueTxt;
    [SerializeField] TMP_Text _countTxt;

    [SerializeField] TMP_Text _costTxt;
    [SerializeField] Image _costImg;

    [SerializeField] GameObject _timeImg;
    [SerializeField] TMP_Text _timeTxt;

    [SerializeField] GameObject _doneGray;
    [SerializeField] GameObject _guildLevelGray;
    [SerializeField] TMP_Text _guildLevelTxt;
    [SerializeField] GameObject _heroPieceGray;

    [SerializeField] GameObject _firstImg;

    EShopType _shopType;

    ShopItemData _shopItemData;
    PackageShopItemData _packageShopItemData;
    CurrencyShopItemData _currencyShopItemData;

    AtlasManager atlas => Managers.Instance.GetAtlasManager();

    public override void SetData(ItemData data, int index)
    {
        _shopItemData = data as ShopItemData;
        _shopType = _shopItemData._type;
        switch (_shopItemData._type)
        {
            case Define.EShopType.PackageShop:
                _packageShopItemData = _shopItemData as PackageShopItemData;
                DrawPackage();
                break;
            case Define.EShopType.CashShop:
                DrawCash();
                break;
            case Define.EShopType.MidCashShop:
                DrawMidCash();
                break;
            case Define.EShopType.TicketShop:
                DrawTicket();
                break;
            case Define.EShopType.GuildShop:
                _currencyShopItemData = _shopItemData as CurrencyShopItemData;
                DrawGuild();
                break;
            case Define.EShopType.GoldShop:
                _currencyShopItemData = _shopItemData as CurrencyShopItemData;
                DrawGold();
                break;
            case Define.EShopType.HeroPieceShop:
                _currencyShopItemData = _shopItemData as CurrencyShopItemData;
                DrawHeroPieceShop();
                break;
            default:
                break;
        }
        _index = index;
    }

    private void DrawPackage()
    {
        PackageShop db = _packageShopItemData.GetData() as PackageShop;

        _countTxt.gameObject.SetActive(true);
        _timeImg.gameObject.SetActive(db.Limited);
        _valueTxt.gameObject.SetActive(false);
        _costImg.gameObject.SetActive(true);
        _firstImg.gameObject.SetActive(false);

        _doneGray.SetActive(_packageShopItemData.count <=  0);
        _guildLevelGray.SetActive(false);

        _nameText.text = db.ItemTitle;
        _icon.sprite = atlas.GetSprite(EAtlasType.ShopAtlas, db.Resource);
        _countTxt.text = $"{_packageShopItemData.count}/{db.PurchaseCount}";

        if (db.Limited)
        {
            DateTime now = ServerTime.Instance.CurrentTime();
            int remainDays = (_packageShopItemData.endTime - now).Days;
            _timeTxt.text = $"D-{remainDays:D2}";
        }

        _costImg.sprite = atlas.GetSprite(EAtlasType.ItemIconAtlas, ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, ECurrency.MidCash).Icon);
        _costTxt.text = $"{db.Price}";
    }

    private void DrawCash()
    {
        CashShop db = _shopItemData.GetData() as CashShop;
        _countTxt.gameObject.SetActive(false);
        _timeImg.gameObject.SetActive(false);
        _valueTxt.gameObject.SetActive(true);
        _costImg.gameObject.SetActive(true);
        _firstImg.gameObject.SetActive(false);

        _doneGray.SetActive(false);
        _guildLevelGray.SetActive(false);

        _nameText.text = db.ItemTitle;
        _icon.sprite = atlas.GetSprite(EAtlasType.ShopAtlas, db.Resource);
        _valueTxt.text = $"{db.RewardCount_1} + {db.RewardCount_2}";


        _costImg.sprite = atlas.GetSprite(EAtlasType.ItemIconAtlas, ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, ECurrency.MidCash).Icon);
        _costTxt.text = $"{db.Price}";
    }

    private void DrawMidCash()
    {
        MidCashShop db = _shopItemData.GetData() as MidCashShop;
        _countTxt.gameObject.SetActive(false);
        _timeImg.gameObject.SetActive(false);
        _valueTxt.gameObject.SetActive(true);
        _costImg.gameObject.SetActive(false);
        _firstImg.gameObject.SetActive((_shopItemData as MidCashShopItemData)._isfirstBuy);


        _doneGray.SetActive(false);
        _guildLevelGray.SetActive(false);

        _nameText.text = db.ItemTitle;
        _icon.sprite = atlas.GetSprite(EAtlasType.ShopAtlas, db.Resource);
        int count = db.RewardCount;
        if ((_shopItemData as MidCashShopItemData)._isfirstBuy)
            count *= 2;
        _valueTxt.text = count.ToString();

#if IAP
        Product product = Managers.Instance.IAP.GetProduct(db.ProductID);
        if (product != null)
            _costTxt.text = $"{product.metadata.localizedPriceString}";
#endif
    }

    private void DrawTicket()
    {
        TicketShop db = _shopItemData.GetData() as TicketShop;
        _countTxt.gameObject.SetActive(false);
        _timeImg.gameObject.SetActive(false);
        _valueTxt.gameObject.SetActive(false);
        _costImg.gameObject.SetActive(true);
        _firstImg.gameObject.SetActive(false);

        _doneGray.SetActive(false);
        _guildLevelGray.SetActive(false);

        _nameText.text = db.ItemTitle;
        _icon.sprite = atlas.GetSprite(EAtlasType.ItemIconAtlas, ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, db.RewardID).Icon);

        _costImg.sprite = atlas.GetSprite(EAtlasType.ItemIconAtlas, ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, db.CostID).Icon);
        _costTxt.text = db.CostValue.ToString();
    }

    private void DrawGuild()
    {
        GuildShop db = _currencyShopItemData.GetData() as GuildShop;
        _countTxt.gameObject.SetActive(true);
        _timeImg.gameObject.SetActive(false);
        _valueTxt.gameObject.SetActive(true);
        _costImg.gameObject.SetActive(true);
        _firstImg.gameObject.SetActive(false);


        _doneGray.SetActive(false);
        _guildLevelGray.SetActive(false);

        _nameText.text = db.ItemTitle;
        _icon.sprite = Utils.ReturnRewardIconSprite(db.RewardType, db.RewardID);
        _valueTxt.text = db.RewardCount.ToString();


        _countTxt.text = $"{_currencyShopItemData.count}/{db.PurchaseCount}";

        _costImg.sprite = atlas.GetSprite(EAtlasType.ItemIconAtlas, ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, db.CostID).Icon);
        _costTxt.text = db.CostValue.ToString();

        // 길드 관련 예외처리
        int guildLevel = UserInfo.ExistGuild ? UserInfo.guildInfo.level : 1;
        _guildLevelGray.SetActive(guildLevel < db.UnlockConditionGuildLevel);
        if (_guildLevelGray.activeSelf)
            _guildLevelTxt.text = $"연합레벨\nLv.{db.UnlockConditionGuildLevel}\n이상";
        else
            _doneGray.SetActive(_currencyShopItemData.count <= 0);


    }
    private void DrawGold()
    {
        GoldShop db = _currencyShopItemData.GetData() as GoldShop;
        _countTxt.gameObject.SetActive(true);
        _timeImg.gameObject.SetActive(false);
        _valueTxt.gameObject.SetActive(true);
        _costImg.gameObject.SetActive(true);
        _firstImg.gameObject.SetActive(false);


        _doneGray.SetActive(false);
        _guildLevelGray.SetActive(false);

        _nameText.text = db.ItemTitle;
        _icon.sprite = Utils.ReturnRewardIconSprite(db.RewardType, db.RewardID);
        _valueTxt.text = db.RewardCount.ToString();


        _countTxt.text = $"{_currencyShopItemData.count}/{db.PurchaseCount}";

        _costImg.sprite = atlas.GetSprite(EAtlasType.ItemIconAtlas, ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, db.CostID).Icon);
        _costTxt.text = db.CostValue.ToString();

        // 골드 관련 예외처리
        _doneGray.SetActive(_currencyShopItemData.count <= 0);

    }

    private void DrawHeroPieceShop()
    {
        HeroPieceShop db = _currencyShopItemData.GetData() as HeroPieceShop;
        _countTxt.gameObject.SetActive(true);
        _timeImg.gameObject.SetActive(false);
        _valueTxt.gameObject.SetActive(true);
        _costImg.gameObject.SetActive(true);
        _firstImg.gameObject.SetActive(false);

        _doneGray.SetActive(false);
        _guildLevelGray.SetActive(false);

        _nameText.text = db.ItemTitle;
        _icon.sprite = Utils.ReturnRewardIconSprite(db.RewardType, db.RewardID);
        _valueTxt.text = db.RewardCount.ToString();

        _heroPieceGray.SetActive(db.RewardType == ERewardType.HeroPiece && !UserInfo.GetCharacterItemData(db.RewardID).isOpen);

        int realCount = _currencyShopItemData.count;
        if (db.RewardType == ERewardType.HeroPiece)
        {
            realCount = Math.Min(UserInfo.ReturnRemainPiece(db.RewardID) - UserInfo._dicCharacterItemData[db.RewardID]._currentCount, realCount);
        }

        _countTxt.text = $"{realCount}/{db.PurchaseCount}";

        _costImg.sprite = atlas.GetSprite(EAtlasType.ItemIconAtlas, ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, db.CostID).Icon);
        _costTxt.text = db.CostValue.ToString();

        _doneGray.SetActive(realCount <= 0);
    }

    private void Update()
    {
        if (_shopType == EShopType.PackageShop && (_packageShopItemData.GetData() as PackageShop).Limited)
        {
            DateTime now = ServerTime.Instance.CurrentTime();
            int remainDays = (_packageShopItemData.endTime - now).Days;
            _timeTxt.text = $"D-{remainDays:D2}";
        }
    }
    public void Click()
    {
        if (_doneGray.activeSelf || _guildLevelGray.activeSelf || _heroPieceGray.activeSelf) return;

        // 한정 Package 예외처리
        if (_shopType == EShopType.PackageShop && (_packageShopItemData.GetData() as PackageShop).Limited
            && _packageShopItemData.endTime <= ServerTime.Instance.CurrentTime())
        {
            UIManager.ShowCommonToastMessage("판매 기간이 종료되었습니다.");
            return;
        }

        Managers.Instance.Sound.PlaySFX("Effect", "BTN_MenuOpen");
        UISubShop uISubShop = UIManager.ShowUISubBase<UISubShop>(UIManager.UIShop, "UISubShop");
        uISubShop.SetShopTypeOpenToStack(_shopItemData);
    }
}
