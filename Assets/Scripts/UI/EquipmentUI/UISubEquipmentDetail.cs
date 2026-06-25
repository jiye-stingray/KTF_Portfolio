using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UISubEquipmentDetail : UISubBase
{
    [SerializeField] protected GameObject _bg;

    [SerializeField] protected TMP_Text _nameTxt;
    [SerializeField] protected Image _gradeBg;
    [SerializeField] protected Image _icon;
    [SerializeField] protected Image _factionIcon;
    [SerializeField] protected Image _lockImg;
    [SerializeField] protected GameObject _legendayEffect;
    [SerializeField] protected GameObject _mythicEffect;

    [SerializeField] protected RectTransform _statusAreaRect;
    [SerializeField] protected StatusText[] _statusTexts;

    [Header("Button")]
    [SerializeField] protected RectTransform _buttonArea;
    [SerializeField] protected GameObject _ChangeButton;
    [SerializeField] protected GameObject _DecompositionButton;
    [SerializeField] protected UICostButton _upgradeCostBtn;        // 업그레이드 또는 재설정 (각 UI prefab 마다 다름)

    protected EquipmentItemData _data;

    public void SetDataOpenToStack(EquipmentItemData data)
    {
        _data = data;
        base.OpenToStack();
        Refresh();
        PlayOpenAnimation();
    }

    private void PlayOpenAnimation()
    {
        _bg.transform.localScale = Vector3.zero;
        _bg.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public override void Refresh()
    {
        _nameTxt.text = $"[{Define.ReturnGradeString(_data.data.Grade)}] {_data.data.UIName}";
        _nameTxt.color = Utils.HexToColor(Define.GradeColorHex[_data.data.Grade]);
        _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.EquipmentAtlas, _data.data.Name);
        _factionIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)_data.data.Faction}");
        _gradeBg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{_data.data.Grade}");

        var grade = _data.data.Grade;
        _legendayEffect.SetActive(grade == EGradeType.Legendary || grade == EGradeType.Legendary_Plus);
        _mythicEffect.SetActive(grade == EGradeType.Mythic);

        _lockImg.sprite = _data.isLock ? Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.PictogramAtlas, "Lock") :
            Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.PictogramAtlas, "lock2");
        //초기화
        for (int i = 0; i < _statusTexts.Length; i++)
        {
            _statusTexts[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < _data.data.StatType.Length; i++)
        {
            _statusTexts[i].gameObject.SetActive(true);
            _statusTexts[i].SetData(_data.data.StatType[i], _data.mainStatus);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(_statusAreaRect);

        _DecompositionButton.SetActive(!_data.isSet);

        _upgradeCostBtn.gameObject.SetActive(_data.data.Grade == EGradeType.Legendary_Plus);
        if(_data.data.Grade == EGradeType.Legendary_Plus)
        {
            _upgradeCostBtn.Init(new ECurrency[] { 
                (ECurrency)ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "MythicEquipmentUpgradeCurrency_1").Value ,
                (ECurrency)ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "MythicEquipmentUpgradeCurrency_2").Value },
               new int[] {
                   ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "MythicEquipmentUpgradeValue_1").Value ,
                   ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "MythicEquipmentUpgradeValue_2").Value 
               });
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonArea);

    }

    public void LockBtnClick()
    {

        if (_data.isLock)
            Managers.Instance.GetServerManager().OnPostUnLockEquipment(_data.id, mainUI);
        else
            Managers.Instance.GetServerManager().OnPostLockEquipment(_data.id, mainUI);

        _lockImg.sprite = _data.isLock ? Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.PictogramAtlas, "Lock") :
            Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.PictogramAtlas, "lock2");
    }

    public void ChangeBtnClick()
    {
        ClickCloseBtn();
        if(_data.isSet )
        {
            // 장비 선택 UI 올리기
            Managers.Instance.GetUIManager().ShowUISubBase<UISubEquipmentChange>(mainUI, "UISubEquipmentChange").InitData(_data,_data.data.Faction,_data.data.Type);
        }
        else
        {
            // 본인 data로 장비 교체
            Managers.Instance.GetServerManager().OnPostEquipEquipment(_data.data.Faction,_data.id);
        }
    }

    public void DecompositionBtnClick()
    {
        if(_data.isLock)
        {
            UIManager.ShowCommonToastMessage("잠겨있는 장비 입니다.");
            return;
        }

        if(UserInfoData._dicEquipment[_data.data.Faction][(int)_data.data.Type] != null &&
            _data.data.Grade > UserInfoData._dicEquipment[_data.data.Faction][(int)_data.data.Type].data.Grade)
        {
            Managers.Instance.GetUIManager().ShowConfirmPopUp("현재 장착중인 장비 보다 높은 등급의 장비가 포함되어 있습니다.", "정말 분해하겠습니까?", () =>
            {
                // 서버 연결 
                Managers.Instance.GetServerManager().OnGetEquipmentDisassembly(_data.id);    
                ClickCloseBtn();
            });
            return;
        }
        // 서버 연결
        Managers.Instance.GetServerManager().OnGetEquipmentDisassembly(_data.id);    
        ClickCloseBtn();
    }

    public void UpgradeBtnClick()
    {
        if (!_upgradeCostBtn.gameObject.activeSelf || _upgradeCostBtn.isGray)
        {
            Managers.Instance.GetUIManager().ShowCommonToastMessage("재화가 부족합니다.");
            return;
        }

        // 전투력 미리 셋팅 
        Managers.Instance.GetUIManager().UIEquipmentSetting._battlePower = UserInfoData.EquipmentFactionBattlePower(_data.data.Faction);

        // 업그레이드 서버 연결
        Managers.Instance.GetServerManager().OnPostUpgradeEquipment(_data.id,(equipmentDto) =>
        {
            ClickCloseBtn();        // 반드시 본인 거 닫고 호출 (stack 꼬이기 X) 

            // 장착한 장비 적용
            if (Managers.Instance.UserInfo().IsEquipped(_data.data.Faction, _data.id))
            {
                Managers.Instance.UserInfo().RefreshEquipEquipmentItemData(_data.data.Faction);

                // 전투력 토스트 메시지
                double newBattlePower = UserInfoData.EquipmentFactionBattlePower(_data.data.Faction) - UIManager.UIEquipmentSetting._battlePower;
                UIManager.ShowUIToast<UIToastBase>(newBattlePower.ToString("0"), "ChangeBattlePowerToastMessage");
            }

            UIManager.ShowUISubBase<UISubMythicEquipmentDetail>(mainUI, "UISubMythicEquipmentDetail").SetDataOpenToStack(UserInfoData._dicEquipmentItemData[equipmentDto.id]);
            mainUI.Refresh();
        });
    }
}
