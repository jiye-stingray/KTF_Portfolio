using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 경쟁 컨텐츠 에서 랭킹을 보여줄 때 사용
/// ex. rangkind dungeon , pvp
/// </summary>

[System.Serializable]
public class RankingItemData : ItemData
{
    public int ranking;
    public string name;
    public int level;
    public int thumbnail;
    public int frame;
    public int score;
    public float totalPercent;
}
