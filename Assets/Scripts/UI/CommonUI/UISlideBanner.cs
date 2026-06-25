// BannerCarousel_DOTween_DragSnap.cs
// ScrollRect + 중앙 정렬 배너 + 좌우 버튼 + 드래그 후 스냅 + DOTween

using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UISlideBanner : MonoBehaviour
{
    [Header("UI References")]
    public List<RectTransform> items = new List<RectTransform>(); // 움직일 배너 리스트
    public int currentIndex = 0;

    [Header("Animation")]
    public float moveDuration = 0.5f;        // 트윈 이동 시간

    [Header("Inspector Setting")]
    public float spacing = 0; // 배너 사이 간격
    public bool useTween = false;
    
    
    UnityAction moveAction = null;

    public void Init(UnityAction _moveAction, int initIndex)
    {
        moveAction = _moveAction;
        currentIndex = initIndex;
        //Reposition(currentIndex);
    }

    public void MoveLeft()
    {
        currentIndex = Mathf.Max(0, currentIndex - 1);
        MoveToCurrent(useTween);
    }

    public void MoveRight()
    {
        currentIndex = Mathf.Min(items.Count - 1, currentIndex + 1);
        MoveToCurrent(useTween);
    }

    public void Reposition(int oneBased, bool useTween = false)
    {
        currentIndex = oneBased;
        MoveToCurrent(useTween);
    }

    private void MoveToCurrent(bool useTween = true)
    {
        if (items == null || items.Count == 0) return;

        for (int i = 0; i < items.Count; i++)
        {
            RectTransform banner = items[i];

            float xPos = (i - currentIndex) * banner.rect.width + spacing;
            Vector2 targetPos = new Vector2(xPos, banner.anchoredPosition.y);

            if(useTween == true)
                banner.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutCubic);
            else
                banner.anchoredPosition = targetPos;

        }

        moveAction?.Invoke();
    }
}