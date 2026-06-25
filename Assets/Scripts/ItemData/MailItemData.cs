using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[Serializable]
public class MailItemData : ItemData
{
    public long mailId = 0;
    public int mailType = 0;
    public string mailTitle = null;
    public string itemName = null;
    
    public DateTime expireDate;
    public int adMailIndex; // 광고우편용
    public int adMailDay; // 광고우편용

    //Reward
    public ERewardType rewardType;
    public int rewardTableID;
    public int rewardCount;
    public string synergyCode;
}
