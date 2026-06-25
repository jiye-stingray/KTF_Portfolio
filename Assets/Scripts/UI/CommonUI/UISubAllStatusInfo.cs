using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISubAllStatusInfo : UISubBase
{
    [SerializeField] Transform _content;



    public void SetData(EStatus[] showStatusList, Status status)
    {
        for (int i = 0; i < showStatusList.Length; i++)
        {
            StatusText statusTxt = Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/EtcUI/StatusText",_content).GetComponent<StatusText>();
            statusTxt.SetData(showStatusList[i], status);
        }
    }
}
