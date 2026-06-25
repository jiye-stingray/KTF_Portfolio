using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Define;

public enum EGachaGrade
{
    NORMAL = 0,
    RARE = 1,
    EPIC = 2,
    LEGEND = 3
}

public class UIGachaResultPopup : UISubBase
{
    
    [SerializeField] List<GachaScrollviewItem> _scrollViewItems;
    [SerializeField] protected GachaCostButton _oneGachaCostBtn;
    [SerializeField] protected GachaCostButton _tenGachaCostBtn;
    [SerializeField] GameObject _buttonArea;
    [SerializeField] GameObject _skipButton;

    [SerializeField] GameObject _scrollObj;
    [SerializeField] ScrollPaperRevealUI _scrollPaperReveal;

    private EGachaType _gachaType;
    private EGachaCountType _gachaCountType;
    private GachaRewardListDto[] _gachaRewardList;
    private GachaGroup _gachaGroupData;

    [SerializeField] Animator _gachaScrollAnim;
    
    private readonly Dictionary<EGachaGrade, string> GachaAnimationName = new()
    {
        { EGachaGrade.NORMAL, "1_Gacha_N" },
        { EGachaGrade.RARE,   "2_Gacha_R" },
        { EGachaGrade.EPIC,   "3_Gacha_E" },
        { EGachaGrade.LEGEND, "4_Gacha_L" }
    };

    [SerializeField] GameObject _SealImg;           // 연출용 옥쇄

    public void Init(EGachaType gachaType, EGachaCountType gachaCountType, GachaRewardListDto[] gachaRewardList)
    {
        _gachaType = gachaType;
        _gachaCountType = gachaCountType;
        _gachaRewardList = gachaRewardList;
        EGachaGrade gachaGrade = GetGachaGrade(gachaRewardList);
        _scrollObj.gameObject.SetActive(true);
        _scrollPaperReveal.ResetReveal();

        _gachaGroupData = ClientLocalDB_Simple.GetData<GachaGroup>(DBKey.GachaGroup, _gachaType);
        SetGachaRewardList();

        _skipButton.SetActive(true);
        _oneGachaCostBtn.gameObject.SetActive(Managers.Instance.GetTutorialManager()._currentSequenceId != SequenceID.Gacha 
            && (gachaCountType == EGachaCountType.One || gachaCountType == EGachaCountType.Ad));
        _tenGachaCostBtn.gameObject.SetActive(Managers.Instance.GetTutorialManager()._currentSequenceId != SequenceID.Gacha &&
            (gachaCountType == EGachaCountType.Ten));
        _oneGachaCostBtn.Init(_gachaType, EGachaCountType.One);
        _tenGachaCostBtn.Init(_gachaType, EGachaCountType.Ten);


        _SealImg.gameObject.SetActive(false);
        UIManager.TopCurrencyUI.gameObject.SetActive(false);
        _buttonArea.SetActive(false);
    }
     
    private void SetGachaRewardList()
    {
        for (int i = 0; i < _scrollViewItems.Count; i++)
        {
            GachaScrollviewItem scrollviewItem = _scrollViewItems[i];
            scrollviewItem.gameObject.SetActive(false);
            if(i >= _gachaRewardList.Length)
                continue;

            scrollviewItem.SetData(_gachaRewardList[i]);
        }
    }

    private EGachaGrade GetGachaGrade(GachaRewardListDto[] gachaRewardList)
    {
        EGachaGrade grade = EGachaGrade.NORMAL;
        foreach (var reward in gachaRewardList)
        {
            if (reward.type == 0)
            {
                UnitData unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, reward.tableId);
                if(unitData == null)
                    continue;
                
                EGachaGrade tempGrade = EGachaGrade.NORMAL;
                if(unitData.StartGrade == EGradeType.Common)
                    tempGrade = EGachaGrade.NORMAL;
                else if (unitData.StartGrade == EGradeType.Rare)
                    tempGrade = EGachaGrade.RARE;
                else if (unitData.StartGrade == EGradeType.Epic)
                    tempGrade = EGachaGrade.EPIC;
                else if (unitData.StartGrade == EGradeType.Legendary)
                    tempGrade = EGachaGrade.LEGEND;
                
                if(tempGrade > grade)
                    grade = tempGrade;
            }
        }
        
        return grade;
    }

    Coroutine ScrollActionCor;
    public void ScrollEndAction()
    {
        StartCoroutine(GachaDirectorAnimCor());
    }

    /// <summary>
    /// 가챠 시작 애니메이션
    /// </summary>
    /// <returns></returns>
    IEnumerator GachaDirectorAnimCor()
    {
        bool isBasic = true;
        for (int i = 0; i < _gachaRewardList.Length; i++)
        {
            GachaRewardListDto reward = _gachaRewardList[i];
            if (reward.type == 1) continue;
            UnitData unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, reward.tableId);
            if(unitData.StartGrade >= EGradeType.Legendary)
                isBasic = false;
        }

        // 애니메이션 트리거 
        _gachaScrollAnim.SetBool("isBasic", isBasic);
        _gachaScrollAnim.SetTrigger("GachaTrgger");

        // Animator가 상태에 진입할 때까지 1프레임 대기
        yield return null; 
        Managers.Instance.Sound.PlaySFX("Effect", "GachaStart");
        AnimatorStateInfo stateInfo = _gachaScrollAnim.GetCurrentAnimatorStateInfo(0);
        // Loop가 아닌 애니메이션만 대상으로 함. 애니메이션 끝날 때 까지 대기
        while (stateInfo.normalizedTime < 1f)
        {
            yield return null;
            stateInfo = _gachaScrollAnim.GetCurrentAnimatorStateInfo(0);
        }

    
        _scrollObj.gameObject.SetActive(false);
        ScrollActionCor = StartCoroutine(GachaScrollviewItemCor());
    }

    bool isGachaScrollCor;
    /// <summary>
    /// 가챠 스크롤 
    /// </summary>
    /// <returns></returns>
    IEnumerator GachaScrollviewItemCor()
    {
        isGachaScrollCor = true;
        for (int i = 0; i < _gachaRewardList.Length; i++)
        {
            GachaScrollviewItem scrollviewItem = _scrollViewItems[i];
            scrollviewItem.gameObject.SetActive(true);
            Managers.Instance.Sound.PlaySFX("Effect", "GachaResultSound");
            yield return StartCoroutine(scrollviewItem.SetRewardActionCor());
        }
        isGachaScrollCor=false;
        UIManager.TopCurrencyUI.gameObject.SetActive(true);
        _buttonArea.gameObject.SetActive(true);
        _skipButton.SetActive(false);
    }

    public void SkipBtnClick()
    {
        if(isGachaScrollCor)
        {
            StopCoroutine(ScrollActionCor);
            isGachaScrollCor= false;

            for (int i = 0; i < _gachaRewardList.Length; i++)
            {
                GachaScrollviewItem scrollviewItem = _scrollViewItems[i];
                scrollviewItem.gameObject.SetActive(true);
                scrollviewItem.SetReward();
            }
        }
        isGachaScrollCor = false;
        UIManager.TopCurrencyUI.gameObject.SetActive(true);
        _buttonArea.SetActive(true);
        _skipButton.SetActive(false);
    }

    public override void ClickCloseBtn()
    {
        UIManager.TopCurrencyUI.gameObject.SetActive(true);
        base.ClickCloseBtn();
    }
}
