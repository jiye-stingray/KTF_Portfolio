using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum EHelpType
{
    Character,
    Equipment,
    Constellation,
    Training,
    Relic,
    EquipmentUpgrade,       // 재련
    FactionRelation, // 상성
}

public class UISubHelp : UISubBase
{
    public EHelpType _helpType;

    [SerializeField] TMP_Text _titleTxt;

    [Header("DescriptionArea")]
    [SerializeField] GameObject _characterArea;
    [SerializeField] GameObject _equipmentArea;
    [SerializeField] GameObject _constellationArea;
    [SerializeField] GameObject _trainingArea;
    [SerializeField] GameObject _relicArea;
    [SerializeField] GameObject _equipmentUpgradeArea;
    [SerializeField] GameObject _factionRelationArea;

    public void SetType(EHelpType helpType)
    { 
        _helpType = helpType; 
        OpenToStack();
        Refresh();
    }

    public override void Refresh()
    {
        _characterArea.SetActive(false);
        _equipmentArea.SetActive(false);
        _constellationArea.SetActive(false);
        _trainingArea.SetActive(false);
        _relicArea.SetActive(false);
        _equipmentUpgradeArea.SetActive(false);
        _factionRelationArea.SetActive(false);
        switch (_helpType)
        {
            case EHelpType.Character:
                _titleTxt.text = "[영웅]에 대해";
                _characterArea.SetActive(true);
                break;
            case EHelpType.Equipment:
                _titleTxt.text = "[장비]에 대해";
                _equipmentArea.SetActive(true);
                break;
            case EHelpType.Constellation:
                _titleTxt.text = "[별자리]에 대해";
                _constellationArea.SetActive(true);
                break;
            case EHelpType.Training:
                _titleTxt.text = "[훈련소]에 대해";
                _trainingArea.SetActive(true);
                break;
            case EHelpType.Relic:
                _titleTxt.text = "[유물]에 대해";
                _relicArea.SetActive(true);
                break;
            case EHelpType.EquipmentUpgrade:
                _titleTxt.text = "[재련]에 대해";
                _equipmentUpgradeArea.SetActive(true);
                break;
            case EHelpType.FactionRelation:
                _titleTxt.text = "[상성]에 대해";
                _factionRelationArea.SetActive(true);
                break;
        }
    }

    public void GoToLinkBtnClick()
    {
        Application.OpenURL("https://naver.me/FG33St3Z");
    }
}
