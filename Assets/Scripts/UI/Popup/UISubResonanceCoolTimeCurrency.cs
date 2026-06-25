using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class UISubResonanceCoolTimeCurrency : UISubBase
{
    [SerializeField] private TMP_Text _coolTimeTxt;
    [SerializeField] private TMP_Text _descTxt;


    [SerializeField] CurrencyIcon _currencyIcon;

    ResonanceItemData _data;
    public void InitData(ResonanceItemData data)
    {
        _data = data;

        OpenToStack();
        Refresh();
    }

    public override void Refresh()
    {
        _currencyIcon.Init((ECurrency)ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotDismisCurrency").Value,
            ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotDismisCurrencyValue").Value);

        _descTxt.text = $"공명 슬롯 해제 쿨타임이 남았습니다. " +
            $"{ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotDismisCurrencyValue").Value} " +
            $"{ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, (ECurrency)ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotDismisCurrency").Value).UIName} 를 소모하여 즉시 해제 하겠습니다.";
    }

    private void Update()
    {
        //Date 타임 체크
        DateTime serverCurrentTime = ServerTime.Instance.CurrentTime();
        TimeSpan timeCal = _data._expireDate - serverCurrentTime;
        _coolTimeTxt.text = $"남은시간\n{timeCal.Hours}:{timeCal.Minutes}";
    }

    public void Click()
    {

        if(UserInfoData.GetCurrencyValue((ECurrency)ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotDismisCurrency").Value)
            < ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotDismisCurrencyValue").Value)
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("재화가 부족합니다", "ToastMessage");
            return;
        }
        // 쿨타임 초기화 (추후 서버 연결)

#if USE_SERVER
        Managers.Instance.GetServerManager().OnPostSlotTimeReset(_data._index);
#else
        _data._expireDate = default;
        // 재화 소모
        Managers.Instance.GetUIManager().UICharacterInventory.Refresh();
#endif
        ClickCloseBtn();
    }
}
