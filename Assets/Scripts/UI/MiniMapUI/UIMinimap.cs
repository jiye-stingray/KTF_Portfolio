using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIMinimap : UIBase
{
    #region Tab

    [Serializable] 
    public struct EPages
    {
        public AreaMinimapTab AreaMinimapTab;
        public WorldMinimapTab WorldMinimapTab;
    }
    public EPages _pages;

    public enum ETAB_TYPE
    {
        AreaMinimap,
        WorldMinimap
    }
    ETAB_TYPE _currentTab = ETAB_TYPE.AreaMinimap;     
    [SerializeField] public UITabGroup _group;


    #endregion 

    public UISubWorldDifficulty _uiSubWorldDifficulty;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        // 초기 셋팅
        _pages.AreaMinimapTab._warpPointBuildingKeylist = ClientLocalDB_Simple.GetDB<BuildingInfo>(DBKey.BuildingInfo)
            .Where(b => b.Value.BuildingType == Define.EBuildingType.WarpPoint).Select(b => b.Key).ToList();

        return true;
    }

    public void SetZoneOpenToStack(int mapId)
    {
        _pages.AreaMinimapTab.SetZone(mapId);

        OpenToStack();
    }

    public override void Open()
    {
        base.Open();

#if USE_SERVER  
        _group._currentTapGroupBtn = _group._tapGroupBtns[0];
        OnChangeTab();
        Refresh();
#else

        _group._currentTapGroupBtn = _group._tapGroupBtns[0];
        OnChangeTab();
        Refresh();
#endif
    }

    public override void Refresh()
    {
        _group.Set((int)_currentTab);
    }

    public void OnChangeTab()
    {
        _currentTab = (ETAB_TYPE)_group._currentTapGroupBtn._index;
        switch (_currentTab)
        {
            case ETAB_TYPE.AreaMinimap:
                _pages.AreaMinimapTab.Open();
                _pages.WorldMinimapTab.Close();
                break;
            case ETAB_TYPE.WorldMinimap:
                _pages.AreaMinimapTab.Close();
                _pages.WorldMinimapTab.Open();
                break;
        }
    }

    public void InitUISubWorldDifficulty(int id)
    {
        if(_uiSubWorldDifficulty == null)
        {
            _uiSubWorldDifficulty =
            Managers.Instance.GetUIManager().ShowUISubBase<UISubWorldDifficulty>(Managers.Instance.GetUIManager().UIMinimap, "UISubWorldDifficulty");
        }

        _uiSubWorldDifficulty.InitField(id);
        _uiSubWorldDifficulty.OpenToStack();
    }
}
