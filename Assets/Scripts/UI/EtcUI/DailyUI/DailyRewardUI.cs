using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DailyReward
{
    public enum DailyRewardType
    {
        GOLD,       // 골드
        DIA         // 가상 화폐
    }

    public DailyRewardType Type;    // 보상 타입
    public int Value;               // 보상 값
    public string Name;             // UI에서 표시할 보상 이름
}

public class DailyRewardUI : UIPopupBase
{
    // 현재 보상을 수령할 수 있는지 여부
    private bool canClaimReward;

    // 최대 연속 보상 횟수 8일
    private int maxStreakCount = 8;

    // 수령 쿨타임 (단위: 시간)
    private float claimCoolDown = 24f;

    // 연속 보상 마감 기한 (단위: 시간)
    private float claimDeadLine = 48f;

    private List<DailyRewardItem> rewardPrefabs;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button claimBtn;
    
    [SerializeField] private DailyRewardItem rewardItemPrefab;
    [SerializeField] private Transform rewardsGrid;
    
    [Header("Reward Data")]
    [SerializeField] private List<DailyReward> rewards;

    [Header("UI")]
    [SerializeField] private DailyClaimRewardUI claimRewardUI;
    
    // current
    private int currentStreak
    {
        get => PlayerPrefs.GetInt("currentStreak", 0);
        set => PlayerPrefs.SetInt("currentStreak", value);
    }

    // 마지막 보상 수령 시간
    private DateTime? lastClaimTime
    {
        get
        {
            string data = PlayerPrefs.GetString("lastClaimedTime", null);
            if (!string.IsNullOrEmpty(data))
                return DateTime.Parse(data);
            return null;
        }
        set
        {
            if (value != null)
                PlayerPrefs.SetString("lastClaimedTime", value.ToString());
            else
                PlayerPrefs.DeleteKey("lastClaimedTime");
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        InitDailyRewardUI();
        
        StartCoroutine(CoRewardsStateUpdater());
        
        return true;
    }
    
    void Awake()
    {
#if UNITY_EDITOR
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
#endif
    }
    
  
    
    // 보상 UI 프리팹 초기화
    private void InitDailyRewardUI()
    {
        rewardPrefabs = new List<DailyRewardItem>();

        for (int i = 0; i < maxStreakCount; i++)
        {
            var prefab = Instantiate(rewardItemPrefab, rewardsGrid, false);
            rewardPrefabs.Add(prefab);
        }
    }

    #region TestCode
    // 1초마다 보상 수령 가능 상태 갱신
    private IEnumerator CoRewardsStateUpdater()
    {
        while (true)
        {
            UpdateRewardsState();
            yield return new WaitForSeconds(1f);
        }
    }

    #endregion

    #region UI

    private void UpdateRewardsState()
    {
        canClaimReward = true;

        if (lastClaimTime.HasValue)
        {
            var timeSpan = DateTime.UtcNow - lastClaimTime.Value;

            if (timeSpan.TotalHours > claimDeadLine)
            {
                // 마감시간 초과 시 streak 초기화
                lastClaimTime = null;
                currentStreak = 0;
            }
            else if (timeSpan.TotalHours < claimCoolDown)
            {
                // 쿨타임 미도달 시 수령 불가
                canClaimReward = false;
            }
        }

        UpdateRewardsUI();
    }
    
    private void UpdateRewardsUI()
    {
        claimBtn.interactable = canClaimReward;

        if (canClaimReward)
        {
            statusText.text = "보상을 수령하세요!";
        }
        else if (lastClaimTime.HasValue)
        {
            var nextClaimTime = lastClaimTime.Value.AddHours(claimCoolDown);
            var remainingTime = nextClaimTime - DateTime.UtcNow;

            string cooldownStr = $"{remainingTime.Hours:D2}:{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
            statusText.text = $"다음 보상까지 {cooldownStr} 남음";
        }

        for (int i = 0; i < rewardPrefabs.Count; i++)
        {
            var rewardData = (i < rewards.Count) ? rewards[i] : null;
            rewardPrefabs[i].SetRewardDate(i, currentStreak, rewardData);
        }
    }

    #endregion
    
    public void OnClickClaimReward()
    {
        if (!canClaimReward)
            return;

        var reward = rewards[currentStreak];

        switch (reward.Type)
        {
            case DailyReward.DailyRewardType.GOLD:
                // GameControl.Instance.AddGold(reward.Value);
                break;
            case DailyReward.DailyRewardType.DIA:
                // GameControl.Instance.AddCabolls(reward.Value);
                break;
        }

        // ClaimRewardPanel.Instance.Show(reward);
        claimRewardUI.Show(reward);
        
        lastClaimTime = DateTime.UtcNow;
        currentStreak = (currentStreak + 1) % maxStreakCount;

        UpdateRewardsState();
    }
    
    public override void Close()
    {
        base.Close();
 
    }
}
