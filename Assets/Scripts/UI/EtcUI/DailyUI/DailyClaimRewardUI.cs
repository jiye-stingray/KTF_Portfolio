using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DailyClaimRewardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TextMeshProUGUI rewardValue;
    [SerializeField] private CanvasGroup cg;

    [Header("Reward Icon")]
    [SerializeField] private Sprite rewardGold;
    [SerializeField] private Sprite rewardDia;
    
    private void Start()
    {
        Hide(); // 시작 시 숨김 처리
    }
    
    public void Show(DailyReward reward)
    {
        if (reward == null)
        {
            Debug.LogWarning("[DailyClaimRewardUI] reward가 null입니다.");
            return;
        }

        rewardIcon.sprite = reward.Type == DailyReward.DailyRewardType.GOLD ? rewardGold : rewardDia;
        rewardValue.text = $"획득! {reward.Value} {reward.Name}";



        // DoTween
        // 초기 상태 설정
        transform.localScale = Vector3.zero;
        cg.alpha = 0f;

        // 동시에 알파값 + 스케일 애니메이션
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));
        seq.Join(cg.DOFade(1f, 0.3f));
        seq.OnStart(() => cg.blocksRaycasts = true);
    }
    
    public void Hide()
    {
        // DoTween
        // cg.blocksRaycasts = false;
        // transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
        
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack));
        seq.Join(cg.DOFade(0f, 0.3f));
        seq.OnComplete(() => cg.blocksRaycasts = false);
    }
    
}
