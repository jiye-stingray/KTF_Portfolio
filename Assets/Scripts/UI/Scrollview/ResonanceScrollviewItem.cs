using PolyAndCode.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class ResonanceScrollviewItem : ICell
{
    [SerializeField] GameObject _nullgo;
    [SerializeField] GameObject _coolTimeGo;
    [SerializeField] TMP_Text _coolTimeTxt;
    [SerializeField] GameObject _lockgo;
    [SerializeField] GameObject _lockCurrencyGo;
    [SerializeField] Image _lockCurrencyImg;
    [SerializeField] TMP_Text _lockCurrencyTxt;

    [Header("Info")]
    [SerializeField] protected Image _bg;
    [SerializeField] protected Image _gradeFrameImg;
    [SerializeField] protected Image _icon;
    [SerializeField] protected TMP_Text _levelTxt;
    [SerializeField] ElementPiecesSlider _slider;

    ResonanceItemData _data;

    public void Init(ResonanceItemData data, int index)
    {
        _data = data;
        _index = index;

        Refresh();
    }

    public void Refresh()
    {
        _lockgo.SetActive(_data._isLock);
        _lockCurrencyGo.SetActive(_data._isLock && _index < ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "MaxSlot").Value && _index == UserInfo._unlockResonanceCount);
        if (_lockCurrencyGo.activeSelf)
        {
            _lockCurrencyImg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, 
                $"{ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency,(ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotOpenCurrency_1").Value)).Icon}");
            _lockCurrencyTxt.text = $"x{ClientLocalDB_Simple.GetData<SynchroSetting>(DBKey.SynchroSetting, "SlotOpenCurrencyValue_1").Value}";
        }
        
        if (_data._isLock) return;

        //Date 타임 체크
        DateTime serverCurrentTime = ServerTime.Instance.CurrentTime();

        TimeSpan timeCal;
        if (_data._expireDate == default)
        {
            _coolTimeGo.SetActive(false);
        }
        else
        {
            timeCal = _data._expireDate - serverCurrentTime;
            _coolTimeGo.SetActive(timeCal.TotalSeconds > 0);
        }
            

        _nullgo.SetActive(!_coolTimeGo.activeSelf && _data._characterItemData == null);
        if (_data._characterItemData == null) return;

        // draw info
        // icon 설정 
        UnitData playerUnit = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, _data._characterId);
        _icon.sprite = Managers.Instance.GetAtlasManager().GetCharacterIcon(playerUnit.UnitType, playerUnit.Resource);
        _bg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{Define.ReturnNoPlusGradeType(_data._characterItemData._grade)}");
        _gradeFrameImg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"Frame_grade_{_data._characterItemData._grade}");

        // 레벨 텍스트 -> 덱에 편성된 캐릭터는 슬롯 레벨 표시 
        // 그렇지 않은 캐릭터는 가장 작은 레벨 표시 
        _levelTxt.text = "Lv. " + _data._characterItemData.Level;

        int pieceCost = Utils.ReturnAwakenPieceCost(_data._characterItemData.id, (int)_data._characterItemData._grade);
        _slider.Init(pieceCost, _data._characterItemData);

    }

    public void Update()
    {
        if(!_lockgo.activeSelf && _coolTimeGo.activeSelf)
        {
            //Date 타임 체크
            DateTime serverCurrentTime = ServerTime.Instance.CurrentTime();
            TimeSpan timeCal = _data._expireDate - serverCurrentTime;
            _coolTimeTxt.text = $"남은시간\n{timeCal.Hours}:{timeCal.Minutes}";

            _nullgo.SetActive(timeCal.TotalSeconds <= 0);
            _coolTimeGo.SetActive(timeCal.TotalSeconds > 0);        // cooltimeGO를 nullGO 보다 늦게 체크함으로 Refresh의 nullGo 조건에서 정상작동
        }
    }

    public void Click()
    {
        if(_lockgo.activeSelf || _lockCurrencyGo.activeSelf )
        {
            // 잠겨있는 슬롯 오픈 팝업 띄우기
            Managers.Instance.GetUIManager().ShowUISubBase<UISubResonanceCurrency>(Managers.Instance.GetUIManager().UICharacterInventory,
                "UISubResonanceCurrency").OpenToStack();

        }
        else if(_nullgo.activeSelf)
        {
            // 캐릭터 setting 팝업 띄우기
            UISubRegistrationResonance uISubRegistrationResonance = UIManager.ShowUISubBase<UISubRegistrationResonance>(
                Managers.Instance.GetUIManager().UICharacterInventory, "UISubRegistrationResonance");
            uISubRegistrationResonance.InitIndex(_index);
        }
        else if(_coolTimeGo.activeSelf)
        {
            // 슬롯 등록 해제 팝업 (쿨타임 없이 바로 체크)
            Managers.Instance.GetUIManager().ShowUISubBase<UISubResonanceCoolTimeCurrency>(Managers.Instance.GetUIManager().UICharacterInventory,
                "UISubResonanceCoolTimeCurrency").InitData(_data);
        }
        else if(_data._characterItemData != null)
        {
            // 팝업 띄우기
            Managers.Instance.GetUIManager().ShowConfirmPopUp("","공명에 슬롯에 등록된 캐릭터를 해제 하시겠습니까",
            () =>
            {

#if USE_SERVER
                Managers.Instance.GetServerManager().OnPostUnEquipResonanceSlot(_data._index);
#else
                // 공명슬롯 등록 캐릭터 해제
                UserInfo.ReleaseResonanceCharacter(_data._id);
#endif

                Managers.Instance.GetUIManager().UICharacterInventory.Refresh();
            });

        }
    }
}
