using PolyAndCode.UI;
using Spine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIShopMonthScrollviewItem : ICell
{
    [SerializeField] Image _bg;
    [SerializeField] TMP_Text _nameTxt;

    [SerializeField] Transform _purchaseContent;
    List<RewardItem> _purchaseRewardItems = new List<RewardItem>();

    [SerializeField] Transform _dailyContent;
    List<RewardItem> _dailyRewardItems = new List<RewardItem>();

    [SerializeField] GameObject _dayImg;
    [SerializeField] TMP_Text _dayTxt;
    [SerializeField] GameObject _unpurchasedImg;

    [SerializeField] GameObject _costBtn;
    [SerializeField] TMP_Text _costTxt;
    [SerializeField] Image _costImg;

    MonthShopItemData _data;

    public override void SetData(ItemData data, int index)
    {
        _data = data as MonthShopItemData;
        _index = index;

        DrawItemData();
    }
 
    private void DrawItemData()
    {
        MonthShop db = (_data.GetData() as MonthShop);
        _bg.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.ShopAtlas, db.Resource1);
        _nameTxt.text = db.ItemTitle;

        // Reward
        for (int i = 0; i < _purchaseRewardItems.Count; i++)
        {
            Destroy(_purchaseRewardItems[i].gameObject);
        }
        _purchaseRewardItems.Clear();
        for (int i = 0; i < db.RewardID_1.Length; i++)
        {
            RewardItem rewardItem = Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/Common/RewardItem_126",_purchaseContent).GetComponent<RewardItem>();
            rewardItem.Init(Define.ERewardType.Currency, (int)db.RewardID_1[i], db.RewardCount_1[i]);
            _purchaseRewardItems.Add(rewardItem);
        }

        for (int i = 0; i < _dailyRewardItems.Count; i++)
        {
            Destroy(_dailyRewardItems[i].gameObject);
        }
        _dailyRewardItems.Clear();
        for (int i = 0; i < db.RewardID_2.Length; i++)
        {
            RewardItem rewardItem = Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/Common/RewardItem_126", _dailyContent).GetComponent<RewardItem>();
            rewardItem.Init(Define.ERewardType.Currency, (int)db.RewardID_2[i], db.RewardCount_2[i]);
            _dailyRewardItems.Add(rewardItem);
        }

        // Day
        _dayImg.SetActive(_data.isPurchased);
        _unpurchasedImg.SetActive(!_data.isPurchased);
        if (_data.isPurchased)
            _dayTxt.text = $"잔여: {28 - _data.day + 1}일";        // 첫 구매일부터 28일 (day 는 1) 

        _costBtn.SetActive(!_data.isPurchased);
        if (_costBtn.activeSelf)
        {

            _costTxt.text = $"{db.Price}";
            _costImg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, ECurrency.MidCash).Icon);
        }


    }

    public void Click()
    {
        UISubShop uISubShop = UIManager.ShowUISubBase<UISubShop>(UIManager.UIShop, "UISubShop");
        uISubShop.SetShopTypeOpenToStack(_data);
    }
}
