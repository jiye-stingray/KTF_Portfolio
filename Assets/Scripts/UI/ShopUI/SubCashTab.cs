using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class SubCashTab : UITabBase
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _countTxt;
    [SerializeField] UICostButton _costBtn;

    [SerializeField] RewardItem _cashItem;
    [SerializeField] RewardItem _bonusCashItem;


    CashShop _db;
   public void SetDataOpen(ShopItemData data)
    {
        Open();

        _db = data.GetData() as CashShop;
        Refresh();
    }

    public override void Refresh()
    {
        _nameTxt.text = _db.ItemTitle;
        _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.ItemIconAtlas,
            ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, ECurrency.Cash_Free).Icon);
        _countTxt.text = "x"+ (_db.RewardCount_1 + _db.RewardCount_2 ).ToString();
        _cashItem.Init(ERewardType.Currency, (int)_db.RewardID_1, _db.RewardCount_1 );
        _bonusCashItem.Init(ERewardType.Currency, (int)_db.RewardID_2, _db.RewardCount_2);
        _costBtn.Init(new ECurrency[] { ECurrency.MidCash},new int[] {_db.Price});


    }

    public void CostBtnClick()
    {
        if (_costBtn.isGray)
        {
            UIManager.ShowConfirmPopUp("", "여우구슬이 부족합니다.\n여우 구슬 구매 탭으로 이동 하시겠습니까?", () =>
            {

                UIManager.UIShop._group._currentTapGroupBtn = UIManager.UIShop._group._tapGroupBtns[3];
                UIManager.UIShop.OnChangeTab();
                UIManager.UIShop.Refresh();

                _mainUI.GetComponent<UISubBase>().ClickCloseBtn();
            });
            return;
        }

#if USE_SERVER
        Managers.Instance.GetServerManager().OnPostShopTypePurchase(EShopType.CashShop, _db.ID, 1, "", "", (shopDto, RewardBundleDto, response) =>
        {

            UIManager.UIShop.OnChangeTab();     // scrollview Update

            Managers.Instance.GetServerManager().OnPostRequestMyMail(() =>
            {
                UIManager.MainInfoUI.Refresh();
                UIManager.ShowCommonToastMessage("구매하신 상품은 우편함으로 발송되었습니다.");
                _mainUI.GetComponent<UISubBase>().ClickCloseBtn();
            });
        });
#endif

    }

}
