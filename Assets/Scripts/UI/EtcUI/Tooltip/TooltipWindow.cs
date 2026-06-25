using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipWindow : MonoBehaviour
{
    public static TooltipWindow Instance;

    [SerializeField] private RectTransform backgroundPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Image tooltipImage;
    [SerializeField] private Vector3 tooltipOffset = new Vector3(15f, -15f, 0f);

    private RectTransform rectTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        HideTooltip();
    }

    public void ShowTooltip(string text, Vector3 screenPos, Sprite image = null, float duration = 0f)
    {
        if (tooltipImage != null && image != null)
        {
            tooltipImage.sprite = image;
            tooltipImage.gameObject.SetActive(true);
        }
        else if (tooltipImage != null)
        {
            tooltipImage.gameObject.SetActive(false);
        }
        
        // tooltipText.text = text;
        // tooltipImage.sprite = image;
        // tooltipImage.gameObject.SetActive(image != null);

        // 좌표 보정
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            screenPos + tooltipOffset,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera,
            out anchoredPos
        );

        rectTransform.anchoredPosition = anchoredPos;
        gameObject.SetActive(true);

        if (duration > 0)
        {
            CancelInvoke(nameof(HideTooltip));
            Invoke(nameof(HideTooltip), duration);
        }
    }

    public void HideTooltip()
    {
        CancelInvoke(nameof(HideTooltip));
        gameObject.SetActive(false);
    }
}