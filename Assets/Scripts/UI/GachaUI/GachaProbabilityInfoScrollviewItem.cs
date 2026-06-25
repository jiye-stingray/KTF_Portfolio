using TMPro;
using UnityEngine;
using static Define;

public class GachaProbabilityInfoScrollviewItem : MonoBehaviour
{
    [SerializeField] RewardItem _rewardItem;
    [SerializeField] GameObject _pickupGo;
    [SerializeField] TMP_Text _probabilityTxt;

    float _probability;
    bool _isPickup;

    private ERewardType _rewardType;
    private int _id;
    private int _count;

    public void InitData(ERewardType rewardType, int id, int count, float probability, bool isPickup)
    {
        _rewardType = rewardType;
        _id = id;
        _count = count;
        _probability = probability;
        _isPickup = isPickup;
        
        Refresh();
    }

    public void Refresh()
    {
        _rewardItem.Init(_rewardType, _id, _count);
        _pickupGo.SetActive(_isPickup);       // 픽업 캐릭터 일 때
        _probabilityTxt.text = $"{_probability:F4}%";
        
        _rewardItem.CountText.gameObject.SetActive(_rewardType != ERewardType.Character);
    }
}