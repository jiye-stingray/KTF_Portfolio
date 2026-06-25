using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Define;

/// <summary>
/// 픽업 가챠와 일반 가챠에서만 사용.
/// 천계 가챠에서는 사용 X 
/// </summary>
public class UIGachaProbabilityInfo : UISubBase
{
    [SerializeField] RectTransform _scrollviewTransform;
    [SerializeField] GameObject _legendaryGradeRoot;
    [SerializeField] GameObject _epicGradeRoot;
    [SerializeField] GameObject _rareGradeRoot;
    [SerializeField] GameObject _commonGradeRoot;
    [SerializeField] GameObject _currencyGradeRoot;
    
    [SerializeField] TMP_Text _legendaryProbabilityTxt;
    [SerializeField] TMP_Text _epicProbabilityTxt;
    [SerializeField] TMP_Text _rareProbabilityTxt;
    [SerializeField] TMP_Text _commonProbabilityTxt;
    [SerializeField] TMP_Text _currencyProbabilityTxt;
    
    [SerializeField] Transform _legendaryContent;
    [SerializeField] Transform _epicContent;
    [SerializeField] Transform _rareContent;
    [SerializeField] Transform _commonContent;
    [SerializeField] Transform _currencyContent;
    
    [SerializeField] GameObject _gachaProbabilityScrollviewItemPrefab;
    List<GachaProbabilityInfoScrollviewItem> _itemList = new List<GachaProbabilityInfoScrollviewItem>();
    
    
    private int _pickUpId;
    private int[] _wishList => _gachaItemData._wishList;

    EGachaType _gachaType;
    GachaPercent _gachapercent;
    GachaItemData _gachaItemData;

    public void SetData(EGachaType gachaType) 
    {
        _gachaType = gachaType;
        _gachapercent = ClientLocalDB_Simple.GetData<GachaPercent>(DBKey.GachaPercent, _gachaType);
        _pickUpId = ClientLocalDB_Simple.GetData<GachaSetting>(DBKey.GachaSetting, "PickUpCharacter").Value;
        _gachaItemData = UserInfoData._dicGachaItemData[gachaType];
        
        for (int i = _itemList.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(_itemList[i].gameObject);
        }
        _itemList.Clear();
        
        _legendaryGradeRoot.SetActive(true);
        _epicGradeRoot.SetActive(_gachaType != EGachaType.Celestial);
        _rareGradeRoot.SetActive(_gachaType != EGachaType.Celestial);
        _commonGradeRoot.SetActive(_gachaType != EGachaType.Celestial);
        _currencyGradeRoot.SetActive(_gachaType == EGachaType.Celestial);
        
        if (_gachaType != EGachaType.Celestial)
            SetGeneralProbability();
        else
            SetCelestialProbability();
    }
    private void SetGeneralProbability()
    {
        _legendaryProbabilityTxt.text = $"{(_gachapercent.Legendary / 100f):F4}%";
        _commonProbabilityTxt.text = $"{(_gachapercent.Common / 100f):F4}%";
        _rareProbabilityTxt.text = $"{(_gachapercent.Rare / 100f):F4}%";
        _epicProbabilityTxt.text = $"{(_gachapercent.Epic / 100f):F4}%";
        
        List<UnitData> legendaryUnitDatas = ClientLocalDB.GetGradeUnitList(EGradeType.Legendary);
        List<UnitData> epicUnitDatas = ClientLocalDB.GetGradeUnitList(EGradeType.Epic);
        List<UnitData> rareUnitDatas = ClientLocalDB.GetGradeUnitList(EGradeType.Rare);
        List<UnitData> commonUnitDatas = ClientLocalDB.GetGradeUnitList(EGradeType.Common);
        
        if (_wishList != null && _wishList.Length > 0)
        {
            // 위시리스트가 존재하면 위시리스트만 포함
            legendaryUnitDatas = legendaryUnitDatas
                .Where(unit => _wishList.Contains(unit.ID))
                .ToList();
        }
        else
        {
            legendaryUnitDatas = legendaryUnitDatas
                .Where(unit => unit.Faction != EFactionType.Celestial && unit.Gatcha)
                .ToList();
        }

        int pickUpCount = 0;
        float pickUpProbability = _gachapercent.Legendary / 100f * _gachapercent.Adjustment / 10000f;
        if(_gachapercent.Adjustment > 0)
            pickUpCount = 1;
        float legendaryProbability = (_gachapercent.Legendary / 100f - pickUpProbability) / (legendaryUnitDatas.Count - pickUpCount);
        float epicProbability = _gachapercent.Epic / 100f / epicUnitDatas.Count;
        float rareProbability = _gachapercent.Rare / 100f / rareUnitDatas.Count;
        float commonProbability = _gachapercent.Common / 100f / commonUnitDatas.Count;

        if (pickUpProbability != 0)
        {
            UnitData pickUpUnit = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, _pickUpId);
            List<UnitData> unitList = new List<UnitData>();
            if (pickUpUnit.StartGrade == EGradeType.Legendary)
                unitList = legendaryUnitDatas;
            else if (pickUpUnit.StartGrade == EGradeType.Epic)
                unitList = epicUnitDatas;
            
            unitList.Sort((a, b) =>
            {
                if (a.ID == _pickUpId) return -1; // a가 픽업 대상이면 앞으로
                if (b.ID == _pickUpId) return 1;  // b가 픽업 대상이면 뒤로
                return 0; // 둘 다 아니면 그대로
            });
        }
        
