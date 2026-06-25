using System.Collections;
using UnityEngine;
using static Define;
using Animation = Spine.Animation;

public class FogPointBuilding : InstallationBuilding
{
    FogObject FogObject => MapManager.GetFogObject(BuildingData._data.ID);

    public override void ChangeSpeechBox(int idx)
    {
        if (_SpeechBox != null) DestroyImmediate(_SpeechBox.gameObject);

        if (!BuildingData.isOpen)
        {
            GameObject tButton = Managers.Instance.GetUIManager()
                .ShowUIBase<UIPayToUnlockSpeechBox>("UIPayToUnlockSpeechBox", UIManager.SpeechCanvas).gameObject;
            _SpeechBox = tButton.GetComponent<UIPayToUnlockSpeechBox>();
            _SpeechBox.InitData(idx, this);
            _SpeechBox.Close();
        }
    }

    protected override IEnumerator ChangeBuildingCoroutine()
    {
        string animationName = "action";
        Animation anim = _spineAnimation.FindAnimation(animationName);
        if (anim == null)
            yield break;

        Squad.BattleStop();
        _spineAnimation.SetAnimation(animationName, false);
        yield return new WaitForSeconds(anim.Duration);
        CreateBuildingObject();
        if (FogObject != null)
            yield return StartCoroutine(FogObject.FinishCoroutine());
        yield return null;
        MapManager.GenerateMap(_zoneIndex);
        Squad.BattleStart();
    }
    
    public override void CreateBuildingObject()
    {
        if (this == null || gameObject == null)
            return;

        if (_spineAnimation == null)
            return;
        
        base.CreateBuildingObject();
        gameObject.SetActive(BuildingData._data.BuildOpenConditionType != EBuildingOpenConditionType.BuildingOpen || BuildingData.isCondition);

        if (BuildingData._data.BuildOpenConditionType == EBuildingOpenConditionType.BuildingOpen && UserInfoData.GetInstallationBuilding(BuildingData._data.BuildOpenConditionValue)._isOpening)
        {
            _spineAnimation.SetAnimation("action2", false);
            _spineAnimation.AnimationStop();
        }
    }
}