using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIResurrectionBar : MonoBehaviour
{    
    [SerializeField] private Image _timeImg;

    public void Init()
    {
        GetComponent<Canvas>().sortingOrder = Define.SortingLayers.UNIT + 1;
        UpdateUI(0);

        // transform.localPosition = Vector3.up * (_srcUnit.GetSpineHighest() * 1.2f);
        // 원거리 캐릭터 높이값이 다르게 책정되어서 임시로 1.5로 맞춤
        transform.localPosition = Vector3.up * 1.5f;
    }

    public void UpdateUI(float value)
    {
       _timeImg.fillAmount = value;
    }
}
