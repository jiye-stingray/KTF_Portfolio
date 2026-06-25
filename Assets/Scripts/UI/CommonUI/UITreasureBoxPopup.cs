using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITreasureBoxPopup : UIPopupBase
{
    [SerializeField] private TMP_Text _treasureBoxCount;
    [SerializeField] RewardItem _rewardItem;
    [SerializeField] private GameObject _rewardButton;
    [SerializeField] private TMP_Text _multipleText;
    [SerializeField] private TMP_Text _treasureADCountTxt;
    [SerializeField] private Image _adIcon;
    
    private TreasureBoxData _treasureBoxData;

    public void SetData(TreasureBoxData treasureBoxData)
    {
        _treasureBoxData = treasureBoxData;
        int maxCount = ClientLocalDB.GetFieldConfigInt("TreasureBoxLimit");
        _treasureBoxCount.text = $"({_treasureBoxData.boxIndex + 1}/{maxCount})";
        int adBonus = ClientLocalDB.GetFieldConfigInt("TreasureBoxAdBonus");
        _rewardButton.SetActive(!UserInfoData._isAdsRemoved);
        _multipleText.text = $"광고X{adBonus}";
        
        _rewardItem.Init(Define.ERewardType.Currency, (int)_treasureBoxData.currencyType, _treasureBoxData.count);
        _treasureADCountTxt.text = $"{_treasureBoxData.count * adBonus}개";
        _adIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.ItemIconAtlas, "Advert_Icon");
    }

    public void OnBoxReceiveRewardClicked()
    {
        if (Managers.Instance.GetMapManager()._treasureBox == null)
        {
            UIManager.ShowCommonToastMessage("보물상자가 제거 되었습니다.");
            ClickCloseBtn();
            return;
        }

        BestHttp_GameManager.OnTreasureBoxReceiveReward(_treasureBoxData.boxIndex);
        ClickCloseBtn();
    }
    
    public void OnBoxADReceiveRewardClicked()
    {
        if (Managers.Instance.GetMapManager()._treasureBox == null)
        {
            UIManager.ShowCommonToastMessage("보물상자가 제거 되었습니다.");
            ClickCloseBtn();
            return;
        }
        
#if ADMOB
        if (UserInfoData._isAdsRemoved)
        {
            BestHttp_GameManager.OnTreasureBoxADReceiveReward(_treasureBoxData.boxIndex);
            ClickCloseBtn();
            return;
        }

        AdmobManager.Instance.ShowRewarded(earned =>
        {
            if (earned)
            {
                BestHttp_GameManager.OnTreasureBoxADReceiveReward(_treasureBoxData.boxIndex);
                ClickCloseBtn();
            }
        });
        #endif
    }
}
