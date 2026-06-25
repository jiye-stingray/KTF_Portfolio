using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentItemUI : MonoBehaviour
{
    [SerializeField] GameObject _gray;
    public bool IsLock =>  _gray.activeSelf;
    UserInfoData userInfoData => Managers.Instance.UserInfo();
    ContentsOpen openContentBase => ClientLocalDB_Simple.GetData<ContentsOpen>(DBKey.ContentsOpen, gameObject.name);
    public void Refresh()
    {
        #if TUTO
        _gray?.SetActive(false);

        if (openContentBase != null)
        {
            switch (openContentBase.ConditionType)
            {
                case Define.EContentsOpenType.UserLevel:
                    gameObject.SetActive(userInfoData.userLevel.Value >= openContentBase.ConditionValue);
                    break;
                case Define.EContentsOpenType.Dialogue:
                    gameObject.SetActive(userInfoData.dialogKey.Value >= openContentBase.ConditionValue);
                    break;
                default:
                    break;
            }

            _gray?.SetActive(openContentBase.Lock);
        }

        #endif
    }
}
