using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterDescTab : UITabBase
{
    [SerializeField] TMP_Text _name;
    [SerializeField] TMP_Text _descTxt;

    public void  SetData(CharacterClassItemData itemData)
    {
        _name.text = itemData._unitData.Name;
        _descTxt.text = ClientLocalDB_Simple.GetData<DescDB>(DBKey.Desc, itemData.id).Desc;
    }
}
