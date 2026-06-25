using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class BattlePassScrollviewItem : ICell
{
    [Header("Show")]
    [SerializeField] bool _isLastShow;

    [Header("UI")]
    [SerializeField] PassRewardButton _freePassRewardBtn;
    [SerializeField] PassRewardButton _PremiumPassRewardBtn;
    [SerializeField] TMP_Text _levelTxt;
    [SerializeField] GameObject _gray;
    [SerializeField] GameObject _passNextLevelObj;
    [SerializeField] PassNextLevelCostButton _passNextLevelCostBtn;


    int _level;
    int _passType;
    public Pass _PassData;      // 무료 패스 

    public void SetData(int index,int level,int passType)
    {
        _index = index;
        _level = level;
        _passType = passType;

        _PassData = ClientLocalDB_Simple.GetData<Pass>(DBKey.Pass, $"{level}_{passType.ToString()}");

        _freePassRewardBtn.SetData(_PassData, false);
        _PremiumPassRewardBtn.SetData(_PassData, true);


        Refresh();
    }

    /// <summary>
    ///  UI 연출용 최대 레벨 보상 최 하단에 고정
    /// </summary>
    /// <param name="level"></param>
    /// <param name="passType"></param>
    /// <param name="freeData"></param>
    /// <param name="paidData"></param>
    public void SetData(int level, int passType)
    {
        _isLastShow = true;

        _level = level;
        _passType = passType;

        _PassData = ClientLocalDB_Simple.GetData<Pass>(DBKey.Pass, $"{level}_{passType.ToString()}");

        _freePassRewardBtn.SetData(_PassData, false);
        _PremiumPassRewardBtn.SetData(_PassData, true);


        Refresh();
    }

    public void Refresh()
    {
        _levelTxt.text = _level.ToString();
        _freePassRewardBtn.Refresh();
        _PremiumPassRewardBtn.Refresh();

        _gray.SetActive(_level > Managers.Instance.UserInfo().GetPassItemData(_passType).passLevel);

        // UI 연출용 최대 레벨 보상 최 하단에 고정
        if (_isLastShow) return;

        _passNextLevelObj.SetActive(!(Managers.Instance.UserInfo().GetPassItemData(_passType).data.LevelUpType == EpassLevelUpType.UserLevel) &&
            _gray.activeSelf && _level == Managers.Instance.UserInfo().GetPassItemData(_passType).passLevel + 1);       // 다음 레벨인 상태
        if (_passNextLevelObj.activeSelf)
        {
            PassGroup passGroup = ClientLocalDB_Simple.GetData<PassGroup>(DBKey.PassGroup, (int)_passType);
            _passNextLevelCostBtn.Init(
                new ECurrency[] { (Define.ECurrency)passGroup.PassLevelUpItem},
                new int[] { passGroup.PassLevelUpNum });
        }
    }

    /// <summary>
    /// 다음 레벨 구매 버튼 클릭
    /// </summary>
    public void BuyNextLevelClickSuccessAction()
    {
        if (_passNextLevelCostBtn.isGray) return;

        PassGroup passGroup = ClientLocalDB_Simple.GetData<PassGroup>(DBKey.PassGroup, (int)_passType);
        UIManager.ShowConfirmPopUp("", $"옥 {passGroup.PassLevelUpNum}개를 사용하여 패스 레벨을 올리시겠습니까?", () =>
        {
#if USE_SERVER
            Managers.Instance.GetServerManager().OnPostPassPurchaseLevel((int)_passType);
#else
            Managers.Instance.GetUIManager().UIBattlePass.BuyNextLevel();
#endif

        });

    }
}
