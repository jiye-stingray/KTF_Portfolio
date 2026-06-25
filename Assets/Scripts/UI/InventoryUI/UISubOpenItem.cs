using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UISubOpenItem : UISubBase
{
    [SerializeField] protected TMP_Text _nameTxt;
    [SerializeField] protected Image _icon;
    [SerializeField] protected Slider _slider;
    [SerializeField] protected TMP_Text _countTxt;
    [SerializeField] protected Transform _availableCurrencyAreaContent;
    [SerializeField] private GameObject _gray;

    protected Item _currentItemData = null;
    private int _selectIndex = -1; 
    protected List<AvailableCurrencyItem> _availableCurrencyItemList = new List<AvailableCurrencyItem>();

    public virtual void SetItemDataOpenToStack(Item itemData)
    {
        _currentItemData = itemData;
        int maxCount = GetMaxOpenCount();
        _slider.wholeNumbers = true;
        _slider.minValue = 0;
        _slider.maxValue = maxCount;
        _slider.value = maxCount;
        _slider.interactable = true;

        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        _slider.onValueChanged.AddListener(OnSliderValueChanged);

        _selectIndex = -1;

        _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ShopAtlas, itemData.Icon);
        DrawCurrencyItem();

        OpenToStack();
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();

        _nameTxt.text = _currentItemData.UIName;
        OnSliderValueChanged(_slider.value);

    }

    protected virtual void OnSliderValueChanged(float value)
    {
        int count = Mathf.Max(0, (int)value); 

        if(_currentItemData == null) return;
        for (int i = 0; i < _currentItemData.RewardType.Length; i++)
        {
            _availableCurrencyItemList[i].SetCurrency(
                _currentItemData.RewardType[i],
                _currentItemData.RewardID[i],
                _currentItemData.RewardValue[i] * count,
                i,
                i == _selectIndex,
                SelectRewardAction);
        }
        _countTxt.text = count.ToString();

        _gray.SetActive(_selectIndex < 0 || _slider.value <= 0);
    }

    protected virtual int GetMaxOpenCount()
    {
        // 현재 보유 중인 아이템 수량
        int ownedItemCount = UserInfoData.GetItemValue(_currentItemData.ID);
        int maxCount = ownedItemCount;
        return maxCount;
    }

    protected virtual void DrawCurrencyItem()
    {
        _availableCurrencyItemList.ForEach(item => Destroy(item.gameObject));
        _availableCurrencyItemList.Clear();

        if (_currentItemData == null) return;

        int openCount = Mathf.Max(1, (int)_slider.value);
        for (int i = 0; i < _currentItemData.RewardType.Length; i++)
        {
            AvailableCurrencyItem item =
                Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/EtcUI/AvailableCurrencyItem", _availableCurrencyAreaContent)
                .GetComponent<AvailableCurrencyItem>();
            item.SetCurrency(
                _currentItemData.RewardType[i],
                _currentItemData.RewardID[i],
                _currentItemData.RewardValue[i] * openCount,
                i,
                i == _selectIndex,
                SelectRewardAction);
            _availableCurrencyItemList.Add(item);
        }
    }

    public void SelectRewardAction(int index)
    {
        _selectIndex = index;
        Refresh();
    }

    

    public virtual void Click()
    {
        if (_gray.activeSelf)
        {
            if (_selectIndex < 0) UIManager.ShowCommonToastMessage("받을 재화를 선택하세요");
            else if (_slider.value <= 0) UIManager.ShowCommonToastMessage("1개 이상을 선택하세요.");

            return;
        }

        ServerConnect(_currentItemData.ID, _selectIndex, (int)_slider.value, false);
    }

    public void ServerConnect(int id, int idx, int value, bool isAD)
    {
        if(isAD)
        {
            // 광고

#if ADMOB
            if(UserInfoData._isAdsRemoved)
            {
                // 서버 연결
                Managers.Instance.GetServerManager().OnGetADOpenBox(id, idx, value, (rewardBundleDto) =>
                {
                    // UI Refresh
                    UIManager.UIInventory.Refresh();
                    // rewardPopup
                    UIManager.ShowRewardPopup(rewardBundleDto).Forget();

                    ClickCloseBtn();        // ui 닫기
                }); 
                
            }
            else
            {
                AdmobManager.Instance.ShowRewarded(earned =>
                {
                    if (earned)
                    {
                        // 서버 연결
                        Managers.Instance.GetServerManager().OnGetADOpenBox(id, idx, value, (rewardBundleDto) =>
                        {
                            // UI Refresh
                            UIManager.UIInventory.Refresh();
                            // rewardPopup
                            UIManager.ShowRewardPopup(rewardBundleDto).Forget();

                            ClickCloseBtn();        // ui 닫기
                        });
                    }
                });

            }
#else
            // 서버 연결
            Managers.Instance.GetServerManager().OnGetADOpenBox(id, idx, value, (rewardBundleDto) =>
            {
                // UI Refresh
                UIManager.UIInventory.Refresh();
                // rewardPopup
                UIManager.ShowRewardPopup(rewardBundleDto).Forget();

                ClickCloseBtn();        // ui 닫기
            });
#endif



        }
        else
        {
            // 서버 연결
            Managers.Instance.GetServerManager().OnPostOpenItemBox(id, idx, value, (rewardBundleDto) =>
            {
                // UI Refresh
                UIManager.UIInventory.Refresh();
                // rewardPopup
                UIManager.ShowRewardPopup(rewardBundleDto).Forget();

                ClickCloseBtn();        // ui 닫기
            });

        }
    }
}
