using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stack 에 안 넣고 그냥 Open 하기.
/// </summary>
public class UIGuideQuest : MonoBehaviour
{
    public GuideQuestOngoingTap _guideQuestOngoingTap;
    public GuideQuestClearTab _guideQuestClearTap;
    public GameObject _returnCameraButton;
    public GuideQuest _data;

    public UserInfoData UserInfoData => Managers.Instance.UserInfo();
    
    public void Init()
    {
        _returnCameraButton.SetActive(false);
        Refresh();
    }
    
    public void Refresh()
    {
        _data = UserInfoData.GetCurrentGuideQuest;
        // 조건에 따라서 Refresh tap 을 세팅하고 Refresh 
        if(UserInfoData.isGuideQuestFinish)
        {
            // UI 삭제
            gameObject.SetActive(false);
            return;
        }

        if(UserInfoData.isGuideQuestClear)
        {
            _guideQuestOngoingTap.Close();
            _guideQuestClearTap.Open();
        }
        else
        {
            _guideQuestOngoingTap.Open();
            _guideQuestClearTap.Close();
        }
    }

    public void ReturnCamera()
    {
        Managers.Instance.GetMapManager().CameraReturn(true).Forget();
        _returnCameraButton.gameObject.SetActive(false);
    }
}
