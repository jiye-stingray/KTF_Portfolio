using DG.Tweening;
using MarchingBytes;
using TMPro;
using UnityEngine;

public class AddDungeonCountTimeText : MonoBehaviour
{
    TextMeshPro _count;
    

    private Sequence _seq;

    private const float MoveUpDistance = 1f; // 위로 올라가는 거리 (월드 단위)
    private const float StartScale = 0.6f;
    private const float EndScale = 1f;
    private const float FadeInTime = 0.12f;  // 투명 -> 또렷
    private const float HoldTime = 0.15f;    // 또렷하게 유지
    private const float FadeOutTime = 0.25f; // 또렷 -> 투명

    public void Init(double timeCount)
    {
        if (_count == null)
            _count = GetComponent<TextMeshPro>();
        _count.sortingOrder = Define.SortingLayers.DAMAGE;

        _count.text = $"+ {timeCount:0.0}s"; // 소수점 첫째자리까지 표시
        
        DoAnimation();
    }

    private void DoAnimation()
    {
        _seq?.Kill();

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * MoveUpDistance;

        // 초기 상태: 투명 + 살짝 작게
        _count.alpha = 0f;
        transform.localScale = Vector3.one * StartScale;
        transform.position = startPos;

        float totalTime = FadeInTime + HoldTime + FadeOutTime;

        _seq = DOTween.Sequence();

        // 위로 뿅 올라감 (전체 시간 동안 이동)
        _seq.Append(transform.DOMove(endPos, totalTime).SetEase(Ease.OutCubic));

        // 뿅 하고 커지면서 또렷해짐
        _seq.Join(transform.DOScale(EndScale, FadeInTime).SetEase(Ease.OutBack));
        _seq.Join(_count.DOFade(1f, FadeInTime).SetEase(Ease.OutQuad));

        // 유지 후 투명해지면서 사라짐
        _seq.Insert(FadeInTime + HoldTime, _count.DOFade(0f, FadeOutTime).SetEase(Ease.InQuad));

        _seq.OnComplete(() =>
        {
            _seq = null;

            // 마지막엔 pool 반환 (비활성화)
            if (EasyObjectPool._instance != null)
                EasyObjectPool.instance.ReturnObjectToPool(gameObject);
        });
    }

    private void OnDisable()
    {
        _seq?.Kill();
        _seq = null;
    }
}
