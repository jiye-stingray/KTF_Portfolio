using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITooltip : MonoBehaviour
{

    Canvas _canvas;

    [SerializeField] TMP_Text _name;
    [SerializeField] TMP_Text _desc;
    [SerializeField] RectTransform _bgRectTransform;

    public void Awake()
    {
        Managers.Instance.GetUIManager().UITooltip = this;
        _bgRectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();

        gameObject.SetActive(false);
    }

    public void Init(string name, string desc,Vector3 pos)
    {
        gameObject.SetActive(true);

        _name.text = name;
        _desc.text = desc;

        // 좌표 보정
        Vector2 anchoredPos;
        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        // 월드 좌표 → 스크린 좌표 → Tooltip UI의 부모 기준 로컬 좌표
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _bgRectTransform.parent as RectTransform,     // 기준이 될 부모
            RectTransformUtility.WorldToScreenPoint(cam, pos),
            cam,
            out anchoredPos
        );

        _bgRectTransform.anchoredPosition = anchoredPos;

    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
