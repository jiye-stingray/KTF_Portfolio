using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class FactionSynergyItem : MonoBehaviour
{
    [SerializeField] private EFactionType _factionType;
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _countText;
    
    public EFactionType FactionType => _factionType;
    
    public void SetData(int count)
    {
        FactionSynergy factionSynergy = ClientLocalDB_Simple.GetFactionSynergy(_factionType, count);
        _image.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)_factionType}");
        SetText(factionSynergy, count);
    }

    private void SetText(FactionSynergy factionSynergy, int count)
    {
        Color color = Utils.HexToColor("#FFF4EA");
        if (factionSynergy == null)
            color.a = 160f/255f;
        else
        {
            switch (factionSynergy.Grade)
            {
                case 2:
                    color = Utils.HexToColor("#51C441");
                    break;
                case 3:
                    color = Utils.HexToColor("#FAC500");
                    break;
            }
        }
        
        _countText.text = count.ToString();
        _countText.color = color;
    }
}
