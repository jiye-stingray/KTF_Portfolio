using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class CharacterInventoryScrollviewItem : ICell
{
    protected CharacterClassItemData _data;

    [SerializeField] public GameObject _select01;
    [SerializeField] protected Image _bg;
    [SerializeField] protected Image _gradeFrameImg;
    [SerializeField] protected Image _icon;
    [SerializeField] protected Image _factionIcon;
    [SerializeField] protected TMP_Text _levelTxt;
    [SerializeField] protected TMP_Text _nameTxt;
    [SerializeField] protected GameObject _gray;
    [SerializeField] private GameObject _equip;
    [SerializeField] private GameObject _redDot1; // 레벨업 가능
    [SerializeField] private GameObject _redDot2; // 각성 가능


    public void Init(CharacterClassItemData data, int index)
    {
        _data = data;
        _index = index;

        Refresh();
    }

    public virtual void Refresh()
    {
        _levelTxt.gameObject.SetActive(!(_data == null || !_data.isOpen));
        if (_data == null)
        {
            // null 처리
            return;
        }
        if(_gray != null)
            _gray.SetActive(!_data.isOpen);

        // icon 설정 
        UnitData playerUnit = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, _data.id);
        _icon.sprite = Managers.Instance.GetAtlasManager().GetCharacterIcon(playerUnit.UnitType, playerUnit.Resource);
        _bg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{Define.ReturnNoPlusGradeType(_data._grade)}");
        _gradeFrameImg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"Frame_grade_{_data._grade}");
        _factionIcon.sprite = AtlasManager.GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)playerUnit.Faction}");

        // 레벨 텍스트 -> 덱에 편성된 캐릭터는 슬롯 레벨 표시 
        // 그렇지 않은 캐릭터는 가장 작은 레벨 표시 
        _levelTxt.text = "Lv. " + _data.Level;
        _nameTxt.text = _data._unitData.Name;

        _equip.SetActive(UserInfo.CheckContainDeck(EServerContentType.Field, _data.id));

        _redDot1.SetActive(RedDotManager.EnableCharacterLevelUp(_data.id));
        _redDot2.SetActive(RedDotManager.EnableCharacterAwakening(_data));
    }

    public virtual void Click()
    {
        if (_data == null) return;

        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        // 인벤토리 리스트 기준 현재 선택 인덱스 공유 (좌/우 스위칭용)
        if (UIManager.UICharacterInventory != null)
            UIManager.UICharacterInventory._currentIndex._index = _index;

        // 캐릭터 정보 UI 띄우기 
        UIManager.UICharacterDetail.SetDataOpenToStack(_data);
    }

}
