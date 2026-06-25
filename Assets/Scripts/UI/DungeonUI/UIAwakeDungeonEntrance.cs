using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIAwakeDungeonEntrance : UIBase
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] TMP_Text _clearTxt;
    [SerializeField] TMP_Text _maxMonsterCntTxt;
    [SerializeField] Transform _rewardIconArea;
    [SerializeField] AwakeDungeonEntranceButton _entranceBtn;
    [SerializeField] CurrencyIcon _currencyIcon;
    
    EDungeonType _dungeonType = EDungeonType.Constellation;
    public Dungeon _dungeonData;
    public UISubWipeoutDungeon _uiSubWipeoutDungeon = null;
    List<RewardItem> _rewardItems = new List<RewardItem>();

    private UIBattleWinPopup _uiBattleWinPopup;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _dungeonData = ClientLocalDB_Simple.GetData<Dungeon>(DBKey.Dungeon, $"{EDungeonType.Constellation}_{0}");
        _nameTxt.text = _dungeonData.Name;
        _clearTxt.text = AwakeDungeonClearDescription;
        return true;

    }
    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public override void Refresh()
    {
        _maxMonsterCntTxt.text = UserInfoData._maxConstellationDungeonMonsterCnt.ToString();
        _entranceBtn.Init();

        DrawReward();
    }

    private void DrawReward()
    {
        if (_rewardItems.Count > 0)
            return;

        List<int> rewardIds = new List<int>();
        List<int> rewardValues = new List<int>();
        int rewardID_1 = ClientLocalDB_Simple.GetData<DungeonSetting>(DBKey.DungeonSetting, "ConstellationDungeonRewardID_1").Value;
        rewardIds.Add(rewardID_1);
        int rewardValue_1 = ClientLocalDB_Simple.GetData<DungeonSetting>(DBKey.DungeonSetting, "ConstellationDungeonRewardValue_1").Value;
        rewardValues.Add(rewardValue_1 * UserInfoData._maxConstellationDungeonMonsterCnt);
        _currencyIcon.Init((ECurrency)rewardID_1);
    }
    
    public void OpenBattleWinPopup(EContent contentType, EFactionType factionType, int level, double point, bool isWipeOut, RewardBundleDto rewardBundle)
    {
        if(_uiBattleWinPopup == null)
            _uiBattleWinPopup = UIManager.ShowUISubBase<UIBattleWinPopup>(this, "UIBattleWinPopup");
        _uiBattleWinPopup.Init(contentType, factionType, level, point, isWipeOut, rewardBundle);
    }
}