using Assets.SimpleSignIn.Google.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UITowerDungeonEntrance : UIBase
{
    [SerializeField] TowerDungeonEntranceItem[] _towerDungeonItemEntranceItems;
    [SerializeField] Image _factionBg;
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] TMP_Text _weekTxt;
    [SerializeField] TMP_Text _countTxt;

    [SerializeField] GameObject _gray;
    [SerializeField] GameObject _clear;
    EFactionType _factionType;
    DayOfWeek[] _dayofWeek => TowerDungeonWeek[_factionType];
    int _currentClearLevel;
    int _dayCnt;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public void InitDataOpenToStack(EFactionType factionType)
    {
        _factionType = factionType;
        _nameTxt.text = TowerDungeonName[_factionType];
        _countTxt.gameObject.SetActive(_factionType != EFactionType.All);
        if (_factionType != EFactionType.All)
        {
            string s = string.Join(", ", _dayofWeek.Select(d => KoDayShort[(int)d]));
            _weekTxt.text = s;
            _dayCnt = Utils.GetTowerDungeonDayCount(factionType);
            _countTxt.text = $"일일입장:{_dayCnt}/{3}";
        }
        else
            _weekTxt.text = "모든 요일";
        
        _factionBg.sprite = Managers.Instance.GetResObjectManager().Load<Sprite>($"Texture/Dungeon/DungeonMainImg_Tower_{_factionType}");
        _currentClearLevel = (int)Utils.GetDungeonClearValue(EDungeonType.Tower, _factionType);

        OpenToStack();
    }

    public override void Refresh()
    {
        List<int> LevelList = GetLevelList(_currentClearLevel + 1);     // 새롭게 클리어할 던전이 가운데에 세팅

        for (int i = 0; i < LevelList.Count; i++)
        {
            // 역순으로 세팅
            _towerDungeonItemEntranceItems[LevelList.Count - 1 - i].Init(_factionType, LevelList[i]);
        }

        _gray.SetActive(_factionType != EFactionType.All && _dayCnt <= 0);
        _clear.SetActive(_currentClearLevel ==
            Managers.Instance.GetSimpleDBManager().GetDungeonMaxLevel(EDungeonType.Tower,_factionType));
    }

    private List<int> GetLevelList(int x)
    {
        int min = 1;
        int max = ClientLocalDB.GetDungeonMaxLevel(EDungeonType.Tower, _factionType);

        // X를 중심으로 앞뒤 2개씩
        int start = x - 2;
        int end = x + 2;

        // 범위 벗어나면 조정
        if (start < min)
        {
            end += (min - start);
            start = min;
        }
        if (end > max)
        {
            start -= (end - max);
            end = max;
        }

        // 다시 한 번 최소 범위 보정
        start = Math.Max(start, min);
        end = Math.Min(end, max);

        // 결과 리스트 만들기
        return Enumerable.Range(start, end - start + 1).ToList();
    }

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public void Click()
    {

        if(_clear.activeSelf)
        {
            UIManager.ShowCommonToastMessage("다음 업데이트를 기대해주세요!");
            return;
        }
        else  if(_gray.activeSelf)
        {
            UIManager.ShowCommonToastMessage("일일 입장 횟수를 초과하였습니다.");
            return;
        }

        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        //// 장비 갯수 카운트 후 입장 불가 체크
        TowerDungeon dungeon = ClientLocalDB_Simple.GetData<TowerDungeon>(DBKey.TowerDungeon, $"{_factionType}_{_currentClearLevel + 1}");
        RewardData reward = ClientLocalDB_Simple.GetData<RewardData>(DBKey.DungeonReward, dungeon.RewardID);

        int count = 0;
        for (int i = 0; i < reward.RewardType.Length; i++)
        {
            if (reward.RewardType[i] == ERewardType.EquipmentBox)
                count += reward.RewardValue[i];

        }
        if (UserInfoData._dicEquipmentItemData.Count + count > 300)      // 갯수 db에 따로 X 
        {
            // 팝업 띄우기
            Managers.Instance.GetUIManager().ShowConfirmPopUp("가방의 공간이 부족합니다", "( 클릭 시 장비 화면으로 이동합니다 )",
            () =>
            {
                Managers.Instance.GetUIManager().UIEquipmentSetting.OpenToStack();
            });

            return;
        }

        UIManager.UIDeckSetting.InitContentType(EContent.Tower, _currentClearLevel + 1, _factionType);
        UIManager.UIDeckSetting.OpenToStack();
    }
}
