using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementPiecesSlider : MonoBehaviour
{
    
    [SerializeField] Slider _slider;
    [SerializeField] TMP_Text _valueTxt;
    [SerializeField] Image _icon;

    int _pieceCost;
    CharacterClassItemData _data;

    public void Init(int pieceCost, CharacterClassItemData data)
    {
        _data = data;
        _pieceCost = pieceCost;
        Refresh();
    }

    void Refresh()
    {

        _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.ItemIconAtlas, $"Piece_Cr_{_data.id.ToString("D3")}");

        if(_data.IsMaxGrade())
        {
            _slider.value = 100;
            _valueTxt.text = "MAX";
            return;
        }

        _slider.value = ((float)_data._currentCount / _pieceCost) * 100;
        _valueTxt.text = $"{_data._currentCount}/{_pieceCost}";
    }
}
