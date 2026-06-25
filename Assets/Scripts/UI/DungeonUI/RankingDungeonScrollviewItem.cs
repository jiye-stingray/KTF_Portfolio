using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class RankingDungeonScrollviewItem : ICell
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] TMP_Text _scoreTxt;
    [SerializeField] TMP_Text _rankingTxt;

    [SerializeField] Image _thumbnail;
    [SerializeField] Image _frameImage;
    [SerializeField] Image _rankingIcon;

    RankingItemData _data = null;

    public override void SetData(ItemData data, int index)
    {
        _index = index;
        _data = data as RankingItemData;
        Refresh();
    }

    private void Refresh()
    {
        DrawBg(_data.ranking);
        _nameTxt.text = _data.name;
        _scoreTxt.text = _data.score.ToString();
        if (_data.ranking == 0)
            _rankingTxt.text = "-";
        else
            _rankingTxt.text = $"{_data.ranking} 위";

        _thumbnail.sprite =
            Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.CharacterIconAtlas, $"Thum_SD_Cr_{_data.thumbnail.ToString("000")}");
        if (_frameImage)
        {
            var frameSprite = _data.frame <= 0 ? null
                : Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.FrameAtlas, $"FrameImg_{_data.frame.ToString("000")}");
            _frameImage.sprite = frameSprite;
            _frameImage.gameObject.SetActive(frameSprite != null);
        }

    }

    private void DrawBg(int ranking)
    {
        _rankingIcon.gameObject.SetActive(true);
        switch (ranking)
        {
            case 1:
                _rankingIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.PictogramAtlas,"Mapae_1");
                break;
            case 2:
                _rankingIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.PictogramAtlas,"Mapae_2");
                break;
            case 3:
                _rankingIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.PictogramAtlas,"Mapae_3");
                break;
            default:
                _rankingIcon.gameObject.SetActive(false);
                break;
        }
    }
}
