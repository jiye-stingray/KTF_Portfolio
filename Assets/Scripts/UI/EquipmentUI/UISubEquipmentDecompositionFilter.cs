using Only1Games.UI;
using PolyAndCode.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UISubEquipmentDecompositionFilter : UISubBase , IRecyclableScrollRectDataSource
{
    [SerializeField] EFactionType _factionType;
    [SerializeField] EGradeType _eGradeType = EGradeType.Legendary_Plus;

    [SerializeField] Image _factionIcon;
    [SerializeField] TMP_Text _factionTxt;

    [SerializeField] UIDropDownMenu _dropDown;
    [SerializeField] RecyclableScrollRect _scrollRect;

    List<EquipmentItemData> _equipmentList = new List<EquipmentItemData>();

    public void SetFaction(EFactionType factionType)
    {
        _factionType = factionType;

        _dropDown.Set((int)_eGradeType - 1);

        OpenToStack();
        Refresh();
    }

    #region Recycle
    public int GetItemCount()
    {
        return _equipmentList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as EquipmentInfoScrollviewItem;
        item.Init(_equipmentList[index],index);
    }
    #endregion

    public override void Refresh()
    {

        _equipmentList.Clear();
        _equipmentList = UserInfoData._dicEquipmentItemData.Values.Where(item => !item.isSet && !item.isLock && 
                                (_factionType == EFactionType.All || item.data.Faction == _factionType)
                                && item.data.Grade <= _eGradeType)
                                .OrderByDescending(item => item.data.Grade)
                                .ThenByDescending(item => item.id)
                                .ToList();

        _factionIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)_factionType}");
        _factionTxt.text = Define.ReturnFactionString(_factionType);

        _scrollRect.Initialize(this);
        _scrollRect.ReloadData();
    }

    public void Click()
    {
        if (_equipmentList.Count <= 0)
        {
            // 분해 한 장비가 없다는 토스트 메시지
            UIManager.ShowUIToast<UIToastBase>("분해할 장비가 없습니다", "ToastMessage");
            return;
        }

        if(RedDotManager.AutoSettingEquipmentRedDot(_factionType))       // 더 강한 장비를 장착할 수 있는지에 대한 여부 (RedDot 때 작업했던 함수 사용)
        {
            Managers.Instance.GetUIManager().ShowConfirmPopUp("현재 장착중인 장비 보다 높은 등급의 장비가 포함되어 있습니다.", "분해하시겠습니까?",
                Decomposition, ClickCloseBtn);
            return;
        }

        Decomposition();
    }

    private void Decomposition()
    {
        // 서버 연결
        if (_factionType == EFactionType.All)
            Managers.Instance.GetServerManager().OnGetAllBatchDisassembly((int)_eGradeType);
        else
            Managers.Instance.GetServerManager().OnPostEquipmentBatchDisassembly((int)_factionType, (int)_eGradeType);

        ClickCloseBtn();

    }

    public void ChangeDropDownEvent()
    {
        _eGradeType = (EGradeType)(_dropDown.currentIndex + 1);
        Refresh();
    }


}
