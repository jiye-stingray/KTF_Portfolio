using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MinimapActionButton : MonoBehaviour
{
    public int _mapIdx;
    public Vector3 _teleportPosition;
    bool _isTownPortal;

    PortalBuilding _portal;
   

    public void Init(PortalBuilding portal, Vector3 position,  bool  isTownPortal)
    {
        _portal = portal;
        _mapIdx = portal.GetZoneIndex();
        _teleportPosition = position;
        _isTownPortal = isTownPortal;
    }

    public virtual void Click()
    {

        Managers.Instance.GetObjectUnitManager().playerSquad.TeleportHeroes(_mapIdx, _teleportPosition);

        if (!_isTownPortal)
        {
            _portal._destPortal.gameObject.SetActive(false);
            _portal.gameObject.SetActive(false);

            Managers.Instance.GetServerManager().OnOffPortal();
        }

        Managers.Instance.GetUIManager().UIMinimap.ClickCloseBtn();

    }

}
