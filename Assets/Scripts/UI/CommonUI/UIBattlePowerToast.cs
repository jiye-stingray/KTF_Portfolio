using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIBattlePowerToast : UIToastBase
{
    [SerializeField] Image _icon;
    bool _isMinus;
    public override void SetText(string txt)
    {
        double value =  double.Parse(txt);
        _isMinus = value < 0;

        // icon 업데이트
        _icon.sprite = _isMinus ? Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.PictogramAtlas, "Pictogram_BattlePowerDown") :
             Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.PictogramAtlas, "Pictogram_BattlePowerUp");

        txt = Math.Abs(value).ToString();     



        base.SetText(txt);
    }

    protected override void JoinExtraFades(Sequence seq)
    {
        if (_icon != null)
            seq.Join(_icon.DOFade(0, 1));
    }
}
