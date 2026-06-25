using UnityEngine;
using UnityEngine.UI;

public static class UILineUtil
{
    /// <summary>
    /// 같은 부모 RectTransform 기준의 두 점 a-b를 잇도록
    /// 주어진 RectTransform의 x, y, width, Rotation.z 만 갱신한다.
    /// - height(sizeDelta.y), pivot, 기타 Image 설정값은 건드리지 않음.
    /// - 현재 pivot.x를 따라 배치 위치를 자동 결정 (0=a, 0.5=중점, 1=b).
    /// sizeH 인자로 값 받기
    /// </summary>
    public static void Apply(RectTransform rt, Vector2 a, Vector2 b, int sizeH)
    {
        if (rt == null) return;

        Vector2 dir = b - a;
        float length = dir.magnitude;

        // 길이 0인 경우: width만 0으로 만들고 회전은 유지
        if (length < Mathf.Epsilon)
        {
            var sz = rt.sizeDelta;
            rt.sizeDelta = new Vector2(0f, sz.y); // width만 수정
            return;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // width만 변경 (height 유지)
        var size = rt.sizeDelta;
        size.x = length;
        size.y = sizeH;
        rt.sizeDelta = size;

        // 회전 z만 변경
        var euler = rt.localEulerAngles;
        euler.z = angle;
        rt.localEulerAngles = euler;

        // 현재 pivot.x에 따라 배치점 결정: 0=a, 0.5=(a+b)/2, 1=b
        float t = rt.pivot.x;                  // 요구: pivot은 변경하지 않음(읽기만)
        Vector2 pos = Vector2.Lerp(a, b, t);   // 선분 상의 보간 위치
        rt.anchoredPosition = pos;             // x,y만 갱신
    }
}
