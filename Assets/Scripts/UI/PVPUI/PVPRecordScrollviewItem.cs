using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PVPRecordScrollviewItem : ICell
{
    [SerializeField] TMP_Text _resultTxt;
    [SerializeField] Image _thumbIcon;
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] TMP_Text _timeTxt;
    [SerializeField] TMP_Text _changePointTxt;

    public override void SetData(ItemData data, int index)
    {
        _index = index;
    }
}
