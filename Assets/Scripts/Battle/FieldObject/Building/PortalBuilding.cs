using Cysharp.Threading.Tasks;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using static Define;

public class PortalBuilding : BaseBuilding
{
    public bool _isTownPortal;
    public PortalBuilding _destPortal;

    //id - 0 : 마을에 존재하는 포탈,  1 : 유저가 소환 하는 포탈 
    public override async UniTask Init(int idx)
    {
        _idx = idx;
        _collider.isTrigger = true;

        await SetSpine();

        // Spine SkeletonAnimation은 SpriteRenderer 를 사용하지 않고 MeshRenderer을 사용함.
        // 그렇기떄문에 2D Sort Axis가 안먹히게 되는데 SortingGroup을 SpriteRenderer, MeshRenderer을같이 계산함.
        SortingGroup tSort = Utils.GetOrAddComponent<SortingGroup>(gameObject);
        tSort.sortingOrder = Define.SortingLayers.UNIT;

        _isTownPortal = (EPortalType)idx == EPortalType.TownPortal;

        _SpeechBox = Managers.Instance.GetUIManager().ShowUIBase<UISpeechBox>("UISpeechBox", UIManager.SpeechCanvas);
        _SpeechBox.InitData(idx, this);
        _SpeechBox.Close();

        // Debug.LogError("DummyPortal Init");
    }

    public override void SuccessSpeechBtnClick()
    {
        var playerSquad = Managers.Instance.GetObjectUnitManager().playerSquad;

        playerSquad.TeleportHeroes(_destPortal.GetZoneIndex(), _destPortal.transform.position);

        if (_isTownPortal)
        {
            _destPortal.gameObject.SetActive(false);
            gameObject.SetActive(false);

            Managers.Instance.GetServerManager().OnOffPortal();
        }
    }

    protected override async UniTask SetSpine()
    {
        string spineResource = "Object_Event_Portal_1";
        SkeletonDataAsset skeletonData = await Managers.Instance.GetResObjectManager().LoadAsync<SkeletonDataAsset>(
            $"{spineResource}/{spineResource}_SkeletonData.asset");
        if (skeletonData == null)
            return;

        _spineAnimation.SetSpine(skeletonData);
        _spineAnimation.SetAnimation("idle", true);
    }

    public void ConnectPortal(PortalBuilding destPortal)
    {
        _destPortal = destPortal;
    }
}