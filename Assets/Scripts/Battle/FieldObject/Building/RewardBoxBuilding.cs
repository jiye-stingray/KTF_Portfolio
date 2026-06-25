using Cysharp.Threading.Tasks;
using Spine.Unity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class RewardBoxBuilding : BaseBuilding
{
    public override async UniTask Init(int idx)
    {
        _idx = idx;
        _collider.isTrigger = true;

        await SetSpine();

        // Spine SkeletonAnimation은 SpriteRenderer 를 사용하지 않고 MeshRenderer을 사용함.
        // 그렇기떄문에 2D Sort Axis가 안먹히게 되는데 SortingGroup을 SpriteRenderer, MeshRenderer을같이 계산함.
        SortingGroup tSort = Utils.GetOrAddComponent<SortingGroup>(gameObject);
        tSort.sortingOrder = SortingLayers.UNIT;

        _SpeechBox = Managers.Instance.GetUIManager().ShowUIBase<UISpeechBox>("UISpeechBox", UIManager.SpeechCanvas);
        _SpeechBox.InitData(idx, this);
        _SpeechBox.worldOffset = new Vector3(0, 3, 0);
        _SpeechBox.Close();
    }

    public override void SuccessSpeechBtnClick()
    {
        Managers.Instance.GetObjectUnitManager().playerSquad._circleCollider.enabled = false;
        StartCoroutine(ChangeBuildingCoroutine());
    }

    protected override async UniTask SetSpine()
    {
        string spineResource = "Object_Event_Treasure";
        SkeletonDataAsset skeletonData = await Managers.Instance.GetResObjectManager().LoadAsync<SkeletonDataAsset>(
            $"{spineResource}/{spineResource}_SkeletonData.asset");
        if (skeletonData == null)
            return;

        _spineAnimation.SetSpine(skeletonData);
        _spineAnimation.SetAnimation("idle", true);
    }
    
    public IEnumerator ChangeBuildingCoroutine()
    {
        string animationName = "action";
        Spine.Animation anim = _spineAnimation.FindAnimation(animationName);
        if (anim == null)
            yield break;
        
        _spineAnimation.SetAnimation(animationName, false);

        yield return new WaitForSeconds(2.0f);
        
        Managers.Instance._dungeonFieldBase.GameWinClicked();
    }
    
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (_SpeechBox == null)
            return;

        if (collision.tag.Equals("Squad"))
            _enterSquad = true;
    }
    
    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (_SpeechBox == null)
            return;

        if (collision.tag.Equals("Squad"))
            _enterSquad = false;
    }
}