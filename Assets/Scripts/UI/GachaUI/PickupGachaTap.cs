using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class PickupGachaTap : GachaTap
{
    [SerializeField] GameObject _characterIllustration;

    private const int BaseSize = 1920;
    public void Init(ScheduleDto schedule)
    {
        Open();
        Init(EGachaType.PickUp);
        SetTimer(schedule);
        Refresh();
        SetIllustration().Forget();
    }


    private void SetTimer(ScheduleDto schedule)
    {
        DateTime endTime = DateTime.Parse(schedule.endTime);
        TimeSpan remaining = endTime - ServerTime.Instance.CurrentTime();

        TimeData timeData = new TimeData();
        timeData.SetByDuration(remaining.TotalSeconds);
        _timer.Set(timeData);

        _timer.SetFinishString("종료 되었습니다.");
    }

    public override void Close()
    {
        _timer.StopTimer();
        base.Close();
    }

    private async UniTask SetIllustration()
    {
        if(_characterIllustration != null)
            await UniTask.Yield();

        _characterIllustration = await Managers.Instance.GetResObjectManager().InstantiateAsync("Illust_Gacha_PickUp", this.transform);
        float ratio = GetComponent<RectTransform>().rect.height / BaseSize;
        _characterIllustration.transform.localScale = new Vector3(ratio, ratio, 1);
    }

    public override void Refresh()
    {
        _descriptionText.text = $"{_gachaGroupData.CeilingCount}회 모집 시 {_pickupCharacter.Name} 확정";
        base.Refresh();
    }
}
