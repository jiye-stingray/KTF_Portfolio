using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITabGroupButton_Constellation : UITabGroupButton
{
    public override void Set(bool isnotGray)
    {
        if(_index != 0)
        {
            // 대형 노드 오픈이 안되어 있을때는 Lock
            _lock.SetActive(
                !Managers.Instance.UserInfo().GetConstellationItemData(
                    (ClientLocalDB_Simple.GetData<ConstellationBoard>(DBKey.ConstellationBoard,_index + 1).OpenCondition))._isOpen
                );
        }
        base.Set(isnotGray);
    }

    public override void OnClick()
    {
        if (_lock.activeSelf)
        {
            Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("현재 별자리의 큰 별을 해금하세요", "ToastMessage");

            return;
        }
        Managers.Instance.GetUIManager().UIConstellation._currentItemData = null;
        base.OnClick();
    }

}
