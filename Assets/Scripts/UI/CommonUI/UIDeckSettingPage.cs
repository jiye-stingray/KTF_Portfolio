using PolyAndCode.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIDeckSettingPage : UIBase, IRecyclableScrollRectDataSource
{
    [SerializeField] List<SquadCharacterButton> _squadCharacterBtns;
    [SerializeField] TMP_Text _deckCountTxt;
    [SerializeField] RectTransform _content;
    [SerializeField] Button _battleStartBtn;
    [SerializeField] private TMP_Text _battlePowerText;

    List<CharacterClassItemData> _dataList = new List<CharacterClassItemData>();
    [SerializeField] RecyclableScrollRect _scrollView;
    [SerializeField] GameObject _saveDeckBtn;
    [SerializeField] FactionSynergyItem[] _synergyItems; 

    public EContent _contentType = EContent.Field;           // 스크립트로 Setting 해주기
    public EFactionType _factionType; // 타워던전에서 사용
    public EServerContentType _serverContentType;
    
    private IndexWrapper _tempDeckSettingId = new IndexWrapper();               // Setting 하려고 하는 ID (첫번째로 누른)
    private IndexWrapper _tempDeckSettingIndex = new IndexWrapper();
    List<SquadCharacterInventoryScrollviewItem> _itemList = new List<SquadCharacterInventoryScrollviewItem>();

    private DeckData _deckData;
    public DeckData _tempDeckData;
    
    public int _tempSlotIdx = -1;           // 연출용
    private int _level;
    private Squad Squad => Managers.Instance.GetObjectUnitManager().playerSquad;
    
    private UISubFactionSynergyDetail _factionSynergyDetail;

    private void Awake()
    {
        for (int i = 0; i < _squadCharacterBtns.Count; i++)
        {
            SquadCharacterButton btn = _squadCharacterBtns[i];
            btn.Init(i, OnClickSetSquad);
        }
    }

    public void InitContentType(EContent contentType, int level = 0, EFactionType factionType = EFactionType.All)
    {
        _init = false;
        _contentType = contentType;
        _factionType = factionType;
        _serverContentType = ReturnServerDungeonType(_contentType, factionType);
        _level = level;

        _tempDeckSettingId._index = -1;
        _tempDeckSettingIndex._index = -1;

        _saveDeckBtn.SetActive(_contentType == EContent.Field);

        InitDeckData();
        InitScrollItemData();
        Refresh();
    }

    private void InitDeckData()
    {
        EServerContentType contentType = ReturnServerDungeonType(_contentType, _factionType);
        _deckData = UserInfoData.GetDeckData(contentType);
        _tempDeckData = new DeckData(contentType);
        int[] deck = _deckData.idList;
        for (int i = 0; i < _tempDeckData.idList.Length; i++)
        {
            _tempDeckData.idList[i] = deck[i];   
        }
    }
    
    //저장 취소 했을때
    public void ResetDeckData()
    {
        _tempDeckSettingId._index = -1;
        _tempDeckSettingIndex._index = -1;

        InitDeckData();
        Refresh();
    }

    public void InitScrollItemData()
    {
        _dataList = ReturnCharacterList().ToList();
        _scrollView.Initialize(this);
    }

    #region Recycle ScrollView
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as SquadCharacterInventoryScrollviewItem;
        item.Init(_dataList[index], _tempDeckData, index, OnClickSetSquad);
    }
    #endregion

    public override void Refresh()
    {
        _battleStartBtn.gameObject.SetActive(_contentType != EContent.Field);
        _deckCountTxt.text = $"{_tempDeckData.idList.Count(a => a > 0)}/{_tempDeckData.idList.Length}";
        RefreshSquadCharacter();
        RefreshWaitingCharacterList();
        CalculateBattlePower();
        RefreshFactionSynergyUI(_tempDeckData.idList);
        _tempSlotIdx = -1;      // 초기화
    }

    /// <summary>
    /// 현재 편성된 덱을 전투력 오름차순으로 자동 정렬
    /// </summary>
    private void AutoSettingDeck()
    {
        if (_tempDeckData == null || _tempDeckData.idList == null || _tempDeckData.idList.Length == 0)
            return;

        var sortedIds = _dataList
            .OrderByDescending(C => C._statusInfo._battlePower)
            .ThenByDescending(c => c._grade)
            .ThenByDescending(c =>
                ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, c.id).StartGrade)
            .ThenBy(c => c.id)
            .Select(c =>  c.id)
            .ToList();

        for (int i = 0; i < _tempDeckData.idList.Length; i++)
        {
            _tempDeckData.idList[i] = (i < sortedIds.Count) ? sortedIds[i] : 0;
        }

    }

    public void AutoSettingDeckBtnClick()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        AutoSettingDeck();
        UIManager.UIDeckSetting.Refresh();
    }

    private void RefreshSquadCharacter()
    {
        var deckSlotList = ClientLocalDB_Simple.GetDB<FieldDeckSlot>(DBKey.FieldDeckSlot).Values.ToList();

        for (int i = 0; i < _squadCharacterBtns.Count; i++)
        {
            if (i < _tempDeckData.idList.Length)
            {
                _squadCharacterBtns[i].gameObject.SetActive(true);
                CharacterClassItemData data = Managers.Instance.UserInfo().GetCharacterItemData(_tempDeckData.idList[i]);
                _squadCharacterBtns[i].SetData(data, _tempDeckData, _tempSlotIdx);
                _squadCharacterBtns[i].Refresh();
            }
            else
            {
                if (_contentType == EContent.Tower)
                {
                    _squadCharacterBtns[i].gameObject.SetActive(false);
                }
                else
                {
                    _squadCharacterBtns[i].gameObject.SetActive(true);
                    int unlockLevel = 0;
                    var slotData = deckSlotList?.Find(s => s.SlotNum == i + 1);
                    if (slotData != null)
                        unlockLevel = slotData.UnlockLevel;

                    _squadCharacterBtns[i].SetLock(unlockLevel);
                }
            }
        }
    }


    public void RefreshWaitingCharacterList()
    {
        _scrollView.ReloadData();
    }

    /// <summary>
    /// 컨텐츠 타입에 따라서 캐릭터 필터링 걸어서 보여줌
    /// </summary>
    /// <returns></returns>
    private List<CharacterClassItemData> ReturnCharacterList()
    {
        List<CharacterClassItemData> characterClassItemList = UserInfoData._dicCharacterItemData.Values.ToList();

        characterClassItemList = characterClassItemList
            .Where(c => c.isOpen)
            .Where(c => c._unitData.Live)
            .Where(c => ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, c.id).Faction == _factionType || _factionType == EFactionType.All)
            .OrderByDescending(C => C._statusInfo._battlePower)
            .ThenByDescending(c => c._grade)
            .ThenByDescending(c =>
                ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, c.id).StartGrade)
            .ThenBy(c => c.id)
            .ToList();

        return characterClassItemList;
    }

    public void Click()
    {
        if (_tempDeckData.idList.Count(a => a > 0) == 0) // 세팅된 캐릭터가 하나도 없을 때
        {
            UIManager.ShowCommonToastMessage("최소 1명은 배치되어야 합니다.");
            return;
        }
        
        UserInfoData.SetDeckData(_serverContentType, _tempDeckData);
        UserInfoData.SaveDungeonDeck();
        UserInfoData.zoneId = Squad._zoneIndex;
        UserInfoData.squadPosition = Squad.transform.position;
        BestHttp_GameManager.OnPostDungeonCurrencyCheck(_contentType, _factionType, _level);
    }

    //필드덱만 체크
    public bool ChangeFieldDeckCheck()
    {
        if (_contentType != EContent.Field)
            return false;

        if (!Utils.AreArraysEquivalent(_deckData.idList, _tempDeckData.idList))
            return true;

        return false;
    }

    private void OnClickSetSquad(int characterID, int slotIndex)
    {
        int tempDeckIdx = _tempDeckData.ReturnDeckIdx(characterID);
        if (tempDeckIdx == -1)     // 전에 클릭한 캐릭터가 deck 에 편성이 되어있지 않다면 캐릭터 편성
        {
            int emptySlot = _tempDeckData.ReturnDeckIdx(0);
            _tempSlotIdx = tempDeckIdx;
            _tempDeckData.ChangeID(characterID, emptySlot);
        }
        else //편성이 이미 되어 있다면 편성 해제
        {
            if (_tempDeckData.idList.Count(a => a > 0) <= 1) // 캐릭터가 하나만 setting 되어 있었을 때
            {
                UIManager.ShowCommonToastMessage("최소 1명은 배치되어야 합니다.");
                return;
            }
            
            _tempSlotIdx = tempDeckIdx;
            _tempDeckData.ChangeID(0, tempDeckIdx);
        }

        UIManager.UIDeckSetting.Refresh();
    }

    public void SaveDeckBtnClick()
    {
        if (!ChangeFieldDeckCheck())
            return;
        
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        SaveFieldDeck();
    }

    public void SaveFieldDeck()
    {
        if (Squad.IsBattleCheck())
        {
            UIManager.ShowCommonToastMessage("전투중에는 덱 변경을 할 수 없습니다.");
            ResetDeckData();
            return;
        }
        Managers.Instance.GetServerManager().OnPostSetMyHeroDeck(_tempDeckData.idList.ToList());
    }

    private void CalculateBattlePower()
    {
        double power = 0;
        foreach (int id in _tempDeckData.idList)
        {
            if (id == 0)
                continue;

            CharacterClassItemData characterItem = UserInfoData._dicCharacterItemData[id];
            power += characterItem._statusInfo._battlePower;
        }
        
        _battlePowerText.text = power.ToString();
    }

    private void RefreshFactionSynergyUI(int[] idList)
    {
        Dictionary<EFactionType, int> factionCounts = ClientLocalDB_Simple.CalculateFactionCount(idList);
        foreach (var item in _synergyItems)
        {
            EFactionType factionType = item.FactionType;
            int count = 0;
            if(factionCounts.TryGetValue(factionType, out int factionCount))
                count = factionCount;
            
            item.SetData(count);
        }
    }

    public void OpenFactionSynergyDetail()
    {
        if (_factionSynergyDetail == null)
        {
            _factionSynergyDetail = UIManager.ShowUISubBase<UISubFactionSynergyDetail>(UIManager.UIDeckSetting, "UISubFactionSynergyDetail");
            _factionSynergyDetail.OpenToStack();
        }
        
        _factionSynergyDetail.Init(_tempDeckData.idList);        
    }
}
