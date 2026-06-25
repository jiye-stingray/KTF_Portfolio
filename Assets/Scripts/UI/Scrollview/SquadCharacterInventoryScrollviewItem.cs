using PolyAndCode.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Define;

public class SquadCharacterInventoryScrollviewItem : CharacterInventoryScrollviewItem
{
    [SerializeField] GameObject selectObject;

    UnityAction<int,int> _setSquadAction;
    DeckData _deckData;

    public void Init(CharacterClassItemData data, DeckData deckData, int index, UnityAction<int, int> setSquadAction = null)
    {
        _data = data;
        _index = index;
        _deckData = deckData;
        // _deckSettingId = deckSettingId;
        // _deckSettingIndex = deckSettingIndex;
        _setSquadAction = setSquadAction;
        Refresh();
    }

    public override void Refresh()
    {
        //by rainful 2025-05-14 등급별 프레임 등의 내용 refresh를 위해 추가.
        //작업자분께서 확인 후 변경이 필요하면 그렇게 해주시면 되십니다.

        // icon 설정 
        UnitData playerUnit = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, _data.id);

        _icon.sprite = Managers.Instance.GetAtlasManager().GetCharacterIcon(playerUnit.UnitType, playerUnit.Resource);
        _bg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{Define.ReturnNoPlusGradeType(_data._grade)}");
        _gradeFrameImg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"Frame_grade_{_data._grade}");
        _factionIcon.sprite = AtlasManager.GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)playerUnit.Faction}");


        _levelTxt.text = $"Lv.{_data.Level}";
        _nameTxt.text = playerUnit.Name;

        int tempDeckIdx = _deckData.ReturnDeckIdx(_data.id);
        selectObject.SetActive(tempDeckIdx != -1);
     
    
    }

    public override void Click()
    {
        int tempDeckIdx = _deckData.ReturnDeckIdx(_data.id);
        if (tempDeckIdx == -1)     // 전에 클릭한 캐릭터가 deck 에 편성이 되어있지 않다면 캐릭터 편성
        {
            int emptySlot = _deckData.ReturnDeckIdx(0);

            // 2025-07-06 by.jiye
            Managers.Instance.GetUIManager().UIDeckSetting.stab.deckSettingPage._tempSlotIdx = emptySlot;
            _setSquadAction?.Invoke(_data.id, emptySlot);
        }
        else //편성이 이미 되어 있다면 편성 해제
        {
            if (_deckData.idList.Count(a => a > 0) <= 1)
                return;
            
            _setSquadAction?.Invoke(_data.id, tempDeckIdx);
        }
    }
}
