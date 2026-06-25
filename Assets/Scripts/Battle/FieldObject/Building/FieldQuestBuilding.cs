using Cysharp.Threading.Tasks;
using MarchingBytes;
using Spine.Unity;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections;
using static Define;

public class FieldQuestBuilding : InstallationBuilding
{
    FieldQuest _fieldQuestData;
    UISpeechBubble _speechBubble;

    int _stayTime = 1;

    float _ticTime;
    float _maxTicTime = 5.0f;

    private FollowCamera GameCamera => Managers.Instance.GetCameraManager().FollowCam;

    public override async UniTask Init(int idx)
    {
        _fieldQuestData = ClientLocalDB_Simple.GetData<FieldQuest>(DBKey.FieldQuest, idx);
        await base.Init(idx);
    }

    public override void ChangeSpeechBox(int idx)
    {
        if (_SpeechBox != null) DestroyImmediate(_SpeechBox.gameObject);

        if (!BuildingData.isOpen)
        {
            GameObject speechObject = Managers.Instance.GetUIManager()
                .ShowUIBase<UIPayToUnlockSpeechBox>("UIPayToUnlockSpeechBox", UIManager.SpeechCanvas).gameObject;
            _SpeechBox = speechObject.GetComponent<UIPayToUnlockSpeechBox>();
            _SpeechBox.InitData(idx, this);
            _SpeechBox.Close();
        }
    }

    protected override IEnumerator ChangeBuildingCoroutine()
    {
        string animationName = "action";
        Spine.Animation anim = _spineAnimation.FindAnimation(animationName);
        if (anim == null)
            yield break;

        UIManager.MainInfoUI.Close();
        Squad.BattleStop();
        GameCamera.SetTarget(this.transform);
        GameCamera.SetZoomIn();
        _spineAnimation.SetAnimation(animationName, false);
        yield return new WaitForSeconds(1f);
        SpawnObject();
        yield return new WaitForSeconds(1f);
        GameCamera.ResetZoom();
        yield return new WaitForSeconds(1f);
        CreateBuildingObject();
        GameCamera.SetTarget(Squad.transform);
        UIManager.MainInfoUI.Open();
        Squad.BattleStart();

        // // 추후 다이알로그 추가
        // if (_idx == 50001 || _idx == 50004)
        // {
        //     int dialogueId = _idx == 50001 ? 4 : 5;
        //     Squad.StartDialogue(dialogueId);
        // }
    }

    public override void CreateBuildingObject()
    {
        if (this == null || gameObject == null)
            return;

        if (_spineAnimation == null)
            return;
        
        gameObject.SetActive(BuildingData.isCondition && !BuildingData._isBuild.Value);
        _buildingState = BuildingData.isOpen ? EBuildingState.Clear : EBuildingState.Idle;
        _spineAnimation.SetAnimation(BuildingData.isOpen ? ObjectAnimationName.CLEAR : ObjectAnimationName.IDLE, true);
        ChangeSpeechBox(_idx);
    }
    
    //보상 연출
    private void SpawnObject()
    {
        int rewardCount = _fieldQuestData.RewardCount[0];
        int count = rewardCount / 1000;
        int result = Math.Min(count, 5);
        for (int i = 0; i < result; i++)
        {
            Managers.Instance.GetObjectUnitManager().SpawnFieldDropItem(_fieldQuestData.RewardList[0], transform.position);            
        }
    }
}