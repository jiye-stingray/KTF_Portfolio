using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHpBar : UIBase
{
    public Slider _slider;
    public Image _timeImg;
    public Image _factionIcon;

    private BaseUnit _srcUnit;
    Color hpColor;
    float _time = 1.0f;
    float _maxTime = 1.0f;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        GetComponent<Canvas>().sortingOrder = Define.SortingLayers.HP_BAR;
        return true;
    }

    public void InitData(BaseUnit srcUnit)
    {
        _srcUnit = srcUnit;
        _slider.value = 1;
        
        if(_factionIcon != null)
            _factionIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)srcUnit._unitDataTable.Faction}");
    }

    public void UpdateUI(float value)
    {
        if (_srcUnit.IsDie)
            return;

        _time = _maxTime;
        _slider.value = value;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (_srcUnit.IsDie)
        {
            //by rainful 2025-05-25 죽으면 곧바로 hp 바 사라지도록 
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            return;
        }
            

        _time -= Time.deltaTime;

        if(_time <= 0)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }
}
