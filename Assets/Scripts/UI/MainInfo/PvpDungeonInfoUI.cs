using System;
using TMPro;
using UnityEngine.UI;

public class PvpDungeonInfoUI : UIBase
{
    public TMP_Text _phaseLabel;
    public TMP_Text _totalDamageLabel;
    public TMP_Text _damageLabel;
    // public CountDownTimer _timer;
    public Slider _damageSlider;

    public override bool Init()
    {
        return base.Init();
    }
}
