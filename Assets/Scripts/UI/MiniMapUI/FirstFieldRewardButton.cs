using Best.HTTP.SecureProtocol.Org.BouncyCastle.Asn1.X9;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class FirstFieldRewardButton : EventRewardButton
{
    int _difficultyLevel;
    FieldItemData _fieldItemData;
    FieldDetail _fieldDetail;
    [SerializeField] GameObject[] _able;

    bool _isGet;
    public void SetData(int fieldId, int difficultyLevel, bool isGet)
    {
        _difficultyLevel = difficultyLevel;
        _fieldItemData = userinfo._dicFieldItemData[fieldId];
        _fieldDetail = ClientLocalDB_Simple.GetData<FieldDetail>(DBKey.FieldDetail, $"{fieldId}_{difficultyLevel}");

        // 최초 보상 Setting
        RewardData rewardData = new RewardData()
        {
            ID = _fieldDetail.ID,     // not use
            RewardType = new ERewardType[] { ERewardType.Currency },
            RewardId = new int[] { _fieldDetail.FirstClearReward },
            RewardValue = new int[] { _fieldDetail.FirstClearRewardValue }
        };
        _rewardData = rewardData;

        _isGet = isGet;
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();

        // 획득 가능 상태 체크
        _GetImg.SetActive(_isGet);

        bool able = _difficultyLevel <= _fieldItemData.difficultyLevel;
        _able[0].SetActive(able && !_isGet);
        _able[1].SetActive(_able[0].activeSelf);
    }

    public override void Click()
    {
        if (_isGet) return;
        #if USE_SERVER
        Managers.Instance.GetServerManager().OnPostGetFirstClearReward(_fieldItemData.ID, _difficultyLevel);
    #endif
    }
}
