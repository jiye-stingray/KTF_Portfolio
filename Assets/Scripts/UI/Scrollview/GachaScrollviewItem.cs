using Cysharp.Threading.Tasks;
using PolyAndCode.UI;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static Define;

public class GachaScrollviewItem : MonoBehaviour
{
    [SerializeField] private int _index;
    [SerializeField] private RewardItem _item;

    [SerializeField] private GameObject _characterRoot;
    [SerializeField] private Image _thumbnail;
    [SerializeField] private Image _bgImg;
    [SerializeField] private Image _frame;
    [SerializeField] private Image _frame2;
    [SerializeField] private Image _footHoldImg;

    [SerializeField] private GameObject _holdRoot;
    [SerializeField] private RewardItem _maxGradeRewardItem;

    [SerializeField] private GameObject _newTxt;

    [SerializeField] private TMP_Text _gradeText;

    [SerializeField] private GameObject _effectEpic;
    [SerializeField] private GameObject _effectLegend;

    [SerializeField] SpineAnimation _spineAnimation;
    [SerializeField] Animator _anim;

    public GachaRewardListDto _data;
    
    public void SetData(GachaRewardListDto data)
    {
        _data = data;
        _item.gameObject.SetActive(false);
        _newTxt.SetActive(false);
    }
    
    public IEnumerator SetRewardActionCor()
    {
        if (_data.type == 0)     // 추후 애니메이션 연출 후 실행 
            yield return StartCoroutine(SetCharacterRewardCor());
        else
            SetCurrencyReward();

        yield break;
    }

    /// <summary>
    /// 연출 없는 바로 셋팅하기
    /// </summary>
    public void SetReward()
    {
        if (_data.type == 0)     
            SetCharacterReward();
        else
            SetCurrencyReward();
    }

    /// <summary>
    /// 높은 등급  캐릭터 연출로 인한 코루틴 
    /// </summary>
    /// <returns></returns>
    IEnumerator SetCharacterRewardCor()
    {
        _newTxt.SetActive(_data.isNew);
        _characterRoot.SetActive(true);
        
        UnitData unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, _data.tableId);
        DrawItem(unitData);
        if (_data.isNew || unitData.StartGrade >= EGradeType.Legendary)
        {
            yield return new WaitForSeconds(0.2f); // 사운드 연출용  
            Managers.Instance.Sound.PlaySFX("Effect", "GachaCharacter");
            UIGachaResultCharacterPopup popup = Managers.Instance.GetUIManager().ShowUISubBase<UIGachaResultCharacterPopup>(Managers.Instance.GetUIManager().UIGacha, "UIGachaResultCharacterPopup");
            yield return popup.SetUnitData(unitData,_data.isNew).ToCoroutine();
            popup.StartCoroutine(popup.ActionCoroutine());
            yield return new WaitUntil(() => popup.IsClose);        // 닫힐 때 까지
        }

        // 연출 애니메이션  & 대기
        // 상태 진입까지 대기
        while (!_anim.GetCurrentAnimatorStateInfo(0).IsName("GachaItemReveal"))
            yield return null;

        // 애니메이션 끝날 때까지 대기
        while (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
    }

    /// <summary>
    /// 연출 없이 바로 설정하기
    /// </summary>
    public void SetCharacterReward()
    {
        _characterRoot.SetActive(true);
        _newTxt.SetActive(_data.isNew);

        UnitData unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, _data.tableId);

        DrawItem(unitData);
    }

    private void DrawItem(UnitData unitData)
    {
        _gradeText.text = $"{Define.ReturnGradeString(unitData.StartGrade)}";

        _bgImg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.GachaAtlas, $"Gacha_frame_BG_{unitData.StartGrade}");
        _frame.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.GachaAtlas, $"frame_{unitData.StartGrade}");
        _frame2.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.GachaAtlas, $"frame_{unitData.StartGrade}2");
        _footHoldImg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.GachaAtlas, $"foothold_{unitData.StartGrade}");

        _thumbnail.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.GachaIllustAtlas,
            $"Gacha_Illustration_Cr_{_data.tableId.ToString("D3")}");

        // 편린 (max Grade 시 표기)
        int currencyId = ClientLocalDB_Simple.GetData<GachaSetting>(DBKey.GachaSetting, "GachaSwapCurrency").Value;
        int currencyValue = ClientLocalDB_Simple.GetData<GachaSetting>(DBKey.GachaSetting, $"{unitData.StartGrade}SwapCurrency").Value;
        _maxGradeRewardItem.Init(ERewardType.Currency, currencyId, currencyValue);

        //_item.gameObject.SetActive(_data.maxGrade);
        //_characterRoot.SetActive(!_data.maxGrade);
        _holdRoot.SetActive(!_data.maxGrade);
        _maxGradeRewardItem.gameObject.SetActive(_data.maxGrade);


        //if (_data.maxGrade) return;

        SetCharacterSpine(unitData).Forget();
        
        _effectEpic.SetActive(false);
        _effectLegend.SetActive(false);

        if (unitData.StartGrade >= EGradeType.Legendary)
        {
            _effectEpic.SetActive(false);
            _effectLegend.SetActive(true);
        }
        else if (unitData.StartGrade >= EGradeType.Epic)
        {
            _effectEpic.SetActive(true);
            _effectLegend.SetActive(false);
        }
    }

    private async UniTask SetCharacterSpine(UnitData unitData)
    {
        _spineAnimation.gameObject.SetActive(false);
        string spineName = unitData.Resource;
        SkeletonDataAsset skeletonData = await Managers.Instance.GetResObjectManager().LoadAsync<SkeletonDataAsset>($"{spineName}/{spineName}_SkeletonData.asset");
        if (skeletonData == null)
            return;
        
        _spineAnimation.gameObject.SetActive(true);
        _spineAnimation.SetSpine(skeletonData);
        _spineAnimation.AnimationState.TimeScale = 1.0f;
        _spineAnimation.SetAnimation(CharacterAnimationName.IDLE, true);
    }

    public void SetCurrencyReward()
    {
        _characterRoot.SetActive(false);
        _item.gameObject.SetActive(true);
        _item.Init(ERewardType.Currency, _data.tableId, _data.count);
        _bgImg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.GachaAtlas, $"Gacha_frame_BG_Common");

        _effectEpic.SetActive(false);
        _effectLegend.SetActive(false);
        _spineAnimation.gameObject.SetActive(false);
    }
}
