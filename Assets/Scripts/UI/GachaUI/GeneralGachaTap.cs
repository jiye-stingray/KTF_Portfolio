using static Define;

public class GeneralGachaTap : GachaTap
{
    public override void Open()
    {
        base.Open();
        Init(EGachaType.General);
        Refresh();
        StartImageRotation();
    }

    public override void Close()
    {
        StopImageRotation();
        base.Close();
    }

    public override void Refresh()
    {
        _descriptionText.text = $"{_gachaGroupData.CeilingCount}회 모집 시 전설 등급 확정";
        base.Refresh();
    }
}
