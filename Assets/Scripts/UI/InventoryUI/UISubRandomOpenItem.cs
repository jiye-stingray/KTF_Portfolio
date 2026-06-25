using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UISubRandomOpenItem : UISubOpenItem
{
    [SerializeField] OpenRandomItemButton _openRandomItemBtn;

    Item _item;
    int _unlockKeyID;
    int _keyCnt;
    int _adCnt;

    public override void SetItemDataOpenToStack(Item itemData)
    {
        _item = itemData;
        _unlockKeyID = itemData.UnlockKey;
        if(_unlockKeyID > 0)
        {
            _adCnt = Utils.ReturnItemUnlockKeyADCnt(itemData.UnlockKey);
            _keyCnt = UserInfoData.GetCurrencyValue((ECurrency)itemData.UnlockKey);
        }
        base.SetItemDataOpenToStack(itemData);
    }


    public override void Refresh()
    {
        _nameTxt.text = _currentItemData.UIName;
        OnSliderValueChanged(_slider.value);
    }

    protected override int GetMaxOpenCount()
    {
        if(_unlockKeyID <= 0)
        {
            return UserInfoData._dicitemItems[_item.ID];
        }
        if (_keyCnt <= 0) return 1;
        return Mathf.Min(_keyCnt, UserInfoData._dicitemItems[_item.ID]);
    }

    protected override void OnSliderValueChanged(float value)
    {
        _countTxt.text = ((int)value).ToString();
        int useKey = _keyCnt <= 0 ? 0 : (int)_slider.value;
        _openRandomItemBtn.Init(_item.UnlockKey, useKey, _adCnt, (int)value);

    }

    protected override void DrawCurrencyItem()
    {
        _availableCurrencyItemList.ForEach(item => Destroy(item.gameObject));
        _availableCurrencyItemList.Clear();

        if (_currentItemData == null) return;

        for (int i = 0; i < _currentItemData.RewardType.Length; i++)
        {
            AvailableCurrencyItem item =
                Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/EtcUI/AvailableCurrencyItem", _availableCurrencyAreaContent)
                .GetComponent<AvailableCurrencyItem>();
            item.SetCurrency(
                _currentItemData.RewardType[i],
                _currentItemData.RewardID[i],
                _currentItemData.RewardValue[i],
                i,
                false,
                null);
            _availableCurrencyItemList.Add(item);
        }
    }

    public void BtnConnect(bool isAD)
    {
        ServerConnect(_currentItemData.ID, 0, (int)_slider.value, isAD);
    }

    

    public void ProbabilityBtnClick()
    {
        // 추후 라운지 주소 입력
        Application.OpenURL("https://naver.me/FG33St3Z");
    }
}
