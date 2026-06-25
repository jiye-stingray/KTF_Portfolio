using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIGachaResultCharacterPopup : UISubBase
{
    [SerializeField] Image _bg;
    [SerializeField] Image _characterImg;
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] Image _gradeBg;
    [SerializeField] Image _factionIcon;
    [SerializeField] TMP_Text _factionTxt;
    [SerializeField] GameObject _newTxt;

    UnitData _unitData;
    bool _isNew;

    bool _isClose;
    public bool IsClose => _isClose;

    public async UniTask SetUnitData(UnitData unitData, bool isNew)
    {
        _unitData = unitData;
        _isNew = isNew;
        await SetCharacterImg();
        _isClose = false;
        OpenToStack();
    }

    public IEnumerator ActionCoroutine()
    {
        _bg.sprite = Managers.Instance.GetResObjectManager().Load<Sprite>($"Texture/FactionBg/{_unitData.Faction}");
        _nameTxt.text = _unitData.Name;
        _newTxt.SetActive(_isNew);
        _factionIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)_unitData.Faction}");
        _gradeBg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.GachaAtlas, $"frame_{_unitData.StartGrade}_3");
        _factionTxt.text = Define.ReturnFactionString(_unitData.Faction);

        while (!_isClose)
        {
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    private async UniTask SetCharacterImg()
    {
        _characterImg.sprite = await Managers.Instance.GetResObjectManager().LoadAsync<Sprite>($"Illustration_{_unitData.Resource}");
    }

    //임시 사용
    public void Action()
    {
        _bg.sprite = Managers.Instance.GetResObjectManager().Load<Sprite>($"Texture/FactionBg/{_unitData.Faction}");
        _nameTxt.text = _unitData.Name;
        _newTxt.SetActive(_isNew);
        _factionIcon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)_unitData.Faction}");
        _gradeBg.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.GachaAtlas, $"frame_{_unitData.StartGrade}_3");
        _factionTxt.text = Define.ReturnFactionString(_unitData.Faction);
    }

    public override void ClickCloseBtn()
    {
        _isClose = true;
        base.ClickCloseBtn();
    }
}
