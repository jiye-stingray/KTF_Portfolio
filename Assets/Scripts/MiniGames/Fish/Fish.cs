using System;
using UnityEngine;

[Serializable]
public class Fish
{
    public int index;
    public string fishName;

    /// <summary>
    /// 실제 물고기 스프라이트를 찾기 위한 코드 (ex: Resources/Fish/fish01.png)
    /// </summary>
    public string fishCode;

    /// <summary>
    /// SMALL / MEDIUM / LARGE / TRESH(원본 데이터 표기 유지) 등
    /// </summary>
    public string rare;

    public string description;
    public string rewardCode;
    public int rewardCount;
    public int RankingPoint;

    /// <summary>
    /// 랜덤 선택 가중치(값이 클수록 더 자주 등장)
    /// </summary>
    public int SpokeWeight;

    [NonSerialized] public Sprite fishSprite;

    public override string ToString()
        => $"Fish[{index}] {fishName} ({rare}) Weight:{SpokeWeight} Reward:{rewardCode} x{rewardCount} RP:{RankingPoint}";
}