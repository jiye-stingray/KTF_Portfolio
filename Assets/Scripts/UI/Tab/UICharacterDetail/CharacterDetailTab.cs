using System.Linq;
using TMPro;
using UnityEngine;
using static Define;

public class CharacterDetailTab : UITabBase
{
    [Header("Text")]
    [SerializeField] TMP_Text _battlePowerTxt;
    [SerializeField] TMP_Text _atkTxt;
    [SerializeField] TMP_Text _maxHpTxt;

    [Header("Button")]
    [SerializeField] CharacterMercenaryCostButton _characterMercenaryCostBtn;
    [SerializeField] CharacterLevelUpCostButton _characterLevelUpCostBtn;
    [SerializeField] CharacterLevelUpCostButton _characterBreakthroughCostButton;
    [SerializeField] GameObject _maxLevelGo;
    [SerializeField] GameObject _resonanceBtn;
    [SerializeField] ElementPiecesSlider _elementSlider;

    [Header("StatusInfo")]
    [SerializeField] StatusInfoUI _statusInfoUI;

    private int Id => _itemData.id;
    public int Level => _itemData.Level;
    public EGradeType Grade => _itemData._grade;
    public CharacterClassItemData _itemData;
    private UnitData UnitData => ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, Id);
    private bool _isDummyData;
    
    bool IsBreakthroughLevel => Level % 10 == 0;
    bool IsMaxLevel => Level >= ClientLocalDB_Simple.GetDB<CharacterLevel>(DBKey.CharacterLevel).Last().Value.Level;
    bool CanShowBreakthrough => IsBreakthroughLevel && IsMaxLevel == false;

    public void SetData(CharacterClassItemData itemData, bool isDummyData)
    {
        _itemData = itemData;
        _isDummyData = isDummyData;
        Refresh();
    }

    public void CharacterLevelUpBtnSuccessAction()
    {
        UIManager.UICharacterDetail._beforeBattlePower = _itemData._statusInfo._battlePower;
        UIManager.UICharacterDetail._beforeAtk = _itemData._statusInfo._growthStatus._totalAttack;
        UIManager.UICharacterDetail._beforeMaxhp = _itemData._statusInfo._growthStatus._totalMaxHp;

        Managers.Instance.GetServerManager().OnPostHeroLevelUp(Id);
    }

    public void OnAwakenUpSuccessAction()
    {
        UIManager.UICharacterDetail._beforeBattlePower = _itemData._statusInfo._battlePower;
        UIManager.UICharacterDetail._beforeAtk = _itemData._statusInfo._growthStatus._totalAttack;
        UIManager.UICharacterDetail._beforeMaxhp = _itemData._statusInfo._growthStatus._totalMaxHp;

        Managers.Instance.GetServerManager().OnAwakenUp(Id);
    }

    public override void Refresh()
    {
        _battlePowerTxt.text = _itemData._statusInfo._battlePower.ToString();
        _atkTxt.text = _itemData._statusInfo._growthStatus._totalAttack.ToString();
        _maxHpTxt.text = _itemData._statusInfo._growthStatus._totalMaxHp.ToString();
            
        int pieceCost = Utils.ReturnAwakenPieceCost(Id, (int)Grade);        
        _elementSlider.Init(pieceCost, _itemData);

        _maxLevelGo.SetActive(true);
        _characterMercenaryCostBtn.gameObject.SetActive(true);
        _characterLevelUpCostBtn.gameObject.SetActive(true);
        _characterBreakthroughCostButton.gameObject.SetActive(true);

        _statusInfoUI.gameObject.SetActive(false);
        if (!_isDummyData)
        {

            // 각성 체크
            _characterMercenaryCostBtn.Init(new int[] { pieceCost }, Id);
        
            _characterLevelUpCostBtn.gameObject.SetActive(_itemData.isOpen && !CanShowBreakthrough);
            _characterBreakthroughCostButton.gameObject.SetActive(_itemData.isOpen && CanShowBreakthrough);
            
            // max check
            // 프리팹 구조 변경으로 인한 체크 방식 변경 by.jiye 2025-12-04
            _maxLevelGo.SetActive(IsMaxLevel);
            _resonanceBtn.SetActive(!IsMaxLevel && UserInfoData.ReturnResonanceCharacter(Id));
            if(_maxLevelGo.activeSelf || _resonanceBtn.activeSelf)
            {
                _characterLevelUpCostBtn.gameObject.SetActive(false);
                _characterBreakthroughCostButton.gameObject.SetActive(false);
            }
            else
            {
                // LevelUp 버튼
                if (_characterLevelUpCostBtn.gameObject.activeSelf)
                {
                    CharacterLevel slotLevel = ClientLocalDB_Simple.GetData<CharacterLevel>(DBKey.CharacterLevel, Level);
                    _characterLevelUpCostBtn.Init(slotLevel.CurrencyID, slotLevel.CurrencyValue);
                
                }
                // 돌파 버튼
                if (_characterBreakthroughCostButton.gameObject.activeSelf)
                {
                    if (CanShowBreakthrough)
                    {
                        CharacterLevel slotLevel = ClientLocalDB_Simple.GetData<CharacterLevel>(DBKey.CharacterLevel, Level);
                        _characterBreakthroughCostButton.Init(slotLevel.CurrencyID, slotLevel.CurrencyValue);
                    }
                }
            }
        }
        else
        {
            _maxLevelGo.SetActive(false);
            _characterMercenaryCostBtn.gameObject.SetActive(false);
            _characterLevelUpCostBtn.gameObject.SetActive(false);
            _characterBreakthroughCostButton.gameObject.SetActive(false);
            _resonanceBtn.gameObject.SetActive(false);
        }
    }

    public void ShowStatusInfoObj()
    {
        _statusInfoUI.gameObject.SetActive(true);
        _statusInfoUI.Init(_itemData._statusInfo);
    }

    public void CloseStatusInfoObj()
    {
        _statusInfoUI.gameObject.SetActive(false);
    }

    public void ResonanceBtnClick()
    {
        // 공명관리 페이지 이동 안내 팝업
        UIManager.ShowConfirmPopUp("공명중인 영웅은 레벨업을 할 수 없습니다.", "공명관리 페이지로 이동하겠습니까?", () =>
        {
            UIManager.UICharacterInventory.OpenToStack();
            UIManager.UICharacterInventory._isEditing = true;
            UIManager.UICharacterInventory.Refresh();
        });

    }

    public void ShowHelpPopup()
    {
        UIManager.ShowUISubBase<UISubHelp>(UIManager.UICharacterDetail, "UISubHelpPopup").SetType(EHelpType.Character);
    }

    public override void Close()
    {
        _statusInfoUI.gameObject.SetActive(false);
        base.Close();
    }
}
