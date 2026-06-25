using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ScrollPaperRevealUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform rootRect;      // GachaScrollRevealRoot
    [SerializeField] private RectTransform maskRect;      // PaperMask (RectMask2D)
    [SerializeField] private RectTransform handleRect;    // Handle (this)

    [Header("Reveal Range")]
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private float maxHeight = 1200f;     // PaperImage 높이와 맞추는 걸 권장

    [Header("Tuning")]
    [SerializeField] private float pullSensitivity = 1.0f;
    [SerializeField] private float snapSpeed = 14f;       // 손 떼었을 때 Lerp 속도
    [SerializeField] private float completeThreshold = 0.97f;

    [Header("Behavior")]
    [SerializeField] private bool rewindIfNotComplete = false; // 끝까지 못 당기면 되감기

    [Header("Tween")]
    [SerializeField] private float completeTweenDuration = 0.35f;
    [SerializeField] private float rewindTweenDuration = 0.25f;
    [SerializeField] private Ease completeEase = Ease.OutCubic;
    [SerializeField] private Ease rewindEase = Ease.InOutCubic;
    private Tween _heightTween;

    [Header("Events")]
    public UnityEvent onCompleted; // 100% 도달 시 호출

    private bool _dragging;
    private bool _completed;
    private float _targetHeight;


    [Header("Debug")]
    [Range(156f, 1477f)]
    [SerializeField] float _debugH;

    private void Awake()
    {
        if (handleRect == null) handleRect = (RectTransform)transform;
    }

    private void Start()
    {
        _completed = false;
        _dragging = false;

        SetHeight(minHeight);
        _targetHeight = minHeight;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_completed) return;
        _dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_completed) return;

        // 세로 드래그만 반영 (위로 드래그하면 +)
        float delta = eventData.delta.y * pullSensitivity;

        _targetHeight = Mathf.Clamp(_targetHeight - delta, minHeight, maxHeight);
        SetHeight(_targetHeight);

        // 진행도 기반 FX/사운드 넣고 싶으면 여기서 progress 사용
        // float p = GetProgress();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_completed) return;

        _dragging = false;

        float p = GetProgress();

        // 50% 기준 분기
        if (p >= 0.3f)
        {
            TweenToHeight(maxHeight, completeTweenDuration, completeEase, onComplete: () =>
            {
                // 완료 처리
                _completed = true;
                SetHeight(maxHeight);
            });
            return;
        }
        else
        {
            TweenToHeight(minHeight, rewindTweenDuration, rewindEase, onComplete: () =>
            {
                _targetHeight = minHeight;
                SetHeight(minHeight);
            });
            return;

        }
    }

    private void TweenToHeight(float to, float duration, Ease ease, System.Action onComplete = null)
    {
        KillTween();

        float from = maskRect.sizeDelta.y;

        // DOTween으로 "높이 값"을 트윈하고, 매 프레임 SetHeight로 반영
        _heightTween = DOTween.To(() => from, h =>
        {
            from = h;
            SetHeight(h);
        }, to, duration)
        .SetEase(ease)
        .OnComplete(() =>
        {
            _heightTween = null;
            onComplete?.Invoke();
        });
    }

    private void KillTween()
    {
        if (_heightTween != null && _heightTween.IsActive())
        {
            _heightTween.Kill();
            _heightTween = null;
        }
    }

    private void Update()
    {
        if (_completed) return;

        if (_dragging) return;

        float current = maskRect.sizeDelta.y;
        float progress = GetProgress();

        // 스냅 목적지 결정
        float desired;
        if (progress >= completeThreshold)
        {
            desired = maxHeight;
        }
        else
        {
            desired = rewindIfNotComplete ? minHeight : _targetHeight;
        }

        float next = Mathf.Lerp(current, desired, Time.deltaTime * snapSpeed);
        SetHeight(next);

        // 완료 처리
        if (GetProgress() >= 0.999f)
        {
            _completed = true;
            SetHeight(maxHeight);
            onCompleted?.Invoke();
        }
    }

    private void SetHeight(float h)
    {
        // PaperMask 높이 조절
        var size = maskRect.sizeDelta;
        size.y = h;
        maskRect.sizeDelta = size;

        // Handle을 마스크 끝에 붙이기 (Mask Pivot이 아래(0)라는 가정)
        var pos = handleRect.anchoredPosition;
        pos.y = -h;
        handleRect.anchoredPosition = pos;
    }

    public float GetProgress()
    {
        return Mathf.InverseLerp(minHeight, maxHeight, maskRect.sizeDelta.y);
    }

    public void ResetReveal()
    {
        _completed = false;
        _dragging = false;
        _targetHeight = minHeight;
        SetHeight(minHeight);
    }

    public void ForceComplete()
    {
        _completed = true;
        SetHeight(maxHeight);
        onCompleted?.Invoke();
    }

    #region Debug

    public void OnValidate()
    {
        ApplyDebug();
    }
    public void ApplyDebug()
    {
        SetHeight(_debugH);
    }
    #endregion

}