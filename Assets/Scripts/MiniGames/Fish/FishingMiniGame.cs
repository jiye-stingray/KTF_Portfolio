using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.U2D;

public class FishingMiniGame : MonoBehaviour
{
    public enum FishingResult { Success, Fail, Cancel }
    
    const float ProgressInitial = 0.0f;
    const float Half = 0.5f;

    [Header("Fishing Area")]
    [Tooltip("waterRect의 top 과 bottom 을 bounds로 계산해서 할당합니다")]
    Transform topBounds;
    Transform bottomBounds;

    [Tooltip("연결하면 시작 시 이동 범위를 Water 이미지의 위/아래 끝에 맞춥니다. 비우면 Top/BottomBounds 사용")]
    [SerializeField] RectTransform waterRect;

    [Header("Fish Movement")]
    [SerializeField] Transform fish;
    [SerializeField] float smoothMotion = 3f;
    [SerializeField] float fishTimeRandomizer = 3f;

    [Header("Hook")]
    [SerializeField] Transform hook;
    [Tooltip("시작 시 (Hook 이미지 높이 + 여유) ÷ 이동범위로 자동 계산됩니다 (인스펙터 값은 무시)")]
    [SerializeField] float hookSize = 0.18f;
    [Tooltip("실제 잡기 영역을 보이는 훅보다 후하게 만드는 여유 픽셀(위아래 합산). 7이면 기존 0.15 세팅과 동일, 0이면 시각과 정확히 일치")]
    [SerializeField] float hookCatchPaddingPx = 7f;
    [SerializeField] float hookSpeed = 0.1f;
    [SerializeField] float hookGravity = 0.05f;

    [Header("Progress Bar")]
    [SerializeField] Image progressBarFill;
    [SerializeField] Transform progressBarContainer;
    [SerializeField] float hookPower = 0.5f;
    [SerializeField] float progressBarDecay = 0.23f;

    [Header("Pull Button (모바일)")]
    [Tooltip("버튼을 누르고 있는 동안 훅이 올라갑니다. 비워두면 Z키/스페이스 사용")]
    [SerializeField] Button pullButton;

    [Header("Timer UI (옵션)")]
    [Tooltip("남은 초(실패 타이머)를 표시할 Text. 비워두면 표시 안함")]
    [SerializeField] private TextMeshProUGUI remainSecText;
    [Tooltip("표시 포맷 예: '남은 시간: {0:0.0}s'")]
    [SerializeField] private string remainSecFormat = "남은 시간: {0:0.0}s";
    
    [Header("Timer & Events")]
    [SerializeField] float failTimerMax = 10f;
    [SerializeField] UnityEvent winEvent = new UnityEvent();
    [SerializeField] UnityEvent loseEvent = new UnityEvent();

    [Header("Fish Data (JSON)")]
    [Tooltip("Fish.json(TextAsset)을 연결하세요. 예: [ {...}, {...} ] 또는 {\"Fishes\":[...]}")]
    [SerializeField] private TextAsset fishJson;

    [Header("Atlas")]
    [SerializeField] private string fishAtlasPath = "Atlas/FishAtlas"; // Resources/Atlas/FishAtlas.spriteatlasv2
    private SpriteAtlas _atlas;
    private Image _fishImage;

    // -----------------------
    // Runtime
    // -----------------------
    float _fishPosition;
    float _fishSpeed;
    float _fishTimer;
    float _fishTargetPosition;

    float _hookPosition;
    float _hookPullVelocity;

    float _catchProgress;
    float _failTimer;

    float _topY;
    float _bottomY;

    bool _paused;
    bool _isPullHeld;
    bool _useUgui;
    bool _ended;
    
    // -----------------------
    // Fish Selection
    // -----------------------
    [Serializable]
    private class FishDatabase
    {
        public List<Fish> Fishes;
    }

    private readonly List<Fish> _fishList = new List<Fish>();
    private bool _fishLoaded = false;

    public Fish CurrentFish { get; private set; }
    public event Action<Fish> OnFishCaught;
    public event Action<FishingResult> OnFishingEnded;
    
    // -----------------------
    // Public API
    // -----------------------
    public void StartFishing()
    {
        gameObject.SetActive(true);

        LoadFishFromJsonIfNeeded();
        SelectRandomFishFromJson();    

        if (CurrentFish == null)
        {
            Debug.LogError("[FishingMiniGame] CurrentFish 없음 — 미니게임 시작 중단 (fishJson/Atlas 확인 필요)");
            End(FishingResult.Fail);
            return;
        }

        ResetState();
        SetupPullButton();
        CacheBounds();
        ApplyAutoHookSize();   // 이동범위(_topY/_bottomY) 확정 후 계산

        UpdateRemainSecUI();
        
    }

