using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

// public class EventGroup : MonoBehaviour
// {
//     public HorizontalScrollSnap scrollSnap;
//     public List<EventNaviButton> naviButtons;
//
//     private void Awake()
//     {
//         scrollSnap.OnSelectionPageChangedEvent.AddListener(ChangePage);
//     }
//     
//     private void ChangePage(int index)
//     {
//         for (var i = 0; i < naviButtons.Count; i++)
//         {
//             naviButtons[i].OnActive(index == i);
//         }
//     }
// }

using System.Collections;

public class EventGroup : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private HorizontalScrollSnap scrollSnap;
    [SerializeField] private List<EventNaviButton> naviButtons = new();

    [Header("Auto Scroll")]
    [SerializeField] private bool useAutoScroll = true;
    [SerializeField] private float autoScrollInterval = 3f;
    [SerializeField] private bool loop = true;

    private Coroutine autoScrollCoroutine;

    private void Awake()
    {
        if (scrollSnap != null)
            scrollSnap.OnSelectionPageChangedEvent.AddListener(ChangePage);
    }

    private void OnEnable()
    {
        RefreshPageState();
        StartAutoScroll();
    }

    private void OnDisable()
    {
        StopAutoScroll();
    }

    private void OnDestroy()
    {
        if (scrollSnap != null)
            scrollSnap.OnSelectionPageChangedEvent.RemoveListener(ChangePage);
    }

    private void ChangePage(int index)
    {
        UpdateNaviButtons(index);
        RestartAutoScroll();
    }

    private void RefreshPageState()
    {
        if (scrollSnap == null)
            return;

        UpdateNaviButtons(scrollSnap.CurrentPage);
    }

    private void UpdateNaviButtons(int currentIndex)
    {
        for (int i = 0; i < naviButtons.Count; i++)
        {
            if (naviButtons[i] == null)
                continue;

            naviButtons[i].OnActive(i == currentIndex);
        }
    }

    private void StartAutoScroll()
    {
        if (!CanAutoScroll())
            return;

        StopAutoScroll();
        autoScrollCoroutine = StartCoroutine(CoAutoScroll());
    }

    private void StopAutoScroll()
    {
        if (autoScrollCoroutine == null)
            return;

        StopCoroutine(autoScrollCoroutine);
        autoScrollCoroutine = null;
    }

    private void RestartAutoScroll()
    {
        if (!gameObject.activeInHierarchy)
            return;

        StartAutoScroll();
    }

    private bool CanAutoScroll()
    {
        if (!useAutoScroll)
            return false;

        if (scrollSnap == null)
            return false;

        if (naviButtons == null || naviButtons.Count <= 1)
            return false;

        return true;
    }

    private IEnumerator CoAutoScroll()
    {
        var wait = new WaitForSeconds(autoScrollInterval);

        while (true)
        {
            yield return wait;

            int currentPage = scrollSnap.CurrentPage;
            int lastPage = naviButtons.Count - 1;

            if (currentPage >= lastPage)
            {
                if (!loop)
                    yield break;

                scrollSnap.GoToScreen(0);
            }
            else
            {
                scrollSnap.GoToScreen(currentPage + 1);
            }
        }
    }
}
