using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define; 

public class UIRelicPartDetailItem : MonoBehaviour
{
    [SerializeField] Image _gradeBg;
    [SerializeField] Image _icon;
    [SerializeField] GameObject _lockImg;
    [SerializeField] GameObject _unLockImg;

    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] UIRelicStatusText _mainOptionStatusTxt;
    [SerializeField] UIRelicStatusText[] _subStatusTxt;
    
    AtlasManager AtlasManager => Managers.Instance.GetAtlasManager();

    public void SetItem(RelicPartsItemData relicPartsItemData)
    {
        RelicParts relicParts = ClientLocalDB_Simple.GetData<RelicParts>(DBKey.RelicParts, $"{relicPartsItemData._relicBaseId}_{(int)relicPartsItemData._partsType}");
        
        _gradeBg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{relicPartsItemData._grade.ToString()}");
        _icon.sprite = AtlasManager.GetSprite(EAtlasType.RelicAtlas, relicParts.ResourceName);
        
        if(_lockImg != null)
            _lockImg.SetActive(relicPartsItemData._isLock);
        
        if(_unLockImg != null)
            _unLockImg.SetActive(!relicPartsItemData._isLock);

        _nameTxt.text = relicParts.RelicPartsName;

        Status mainStatus = relicPartsItemData.GetMainPartsStatus();
        _mainOptionStatusTxt.SetData(relicParts.MainStatType, mainStatus);

        List<Status> subPartsStatusList = relicPartsItemData.GetSubPartsStatusList();

        for (int i = 0; i < _subStatusTxt.Length; i++)
        {
            UIRelicStatusText statusText = _subStatusTxt[i];
            bool existOption = i < subPartsStatusList.Count;

            EOptionGradeType gradeType = existOption ? relicPartsItemData.GetSubPartsGrade(i) : EOptionGradeType.None;
            EStatus statusType = existOption ? relicPartsItemData.GetSubPartsStatusType(i) : EStatus.NULL;
            Status subPartsStatus = existOption ? subPartsStatusList[i] : null;
            statusText.SetData(statusType, subPartsStatus, gradeType);
        }
    }
}
