using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishDexScrollItem : ICell
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;

    public void SetData(int index, Fish fish, bool caught, Sprite iconSprite, string lockedName)
    {
        _index = index;
        if (caught)
        {
            if (nameText != null)
                nameText.text = fish.fishName;
        }
        else
        {
            if (nameText != null)
                nameText.text = lockedName;
        }

        if (icon != null)
        {
            icon.sprite = iconSprite;
            icon.color = Color.white;
        }
    }
}