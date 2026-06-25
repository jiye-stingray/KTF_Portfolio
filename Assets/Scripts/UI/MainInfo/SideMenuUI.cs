using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
// using Sirenix.OdinInspector;

public class SideMenuUI : MonoBehaviour
{
    [Header("space between menu items")]
    [SerializeField] Vector2 spacing;
    
    [Space]
    [Header("Main button rotation")]
    [SerializeField] float rotationDuration;
    [SerializeField] Ease rotationEase;

    [Space]
    [Header("Animation")]
    [SerializeField] float expandDuration;
    [SerializeField] float collapseDuration;
    [SerializeField] Ease expandEase;
    [SerializeField] Ease collapseEase;

    [Space]
    [Header("Fading")]
    [SerializeField] float expandFadeDuration;
    [SerializeField] float collapseFadeDuration;

    [SerializeField] private RectTransform mainButtonTransform;
     
    [SerializeField] private Image backgroundImg; 
    [SerializeField] private RectTransform panelRect;

    [SerializeField] private SideMenuItemUI[] menuItems;

    bool isAnimating = false;
    bool isExpanded = false;

    Vector2 mainButtonPosition;
    int itemsCount;
    int activeItemCount;
    

    public void Init()
    {
        itemsCount = panelRect.childCount;
        menuItems = new SideMenuItemUI[itemsCount];
        for (int i = 0; i < itemsCount; i++)
            menuItems[i] = panelRect.GetChild(i).GetComponent<SideMenuItemUI>();

        mainButtonPosition = mainButtonTransform.localPosition;

        ResetPositions();
    }

    public void Refresh()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].Refresh();
        }
        //ResetPositions();
    }

    void ResetPositions()
    {
        isExpanded = true;
        isAnimating = false;
        ToggleMenu(isExpanded);
    }

    public void SideMainBtnClick()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        ToggleMenu(!isExpanded);
    }

    public void ToggleMenu(bool expended)
    {
        //if (isAnimating) return;

        DOTween.Kill(this, complete: false);            // 강제 종료
        isAnimating = true;
        isExpanded = expended;

        activeItemCount = 0;

        if (isExpanded)
        {
            ResetBackGroundSize();
            for (int i = 0; i < itemsCount; i++)
            {

                menuItems[i].gameObject.SetActive(true);
                menuItems[i].rectTrans.DOLocalMove(mainButtonPosition + spacing * (activeItemCount + 1), expandDuration)
                                      .SetEase(expandEase)
                                      .OnUpdate(UpdateBackgroundSize)
                                      .SetId(this);
                menuItems[i].img.DOFade(1f, expandFadeDuration).From(0f).SetId(this);
                activeItemCount++;
            }

            backgroundImg.DOFade(0.7f, expandFadeDuration).From(0f).SetId(this)
                .OnComplete(() => isAnimating = false);
        }
        else
        {
            for (int i = 0; i < itemsCount; i++)
            {
                //if (!menuItems[i].CanOpenButton())
                //    continue;

                int index = i;
                menuItems[index].rectTrans.DOLocalMove(mainButtonPosition, collapseDuration)
                                          .SetEase(collapseEase)
                                          .OnUpdate(UpdateBackgroundSize)
                                          .SetId(this);

                menuItems[index].img.DOFade(0f, collapseFadeDuration).OnComplete(() =>
                {
                    menuItems[index].gameObject.SetActive(false);
                }).SetId(this);
            }

            backgroundImg.DOFade(0f, collapseFadeDuration)
                .OnComplete(() => isAnimating = false)
                .SetId(this);
        }
    }

    void UpdateBackgroundSize()
    {
        // menuItems 의 min/max localPosition 계산해서 Panel 크기 조정
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (var item in menuItems)
        {
            float y = item.rectTrans.localPosition.y;
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);
        }

        // Panel 크기 갱신
        float height = Mathf.Abs(maxY - minY) + 200f; // 여유 padding 200
        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, height);
    }
    void ResetBackGroundSize()
    {
        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, 0);
    }
    
}
