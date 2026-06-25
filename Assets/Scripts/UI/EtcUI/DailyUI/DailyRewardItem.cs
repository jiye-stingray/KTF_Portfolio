using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyRewardItem : MonoBehaviour
{
    
    [Header("UI")]
    [SerializeField] private Image background;
    [SerializeField] private Color defaultColor;             // 일반 상태 배경색
    [SerializeField] private Color currentStreakerColor;     // 현재 연속 출석 보상 배경색
    

    [SerializeField] private TextMeshProUGUI dayText;        // "Day" 텍스트
    [SerializeField] private TextMeshProUGUI rewardValue;    // 보상 수치 텍스트

    
    [Header("Reward Icons")]
    [SerializeField] private Image rewardIcon;               // 보상 아이콘 이미지
    [SerializeField] private Sprite rewardGold;              // Gold 아이콘
    [SerializeField] private Sprite rewardDia;               // Dia 아이콘
    
    public void SetRewardDate(int day, int currentStreak, DailyReward reward)
    {
        if (reward == null)
        {
            MyLogger.LogWarning($"[RewardItem] reward 정보가 null입니다. (day: {day})");
            return;
        }

        dayText.text = $"Day {day + 1}";
        rewardValue.text = reward.Value.ToString();

        // 아이콘 설정
        switch (reward.Type)
        {
            case DailyReward.DailyRewardType.GOLD:
                rewardIcon.sprite = rewardGold;
                break;
            case DailyReward.DailyRewardType.DIA:
                rewardIcon.sprite = rewardDia;
                break;
            default:
                MyLogger.LogWarning($"[RewardItem] 알 수 없는 보상 타입: {reward.Type}");
                break;
        }

        // 현재 streak에 해당하는 보상은 강조 색상 처리
        background.color = (day == currentStreak) ? currentStreakerColor : defaultColor;
    }
}
