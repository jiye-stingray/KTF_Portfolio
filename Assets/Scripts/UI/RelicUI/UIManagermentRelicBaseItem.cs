using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIManagermentRelicBaseItem : MonoBehaviour
{
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _levelTxt;
    [SerializeField] GameObject _lockGo;

    [SerializeField] GameObject _redDot;
    
    private RelicBase _relicBase;
    private UserInfoData UserInfoData => Managers.Instance.UserInfo();
    private UIManager UIManager => Managers.Instance.GetUIManager();
    private AtlasManager AtlasManager => Managers.Instance.GetAtlasManager();

    string _relicId;
    private bool _isEmpty;
    private bool _isLocked;
    
    public void Init(string relicId)
    {
        _relicId = relicId;
        RelicBase relicBase = ClientLocalDB_Simple.GetData<RelicBase>(DBKey.RelicBase, relicId);
        RelicItemData relicItemData = UserInfoData.GetRelicItemData(relicBase.Id);
        _icon.sprite = AtlasManager.GetSprite(EAtlasType.RelicAtlas, relicBase.ResourceName);
        _isLocked = !UserInfoData._dicFieldItemData[relicBase.OpenFieldId].isOpen; 
        SetLock();
        if (_isLocked)
            return;
        
        _redDot.SetActive(relicItemData._equipHeroId == 0);
        _levelTxt.text = $"Lv.{relicItemData._level}";
    }

    public void SetEmpty(bool state)
    {
        _isEmpty = state;
        _isLocked = state;
        _icon.gameObject.SetActive(!state);
        _redDot.SetActive(false);
        SetLock();
    }

    private void SetLock()
    {
        _levelTxt.gameObject.SetActive(!_isLocked);
        _lockGo.SetActive(_isLocked);
    }

    public void OnRelicBaseClicked()
    {
        if (_isEmpty || _isLocked)
        {
            UIManager.ShowCommonToastMessage("아직 해금되지 않은 유물입니다.");
            return;
        }

        UIManager.UIRelicManagement.Init(int.Parse(_relicId));
        UIManager.UIRelicManagement.OpenToStack();
    }
}
