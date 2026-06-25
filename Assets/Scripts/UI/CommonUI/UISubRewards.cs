using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UISubRewards : UIPopupBase
{
    [Header("Reward")]
    [SerializeField] ScrollRectDynamicPopulator _scrollView;
    List<ItemData> _items = new List<ItemData>();
    
    private UniTaskCompletionSource _closeTcs;

    public override void OpenToStack()
    {
        Debug.Log($"[RewardPopup] Open {GetInstanceID()}");
        _closeTcs = new UniTaskCompletionSource();
        base.OpenToStack();
    }

    public void SetRewardData(RewardBundleDto rewardBundleDto)
    {
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "SE_reward");
        
        _items.Clear();
        foreach (var rewardDto in rewardBundleDto.currencyRewardDtoList)
        {
            RewardItemData itemData = ConvertRewardItemData(rewardDto, ERewardType.Currency);
            _items.Add(itemData);
        }

        foreach (var rewardDto in rewardBundleDto.characterRewardDtoList)
        {
            RewardItemData itemData = ConvertRewardItemData(rewardDto, ERewardType.Character);
            _items.Add(itemData);
        }

        foreach (var rewardDto in rewardBundleDto.equipmentRewardDtoList)
        {
            RewardItemData itemData = ConvertRewardItemData(rewardDto, ERewardType.Equipment);
            _items.Add(itemData);
        }
        foreach (var rewardDto in rewardBundleDto.emblemRewardDtoList)
        {
            RewardItemData itemData = ConvertRewardItemData(rewardDto, ERewardType.Emblem);
            _items.Add(itemData);
        }

        //유물 파츠 보상은 따로 처리
        // foreach (var rewardDto in rewardBundleDto.relicPartsRewardDtoList)
        // {
        //     RewardItemData itemData = ConvertRewardItemData(rewardDto, ERewardType.RelicParts);
        //     _items.Add(itemData);
        // }

        foreach (var rewardDto in rewardBundleDto.itemBoxRewardDtoList)
        {
            RewardItemData itemData = ConvertRewardItemData(rewardDto, ERewardType.ItemBox);
            _items.Add(itemData);
        }

        _scrollView.Init((cell, data, index) =>
        {
            cell.SetData(data, index);
        });

        _scrollView.Populate(_items);
    }

    //재화, 캐릭터 보상
    private RewardItemData ConvertRewardItemData(RewardDto rewardDto, ERewardType rewardType)
    {
        RewardItemData itemData = new RewardItemData();
        itemData._index = rewardDto.tableId;
        itemData._rewardType = rewardType;
        itemData._count = rewardDto.count;

        return itemData;
    }

    //장비 보상
    private RewardItemData ConvertRewardItemData(EquipmentDto rewardDto, ERewardType rewardType)
    {
        RewardItemData itemData = new RewardItemData();
        itemData._index = rewardDto.tableId;
        itemData._rewardType = rewardType;
        return itemData;
    }
    //유물 부위 보상
    //파츠 보상은 따로 처리
    // private RewardItemData ConvertRewardItemData(RelicPartsRewardDto rewardDto, ERewardType rewardType)
    // {
    //     RewardItemData itemData = new RewardItemData();
    //     itemData._index = rewardDto.relicId;
    //     itemData._rewardType = rewardType;
    //     return itemData;
    // }

    //아이템 상자 보상
    private RewardItemData ConvertRewardItemData(ItemBoxDto rewardDto, ERewardType rewardType)
    {
        RewardItemData itemData = new RewardItemData();
        itemData._index = rewardDto.boxId;
        itemData._rewardType = rewardType;
        itemData._count = rewardDto.count;
        return itemData;
    }

    //엠블럼 보상
    private RewardItemData ConvertRewardItemData(EmblemDto rewardDto, ERewardType rewardType)
    {
        EmblemRewardItemData itemData = new EmblemRewardItemData();
        itemData._index = rewardDto.id;
        itemData._rewardType = rewardType;
        itemData.synergyList = rewardDto.synergyList;
        //itemData._level = rewardDto.level;

        return itemData;
    }

    public override void ClickCloseBtn()
    {
        _closeTcs?.TrySetResult();
        base.ClickCloseBtn();
    }

    public UniTask WaitUntilClosedAsync()
    {
        return _closeTcs.Task;
    }
    
    private void OnDestroy()
    {
        _closeTcs?.TrySetResult();
    }
}