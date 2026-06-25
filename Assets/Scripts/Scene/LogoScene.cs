using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class LogoScene : MonoBehaviour
{
    [SerializeField] private Image logoImg;
    [SerializeField] private float loadingTime = 2f;

    private IEnumerator Start()
    {
        // 1. 하드웨어(모니터) 동기화를 끕니다.
        QualitySettings.vSyncCount = 0;
    
        // 2. 이제 소프트웨어적으로 원하는 프레임을 설정
        Application.targetFrameRate = 60;
    
        // 3. 백그라운드에서도 게임이 멈추지 않게 합니다.
        Application.runInBackground = true;
        
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 로고 페이드 아웃
        if (logoImg != null)
            logoImg.DOFade(0f, loadingTime);
        
        yield return new WaitForSeconds(loadingTime);
        
        MyLogger.Log("[LogoScene] Managers 초기화 시작");
        Managers.Instance.Init();

        
        SceneManager.LoadScene(Loading.Patch);
    }
}