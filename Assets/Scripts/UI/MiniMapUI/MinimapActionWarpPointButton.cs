using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MinimapActionWarpPointButton : MinimapActionButton
{
    public int _idx;
    BuildingData _buildingData;

    [SerializeField] GameObject _gray;
    bool isOpen;

    UnityAction action;

    public void Init(int id,int mapIdx, Vector3 position)
    {
        _idx = id;
        _mapIdx = mapIdx;
        _teleportPosition = position;

        _buildingData = Managers.Instance.GetObjectUnitManager().hashBuilding.FirstOrDefault(b => b.BuildingData != null && b.BuildingData._data.ID == _idx).BuildingData;

        action = WarpAction; 

        Refresh();
    }

    public void Refresh()
    {
        if(_buildingData == null)
        {
            Destroy(gameObject);
            return;
        }

        _gray.gameObject.SetActive(!_buildingData.isOpen);
    }

    //UIDefaultPopup confirmPopup;
    public override void Click()
    {
        if (_gray.activeSelf) return;

        //confirmPopup = Managers.Instance.GetUIManager().ShowPopup<UIDefaultPopup>("ConfirmPopup");
        string desc = $"{_buildingData._data.Name}로 이동하시겠습니까?";
        //confirmPopup.Init("순간이동", desc, action);

        Managers.Instance.GetUIManager().ShowConfirmPopUp("순간이동",desc,action);
    }

    private void WarpAction()
    {
        Managers.Instance.GetObjectUnitManager().playerSquad.TeleportHeroes(_mapIdx, _teleportPosition);

/*        if(confirmPopup != null ) 
            confirmPopup.ClickCloseBtn();
        Managers.Instance.GetUIManager().UIMinimap.ClickCloseBtn();*/

        Managers.Instance.GetUIManager().AllCloseStackUI();
    }

}