    public void StopFishing()
    {
        // StopMiniGame();
        End(FishingResult.Cancel);
    }

    public void StopMiniGame()
    {
        // _paused = true;
        // gameObject.SetActive(false);
        End(FishingResult.Cancel);
    }
    
    public Sprite GetCurrentFishSprite()
    {
        return _fishImage != null ? _fishImage.sprite : null;
    }
    
    void End(FishingResult result)
    {
        if (_ended) return;

        _ended = true;
        _paused = true;

        OnFishingEnded?.Invoke(result);

        gameObject.SetActive(false);
    }

    // -----------------------
    // Unity
    // -----------------------
    void FixedUpdate()
    {
        if (_paused || _ended) return;

        float dt = Time.fixedDeltaTime;
        UpdateFish(dt);
        UpdateHook(dt);
        UpdateProgress(dt);

        UpdateRemainSecUI();
    }

    // -----------------------
    // JSON
    // -----------------------
    private void LoadFishFromJsonIfNeeded()
    {
        if (_fishLoaded) return;
        _fishLoaded = true;

        _fishList.Clear();

        if (fishJson == null || string.IsNullOrWhiteSpace(fishJson.text))
        {
            Debug.LogError("[FishingMiniGame] fishJson(TextAsset)이 비어있어. Fish.json 연결 필요");
            return;
        }

        try
        {
            // 1) JSON이 배열인 경우: [ {...}, {...} ]
            var list = JsonConvert.DeserializeObject<List<Fish>>(fishJson.text);
            if (list != null && list.Count > 0)
            {
                _fishList.AddRange(list);
                MyLogger.Log($"[FishingMiniGame] Fish.json 로드 완료 (array): {_fishList.Count}종");
                return;
            }

            // 2) JSON이 래퍼 객체인 경우: { "Fishes": [ ... ] }
            var db = JsonConvert.DeserializeObject<FishDatabase>(fishJson.text);
            if (db?.Fishes != null && db.Fishes.Count > 0)
            {
                _fishList.AddRange(db.Fishes);
                MyLogger.Log($"[FishingMiniGame] Fish.json 로드 완료 (wrapper): {_fishList.Count}종");
                return;
            }

            Debug.LogError("[FishingMiniGame] Fish.json 파싱은 되었는데 목록이 비어있어. 포맷/키 확인 필요");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FishingMiniGame] Fish.json 파싱 실패: {e.Message}");
        }
    }

    private void SelectRandomFishFromJson()
    {
        if (_fishList.Count == 0)
        {
            CurrentFish = null;
            MyLogger.LogWarning("[FishingMiniGame] 물고기 목록이 비어있어서 선택 불가");
            return;
        }

        // SpokeWeight 가중치 랜덤
        int total = 0;
        for (int i = 0; i < _fishList.Count; i++)
            total += Mathf.Max(1, _fishList[i].SpokeWeight);

        int roll = UnityEngine.Random.Range(0, total);
        int acc = 0;

        CurrentFish = _fishList[0]; // fallback
        for (int i = 0; i < _fishList.Count; i++)
        {
            acc += Mathf.Max(1, _fishList[i].SpokeWeight);
            if (roll < acc)
            {
                CurrentFish = _fishList[i];
                break;
            }
        }

        MyLogger.Log($"[FishingMiniGame] 이번 낚시 대상: {CurrentFish?.fishName} ({CurrentFish?.fishCode})");

        // fishCode -> SpriteAtlas -> Fish[Image] 교체
        ApplyCurrentFishSprite();
    }

    // -----------------------
    // Atlas Sprite Apply
    // -----------------------
    private void ApplyCurrentFishSprite()
    {
        if (CurrentFish == null) return;

        // 1) Image 캐시
        if (_fishImage == null && fish != null)
            _fishImage = fish.GetComponent<Image>() ?? fish.GetComponentInChildren<Image>(true);

        if (_fishImage == null)
        {
            MyLogger.LogWarning("[FishingMiniGame] Fish 오브젝트에 Image가 없습니다. (fish 또는 자식에 Image 필요)");
            return;
        }

        // 2) Atlas 캐시
        if (_atlas == null)
            _atlas = Resources.Load<SpriteAtlas>(fishAtlasPath);

        if (_atlas == null)
        {
            MyLogger.LogWarning($"[FishingMiniGame] SpriteAtlas 로드 실패: Resources/{fishAtlasPath}");
            return;
        }

        // 3) fishCode로 스프라이트 교체
        if (string.IsNullOrEmpty(CurrentFish.fishCode))
        {
            MyLogger.LogWarning("[FishingMiniGame] CurrentFish.fishCode가 비어있어 스프라이트 교체 불가");
            return;
        }

        var sprite = _atlas.GetSprite(CurrentFish.fishCode);
        if (sprite == null)
        {
            MyLogger.LogWarning($"[FishingMiniGame] Atlas에 스프라이트 없음: fishCode='{CurrentFish.fishCode}' (스프라이트 이름과 완전 일치 필요)");
            return;
        }

        _fishImage.sprite = sprite;
        // _fishImage.SetNativeSize(); // 필요하면만
    }

    // -----------------------
    // Core Updates
    // -----------------------
    void UpdateFish(float dt)
    {
        if (CurrentFish == null) return;

        const float speedMul = 2f;     // 이동 2배
        const float changeMul = 0.7f;  // 방향 변경 30% 더 자주

        _fishTimer -= dt;
        if (_fishTimer <= 0f)
        {
            _fishTimer = UnityEngine.Random.value * (fishTimeRandomizer * changeMul);

            float target = UnityEngine.Random.value;

            // Fish.json SpokeWeight 기준(18: 매우 흔함, 3: 희귀)
            float rarity01 = Mathf.InverseLerp(18f, 3f, CurrentFish.SpokeWeight);

            // 희귀할수록 스파이크(급변) 확률 증가
            float spikeChance = Mathf.Lerp(0.10f, 0.45f, rarity01);

            if (UnityEngine.Random.value < spikeChance)
            {
                // 스타듀 핵심 1줄: 현재 위치에서 위/아래로 급점프
                target = Mathf.Clamp01(_fishPosition + (UnityEngine.Random.value < 0.5f ? -1f : 1f) * 0.55f);
            }

            _fishTargetPosition = target;
        }

        float smoothTime = Mathf.Max(0.05f, smoothMotion / speedMul);

        _fishPosition = Mathf.SmoothDamp(
            _fishPosition,
            _fishTargetPosition,
            ref _fishSpeed,
            smoothTime
        );

        if (fish != null)
            SetPositionY(fish, _fishPosition);
    }

    void UpdateHook(float dt)
    {
        if (IsPullInputActive())
            _hookPullVelocity += hookSpeed * dt;

        _hookPullVelocity -= hookGravity * dt;
        _hookPosition += _hookPullVelocity;

        float halfSize = hookSize * Half;
        if (_hookPosition - halfSize <= 0f && _hookPullVelocity < 0f) _hookPullVelocity = 0f;
        if (_hookPosition + halfSize >= 1f && _hookPullVelocity > 0f) _hookPullVelocity = 0f;

        _hookPosition = Mathf.Clamp(_hookPosition, halfSize, 1f - halfSize);

        if (hook != null)
            SetPositionY(hook, _hookPosition);
    }

    void UpdateProgress(float dt)
    {
        if (progressBarFill != null)
            progressBarFill.fillAmount = _catchProgress;
        else if (progressBarContainer != null)
        {
            var s = progressBarContainer.localScale;
            progressBarContainer.localScale = new Vector3(s.x, _catchProgress, s.z);
        }

        float halfSize = hookSize * Half;
        float min = _hookPosition - halfSize;
        float max = _hookPosition + halfSize;
        bool inZone = _fishPosition > min && _fishPosition < max;

        if (inZone)
        {
            _catchProgress += hookPower * dt;
            if (_catchProgress >= 1f)
            {
                _paused = true;
                Win();
                return;
            }
        }
        else
        {
            _catchProgress -= progressBarDecay * dt;
            _failTimer -= dt;

            if (_failTimer <= 0f)
            {
                _paused = true;
                Lose();
                return;
            }
        }

        _catchProgress = Mathf.Clamp01(_catchProgress);
    }

    void Win()
    {
        if (_ended) return;

        MyLogger.Log($"[FishingMiniGame] 잡았다! Fish Name = {CurrentFish?.fishName}");
        
        // fish register (등록 책임은 미니게임 단독)
        if (CurrentFish != null && !string.IsNullOrEmpty(CurrentFish.fishCode))
            UIFishDex.RegisterFish(CurrentFish.fishCode);

        
        OnFishCaught?.Invoke(CurrentFish);
        winEvent.Invoke();

        End(FishingResult.Success);
    }

    void Lose()
    {
        if (_ended) return;

        loseEvent.Invoke();
        End(FishingResult.Fail);
    }

    // -----------------------
    // Helpers
    // -----------------------
    void ResetState()
    {
        _ended = false;
        _catchProgress = ProgressInitial;
        _paused = false;

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = ProgressInitial;
            progressBarFill.type = Image.Type.Filled;
            progressBarFill.fillMethod = Image.FillMethod.Vertical;
            progressBarFill.fillOrigin = (int)Image.OriginVertical.Bottom;
        }

        if (progressBarContainer != null && progressBarFill == null)
            progressBarContainer.localScale = new Vector3(1f, ProgressInitial, 1f);

        _fishPosition = UnityEngine.Random.value;
        _fishTargetPosition = UnityEngine.Random.value;
        _fishTimer = UnityEngine.Random.value * fishTimeRandomizer;

        _hookPosition = 0f;
        _hookPullVelocity = 0f;

        _failTimer = failTimerMax;
    }

    void CacheBounds()
    {
        // Water가 연결되어 있으면 이동 범위를 Water 이미지의 위/아래 끝으로 맞춤
        if (waterRect != null)
        {
            float centerY = waterRect.anchoredPosition.y;
            _topY = centerY + waterRect.rect.height * (1f - waterRect.pivot.y);
            _bottomY = centerY - waterRect.rect.height * waterRect.pivot.y;
            _useUgui = true;
            return;
        }

        var topRT = topBounds as RectTransform;
        var bottomRT = bottomBounds as RectTransform;
        _useUgui = topRT != null && bottomRT != null;

        if (_useUgui)
        {
            _topY = topRT.anchoredPosition.y;
            _bottomY = bottomRT.anchoredPosition.y;
        }
        else if (topBounds != null && bottomBounds != null)
        {
            _topY = topBounds.position.y;
            _bottomY = bottomBounds.position.y;
        }
    }

    // Hook 이미지 높이를 이동범위로 나눠 hookSize를 동적으로 맞춤 (시각=판정 일치)
    void ApplyAutoHookSize()
    {
        if (!(hook is RectTransform hookRT)) return;

        float range = _topY - _bottomY;
        if (Mathf.Approximately(range, 0f)) return;

        // 보이는 훅 높이에 여유 픽셀을 더해 판정영역을 약간 후하게
        float effectiveHeight = hookRT.rect.height + hookCatchPaddingPx;
        float ratio = Mathf.Abs(effectiveHeight / range);
        hookSize = Mathf.Clamp(ratio, 0.01f, 1f);
    }

    void SetPositionY(Transform t, float normalizedPosition)
    {
        float y = Mathf.Lerp(_bottomY, _topY, normalizedPosition);

        if (t is RectTransform rt)
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
        else if (bottomBounds != null && topBounds != null)
            t.position = Vector3.Lerp(bottomBounds.position, topBounds.position, normalizedPosition);
    }

    bool IsPullInputActive()
    {
        if (pullButton != null)
            return _isPullHeld;

        return Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Space);
    }

    void SetupPullButton()
    {
        _isPullHeld = false;
        if (pullButton == null) return;

        GameObject buttonObject = pullButton.gameObject;
        EventTrigger eventTrigger = buttonObject.GetComponent<EventTrigger>() ?? buttonObject.AddComponent<EventTrigger>();
        eventTrigger.triggers.Clear();

        EventTrigger.Entry onPointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        onPointerDown.callback.AddListener(_ => _isPullHeld = true);
        eventTrigger.triggers.Add(onPointerDown);

        EventTrigger.Entry onPointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        onPointerUp.callback.AddListener(_ => _isPullHeld = false);
        eventTrigger.triggers.Add(onPointerUp);

        EventTrigger.Entry onPointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        onPointerExit.callback.AddListener(_ => _isPullHeld = false);
        eventTrigger.triggers.Add(onPointerExit);
    }
    
    private void UpdateRemainSecUI()
    {
        if (remainSecText == null) return;

        // 현재 로직 기준: 실패 타이머는 "존 밖에 있을 때만" 감소.
        // 보이는 값이 0이면 Fail 직전/직후 상태.
        float sec = Mathf.Max(0f, _failTimer);
        remainSecText.text = string.Format(remainSecFormat, sec);
    }
}