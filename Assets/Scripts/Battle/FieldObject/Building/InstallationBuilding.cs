using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UniRx;
using UnityEngine;
using Animation = Spine.Animation;
using static Define;

public class InstallationBuilding : BaseBuilding
{
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private GameObject _openEffect;

    public override void Awake()
    {
        base.Awake();
        _openEffect = transform.Find("Boom").gameObject;
        _openEffect.SetActive(false);
    }

    public override async UniTask Init(int idx)
    {
        await base.Init(idx);

        BuildingInfo buildingInfo = BuildingData._data;
        _disposables.Clear();
        if (buildingInfo.BuildOpenConditionType == EBuildingOpenConditionType.UserLevel)
        {
            ReactiveProperty<int> levelValue = UserInfoData.userLevel;
            levelValue
                .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
                .TakeUntilDestroy(this)
                .Subscribe(newValue =>
                {
                    CreateBuildingObject();
                }).AddTo(_disposables);
        }
        else if (buildingInfo.BuildOpenConditionType == EBuildingOpenConditionType.BuildingOpen)
        {
            _disposables.Clear();
            ReactiveProperty<bool> openValue = UserInfoData.GetInstallationBuilding(buildingInfo.BuildOpenConditionValue)._isBuild;
            openValue
                .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
                .TakeUntilDestroy(this)
                .Subscribe(newValue =>
                {
                    CreateBuildingObject();
                }).AddTo(_disposables);
        }
        else if (buildingInfo.BuildOpenConditionType == EBuildingOpenConditionType.GuideQuestClearID
                 || buildingInfo.BuildOpenConditionType == EBuildingOpenConditionType.DungeonQuestClearID)
        {
            _disposables.Clear();
            ReactiveProperty<int> guideValue = UserInfoData._currentGuideQuestId;
            guideValue
                .DistinctUntilChanged()     // 값이 같으면 무시 (옵션)
                .TakeUntilDestroy(this)
                .Subscribe(newValue =>
                {
                    CreateBuildingObject();
                }).AddTo(_disposables);
        }
        
        CreateBuildingObject();
    }

    public virtual void ChangeSpeechBox(int idx)
    {
        if (_SpeechBox != null) DestroyImmediate(_SpeechBox.gameObject);

        if (BuildingData.isOpen)
        {
            _SpeechBox = Managers.Instance.GetUIManager()
                .ShowUIBase<UISpeechBox>("UISpeechBox", UIManager.SpeechCanvas);
            _SpeechBox.InitData(idx, this);
            _SpeechBox.Close();
        }
        else
        {
            if (BuildingData._data.BuildOpenConditionType == EBuildingOpenConditionType.Dialogue)
                return;
            
            GameObject tButton = Managers.Instance.GetUIManager()
                .ShowUIBase<UIPayToUnlockSpeechBox>("UIPayToUnlockSpeechBox", UIManager.SpeechCanvas).gameObject;
            _SpeechBox = tButton.GetComponent<UIPayToUnlockSpeechBox>();
            _SpeechBox.InitData(idx, this);
            _SpeechBox.Close();
        }
    }

    public virtual void ChangeBuildingObject()
    {
        StartCoroutine(ChangeBuildingCoroutine());
    }

    protected virtual IEnumerator ChangeBuildingCoroutine()
    {
        string animationName = "action";
        _buildingState = EBuildingState.Action;
        Animation anim = _spineAnimation.FindAnimation(animationName);
        if (anim == null)
            yield break;

        Squad.BattleStop();
        _spineAnimation.SetAnimation(animationName, false);
        yield return new WaitForSeconds(anim.Duration);
        CreateBuildingObject();
        _openEffect.SetActive(true);
        if (_SpeechBox != null)
            _SpeechBox.gameObject.SetActive(true);
        Squad.BattleStart();
        
        // 다이알로그 추가
        if (_idx == 2006)
            Squad.StartDialogue(7);
    }

    public virtual void CreateBuildingObject()
    {
        if (this == null || gameObject == null)
            return;

        if (_spineAnimation == null)
            return;
        
        _buildingState = BuildingData.isOpen ? EBuildingState.Clear : EBuildingState.Idle;
        _spineAnimation.SetAnimation(BuildingData.isOpen ? ObjectAnimationName.CLEAR : ObjectAnimationName.IDLE, true);
        ChangeSpeechBox(_idx);
    }

    public override void ActiveBuilding()
    {
        BuildingData._isBuild.Value = true;

        Save();
        ChangeBuildingObject();
    }

    public void SetFlip(bool flip)
    {
        _spineAnimation.SetFlip(flip);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}