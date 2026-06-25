using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIDungeonEntrance : UIBase
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] Image _img;
    [SerializeField] TMP_Text _difficultyTxt;
    [SerializeField] SwitchDataButton _switchDataRightBtn;
    [SerializeField] SwitchDataButton _switchDataLeftBtn;
    [SerializeField] TMP_Text _clearTxt;

    [Header("Content")]
    [SerializeField] ScrollRectDynamicPopulator _scrollrect;
    [SerializeField] GridLayoutGroup _gridLayoutGroup;
    [SerializeField] RectTransform _content;

    [SerializeField] TMP_Text _keyCntTxt;
    [SerializeField] DungeonEntranceButton _entranceBtn;

    public Dungeon _dungeonData;
    private RewardData _dungeonRewardData;
    EDungeonType _dungeonType;
    IndexWrapper _index = new IndexWrapper();
    int _difficultyLevel = 1;

    public UISubWipeoutDungeon _uiSubWipeoutDungeon = null;
    public UIBattleWinPopup _uiBattleWinPopup = null;

    private int _dungeonMaxLevel;
    
    public void InitType(EDungeonType type)
    {
        _dungeonType = type;
        _dungeonMaxLevel = ClientLocalDB.GetDungeonMaxLevel(_dungeonType);
        _difficultyLevel = Mathf.Min((int)Utils.GetDungeonClearValue(type) + 1, _dungeonMaxLevel);

        _img.sprite = Managers.Instance.GetResObjectManager().Load<Sprite>($"Texture/Dungeon/DungeonMainImg_{_dungeonType}");

        SetLevel(_difficultyLevel);
        _switchDataLeftBtn.Init(_index, 1, _dungeonMaxLevel, ChangeDifficulty);
        _switchDataRightBtn.Init(_index, 1, _dungeonMaxLevel, ChangeDifficulty);
    }

    private void SetLevel(int difficultyLevel)
    {
        _difficultyLevel = difficultyLevel;
        _index._index = _difficultyLevel;
        _dungeonData = ClientLocalDB_Simple.GetData<Dungeon>(DBKey.Dungeon, $"{_dungeonType}_{_difficultyLevel}");
        _dungeonRewardData = ClientLocalDB_Simple.GetData<RewardData>(DBKey.DungeonReward, _dungeonData.RewardID);
        _nameTxt.text = _dungeonData.Name;
        
        DrawClearDescTxt();
        DrawReward();
    }

    public override void Refresh()
    {
        // 던전 레벨 draw 
        _difficultyTxt.text = $"{_difficultyLevel} 단계";

        _entranceBtn.Init(_difficultyLevel, _dungeonType);
    }

    private void DrawClearDescTxt()
    {
        switch (_dungeonType)
        {
            case EDungeonType.Equipment:
                _clearTxt.text = EquipmentDungeonClearDescription;
                break;
            case EDungeonType.Gold:
                _clearTxt.text = GoldDungeonClearDescription;
                break;
        }
    }

    private void DrawReward()
    {
        // dummy setting ( 추후 다른 UI 에서도 사용할 시 내부에서 적용) 2026/05/08
        _gridLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
        _content.anchorMin = new Vector2(0f, 0f);
        _content.anchorMax = new Vector2(0f, 1f);  // 세로 stretch OK
        _content.pivot    = new Vector2(0f, 0.5f);

        var items = new List<ItemData>();
        for (int i = 0; i < _dungeonRewardData.RewardType.Length; i++)
        {
            items.Add(new RewardItemData
            {
                _rewardType = _dungeonRewardData.RewardType[i],
                _index      = _dungeonRewardData.RewardId[i],
                _count      = _dungeonRewardData.RewardValue[i]
            });
        }

        _scrollrect.Init((cell, data, index) => cell.SetData(data, index));
        _scrollrect.Populate(items);

    }
    
    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public void ChangeDifficulty(int index)
    {
        SetLevel(index);
        // 한쪽 전환시 양쪽 다 update 필요
        _switchDataRightBtn.UpdateGray();
        _switchDataLeftBtn.UpdateGray();
        Refresh();
    }

    public void OpenUIWipeOutDungeon()
    {
        if (_uiSubWipeoutDungeon == null)
            _uiSubWipeoutDungeon = UIManager.ShowUISubBase<UISubWipeoutDungeon>(this, "UISubWipeoutDungeon");
        
        _uiSubWipeoutDungeon.InitData(_dungeonData);
    }

    public void OpenBattleWinPopup(EContent contentType, EFactionType factionType, int level, double point, bool isWipeOut, RewardBundleDto rewardBundle)
    {
        if(_uiBattleWinPopup == null)
            _uiBattleWinPopup = UIManager.ShowUISubBase<UIBattleWinPopup>(this, "UIBattleWinPopup");
        _uiBattleWinPopup.Init(contentType, factionType, level, point, isWipeOut, rewardBundle);
    }
}
