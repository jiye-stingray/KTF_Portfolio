// using DG.Tweening;

using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderSnapBackTween : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Slider slider;
    [SerializeField] private float snapBackDuration = 0.25f;
    [SerializeField] private float unlockThreshold = 0.95f;
    [SerializeField] private bool returnToMin = true;

    private float _initialValue;
    private bool _dragging;
    private Tween _tween;

    private void Awake()
    {
        if (!slider) slider = GetComponent<Slider>();
        _initialValue = slider.value;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _dragging = true;
        _tween?.Kill();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;

        if (slider.value >= unlockThreshold) return;

        float target = returnToMin ? slider.minValue : _initialValue;
        _tween?.Kill();
        _tween = DOTween.To(() => slider.value, v => slider.value = v, target, snapBackDuration)
                        .SetEase(Ease.OutCubic);
    }
}
