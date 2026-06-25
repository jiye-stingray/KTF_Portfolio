using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class GuideQuestClearTab : UITabBase
{
    [SerializeField] UIGuideQuest _uiGuideQuest;
    [SerializeField] Image _currencyIcon;
    [SerializeField] TMP_Text _currencyValueTxt;
    
    GuideQuest _data;
    RewardData _rewardData;

    public override void Open()
    {
        _data = _uiGuideQuest._data;
        _rewardData = ClientLocalDB_Simple.GetData<RewardData>(DBKey.GuideQuestReward, _data.RewardID);
        if(_rewardData == null)
        {
            Debug.LogError("rewardData null !!");
            return;
        }
        base.Open();
        // Tutorial Check (추후 튜토리얼 아이디로 변경) 
        Managers.Instance.GetTutorialManager().CheckQuestTutorial();

        Refresh();
    }


    public override void Refresh()
    {
        // Icon 세팅
        switch (_rewardData.RewardType.First())
        {
            case ERewardType.Currency:
                    CurrencyData currencydb = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, _rewardData.RewardId.First());
                    if (currencydb == null) return;
                    _currencyIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, currencydb.Icon);
                break;

            case ERewardType.ItemBox:
                Item db = ClientLocalDB_Simple.GetData<Item>(DBKey.Item, _rewardData.RewardId.First());
                if (db == null) return;
                _currencyIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, db.Icon);
                break;

            default:
                break;
        }
        _currencyValueTxt.text = _rewardData.RewardValue.First().ToString();

    }

    public void Click()
    {
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        if (UserInfoData._fieldId == 1)
            BestHttp_GameManager.OnGetClearGuideQuest();
        else
            BestHttp_GameManager.OnGetDungeonQuestClear();
    }
}
