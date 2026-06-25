using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ETrainingType
{
    Basic,
    Hard,
}

// 전체 데이터
[System.Serializable]
public class TrainingItemData : ItemData
{
    public BasicTraining _trainingBasicData;
    public HardTraining _trainingHardData;

    public int level;
    public bool islevelShow;            // level txt 를 보여주는지 안 보여주는지. 전 레벨 값과 다를 때 true 체크하기
}
