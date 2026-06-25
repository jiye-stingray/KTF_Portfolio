using DG.Tweening;
using UnityEngine;
using TMPro;
using MarchingBytes;

public class UIDamageText : MonoBehaviour
{
    [SerializeField] private bool _isCritical;
    private TextMeshPro _damageText;
    private SpriteRenderer _damageIcon;
    
    private Sequence _seq;
    public void Init(double damage)
    {
        if(_damageText == null)
            _damageText = GetComponent<TextMeshPro>();

        if (_damageIcon == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if(iconTransform !=null)
                _damageIcon = iconTransform.GetComponent<SpriteRenderer>();
        }
            
        
        if (_damageIcon != null)
            _damageIcon.color = Color.white;
        
        _damageText.sortingOrder = Define.SortingLayers.DAMAGE;
        
        _damageText.text = damage.ToString();
        _damageText.alpha = 1;

        DoAnimation();
    }
    
    public void Init(double damage, string color)
    {
        Init(damage);
        _damageText.color = Utils.HexToColor(color);
    }
    
    // private void DoAnimation()
    // {
    //     Sequence seq = DOTween.Sequence();
    //
    //     transform.localScale = new Vector3(0, 0, 0);
    //
    //     seq.Append(transform.DOScale(1.3f, 0.3f).SetEase(Ease.InOutBounce))
    //         .Join(transform.DOMove(transform.position + Vector3.up, 0.3f).SetEase(Ease.Linear))
    //         .Append(transform.DOScale(1.0f, 0.3f).SetEase(Ease.InOutBounce))
    //         .Join(_damageText.DOFade(0, 0.3f).SetEase(Ease.InQuint));
    //     
    //     if (_damageIcon != null)
    //         seq.Join(_damageIcon.DOFade(0, 0.3f).SetEase(Ease.InQuint));
    //
    //     seq.OnComplete(() =>
    //     {
    //         if (EasyObjectPool._instance != null)
    //             EasyObjectPool.instance.ReturnObjectToPool(gameObject);
    //     });
    // }
    
    private void DoAnimation()
    {
        _seq?.Kill();

        transform.localScale = Vector3.zero;

        Vector3 startPos = transform.position;

        float randomX = UnityEngine.Random.Range(-0.25f, 0.25f);
        float randomY = UnityEngine.Random.Range(0.8f, 1.2f);

        Vector3 popPos = startPos + new Vector3(randomX * 0.4f, 0.35f, 0f);
        Vector3 endPos = startPos + new Vector3(randomX, randomY, 0f);

        _damageText.alpha = 1f;

        _seq = DOTween.Sequence();
        
        float popScale = _isCritical ? 1.8f : 1.45f;
        float popTime = _isCritical ? 0.15f : 0.12f;
        _seq.timeScale = _isCritical ? 0.9f : 1f;
        _seq.Append(transform.DOScale(popScale, popTime).SetEase(Ease.OutBack))
            .Join(transform.DOMove(popPos, popTime).SetEase(Ease.OutQuad));

        if (_isCritical)
        {
            // 강한 흔들림
            _seq.Append(transform.DOShakePosition(0.2f, 0.25f, 15, 90f));
            _seq.Join(transform.DOShakeScale(0.2f, 0.5f, 12, 45f));
        }

        // 안정화
        _seq.Append(transform.DOScale(1.0f, 0.12f).SetEase(Ease.InOutSine))

            // 크리티컬은 조금 더 보여줌
            .AppendInterval(_isCritical ? 0.2f : 0.15f)

            // 떠오르며 사라짐
            .Append(transform.DOMove(endPos, 0.45f).SetEase(Ease.OutCubic))
            .Join(transform.DOScale(_isCritical ? 0.95f : 0.85f, 0.45f).SetEase(Ease.InSine))
            .Join(_damageText.DOFade(0f, 0.35f).SetEase(Ease.InQuad));
        
        if (_damageIcon != null)
            _seq.Join(_damageIcon.DOFade(0f, 0.35f).SetEase(Ease.InQuad));

        _seq.OnComplete(() =>
        {
            _seq = null;

            if (EasyObjectPool._instance != null)
                EasyObjectPool.instance.ReturnObjectToPool(gameObject);
        });
    }
}
