using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PVPInfo : ICell
{
    [SerializeField] TMP_Text _rankTxt;
    [SerializeField] Image _thumbIcon;
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] TMP_Text _battlePowerTxt;
    [SerializeField] TMP_Text _pointTxt;

    [SerializeField] PVPCharacterInfoItem[] _pvpCharacterInfoItems;

    [SerializeField] bool isMyPVPInfo;

    
    public override void SetData(ItemData data, int index)
    {
        base.SetData(data, index);
    }

    /// <summary>
    /// 방어덱 셋팅 
    /// </summary>
    public void ChangeDeckSettingBtnClick()
    {
        if (!isMyPVPInfo) return;
    }

}
