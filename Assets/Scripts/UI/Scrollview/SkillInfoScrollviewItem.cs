using PolyAndCode.UI;
using TMPro;
using UnityEngine;

public class SkillInfoScrollviewItem : ICell
{
    [SerializeField] GameObject _grayRoot;

    [SerializeField] TMP_Text[] _skillLevel;
    [SerializeField] TMP_Text _description;

    public SkillInfoItemData _data;
    
    public override void SetData(ItemData data, int index)
    {
        _data = data as SkillInfoItemData;
        _index = index;
        Refresh();
    }

    private void Refresh()
    {
        if (_data == null)
        {
            Debug.LogError("data null!!");
            return;
        }

        foreach (var tmpText in _skillLevel)
        {
            tmpText.text = $"Lv.{_data._skillLevel}";
        }

        _description.text = string.Join(", ", _data._args.ConvertAll(GetDescription));
        _grayRoot.SetActive(!_data.IsOpen);
    }
    
    private string GetDescription(SkillDescriptionArg arg)
    {
        string statusText = Status.ReturnStatusString(arg.Status);
        string powerText = $"{CalculateStatus.ToPercent(arg.Power)}%";

        switch (arg.SkillType)
        {
            case Define.ESkillType.Damage:
                return $"{statusText}의 {powerText} 피해";

            case Define.ESkillType.Heal:
                return $"{statusText}의 {powerText} 회복";

            case Define.ESkillType.Buff:
                return $"{statusText} {powerText} 증가";

            case Define.ESkillType.DeBuff:
                return $"{statusText} {powerText} 감소";

            default:
                return string.Empty;
        }
    }
}
