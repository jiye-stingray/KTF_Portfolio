using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishingUI : UIBase
{

    [Header("Reference")]
    [SerializeField] private FishingMiniGame fishingMinigame;
    [SerializeField] private UIFishDex uiFishDex;
    
    [Header("UI")]
    [SerializeField] private Button startBtn;
    [SerializeField] private Button fishDexBtn;   // 도감 버튼
    [SerializeField] private Button fishingCloseBtn;        // UIFishing 전체 닫기 버튼 (CloseButton)

    [Tooltip("획득 표시를 붙일 부모(예: Content, Grid). 비워두면 이 UI 하위로 생성")]
    [SerializeField] private Transform rewardRoot;

    [Tooltip("획득 UI 프리팹(옵션). Text 또는 Image가 포함되어 있으면 자동으로 세팅합니다.")]
    [SerializeField] private GameObject rewardItemPrefab;

    [Tooltip("프리팹이 없을 때 사용할 기본 텍스트(옵션)")]
    [SerializeField] private Text rewardTextFallback;
    
    [Header("Popup_Reward")]
    [SerializeField] private GameObject popupReward;          // Popup_Reward
    [SerializeField] private TextMeshProUGUI textTitle;             // Popup_Reward/UI_Reward/Text_Tittle (Text or TMP_Text)
    [SerializeField] private Image imageRewardIcon;           // Popup_Reward/UI_Reward/Image_RewardIcon
    [SerializeField] private TextMeshProUGUI textAmount;            // Popup_Reward/UI_Reward/Text_Amount (Text or TMP_Text)
    [SerializeField] private Button buttonClaimReward;        // Popup_Reward/UI_Reward/ButtonClaimReward

    [Header("Text")]
    [SerializeField] private string rewardTitleFormat = "물고기 획득!\n<size=80%>{0}</size>";
    [SerializeField] private string rewardAmountFormat = "x{0}";

    private bool _waitingClaim;
    private bool _isFishing;   // 낚시 진행 중 (LuckySpinUI의 _isSpinning과 동일 패턴)
    [SerializeField] private Button closeButton;            // 보상 팝업 닫기 버튼
    
    
    public override void Open()
    {
        base.Open();
        BindMinigameIfNeeded();
        BindRewardRootIfNeeded();

        if (fishingMinigame != null)
        {
            // 중복 구독 방지: 먼저 해제 후 재구독
            fishingMinigame.OnFishCaught -= HandleFishCaught;
            fishingMinigame.OnFishingEnded -= HandleFishingEnded;
            fishingMinigame.OnFishCaught += HandleFishCaught;
            fishingMinigame.OnFishingEnded += HandleFishingEnded;
        }
        
        UnwirePopupButtons();   // 중복 리스너 방지
        WirePopupButtons();
        HideRewardPopup();
        

        // 처음 열릴 때는 시작 가능
        SetStartInteractable(true);
        
    }

    private void OnDisable()
    {
        if (fishingMinigame != null)
        {
            fishingMinigame.OnFishCaught -= HandleFishCaught;
            fishingMinigame.OnFishingEnded -= HandleFishingEnded;
        }

        UnwirePopupButtons();
    }

    // -----------------------
    // Buttons (UGUI OnClick)
    // -----------------------
    public void OnClickStart()
    {
        BindMinigameIfNeeded();
        if (fishingMinigame == null)
        {
            Debug.LogError("[FishingUI] FishingMiniGame 연결 안됨");
            return;
        }

        // // Start 클릭 시 미니게임 진행
        // fishingMinigame.StartFishing();
        
        HideRewardPopup();          // 혹시 남아있던 팝업 닫기
        _waitingClaim = false;
        _isFishing = true;          // 진행 중 표시 → Close 차단
        SetStartInteractable(false); 

        fishingMinigame.StartFishing();

        
    }

    public void OnClickCancel()
    {
        BindMinigameIfNeeded();
        if (fishingMinigame == null) return;

        fishingMinigame.StopFishing();
    }

    // public void OnClickClose()
    // {
    //     Close();
    // }

    public override void Close()
    {
        // 낚시 진행 중에는 닫기 차단 (버튼/뒤로가기 등 모든 경로 공통 차단)
        if (_isFishing)
        {
            MyLogger.Log("[FishingUI] 낚시 진행 중 — 닫기 무시");
            return;
        }

        OnClickCancel();
        // Release
        base.Close();
    }

    // -----------------------
    // Events
    // -----------------------
    private void HandleFishCaught(Fish fish)
    {
        MyLogger.Log($"[FishingUI] 잡은 물고기: {fish?.fishName}");

        // 도감 등록은 FishingMiniGame.Win()이 단독 담당 (중복 등록 제거)

        //성공 시엔 Claim 전까지 Start 잠금 유지
        _waitingClaim = true;
        SetStartInteractable(false);

        ShowRewardPopup(fish);
        
    }
    
    private void HandleFishingEnded(FishingMiniGame.FishingResult result)
    {
        MyLogger.Log($"[FishingUI] 낚시 종료: {result}");

        if (result == FishingMiniGame.FishingResult.Success)
        {
            // 성공: 보상 Claim 완료까지 닫기 잠금 유지 (_isFishing은 FinishRewardFlow에서 해제)
            if (!_waitingClaim)
            {
                _isFishing = false;
                SetStartInteractable(true);
            }
            return;
        }

        // Fail / Cancel이면 바로 재시작 가능 + 닫기 허용
        _isFishing = false;
        _waitingClaim = false;
        HideRewardPopup();
        SetStartInteractable(true);
    }
    

    // -----------------------
    // Internal
    // -----------------------
    private void BindMinigameIfNeeded()
    {
        if (fishingMinigame != null) return;

        fishingMinigame = GetComponentInChildren<FishingMiniGame>(true);
        if (fishingMinigame != null) return;

        fishingMinigame = FindObjectOfType<FishingMiniGame>(true);

        if (fishingMinigame == null)
            Debug.LogError("[FishingUI] FishingMiniGame 레퍼런스를 찾지 못했어. 인스펙터에 연결하거나 UI 하위에 배치해줘.");
    }
    
    // -----------------------
    // Reward UI
    // -----------------------
    private void ShowRewardPopup(Fish fish)
    {
        if (popupReward == null) return;

        popupReward.SetActive(true);

        if (textTitle != null)
            textTitle.text = string.Format(rewardTitleFormat, fish?.fishName ?? "???");

        if (textAmount != null)
            textAmount.text = string.Format(rewardAmountFormat, 1);

        if (imageRewardIcon != null && fishingMinigame != null)
        {
            var sprite = fishingMinigame.GetCurrentFishSprite();
            if (sprite != null) imageRewardIcon.sprite = sprite;
        }
    }
    
    
    private void BindRewardRootIfNeeded()
    {
        if (rewardRoot != null) return;
        // 기본은 UI 하위
        rewardRoot = transform;
    }
    
    private void SetStartInteractable(bool interactable)
    {
        if (startBtn != null) startBtn.interactable = interactable;

        // 낚시 진행 중에는 도감/전체 닫기 버튼도 함께 잠금, 끝나면 함께 해제
        if (fishDexBtn != null) fishDexBtn.interactable = interactable;
        if (fishingCloseBtn != null) fishingCloseBtn.interactable = interactable;
    }
     
    // -----------------------
    // Reward Popup
    // -----------------------
    private void WirePopupButtons()
    {
        if (buttonClaimReward != null)
            buttonClaimReward.onClick.AddListener(OnClickClaimReward);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnClickCloseReward);
    }
    
    private void UnwirePopupButtons()
    {
        if (buttonClaimReward != null)
            buttonClaimReward.onClick.RemoveListener(OnClickClaimReward);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnClickCloseReward);
    }
    
    private void OnClickClaimReward()
    {
        // TODO: 실제 인벤토리/재화 지급 로직은 여기서 연결
        // 예) Inventory.AddFish(fishingMinigame.CurrentFish);

        FinishRewardFlow();
    }
    
    private void OnClickCloseReward()
    {
        // Claim 버튼 대신 닫기만 해도(원하면) 흐름 종료
        FinishRewardFlow();
    }
    
    private void FinishRewardFlow()
    {
        _waitingClaim = false;
        _isFishing = false;   // 보상 수령 완료 → 닫기 허용
        HideRewardPopup();
        SetStartInteractable(true);
    }
    
    private void HideRewardPopup()
    {
        if (popupReward != null)
            popupReward.SetActive(false);
    }
    
    public void OnClickFishDex()
    {
        if (uiFishDex == null)
        {
            Debug.LogError("[FishingUI] uiFishDex 연결 안됨");
            return;
        }

        uiFishDex.Show();
    }
    
    public void OnClickCloseFishDex()
    {
        if (uiFishDex != null)
            uiFishDex.Hide();
    }
    
}