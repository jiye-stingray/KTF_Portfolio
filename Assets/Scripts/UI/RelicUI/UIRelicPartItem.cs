using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIRelicPartItem : MonoBehaviour
{
    [SerializeField] private ERelicPartsType _relicPartType;
    [SerializeField] private GameObject _equipRoot;
    [SerializeField] Image _gradeBg;
    [SerializeField] Image _icon;
    [SerializeField] GameObject _redDot;

    private AtlasManager AtlasManager => Managers.Instance.GetAtlasManager();
    private UserInfoData UserInfoData => Managers.Instance.UserInfo();
    private UIManager UIManager => Managers.Instance.GetUIManager();
    private RelicPartsItemData _itemData;
    private RelicItemData RelicItemData => UserInfoData.GetRelicItemData(_relicId);
    public int _relicId;
    
    public void Init(int relicId)
    {
        _relicId = relicId;
        bool isEmpty = RelicItemData.IsEmptyParts(_relicPartType);
        _equipRoot.SetActive(!isEmpty);
        SetRedDot(relicId, _relicPartType);
        
        if (isEmpty)
            return;
        
        _itemData = UserInfoData.GetRelicPartsItemData(relicId, _relicPartType, RelicItemData.GetPartsId(_relicPartType));

        RelicParts relicParts = ClientLocalDB_Simple.GetData<RelicParts>(DBKey.RelicParts, $"{_itemData._relicBaseId}_{(int)_itemData._partsType}");
        _icon.sprite = AtlasManager.GetSprite(EAtlasType.RelicAtlas, relicParts.ResourceName);
        _gradeBg.sprite = AtlasManager.GetSprite(EAtlasType.ScrollviewItemAtlas, $"BG_Slot_grade_{_itemData._grade.ToString()}");
    }

    private void SetRedDot(int relicId, ERelicPartsType relicPartsType)
    {
        _redDot.SetActive(RelicItemData.GetPartsId(relicPartsType) == 0 && UserInfoData.EnableRelicPartsEquip(relicId, relicPartsType));
    }

    public void OpenSubRelicDetail()
    {
        int count = UserInfoData.GetRelicPartsItemList(_relicId, _relicPartType).Count;

        if (count == 0)
        {
            UIManager.ShowCommonToastMessage("보유하고 있는 파츠가 없습니다.");
            return;
        }
        
        UISubRelicDetail subRelicDetail = UIManager.ShowUISubBase<UISubRelicDetail>(UIManager.UIRelicManagement, "UISubRelicDetail");
        subRelicDetail.SetData(_relicId, _relicPartType);
        subRelicDetail.OpenToStack();
    }
}
