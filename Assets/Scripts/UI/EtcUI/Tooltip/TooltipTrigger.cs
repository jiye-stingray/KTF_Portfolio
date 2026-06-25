using I2.Loc;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("I2 Localization Keys")]
    public string tooltipTextKey;   // 텍스트용 키
    public string tooltipIconKey;   // 스프라이트용 키

    [Header("Tooltip Settings")]
    public bool useTouchPosition = true; // 툴팁이 터치/마우스 위치 기준으로 뜰지, 아니면 오브젝트 기준
    public Vector3 offset;
    public float tooltipDuration = 0f; // 툴팁을 자동으로 일정 시간 후에 사라짐

    /*                       useTouchPosition             offset  tooltipDuration
     * 버튼 위 툴팁	             true	                (0, 30, 0)	    0
       캐릭터 머리 위에 툴팁	         false	                (0, 2, 0)	    5
       아이템 리스트에서 간단 설명	     true	                (0, 20, 0)	    2
     */
    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 tooltipPos = useTouchPosition
            ? (Vector3)eventData.position
            : transform.position + offset;

        StartTooltipHover(tooltipPos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopTooltipHover();
    }

    public void OnSelect(BaseEventData eventData)
    {
        Vector3 tooltipPos = transform.position + offset;
        StartTooltipHover(tooltipPos);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StopTooltipHover();
    }

    private void StartTooltipHover(Vector3 tooltipPos)
    {
        // 텍스트 로컬라이즈
        string localizedText = LocalizationManager.GetTranslation(tooltipTextKey);

        // 아이콘 로컬라이즈 (I2에서 Sprite로 변환)
        //Sprite localizedSprite = LocalizationManager.GetTranslation(tooltipIconKey) as Sprite;
        Sprite localizedSprite = null;
        TooltipWindow.Instance.ShowTooltip(localizedText, tooltipPos, localizedSprite, tooltipDuration);
    }

    private void StopTooltipHover()
    {
        TooltipWindow.Instance.HideTooltip();
    }
}
