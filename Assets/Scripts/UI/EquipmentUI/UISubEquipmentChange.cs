using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UISubEquipmentChange : UISubBase , IRecyclableScrollRectDataSource
{
    [SerializeField] GameObject _nullAreaGo;
    [SerializeField] GameObject _emptyGo;

    [SerializeField] RecyclableScrollRect _scrollRect;

    [Header("EquipmentInfo")]
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] Image _gradeBg;
    [SerializeField] Image _icon;
    [SerializeField] Image _factionIcon;
    [SerializeField] StatusText[] _statusTxts;
    [SerializeField] EquipmentOptionStausText[] _optionStausTxt;

    EFactionType _factionType;
    EEquipmentType _equipmentType;

    List<EquipmentItemData> _dataList = new List<EquipmentItemData>();

    public EquipmentItemData selecteEquipItemData;        // 정보 선택한 장비

    public void InitData(EquipmentItemData data, EFactionType factionType, EEquipmentType type)
    {
        selecteEquipItemData = data;
        _factionType = factionType;
        _equipmentType = type;
        OpenToStack();
        Refresh();
    }

    #region Recyecle
    public int GetItemCount()
    {
        return  _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as EquipmentInfoChangeScrollviewItem;
        item.Init(_dataList[index], index, this);
    }
    #endregion

    public override void Refresh()
    {
        _nullAreaGo.SetActive(selecteEquipItemData == null);

        if(selecteEquipItemData != null)
        {
            _nameTxt.text = $"[{Define.ReturnGradeString(selecteEquipItemData.data.Grade)}] {selecteEquipItemData.data.UIName}";
            _nameTxt.color = Utils.HexToColor(Define.GradeColorHex[selecteEquipItemData.data.Grade]);
            _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.EquipmentAtlas, selecteEquipItemData.data.Name);
            _factionIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)selecteEquipItemData.data.Faction}");
            _gradeBg.sprite = Managers.Instance.GetAtlasManager()
                .GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{selecteEquipItemData.data.Grade}");
                
            //초기화
            for (int i = 0; i < _statusTxts.Length; i++)
            {
                _statusTxts[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < selecteEquipItemData.data.StatType.Length; i++)
            {
                _statusTxts[i].gameObject.SetActive(true);
                _statusTxts[i].SetData(selecteEquipItemData.data.StatType[i], selecteEquipItemData.mainStatus);

            }

            for (int i = 0; i < _optionStausTxt.Length; i++)
            {
                _optionStausTxt[i].SetDataGrade(selecteEquipItemData.GetOptionStautsType(i),
                    selecteEquipItemData.GetOptionStatus(i), selecteEquipItemData._equipmentOption[i]._optionGradeType);
            }
        }

        _dataList.Clear();
        _dataList = UserInfoData._dicEquipmentItemData.Values.Where( d => _equipmentType == d.data.Type && _factionType == d.data.Faction)
            .OrderByDescending(d => d.data.Grade)
            .ToList();

        _emptyGo.SetActive(_dataList.Count <= 0);
        _scrollRect.Initialize(this);
        _scrollRect.ReloadData();
    }

    public void Click()
    {
        if(selecteEquipItemData != null)
        {
            // 서버 연결
            Managers.Instance.GetServerManager().OnPostEquipEquipment(_factionType, selecteEquipItemData.id);
        }
        ClickCloseBtn();
    }
}
