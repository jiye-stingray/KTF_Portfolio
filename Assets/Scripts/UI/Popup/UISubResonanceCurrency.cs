using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UISubResonanceCurrency : UISubBase
{
    [SerializeField] private GameObject _emptyRoot;
    [SerializeField] private GameObject _currencyRoot;

    [SerializeField] private TMP_Text _currencyTxt;

    [SerializeField] CurrencyIcon _currencyIcon;

    [SerializeField] private GameObject _confirmButton;
    [SerializeField] private GameObject _shopButton;
    [SerializeField] private GameObject _closeButton;

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public override void Refresh()
    {
        CurrencyData currency1 = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotOpenCurrency_1").Value);
        CurrencyData currency2 = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotOpenCurrency_2").Value);
        bool _currency1Able = UserInfoData.GetCurrencyValue((ECurrency)currency1.ID) >= ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotOpenCurrencyValue_1").Value;
        bool _currency2Able = UserInfoData.GetCurrencyValue((ECurrency)currency2.ID) >= ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotOpenCurrencyValue_2").Value;
    
        _emptyRoot.SetActive(!_currency1Able && !_currency2Able);
        _currencyRoot.SetActive(_currency1Able || _currency2Able);
        if (!_currency1Able && !_currency2Able)
        {
            _confirmButton.SetActive(false);
            _closeButton.SetActive(true);
            _shopButton.SetActive(true);
        }
        else if(_currency1Able)
        {
            _confirmButton.SetActive(true);
            _closeButton.SetActive(true);
            _shopButton.SetActive(false);

            _currencyIcon.Init((ECurrency)currency1.ID, ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotOpenCurrencyValue_1").Value);
            _currencyTxt.text = $"{currency1.UIName}을";
        }
        else
        {
            _confirmButton.SetActive(true);
            _closeButton.SetActive(true);
            _shopButton.SetActive(false);

            _currencyIcon.Init((ECurrency)currency2.ID, ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotOpenCurrencyValue_2").Value);
            _currencyTxt.text = $"{currency2.UIName}를";

        }
    }

    /// <summary>
    /// 가챠 슬롯 해금 
    /// </summary>
    public void UnlockResonanceSlotBtnClick()
    {
#if USE_SERVER
        Managers.Instance.GetServerManager().OnGetUnlockResonanceSlot();
#else

        UserInfoData.UnlockResonance();
        Managers.Instance.GetUIManager().UICharacterInventory.Refresh();
#endif
        ClickCloseBtn();
    }

    public void OnMoveShopClicked()
    {
        Managers.Instance.GetUIManager().ShowCommonToastMessage("준비중 입니다.");
    }
}
