using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class GuideQuestOngoingTap : UITabBase
{
    [SerializeField] UIGuideQuest _uiGuideQuest;
    [SerializeField] Image _questIcon;
    [SerializeField] TMP_Text _questOrderText;
    [SerializeField] TMP_Text _descriptionText;
    [SerializeField] Slider _sliderProgress;
    [SerializeField] TMP_Text _progressTxt;
    
    GuideQuest _data => _uiGuideQuest._data;

    public override void Open()
    {
        base.Open();
        _uiProgressValue = UserInfoData.GuideQuestProgressValue; // 초기 세팅
        Refresh();
    }

    public override void Refresh()
    {
        // icon 처리
        _questIcon.sprite = Define.ReturnGuideQuestIcon(_data.ConditionType, _data.QuestIcon, _data.ConditionValue.First());
        _questOrderText.text = $"[길잡이 - {_data.Order}]";
        _descriptionText.text = DescBuilder.ReturnQuestDesc(_data.ConditionType,_data.Desc,_data.ConditionValue);

        UpdateProgressUI();
    }

    #region Progress

    public void ResetUI()
    {
        isCor = false;
        tempBeforeValue = 0;
        _uiProgressValue = 0;
    }


    float _uiProgressValue;     // UI 연출값용


    /// <summary>
    /// 처음 UI 초기화 (연출 X)
    /// </summary>
    public void SetProgressUI()
    {
        if (isCor) StopCoroutine(updateProgressCor);

        _progressTxt.text = $"{UserInfoData.GuideQuestProgressValue} / {_data.ConditionValue.Last()}";
        _sliderProgress.value = (float)UserInfoData.GuideQuestProgressValue / _data.ConditionValue.Last();

        _uiProgressValue = (float)UserInfoData.GuideQuestProgressValue;         // 저장용 (연출 중간 끊김을 대비)

        if (_data.ConditionValue.Last() <= _uiProgressValue)
        {

            // Clear ! 
            // 클리어 UI 바뀌어야 함
            _uiGuideQuest.Refresh();
        }
    }

    Coroutine updateProgressCor = null;

    private float tempBeforeValue;
    public void UpdateProgressUI()
    {
        // Progress Value Set

        if (isCor)
        {
            StopCoroutine(updateProgressCor);
            _sliderProgress.value = (float)tempBeforeValue / _data.ConditionValue.Last();
            _progressTxt.text = $"{tempBeforeValue} / {_data.ConditionValue.Last()}";
            _uiProgressValue = tempBeforeValue;
        }

        tempBeforeValue = (float)UserInfoData.GuideQuestProgressValue;         // 저장용 (연출 중간 끊김을 대비)
        
        if(gameObject.activeInHierarchy)
            updateProgressCor = StartCoroutine(UpdateProgressCor(_uiProgressValue, (float)UserInfoData.GuideQuestProgressValue));
    }




    bool isCor = false;
    IEnumerator UpdateProgressCor(float startValue, float endValue)
    {
        isCor = true;

        for (float i = startValue; i <= endValue; i++)
        {

            _sliderProgress.value = (float)i / _data.ConditionValue.Last();
            _progressTxt.text = $"{i} / {_data.ConditionValue.Last()}";

            yield return new WaitForSeconds(0.1f);
        }

        _uiProgressValue = endValue;
        isCor = false;

        if (_data.ConditionValue.Last() <= _uiProgressValue)
        {

            // Clear ! 
            // 클리어 UI 바뀌어야 함
            _uiGuideQuest.Refresh();
        }
    }
    #endregion

    public void Click()
    {
        if (!CheckGuideQuestClick(_data.ConditionType,_data.ConditionValue)) return;

        GuideQuest guideQuest = UserInfoData.GetCurrentGuideQuest;
        UserInfoData.QuestMoveAction(guideQuest.ConditionType, guideQuest.ArrivalTarget);
        _uiGuideQuest._returnCameraButton.SetActive(true);
    }

    private bool CheckGuideQuestClick(EQuestConditionType type, int[] conditionValue)
    {
        string nameKey = string.Empty;
        switch (type)
        {
            case EQuestConditionType.DungeonClearTarget:
                EDungeonType dungeonType = Define.ReturnDungeonType(conditionValue.First());
                switch (dungeonType)
                {
                    case EDungeonType.Gold:
                        nameKey = "GoldDungeonEnteranceScrollviewItem";
                        break;
                    case EDungeonType.Equipment:
                        nameKey = "EquipmentDungeonEnteranceScrollviewItem";
                        break;
                    case EDungeonType.Constellation:
                        nameKey = "ConstellationDungeonEnteranceScrollviewItem";
                        break;
                    case EDungeonType.Ranking:
                        nameKey = "RankingDungeonEnteranceScrollviewItem";
                        break;
                    default:
                        break;
                }
                break;
            case EQuestConditionType.DungeonClearAll:
                nameKey = "Btn_Challenge_Dungeon";
                break;
            case EQuestConditionType.TowerEnter:
                nameKey = "TowerDungeonEnteranceScrollviewItem";
                break;
            case EQuestConditionType.EquipmentEquip:
            case EQuestConditionType.EquipmentDismiss:
                nameKey = "Btn_Equipment";
                break;
            case EQuestConditionType.TraningCount:
            case EQuestConditionType.TrainingBasicLevel:
            case EQuestConditionType.TrainingHardLevel:
                nameKey = "Btn_Training";
                break;
            case EQuestConditionType.MercenaryLevelup:
            case EQuestConditionType.AllMercenaryAwakenCount:
                nameKey = "Btn_CharacterInventory";
                break;
            case EQuestConditionType.GachaCount:
                nameKey = "Btn_Gacha";
                break;
            case EQuestConditionType.ConstellationOpen:
                nameKey = "Btn_Constellation";
                break;
            case EQuestConditionType.GuildAttend:
                nameKey = "Btn_Guild";
                break;
            default:
                return true;
        }

        ContentsOpen openContent = ClientLocalDB_Simple.GetData<ContentsOpen>(DBKey.ContentsOpen, nameKey);
        switch (openContent.ConditionType)
        {
            case EContentsOpenType.UserLevel:
                if (UserInfoData.userLevel.Value < openContent.ConditionValue)
                {
                    Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>($"{openContent.ConditionValue}레벨에 해금됩니다", "ToastMessage");
                    return false;
                }
                break;
            case EContentsOpenType.Dialogue:
                if(UserInfoData.dialogKey.Value < openContent.ConditionValue)
                {
                    Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>($"마을로 진입해주세요.", "ToastMessage");
                    return false;
                }
                break;
            default:
                break;
        }
        return true;
    }
}
