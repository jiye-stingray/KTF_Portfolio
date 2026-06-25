using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class UISubWorldDifficulty : UISubBase
{
    [SerializeField] TMP_Text _fieldNameTxt;
    [SerializeField] TMP_Text _difficultyTxt;
    [SerializeField] WorldLevelSwitchDataButton _switchDataRightBtn;
    [SerializeField] WorldLevelSwitchDataButton _switchDataLeftBtn;

    [SerializeField] FirstFieldRewardButton _firstFieldRewardButton;
    [SerializeField] CurrencyIcon[] _currencyIcons;      //획득 가능 보상 ID

    [Header("Tooltip")]
    [SerializeField] GameObject _uitooltip;
    [SerializeField] TMP_Text _descTxt;

    IndexWrapper _index = new IndexWrapper();

    FieldItemData _fieldItemData;
    FieldInfo _fieldInfo;

    public int _fieldId;
    public int _difficultyLevel;

    public void InitField(int fieldId)
    {
        _fieldId = fieldId;
        if (_fieldId == UserInfoData._fieldId)     // 클릭한 맵에 위치해 있음
        {
            _difficultyLevel =
                UserInfoData.CurrentDifficultyLevel;
        }
        else
            _difficultyLevel = UserInfoData._dicFieldItemData[_fieldId].currentDifficultyLevel;

        _index._index = _difficultyLevel;

        _fieldInfo = ClientLocalDB_Simple.GetData<FieldInfo>(DBKey.FieldInfo, _fieldId);
        _fieldItemData = UserInfoData._dicFieldItemData[fieldId];
        _fieldNameTxt.text = _fieldInfo.Name;
        _switchDataLeftBtn.Init(fieldId, _index, 1, 10, ChangeDifficulty);
        _switchDataRightBtn.Init(fieldId, _index, 1, 10, ChangeDifficulty);
    }

    public override void Open()
    {
        base.Open();
        ShowUITooltip(false);
        Refresh();
    }

    public override void Refresh()
    {

        _firstFieldRewardButton.SetData(_fieldId,_difficultyLevel,_fieldItemData.isFirstClearRewardGet[_difficultyLevel - 1]);
        _difficultyTxt.text = $"{_difficultyLevel}단계";


        for (int i = 0; i < _currencyIcons.Length; i++)
        {
            _currencyIcons[i].Init((ECurrency)_fieldInfo.FieldRewardIcon[i]);

        }
    }

    public void ChangeDifficulty(int index)
    {
        _difficultyLevel = index;
        Refresh();
        _switchDataLeftBtn.Refresh();
        _switchDataRightBtn.Refresh();
    }

    public void ClickHelpBtn()
    {
        ShowUITooltip(true,$"{ClientLocalDB_Simple.GetData<SkillBase>(DBKey.NpcPassiveSkillBase, _fieldInfo.FieldPassiveType).Description}");
    }

    /// <summary>
    /// Btn 연결용 Event
    /// </summary>
    public void HideTooltip()
    {
        ShowUITooltip(false);
    }

    public void ShowUITooltip(bool show, string text = null)
    {
        _uitooltip.SetActive(show);
        if (!show) return;
        _descTxt.text = text;
    }

    public void EntranceBtnClick()
    {
        bool isShow = false;
        if(_fieldId != 1 ) // 초원 맵(기본 맵)은 해당사항 X 
        {
            if( UserInfoData._fieldId == 1 )        // 현재 유저가 초원맵
            {
                // 최초 입장이 아니면서 
                // 이전 맵 ID와 이전 맵 난이도가 아닐 때 
                if(UserInfoData._previousFieldID != 1 && 
                    (UserInfoData._previousFieldID != _fieldId || UserInfoData._dicFieldItemData[_fieldId].currentDifficultyLevel != _difficultyLevel))          
                {
                    isShow = true;
                }

            }
            else if(_difficultyLevel != UserInfoData.CurrentDifficultyLevel || _fieldId != UserInfoData._fieldId)
            {
                isShow = true;
            }
        }

        if(isShow)
        {
            string desc = $"현재 진행중인 필드의 진척도가 초기화 됩니다.";
            UIManager.ShowConfirmPopUp(desc, "입장하시겠습니까?", () =>
            {
                // 맵 이동 
                // 서버 연결 추후 
#if USE_SERVER
                UserInfoData._portalData.active = false; // 포탈 닫기
                Managers.Instance.GetServerManager().OnPostChangeMap(_fieldId, _difficultyLevel, true);
#else


                UserInfoData._fieldId = _fieldId;

                Managers.Instance.GetMapManager().UnLoadMap();
                Loading.Load(Loading.Field);
#endif
            });

        }
        else
            Managers.Instance.GetServerManager().OnPostChangeMap(_fieldId, _difficultyLevel, false);
    }

    public override void ClickCloseBtn()
    {
        UIManager.UIMinimap._uiSubWorldDifficulty = null;
        base.ClickCloseBtn();
    }
}
