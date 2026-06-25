using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EconomyMainPopup : UIBase
{
    public TextMeshProUGUI txtTime = null;
    // public TextMeshProUGUI txtCurrentStage = null;
    // public TextMeshProUGUI txtCurrentKillMonster = null;
    
    public TextMeshProUGUI txtBattery = null;
    public Slider slider = null;
    
    public GameObject sprNotReachable = null;
    public GameObject sprDataNetWork = null;
    public GameObject sprWifi = null;

    public GameObject goDarkMode = null;

    // private GameObject goPlayCamera = null;
    public float waitTime = 10f;
    private float elapsedTime = 0f;
    private int seconds = 0;
    private int minutes = 0;
    private int hours = 0;

    
    // 여러번 그리지 못하게...  중복 실행 방지
    // 보상 생성 아이템이 있는지?
    private bool isDraw = false;

    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override void Open()
    {
        // (이전 값이 남아있지 않도록)
        elapsedTime = 0;
        seconds = 0;
        minutes = 0;
        hours = 0;
        waitTime = 10f;
        
        UpdateTimerText();
        
        Refresh(); // Open 시 최신 데이터 갱신
        
        base.Open();
        
        // Slider Init
        slider.onValueChanged.RemoveListener(OnSliderDrag); // 기존 리스너 제거
        slider.onValueChanged.AddListener(OnSliderDrag);
        slider.value = 0.0f;
        
        
        
        Debug.LogError("EconomyMainPopup Open");
    }

    public override void Close()
    {
        base.Close();
        
        // 설정에서 BGM이 켜져 있을 때만 재생
        // 설정에서 효과음이 켜져 있을 때만 재생
    }

    public void InitData()
    {
        // RefreshUI(); 
    }

    public override void Refresh()
    {
        base.Refresh();
        RefreshUI();
        
    }
    void RefreshUI()
    {
        // 배터리 정보 표시
        float batteryLevel = SystemInfo.batteryLevel * 100;
        txtBattery.text = $"{batteryLevel}%";

        // 네트워크 상태 업데이트
        UpdateNetworkStatus();
        
        if (!isDraw)
        {
            isDraw = true;
            CheckAddNewItem();
        }
        
        MyLogger.Log("RefreshUI");
    }
    
    private void CheckAddNewItem()
    {
        if (isDraw) return;

        isDraw = true;
        // 아이템 갱신 로직 추가
        isDraw = false;
    }
    
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= 1f)
        {
            elapsedTime = 0;
            seconds++;

            if (seconds >= 60)
            {
                seconds = 0;
                minutes++;
            }
            if (minutes >= 60)
            {
                minutes = 0;
                hours++;
            }

            UpdateTimerText();
        }

        waitTime -= Time.deltaTime;
        if (waitTime <= 0f)
        {
            waitTime = 0f;
            goDarkMode.SetActive(true);
        }
    }
    
    private void UpdateTimerText()
    {
        txtTime.text = string.Format("{0}시간 {1}분 {2}초", hours, minutes, (int)seconds);
        // txtCurrentKillMonster.text = string.Format(I2.Loc.LocalizationManager.GetTranslation("1492"), PlayingActionTracerSystem.Instance.killCountForLog);
    }
    
    public void OnCloseDarkMode()
    {
        waitTime = 10f;
        goDarkMode.SetActive(false);
    }
    
    public void OnSliderDrag(float value)
    {
        waitTime = 10f;
        if (value >= 0.93)
        {
            Close();
        }
    }
    
    
    private void UpdateNetworkStatus()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                sprNotReachable.SetActive(true);
                sprDataNetWork.SetActive(false);
                sprWifi.SetActive(false);
                break;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                sprNotReachable.SetActive(false);
                sprDataNetWork.SetActive(true);
                sprWifi.SetActive(false);
                break;
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                sprNotReachable.SetActive(false);
                sprDataNetWork.SetActive(false);
                sprWifi.SetActive(true);
                break;
        }
    }
    
    
    
}
