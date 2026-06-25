using DG.Tweening;
using UnityEngine;

public class UIRelicCraftPopup : UIPopupBase
{
    [SerializeField] private UIRelicPartDetailItem _relicPartDetailItem;
    [SerializeField] Transform _bg;

    private int _id;
    public void Init(RelicPartsItemData relicPartsItemData)
    {
        _id = relicPartsItemData._id;
        _relicPartDetailItem.SetItem(relicPartsItemData);
        _bg.localScale = Vector3.zero;
        _bg.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnRelicPartsDismiss()
    {
        BestHttp_GameManager.OnPostRelicPartsDismiss(_id);
        ClickCloseBtn();
    }
}
