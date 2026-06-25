using UnityEngine;

[System.Serializable]
public class WarpPointItemData : ItemData
{
    public WarpPointInfo _warpPointInfo;
    public bool IsLock;
    public bool IsSquadField;
    public Vector2 _warpPointPosition; 
}
