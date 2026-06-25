using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PVPRankingScrollviewItem : ICell
{
    [SerializeField] Image _rankingIcon;
    [SerializeField] TMP_Text _rankingTxt;
    [SerializeField] Image _frameImg;
    [SerializeField] Image _thumbImg;
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] TMP_Text _scoreTxt;

    /// <summary>
    /// 추후 필요 데이터 할당
    /// </summary>
    /// <param name="data"></param>
    /// <param name="index"></param>
    public override void SetData(ItemData data, int index)
    {
        _index = index;
    }
}
