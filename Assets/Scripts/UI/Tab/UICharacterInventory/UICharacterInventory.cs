using PolyAndCode.UI;
using SentryToolkit;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UICharacterInventory : UIBase, IRecyclableScrollRectDataSource
{
    [SerializeField] List<CharacterInventoryScrollviewItem> _squadCharacter;

    private List<CharacterClassItemData> _currentVisibleList = new List<CharacterClassItemData>();
    private List<CharacterClassItemData> _notOpenVisibleList = new List<CharacterClassItemData>();
    private List<CharacterClassItemData> _allCharactersList = new List<CharacterClassItemData>();
    public IndexWrapper _currentIndex = new IndexWrapper();
    [SerializeField] TMP_Text _titleBoradTxt;
    [SerializeField] RectTransform _content;
    [SerializeField] GameObject _characterInventoryTab;         //튜토리얼

    [HideInInspector] public UISubCharacterFilter uISubCharacterFilter;
    public EFactionType _filterFactionType = EFactionType.All;

    public bool _isEditing;     // 공명 편집 중일 때
    public Toggle _inventoryToggle;
    public Toggle _edittoggle;

    [Header("ScrollviewArea")]
    [SerializeField] GameObject _inventoryScrollviewArea;
    [SerializeField] Transform _openContent;
    [SerializeField] Transform _notOpenContent;
    [SerializeField] RectTransform _scrollviewRect;

    private const string SCROLLITEM_PATH = "Prefabs/UI/ScrollItem/CharacterInventoryScrollviewItem";
    private List<CharacterInventoryScrollviewItem> _openItems = new List<CharacterInventoryScrollviewItem>();
    private List<CharacterInventoryScrollviewItem> _notOpenItems = new List<CharacterInventoryScrollviewItem>();
    private bool _openContentInited = false;
    private bool _notOpenContentInited = false;

    [SerializeField] GameObject _resonanceScrollviewArea;
    [SerializeField] TMP_Text _resonanceCountTxt;
    [SerializeField] RecyclableScrollRect _resonanceScrollview;

    [SerializeField] TMP_Text _countTxt;
    [SerializeField] private GameObject _redDot;


    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        _inventoryToggle.onValueChanged.AddListener(InventoryToggle);
        _edittoggle.onValueChanged.AddListener(EditToggle);

        return true;
    }

    public override void Open()
    {
        base.Open();

        Refresh();

    }

    public int DataCount => _allCharactersList.Count;

    public bool TryGetDataAt(int index, out CharacterClassItemData data)
    {
        data = null;
        if (index < 0 || index >= _allCharactersList.Count) return false;
        data = _allCharactersList[index];
        return data != null;
    }

    public bool TryFindIndexById(int characterId, out int index)
    {
        index = -1;
        if (_allCharactersList == null || _allCharactersList.Count == 0) return false;
        index = _allCharactersList.FindIndex(d => d != null && d.id == characterId);
        return index >= 0;
    }

    #region Recycle
    public int GetItemCount()
    {
        return UserInfoData._dicResonanceItemData.Count;   // 임시
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as ResonanceScrollviewItem;
        item.Init(UserInfoData._dicResonanceItemData[index], index);
    }

    #endregion

    public override void Refresh()
    {
        #if TUTO
/*        OpenContentBase openContentBase = ClientLocalDB_Simple.GetData<OpenContentBase>(DBKey.ContentsOpen, _characterInventoryTab.name);
        _characterInventoryTab.SetActive(UserInfoData.userLevel.Value >= openContentBase.ConditionValue);*/
        #endif

        if (_isEditing)
            UIManager.TopCurrencyUI.SetCurrency(transform,ECurrency.Meat, ECurrency.Cash_Free);
        else
            UIManager.TopCurrencyUI.SetCurrency(transform,ECurrency.Meat);

        _titleBoradTxt.text = _isEditing ? "공명 편집" : "영웅 관리";

        DrawWaitingCharacterItem();

        if (_isEditing)
        {
            DrawSquadCharacterItem();
            _resonanceCountTxt.text = $"{UserInfoData._unlockResonanceCount}<color=#1E120C>/{ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "MaxSlot").Value}";
            _edittoggle.isOn = true;
        }
        else
        {

            int ownedCount = UserInfoData._dicCharacterItemData.Values.Count(c => c.isOpen);
            int totalCount = ClientLocalDB_Simple.GetDB<UnitData>(DBKey.PlayerCharacter).Where(unit => unit.Value.Live).ToList().Count;
            _countTxt.text = $"{ownedCount:D2}/{totalCount:D2}";

            _inventoryToggle.isOn = true;
        }

        _redDot.SetActive(RedDotManager.CharacterListRedDot());

    }

    public void DrawSquadCharacterItem()
    {
        for (int i = 0; i < UserInfoData._TopLevelCharacterList.Count; i++)
        {
            CharacterClassItemData data = Managers.Instance.UserInfo()._TopLevelCharacterList[i];
            _squadCharacter[i].Init(data, i);

            // 초기화
            _squadCharacter[i]._select01.SetActive(false);
        }

        // 기준 object 활성화 (가장 끝)
        _squadCharacter[4]._select01.SetActive(true);


    } 

    public void DrawWaitingCharacterItem()
    {
        _inventoryScrollviewArea.SetActive(!_isEditing);
        _resonanceScrollviewArea.SetActive(_isEditing);
        if(_isEditing)
        {
            // data setting
            _resonanceScrollview.Initialize(this);
        }
        else
        {
            InitOpenContent();
            RefreshOpenContent();
            InitNotOpenContent();
            RefreshNotOpenContent();

            _allCharactersList = _currentVisibleList.Concat(_notOpenVisibleList).ToList();

            LayoutRebuilder.ForceRebuildLayoutImmediate(_openContent as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_notOpenContent as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollviewRect);
        }
    }


    void InitOpenContent()
    {
        if (_openContentInited) return;

        var allCharacters = ClientLocalDB_Simple.GetDB<UnitData>(DBKey.PlayerCharacter);
        foreach (var _ in allCharacters)
        {
            if(!_.Value.Live)
                continue;
            
            GameObject go = Managers.Instance.GetResObjectManager().Instantiate(SCROLLITEM_PATH, _openContent);
            _openItems.Add(go.GetComponent<CharacterInventoryScrollviewItem>());
        }
        _openContentInited = true;
    }

    void RefreshOpenContent()
    {
        bool isFactionAll = _filterFactionType == EFactionType.All;

        _currentVisibleList = UserInfoData._dicCharacterItemData.Values
            .Where(c => c.isOpen && c._unitData.Live &&
                (isFactionAll || ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, c.id).Faction == _filterFactionType))
            .OrderByDescending(c => c._statusInfo._battlePower)
            .ThenByDescending(c => c._grade)
            .ThenByDescending(c => ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, c.id).StartGrade)
            .ThenBy(c => c.id)
            .ToList();

        for (int i = 0; i < _openItems.Count; i++)
        {
            if (i < _currentVisibleList.Count)
            {
                _openItems[i].gameObject.SetActive(true);
                _openItems[i].Init(_currentVisibleList[i], i);
            }
            else
            {
                _openItems[i].gameObject.SetActive(false);
            }
        }
    }

    void InitNotOpenContent()
    {
        if (_notOpenContentInited) return;

        int count = UserInfoData._dicCharacterItemData.Values.Count(c => !c.isOpen && c._unitData.Live);
        for (int i = 0; i < count; i++)
        {
            GameObject go = Managers.Instance.GetResObjectManager().Instantiate(SCROLLITEM_PATH, _notOpenContent);
            _notOpenItems.Add(go.GetComponent<CharacterInventoryScrollviewItem>());
        }
        _notOpenContentInited = true;
    }

    void RefreshNotOpenContent()
    {
        bool isFactionAll = _filterFactionType == EFactionType.All;

        _notOpenVisibleList = UserInfoData._dicCharacterItemData.Values
            .Where(c => !c.isOpen && c._unitData.Live &&
                (isFactionAll || ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, c.id).Faction == _filterFactionType))
            .OrderByDescending(c => c._grade)
            .ThenByDescending(c => ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, c.id).StartGrade)
            .ThenBy(c => c.id)
            .ToList();
        List<CharacterClassItemData> unownedList = _notOpenVisibleList;

        for (int i = 0; i < _notOpenItems.Count; i++)
        {
            if (i < unownedList.Count)
            {
                _notOpenItems[i].gameObject.SetActive(true);
                _notOpenItems[i].Init(unownedList[i], i);
            }
            else
            {
                _notOpenItems[i].gameObject.SetActive(false);
            }
        }
    }

    public void ClickFilterButton()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        // 팝업 오픈 
        uISubCharacterFilter = Managers.Instance.GetUIManager().ShowUISubBase<UISubCharacterFilter>(this, "UISubCharacterFilter");
        uISubCharacterFilter.OpenToStack();
        uISubCharacterFilter.SetTap(this);
    }

    #region Toggle

    void InventoryToggle(bool isOn)
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        if (isOn)
        {
            _isEditing = false;
            Refresh();
        }
    }

    void EditToggle(bool isOn)
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        if (isOn)
        {
            _isEditing = true;
            Refresh();
        }

    }

    #endregion

    public override void Close()
    {

        _filterFactionType = EFactionType.All;
        uISubCharacterFilter?.ClickCloseBtn();
        uISubCharacterFilter = null;

        _isEditing = false;
        base.Close();
    }

    public void ShowHelpPopup()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        UIManager.ShowUISubBase<UISubHelp>(UIManager.UICharacterInventory, "UISubHelpPopup").SetType(EHelpType.FactionRelation);
    }

    public void TutorialSetting()
    {
        if (_openItems.Count == 0) return;
        _openItems[0].gameObject.AddComponent<UITutorialButton>().buttonID = ButtonID.CharacterInventory_ScrollviewItem;
    }
}
