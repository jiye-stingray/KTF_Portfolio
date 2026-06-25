using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using static Define;

public class UISubGachaCurrency : UISubBase
{
    [SerializeField] private GameObject _emptyRoot;
    [SerializeField] private GameObject _currencyRoot;
    
    [SerializeField] TMP_Text _currencyCountTxt;
    [SerializeField] TMP_Text _descTxt;

    [SerializeField] CurrencyIcon _currencyIcon_ticket;
    [SerializeField] CurrencyIcon _currencyIcon_swapCurrency;

    [SerializeField] private GameObject _confirmButton;
    [SerializeField] private GameObject _shopButton;

    GachaGroup _data;
    EGachaCountType _gachaCountType;
    int _count;
    private CurrencyData _ticketData;
    private CurrencyData _cashData;

    private UserInfoData UserInfo => Managers.Instance.UserInfo();
    private int TicketCount => UserInfo.GetCurrencyValue((ECurrency)_ticketData.ID);
    private int CashCount => UserInfo.GetCurrencyValue(ECurrency.Cash_Free);
    private bool EnableTicket => TicketCount >= _count; // 티켓으로만 가능한지
    private int NeedTicketCount => Mathf.Max(_count - TicketCount, 0); // 부족한 티켓 갯수
    
    private StringBuilder _currencyStr = new StringBuilder();
    
    public void InitData(GachaGroup gachaData, EGachaCountType gachaCountType)
    {
        _data = gachaData;
        _gachaCountType = gachaCountType;
        _count = Convert.ToInt32(_gachaCountType);

        _ticketData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, _data.GachaCurrencyType);
        _cashData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, ECurrency.Cash_Free);
        Refresh();
    }
        
    public override void Refresh()
    {
        int enoughCash = 0;
        _currencyStr.Clear();
        
        // if ((EGachaType)_data.GroupID == EGachaType.Celestial)
        // {
        //     _emptyRoot.SetActive(!EnableTicket);
        //     _currencyRoot.SetActive(EnableTicket);
        //     
        //     _shopButton.SetActive(!EnableTicket);
        //     _confirmButton.SetActive(EnableTicket);
        //     
        //     _currencyIcon_ticket.gameObject.SetActive(true);
        //     _currencyIcon_swapCurrency.gameObject.SetActive(false);
        //     
        //     _currencyIcon_ticket.Init(_data.GachaCurrencyType, _count);
        //     _currencyStr.Append($"<color=#34920C>{_ticketData.UIName} * {_count}</color>");
        // }
        // else
        {
            int needCash = NeedTicketCount * _data.JewelCost;
            bool enableCash = needCash <= CashCount;
            
            _emptyRoot.SetActive(!enableCash);
            _currencyRoot.SetActive(enableCash);
            
            _shopButton.SetActive(!enableCash);
            _confirmButton.SetActive(enableCash);
            
            _currencyIcon_ticket.gameObject.SetActive(TicketCount > 0);
            _currencyIcon_swapCurrency.gameObject.SetActive(NeedTicketCount > 0);
            
            _currencyIcon_ticket.Init(_data.GachaCurrencyType, _count - NeedTicketCount);
            _currencyIcon_swapCurrency.Init(ECurrency.Cash_Free, needCash);
            
            if(TicketCount > 0)
                _currencyStr.Append($"<color=#34920C>{_ticketData.UIName} * {_count - NeedTicketCount}</color>");
            if (TicketCount > 0 && NeedTicketCount > 0)
                _currencyStr.Append(" <color=#34920C>+</color> ");
            if(NeedTicketCount > 0)
                _currencyStr.Append($"<color=#116F97>{_cashData.UIName} * {needCash}</color>");
        }
        
        _currencyCountTxt.text = _currencyStr.ToString();
        _descTxt.text = $"소모하여 모집을 {_count}회 진행하시겠습니까?";
    } 

    public void OnStartGachaClicked()
    {
        if (_emptyRoot.activeSelf)
            return;

        // 가챠 진행
        BestHttp_GameManager.OnStartGacha((EGachaType)_data.GroupID, _gachaCountType);
        ClickCloseBtn();
    }

    public void OnMoveShopClicked()
    {
        UIManager.UIShop.SetCashShopOpenToStack(true);
    }
}
