using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UICharacterDetail : UIBase
{
    #region Tab

    [Serializable] 
    public struct EPages
    {
        public CharacterDetailTab CharacterDetailTap;
        public CharacterSkillTab CharacterSkillTap;
        public CharacterDescTab CharacterDescTab;

    }
    public EPages _pages;

    public enum ETAP_TYPE
    { 
        CharacterDetailPage = 0,
        CharacterSkillPage = 1,
        CharacterDescPage = 2
    }
    ETAP_TYPE _currentTap = ETAP_TYPE.CharacterDetailPage;

    public UITabGroup _group;
    #endregion
    
    private int Id => _itemData.id;
    private int Level => _itemData.Level;
    public EGradeType Grade => _itemData._grade;
    public CharacterClassItemData _itemData;
    private UnitData UnitData => ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, Id);
    private bool _isDummyData;

    [Header("Switch Character")]
    [SerializeField] private RotateSwitchDataButton _switchDataRightBtn;
    [SerializeField] private RotateSwitchDataButton _switchDataLeftBtn;

    [Header("Data")]
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] TMP_Text _levelTxt;
    [SerializeField] Image _rankImg;
    [SerializeField] TMP_Text _rankTxt;
    [SerializeField] Image _factionImg;
    [SerializeField] Image _elementImg;
    [SerializeField] Image _characterIcon;
    [SerializeField] Image _factionBg;

    // for LevelUp
    [HideInInspector] public double _beforeBattlePower;
    [HideInInspector] public double _beforeAtk;
    [HideInInspector] public double _beforeMaxhp;
    
    [Header("Spectator Mode Control")]
    private bool isUIVisible = true;
    public Transform tMainArea;
    public Transform tCurrency_Rack => UIManager.TopCurrencyUI.transform;
    public Transform tTop_Left;
    public Transform tClose;
    public Transform tStatusInfo;
    
    [SerializeField] 
    public Transform tToggleModeButton; // 감상 모드 On/Off 버튼
    
    [Header("Dialogue")]
    [SerializeField] TMP_Text _dialogueText;
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private GameObject _tailSpeech;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
 
        
        // 초기 상태는 UI 보임 (isUIVisible = true)
        isUIVisible = true;
        
        return true;
    }
    
    /// <summary>
    /// 캐릭터 감상 모드를 On/Off 합니다.
    /// UI 전체를 투명하게 만들고 상호작용을 막아 캐릭터만 남깁니다.
    /// </summary>
    public void ToggleSpectatorMode()
    {
        isUIVisible = !isUIVisible;

        // UI 요소들을 그룹으로 묶어 처리
        Transform[] uiElementsToToggle = 
        {
            tMainArea, 
            tCurrency_Rack, 
            tTop_Left,
            tClose,
            tStatusInfo,
            // 이 버튼을 숨기면 뒤로가기 버튼도 사라집니다. (아래 3번 참고)
        };
        
        foreach (var t in uiElementsToToggle)
        {
            if (t != null)
            {
                // isUIVisible 상태에 따라 GameObject 활성화/비활성화
                t.gameObject.SetActive(isUIVisible);
                if (t.transform == tStatusInfo)
                    t.gameObject.SetActive(false);
            }
        }
        
        if(!_isDummyData)
            UIManager.TopCurrencyUI.gameObject.SetActive(isUIVisible);
        
        MyLogger.Log($"[Spectator Mode] 상태: {(isUIVisible ? "OFF (UI 보임)" : "ON (UI 숨김)")}");
        
    }

    public void SetDataOpenToStack(CharacterClassItemData itemData, bool isDummyData = false)
    {
        _itemData = itemData;
        _isDummyData = isDummyData;
        OpenToStack();
    }

    public override void Refresh()
    {
        _nameTxt.text = UnitData.Name;
        _levelTxt.text = $"Lv.{Level}/{ClientLocalDB_Simple.GetDB<CharacterLevel>(DBKey.CharacterLevel).Last().Value.Level}";

        DrawRank();
        DrawIcon();

        SetCharacterImg().Forget();
        _factionBg.sprite  = Managers.Instance.GetResObjectManager().Load<Sprite>($"Texture/FactionBg/CharacterFactionBg_{UnitData.Faction.ToString()}");

        // 설명 버튼 활성 & 비활성화 
        _group._tapGroupBtns[2].gameObject.SetActive(ClientLocalDB_Simple.GetDB<DescDB>(DBKey.Desc).ContainsKey(_itemData.id.ToString()));

        _group.Set((int)_currentTap);
    }

    private async UniTask SetCharacterImg()
    {
        _characterIcon.sprite = await Managers.Instance.GetResObjectManager().LoadAsync<Sprite>($"Illustration_{UnitData.Resource}");
    }

    private void BindSwitchButtons()
    {
        if (_switchDataLeftBtn == null || _switchDataRightBtn == null) return;
        if (UIManager.UICharacterInventory == null) return;

        // 현재 _itemData가 인벤토리 리스트에 존재하면 인덱스를 맞춰준다
        if (_itemData != null && UIManager.UICharacterInventory.TryFindIndexById(_itemData.id, out int foundIndex))
            UIManager.UICharacterInventory._currentIndex._index = foundIndex;

        int count = UIManager.UICharacterInventory.DataCount;
        if (count <= 0)
        {
            _switchDataLeftBtn.gameObject.SetActive(false);
            _switchDataRightBtn.gameObject.SetActive(false);
            return;
        }

        int maxIndex = Mathf.Max(0, count - 1);
        _switchDataLeftBtn.gameObject.SetActive(!_isDummyData);
        _switchDataRightBtn.gameObject.SetActive(!_isDummyData);
        _switchDataLeftBtn.Init(UIManager.UICharacterInventory._currentIndex, 0, maxIndex, OnSwitchCharacter);
        _switchDataRightBtn.Init(UIManager.UICharacterInventory._currentIndex, 0, maxIndex, OnSwitchCharacter);
    }

    private void OnSwitchCharacter(int index)
    {
        if (UIManager.UICharacterInventory == null) return;
        if (!UIManager.UICharacterInventory.TryGetDataAt(index, out CharacterClassItemData data)) return;

        _itemData = data;

        _group._currentTapGroupBtn = _group._currentTapGroupBtn._index == 2 ? _group._tapGroupBtns[0] : _group._currentTapGroupBtn;
        OnChangeTap();
        Refresh();

    }

    private void DrawRank()
    {
        _rankTxt.text = ReturnGradeString(Grade);
        _rankImg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ScrollviewItemAtlas, $"Gradegradient_{Grade}");
    }

    private void DrawIcon()
    {
        _factionImg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)UnitData.Faction}");
    }

    /// <summary>
    /// Group에 changeEvent로 연결
    /// </summary>
    public void OnChangeTap()
    {
        _currentTap = (ETAP_TYPE)_group._currentTapGroupBtn._index;
        switch (_currentTap)
        {
            case ETAP_TYPE.CharacterDetailPage:
                if (!_isDummyData)
                {
                    UIManager.TopCurrencyUI.SetCurrency(this.transform, ReturnLimitBreakStone(Level));
                }
                _pages.CharacterDetailTap.Open();
                _pages.CharacterDetailTap.SetData(_itemData, _isDummyData);
                _pages.CharacterSkillTap.Close();
                _pages.CharacterDescTab.Close();
                break;

            case ETAP_TYPE.CharacterSkillPage:
                if (!_isDummyData)
                {
                    UIManager.TopCurrencyUI.SetCurrency(this.transform, ECurrency.Gold);                
                }
                _pages.CharacterDetailTap.Close();
                _pages.CharacterSkillTap.Open();
                _pages.CharacterDescTab.Close();
                _pages.CharacterSkillTap.SetData(_itemData, _isDummyData);
                break;

            case ETAP_TYPE.CharacterDescPage:
                _pages.CharacterDetailTap.Close();
                _pages.CharacterSkillTap.Close();
                _pages.CharacterDescTab.SetData(_itemData);
                _pages.CharacterDescTab.Open();
                break;
        }
    }

    public override void Open()
    {
        _group._currentTapGroupBtn = _group._tapGroupBtns[0];
        OnChangeTap();
        
        // 감상 모드 초기화
        if (!isUIVisible)
        {
            // 만약 닫을 때 감상 모드였다면, 다시 Open할 때 자동으로 해제 (UI 활성화)
            isUIVisible = true;
            ToggleSpectatorMode(); // 상태 전환
        }
        
        BindSwitchButtons();
        Refresh();
        base.Open();
        
        //
        ShowCharacterDialogue();
        
    }

    private void ShowCharacterDialogue()
    {
        if (_itemData == null)
        {
            Debug.LogError("Character data is null. Cannot show dialogue.");
            return;
        }
    
        // [삭제됨] 기존 사운드 재생 로직 (Managers.Instance.Sound...)
        
        // 로컬라이징 키를 가져와서 대사 텍스트 출력
        int characterId = Id;
        int translationKey = 2000 + characterId;
        string dialogueText = I2.Loc.LocalizationManager.GetTranslation($"{translationKey}");
        
        DisplayDialogueText(dialogueText);
        
        MyLogger.Log($"캐릭터 ID {characterId}의 대사 출력: {dialogueText}");
    }
    
    private void DisplayDialogueText(string text)
    {
        if (_dialogueText == null || _dialoguePanel == null)
        {
            Debug.LogError("Dialogue UI components are not assigned in the Inspector.");
            return;
        }
    
        if (string.IsNullOrEmpty(text))
        {
            _dialoguePanel.SetActive(false);
            _tailSpeech.SetActive(false);
            return;
        }
        
        _dialoguePanel.SetActive(true);
        _tailSpeech.SetActive(true);
        
        _dialogueText.text = text;

        StopAllCoroutines();
        
        StartCoroutine(HideDialogueAfterDelay(5.0f)); 
    }

    private IEnumerator HideDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    
        // 시간이 지나면 대사 패널을 비활성화
        if (_dialoguePanel != null)
        {
            _dialoguePanel.SetActive(false);
            _tailSpeech.SetActive(false);
        }
    }
    
}
