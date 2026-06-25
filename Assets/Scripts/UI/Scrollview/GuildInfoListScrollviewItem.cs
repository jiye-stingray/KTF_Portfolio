using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Define;
using static UnityEngine.Rendering.DebugUI;
using static Utils;


public class GuildInfoListScrollviewItem : ICell
{
    [SerializeField] TMP_Text _guildNameTxt;
    [SerializeField] TMP_Text _guildLevelTxt;
    [SerializeField] TMP_Text _guildMemberTxt;
    [SerializeField] TMP_Text _guildLevelOnMarkTxt;
    [SerializeField] TMP_Text _guildApprovalTxt;
    [SerializeField] Image _guildMark;

    private GuildInfoItemData _data;
    AtlasManager _atlasManager => Managers.Instance.GetAtlasManager();

    UnityAction<GuildInfoListScrollviewItem> _clickAction = null;
    /*
     * *
     */

    public void SetData(ItemData data, int index, UnityAction<GuildInfoListScrollviewItem> clickAction)
    {
        base.SetData(data, index);
        _data = data as GuildInfoItemData;
        _index = index;
        _clickAction = clickAction;
        Refresh();
    }

    private void Refresh()
    {
        if (_data == null)
        {
            Debug.LogError("data null!!");
            return;
        }
        Draw();
    }

    private void Draw()
    {
        _guildNameTxt.text = $"{_data.name} ";
        _guildMemberTxt.text = $"({_data.memberCount}/{_data.maxMemberCount})";
        _guildLevelTxt.text = $"Lv.{_data.level}";
        _guildLevelOnMarkTxt.text = _data.minLevel == 0 ? "조건 없음" : $"Lv.{_data.minLevel}이상";

        StringMaker.Clear();
        switch ((EGuildApprovalType)_data.approvalType)
        {
            case EGuildApprovalType.NONE:
                StringMaker.stringBuilder.Append("자유 가입");
                break;
            case EGuildApprovalType.APPROVAL:
                StringMaker.stringBuilder.Append("승인 가입");
                break;
            case EGuildApprovalType.UNABLE:
                StringMaker.stringBuilder.Append("가입 불가");
                break;
        }
        _guildApprovalTxt.text = StringMaker.stringBuilder.ToString();
        string markSpriteName = $"GuildMark{_data.mark.ToString("00")}";
        _guildMark.sprite = _atlasManager.GetSprite(EAtlasType.GuildAtlas, markSpriteName);

    }

    public void Click()
    {
        _clickAction?.Invoke(this);
    }
}
