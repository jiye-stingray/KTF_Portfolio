using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapProgressRewardListItem : MonoBehaviour
{
    [SerializeField] TMP_Text _progressTxt;
    [SerializeField] MapProgressRewardButton _rewardBtn;

    int _progress;
    FieldQuestReward _fieldQuestReward;

    public void SetData(int progress,FieldQuestReward fieldQuestReward)
    {
        _progress = progress;
        _fieldQuestReward = fieldQuestReward;
        _rewardBtn.SetData(_progress, fieldQuestReward);

        Refresh();
    }

    private void Refresh()
    {
        _progressTxt.text = _progress.ToString();
    }
}
