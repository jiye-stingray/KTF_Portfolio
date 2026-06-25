using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarpPointScrollviewItem : ICell
{
    [SerializeField] Image _warpPointThumbnail;
    [SerializeField] GameObject _fieldCheck;
    [SerializeField] GameObject _grayImg;
    [SerializeField] TMP_Text _fieldNameTxt;
    [SerializeField] GameObject _descGo;
    [SerializeField] TMP_Text _descTxt;
    public WarpPointItemData _data;
    
    public override void SetData(ItemData data, int index)
    {
        _data = data as WarpPointItemData;
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

        _fieldCheck.SetActive(_data.IsSquadField);
        _grayImg.SetActive(_data.IsLock);
        _fieldNameTxt.text = _data._warpPointInfo.Name;
        _warpPointThumbnail.sprite = Managers.Instance.GetResObjectManager().Load<Sprite>($"Texture/WarpPointImg/{_data._warpPointInfo.UiResourceId}");

        _descGo.SetActive(_data._warpPointInfo.IsDesc);
        if(_descGo.activeSelf)
            _descTxt.text = $"권장레벨: {_data._warpPointInfo.Desc}";
    }

    public void Click()
    {
        if (_grayImg.activeSelf) return;
        if(OnClick != null)
            OnClick(_index);
    }
}