        CreateGeneralUnitScrollViewItem(legendaryUnitDatas, legendaryProbability, pickUpProbability, _legendaryContent);
        CreateGeneralUnitScrollViewItem(epicUnitDatas, epicProbability, pickUpProbability, _epicContent);
        CreateGeneralUnitScrollViewItem(rareUnitDatas, rareProbability, pickUpProbability, _rareContent);
        CreateGeneralUnitScrollViewItem(commonUnitDatas, commonProbability, pickUpProbability, _commonContent);
    }
    
    private void SetCelestialProbability()
    {
        Dictionary<string, CelestialGachaTable> celestialGachaTables = ClientLocalDB_Simple.GetDB<CelestialGachaTable>(DBKey.CelestialGachaTable);

        _legendaryProbabilityTxt.text = $"{(_gachapercent.Legendary / 100f):F4}%";
        _currencyProbabilityTxt.text = $"{(_gachapercent.Common / 100f):F4}%";
        foreach (var data in celestialGachaTables)
        {
            CelestialGachaTable celestialGachaTable = data.Value;
            int itemId;
            Transform parent;
            if (celestialGachaTable.Type == ERewardType.Character)
            {
                itemId = _gachaItemData._wishList[0];
                parent = _legendaryContent;
            }
            else
            {
                itemId = celestialGachaTable.ItemID;
                parent = _currencyContent;
            }
            CreateScrollViewItem(celestialGachaTable.Type, itemId, celestialGachaTable.ItemValue, celestialGachaTable.Percent * 0.01f, false, parent);
        }
    }

    private void CreateScrollViewItem(ERewardType rewardType, int id, int count, float probability, bool isPickup, Transform parent)
    {
        GachaProbabilityInfoScrollviewItem item = GameObject.Instantiate(_gachaProbabilityScrollviewItemPrefab, parent)
            .GetComponent<GachaProbabilityInfoScrollviewItem>();
        item.InitData(rewardType, id, count, probability, isPickup);
        _itemList.Add(item);
    }

    private void CreateGeneralUnitScrollViewItem(List<UnitData> unitDatas, float probability, float pickUppProbability, Transform parent)
    {
        foreach (var unit in unitDatas)
        {
            bool pickedUp = unit.ID == _pickUpId;
            CreateScrollViewItem(ERewardType.Character, unit.ID, 0, pickedUp ? pickUppProbability : probability, pickedUp, parent);
        }
    }
}
