using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResonanceItemData : ItemData
{
    public int _index;
    public int _characterId;        // character 없으면 0

    public bool _isLock;        // 해금여부
    public DateTime _expireDate;        // 남은 시간

    public CharacterClassItemData _characterItemData => ReturnCharacterItemData();      // 없으면 null
    public CharacterClassItemData ReturnCharacterItemData()
    {
        return Managers.Instance.UserInfo().GetCharacterItemData(_characterId);
    }
}
