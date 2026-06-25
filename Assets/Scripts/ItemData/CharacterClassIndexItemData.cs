using System;
using static Define;

public class CharacterClassIndexItemData : ItemData
{
    public int ID => _characterClassItem.id;
    public CharacterClassItemData _characterClassItem;
    public IndexWrapper _indexWrapper;
    public Action _clickAction;
}