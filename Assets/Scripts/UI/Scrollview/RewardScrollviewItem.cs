using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardScrollviewItem : ICell
{
    [SerializeField] private RewardItem _item;

    public RewardItemData _data;
    
    public override void SetData(ItemData data, int index)
    {
        _data = data as RewardItemData;
        _index = index;
        Refresh();
    }

    private void Refresh()
    {
        if (_data == null)
        {
            Debug.LogError("data null!!");
            return;
        }
        
        _item.Init(_data._rewardType, _data._index, _data._count);
    }

    public void Click()
    {
        if(OnClick != null)
            OnClick(_index);
    }
}
