using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFieldDungeonSpeechBox : UISpeechBox
{
    [SerializeField] private TMP_Text _name;
    [SerializeField] private Image _icon;
    
    private int _dungeonID;

    public void SetData(BuildingInfo buildingData)
    {
        _name.text = buildingData.Name;
        _icon.sprite = AtlasManager.GetSprite(Define.EAtlasType.ItemIconAtlas, "DungeonKey");
    }
}
