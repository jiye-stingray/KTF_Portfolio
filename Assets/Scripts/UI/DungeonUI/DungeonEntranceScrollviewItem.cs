using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Define;

public class DungeonEntranceScrollviewItem : MonoBehaviour
{
    [SerializeField] EDungeonType _dungeonType;

    [SerializeField] Button _btn;
    [SerializeField] TMP_Text _valueTxt;
    [SerializeField] Image _currencyIcon;
    [SerializeField] Image _redDot;

    int keyCnt;

    UserInfoData UserInfo => Managers.Instance.UserInfo();
    UIManager UIManager => Managers.Instance.GetUIManager();

    ContentsOpen openContentBase => ClientLocalDB_Simple.GetData<ContentsOpen>(DBKey.ContentsOpen, gameObject.name);

    public void Init()
    {
        _btn.onClick.AddListener(Click);
        Refresh();
    }

    void Refresh()
    {
        int maxCnt = 0;
        switch (_dungeonType)
        {
            case EDungeonType.Equipment:
                CurrencyData currencyData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, ECurrency.AdmissionTicket_EquipmentDungeon);
                maxCnt = currencyData.TicketChargeMaxCount;
                keyCnt = UserInfo.GetCurrencyValue(ECurrency.AdmissionTicket_EquipmentDungeon);
                _currencyIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, currencyData.Icon);
                _valueTxt.text = $"{Utils.NumberFormatter.FormatNumber(keyCnt)}/{maxCnt}";
                break;
            case EDungeonType.Gold:
                CurrencyData currencyDataGold = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, ECurrency.AdmissionTicket_GoldDungeon);
                maxCnt = currencyDataGold.TicketChargeMaxCount;
                keyCnt = UserInfo.GetCurrencyValue(ECurrency.AdmissionTicket_GoldDungeon);
                _currencyIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, currencyDataGold.Icon);
                _valueTxt.text = $"{Utils.NumberFormatter.FormatNumber(keyCnt)}/{maxCnt}";
                break;
            case EDungeonType.Tower:
                break;
            case EDungeonType.Constellation:
                _valueTxt.text = $"{UserInfo._maxConstellationDungeonMonsterCnt}";
                break;
            case EDungeonType.Ranking:
                _valueTxt.text = Utils.GetDungeonClearValue(EDungeonType.Ranking).ToString();
                break;
            case EDungeonType.Pvp:
                break;
        }

        _redDot.gameObject.SetActive(RedDotManager.DungeonRedDot(_dungeonType));

        #if TUTO
        gameObject.SetActive(UserInfo.userLevel.Value >= openContentBase.ConditionValue);
        #endif
    }

    public void Click()
    {
        // UI 띄우기
        OpenUI();
    }

    public void OpenUI()
    {
        EContent content = Utils.ParseEnum<EContent>(_dungeonType.ToString());
        UIManager.MainInfoUI.GoNavigation(content);
    }
}

