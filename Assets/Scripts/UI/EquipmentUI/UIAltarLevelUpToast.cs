using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIAltarLevelUpToast : UIToastBase
{
    [SerializeField] TMP_Text _beforeTxt;
    [SerializeField] TMP_Text _currencyTxt;

    [SerializeField] TMP_Text _decoTxt;
    [SerializeField] TMP_Text _battlepowerTxt;

    [SerializeField] Image _icon;
    public override void SetText(string txt, string text2)
    {
        EFactionType type = (EFactionType)Enum.Parse(typeof(EFactionType), txt);

        _beforeTxt.text = $"Lv.{ UserInfoData._dicAltarLevel[type] - 1}";
        _currencyTxt.text = $"Lv.<color=#6CC041>{ UserInfoData._dicAltarLevel[type]}";

        double value = double.Parse(text2);
        bool _isMinus = value < 0;

        // icon 업데이트
        _icon.sprite = _isMinus ? Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.PictogramAtlas, "Pictogram_BattlePowerDown") :
             Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.PictogramAtlas, "Pictogram_BattlePowerUp");

        _battlepowerTxt.text = Math.Abs(value).ToString();
    }

    protected override void JoinExtraFades(Sequence seq)
    {
        seq.Join(_beforeTxt.DOFade(0, 1))
           .Join(_currencyTxt.DOFade(0, 1))
           .Join(_battlepowerTxt.DOFade(0, 1))
           .Join(_icon.DOFade(0, 1))
           .Join(_decoTxt.DOFade(0, 1));
    }
}
