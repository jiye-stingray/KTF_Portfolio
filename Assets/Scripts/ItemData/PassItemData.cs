using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PassItemData : ItemData
{
    public int passType;

    public int passXp;            // 경험치
    public int passLevel = 1;     // 현재 레벨
    public bool isPremium;
    public int freeGetLevel;           // 보상 받은 레벨
    public int premiumGetLevel;           // 보상 받은 레벨
    public DateTime startTime;
    public DateTime endTime;

    public PassGroup data => ClientLocalDB_Simple.GetData<PassGroup>(DBKey.PassGroup,(int)passType);

    public bool isEnd => isPremium && (data.PassMaxLevel == freeGetLevel);       // 마지막 보상을 다 받았는지

    public Pass CurrentPassData => ClientLocalDB_Simple.GetData<Pass>(DBKey.Pass, $"{passLevel}_{passType.ToString()}");

    public string ProductID => data.ProductID;

}
