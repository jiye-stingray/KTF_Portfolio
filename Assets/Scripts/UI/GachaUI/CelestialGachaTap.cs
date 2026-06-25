using System;
using System.Linq;
using static Define;

public class CelestialGachaTap : GachaTap
{
    public override void Open()
    {
        base.Open();
        Init(EGachaType.Celestial);
        Refresh();
        StartImageRotation();
    }

    public override void Close()
    {
        StopImageRotation();
        base.Close();
    }

    public override void Refresh()
    {
        UnitData wishUnit = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, _gachaItemData._wishList[0]);
        _descriptionText.text = $"{_gachaGroupData.CeilingCount}회 모집 시 {wishUnit.Name} 확정";
        base.Refresh();
    }

    public void ShowUIGachaCharacterDetail()
    {
        int id = _gachaItemData._wishList[0];
        UnitData unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, id);
        EGradeType grade = unitData.StartGrade;
        int level = ClientLocalDB_Simple.GetDB<CharacterLevel>(DBKey.CharacterLevel).Last().Value.Level;
        
        CharacterClassItemData itemData = new CharacterClassItemData();
        itemData.id = id;
        itemData.Level = level;
        itemData._grade = grade;
        itemData.InitStatus(null);
        itemData.RefreshStatus();
        
        UIManager.UICharacterDetail.SetDataOpenToStack(itemData, true);
    }
}
