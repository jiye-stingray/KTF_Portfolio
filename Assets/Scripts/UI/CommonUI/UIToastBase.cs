using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIToastBase : UIBase
{
    [SerializeField] protected Image _bgImg;
    [SerializeField] protected TMP_Text _txt;

    protected Vector3 startPosition = new Vector3(0, -50, 0);
    protected Vector3 endPosition = new Vector3(0,0,0);


    RectTransform rectTransform;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        rectTransform = GetComponent<RectTransform>();  
        rectTransform.anchoredPosition = startPosition;
        
        ShowToast();
        return true;
    }

    public virtual void SetText(string txt)
    {
        if(_txt == null ) return;
        _txt.text = txt;

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public virtual void SetText(string text, string text2)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }


    private void ShowToast()
    {
        Sequence toastSequence = DOTween.Sequence();
        toastSequence.Append(rectTransform.DOAnchorPos(endPosition, 1).SetEase(Ease.OutQuad))
            .Append(_bgImg.DOFade(0, 1))
            .Join(_txt.DOFade(0, 1));
        JoinExtraFades(toastSequence);
        toastSequence.OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }

    protected virtual void JoinExtraFades(Sequence seq) { }
}
