using DG.Tweening;
using UnityEngine;
using static Define;
using Cysharp.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIAttendanceEventPopup : UIPopupBase
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup; // ui

    [Header("Reward Buttons")]
    [Tooltip("Inspector에서 자식 객체인 BtnArea를 연결해 주시거나 비워두면 자동으로 찾습니다.")]
    [SerializeField] private Transform _btnArea; 
    [SerializeField] AttendanceEventRewardButton[] _attendanceRewardBtns;
    
    EAttendanceEventType _attendanceEventType;

    private UniTaskCompletionSource _closeTcs;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        // 초기 상태
        gameObject.transform.localScale = Vector3.one * 0.94f;
        canvasGroup.alpha = 0f;

        DOTween.Sequence()
            .Join(canvasGroup.DOFade(1f, 0.20f).SetEase(Ease.OutCubic))
            .Append(gameObject.transform.DOScale(1.02f, 0.18f).SetEase(Ease.OutCubic))
            .Append(gameObject.transform.DOScale(1f, 0.10f).SetEase(Ease.InOutSine));

        return true;
    }

    public void InitType(EAttendanceEventType attendanceEventType)
    {
        _attendanceEventType = attendanceEventType;
        Refresh();
    }

    public override void Refresh()
    {
        for (int i = 1; i <= ReturnAttendanceCount(_attendanceEventType); i++)
        {
            _attendanceRewardBtns[i - 1].SetData(UserInfoData.GetAttendanceData(_attendanceEventType, i),_attendanceEventType);
        }
    }

    public override void ClickCloseBtn()
    {
        DOTween.Kill(gameObject.transform);
        DOTween.Kill(canvasGroup);

        DOTween.Sequence()
            .Join(gameObject.transform.DOScale(0.98f, 0.12f).SetEase(Ease.InCubic)) // 살짝 줄였다가
            .Join(canvasGroup.DOFade(0f, 0.16f).SetEase(Ease.InCubic))
            .OnComplete(() =>
            {
                _closeTcs?.TrySetResult();
                base.ClickCloseBtn();
            });
    }

#if UNITY_EDITOR
    // 인스펙터의 컴포넌트 우클릭 메뉴 또는 점 3개 메뉴에서 실행할 수 있습니다.
    [ContextMenu("출석 버튼 자동 할당 (1~28)")]
    private void AutoAssignRewardButtons()
    {
        // 1. _btnArea가 비어있다면 이름으로 찾기 시도
        if (_btnArea == null)
        {
            // BtnArea가 직속 자식이 아니라면 GetComponentsInChildren으로 찾습니다.
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);
            foreach (var child in allChildren)
            {
                if (child.name == "BtnArea")
                {
                    _btnArea = child;
                    break;
                }
            }

            if (_btnArea == null)
            {
                Debug.LogWarning("[UIAttendanceEventPopup] 'BtnArea'를 찾을 수 없습니다. Inspector에 직접 연결해주세요.");
                return;
            }
        }

        // 2. BtnArea 하위의 모든 AttendanceEventRewardButton 컴포넌트를 배열에 할당
        // (true 옵션을 주어 비활성화된 오브젝트도 찾도록 함)
        _attendanceRewardBtns = _btnArea.GetComponentsInChildren<AttendanceEventRewardButton>(true);

        // 3. 변경된 배열을 Inspector와 씬(Scene)에 강제 저장
        EditorUtility.SetDirty(this);
        
        Debug.Log($"[UIAttendanceEventPopup] {_attendanceRewardBtns.Length}개의 출석 버튼이 자동으로 할당되었습니다!");
    }
#endif

    public UniTask WaitUntilClosedAsync()
    {
        _closeTcs ??= new UniTaskCompletionSource();
        return _closeTcs.Task;
    }
}