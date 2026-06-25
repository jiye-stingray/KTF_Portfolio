using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PVPRewardScrollviewItem : ICell
{
    [SerializeField] TMP_Text _rankingTxt;
    [SerializeField] RewardItem _dailyRewardItem;
    [SerializeField] RewardItem[] _seasonRewardItem;        // 필요시 동적 생성

    public override void SetData(ItemData data, int index)
    {
        _index = index;
    }
}
