using System;
using static Define;

public class RelicPartsIndexItemData : ItemData
{
    public int ID => _relicPartsItemData._id;
    public RelicPartsItemData _relicPartsItemData;
    public IndexWrapper _indexWrapper;
    public Action _clickAction;
}