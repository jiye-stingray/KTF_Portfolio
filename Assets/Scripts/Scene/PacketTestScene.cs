using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PacketTestScene : MonoBehaviour
{
    [SerializeField]
    BestHttp_GameManager bestHttpGameManager;
    // Start is called before the first frame update
    void Awake()
    {
        Managers.Instance.GetSimpleDBManager().LoadAll();

        // StartCoroutine(LoadSceneProgress());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // private IEnumerator LoadSceneProgress()
    // {
    //     AsyncOperation async = SceneManager.LoadSceneAsync("Dev1");
    //     while (!async.isDone)
    //     {
    //         async.allowSceneActivation = ((double)async.progress > 0.8);
    //         yield return null;
    //     }
    // }
    
    public void OnClickLoginTest()
    {
        MyLogger.Log("OnClickLoginTest");
        bestHttpGameManager.OnLoginResponse();
        
    }
    
    public void OnClickGachaTest()
    {
        MyLogger.Log("OnClickGachaTest");
        bestHttpGameManager.OnGachaResponse();
    }

    public void OnClickActiveSkillLevelUp(int value)
    {
        // bestHttpGameManager.TestOnActiveSkillLevelUp(value);
    }

    public void OnClickPassiveSkill1LevelUp(int value)
    {
        // bestHttpGameManager.TestOnPassiveSkill1LevelUp(value);
    }
    
    public void OnClickPassiveSkill2LevelUp(int value)
    {
        // bestHttpGameManager.TestOnPassiveSkill2LevelUp(value);
    }

    public void OnClickGetMyFieldInfo()
    {
        
    }

    public void OnClickPostOpenFog(int value)
    {
        
    }

    public void OnClickPostOpenWarp(int value)
    {
        
    }

    public void OnClickPostOpenPortal()
    {
        // bestHttpGameManager.OnPostPortal();
    }

    public void OnClickGetOffPortal()
    {
        bestHttpGameManager.OnOffPortal();
    }

    public void OnClickGetMyEvents()
    {
        
    }

    public void OnClickPostClearEvent(int value)
    {
        
    }

    public void OnClickGetMyQuests()
    {
        
    }

    public void OnClickPostClearQuest(int value)
    {
        
    }

    public void OnClickGetMyCampings()
    {
        
    }

    public void OnClickPostDungeonCurrencyCheck()
    {
        // bestHttpGameManager.OnPostDungeonCurrencyCheck();
    }

    public void OnClickPostDungeonRun()
    {
        // bestHttpGameManager.OnPostDungeonRun();
    }

    public void OnClickGetDungeonClear()
    {
        // bestHttpGameManager.OnPostDungeonClear();
    }
}
