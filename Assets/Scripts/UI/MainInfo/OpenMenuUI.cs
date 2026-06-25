using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenMenu : MonoBehaviour
{
    public GameObject uiPanel; // UI 요소들을 포함하는 패널
    public RectTransform uiElement; // 이동시킬 UI 요소
    
    public Button openButton; // 메뉴 열기 버튼
    public Button closeButton; // 메뉴 닫기 버튼
    
    public Vector2 startPosition; // 시작 위치
    public Vector2 targetPosition; // 목표 위치
    
    public float moveDuration = 1.0f; // 이동 시간

    private bool isMoving = false; // 이동 중인지 여부

    void Start()
    {
        // 패널 비활성화 (처음에는 숨김)
        if (uiPanel != null)
            uiPanel.SetActive(false);

        // UI 요소를 시작 위치로 설정
        if (uiElement != null)
            uiElement.anchoredPosition = startPosition;

        // 버튼에 이벤트 연결
        if (openButton != null)
            openButton.onClick.AddListener(OpenAndMoveUI);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseAndMoveUI);
    }

    public void OpenAndMoveUI()
    {
        if (uiPanel != null && !isMoving)
        {
            openButton.enabled = false;
            openButton.GetComponent<Image>().enabled = false;
            // 패널 활성화
            uiPanel.SetActive(true);

            // UI 요소를 목표 위치로 이동
            MoveUI(targetPosition, () => { isMoving = false; });
        }
    }

    public void CloseAndMoveUI()
    {
        if (uiPanel != null && !isMoving)
        {
            // UI 요소를 시작 위치로 이동
            MoveUI(startPosition, () =>
            {
                // 이동 완료 후 패널 비활성화
                openButton.enabled = true;
                openButton.GetComponent<Image>().enabled = true;
                
                uiPanel.SetActive(false);
                isMoving = false;
            });
        }
    }

    private void MoveUI(Vector2 targetPos, TweenCallback onComplete)
    {
        isMoving = true;

        // DOTween을 사용하여 UI 요소의 위치 이동
        uiElement.DOAnchorPos(targetPos, moveDuration)
            .SetEase(Ease.OutQuad) // 부드러운 이동
            .OnComplete(onComplete); // 이동 완료 시 콜백 호출
    }
}
