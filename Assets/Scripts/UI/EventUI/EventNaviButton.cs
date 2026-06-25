using UnityEngine;
using UnityEngine.UI;

public class EventNaviButton : MonoBehaviour
{
    public Image focusImage;

    public void OnActive(bool isActive)
    {
        focusImage.color = isActive ? new Color32(255, 102, 110, 255) : Color.white;
    }
}
