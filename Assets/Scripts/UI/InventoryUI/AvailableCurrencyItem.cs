using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class AvailableCurrencyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _name;
    [SerializeField] RewardItem _rewardItem;
    [SerializeField] GameObject _selectImg;

    bool _isSelect;
    int _index;
    Action<int> _clickAction;

    public void SetCurrency(ERewardType reward,int id ,int count,int index,bool isSelect,Action<int> clickAction)
    {
        _index =  index;
        _clickAction = clickAction;
        _isSelect = isSelect;
        _rewardItem.Init(reward, id, count);
        _selectImg.gameObject.SetActive(isSelect);

        _name.text = reward switch
        {
            ERewardType.Currency => ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, id)?.UIName ?? string.Empty,
            ERewardType.ItemBox => ClientLocalDB_Simple.GetData<Item>(DBKey.Item,id)?.UIName ?? string.Empty,
            _ => string.Empty
        };
    }

    public void Click()
    {
        if (_isSelect) return;
        if (_clickAction == null) return;
        _clickAction?.Invoke(_index);
    }
}
