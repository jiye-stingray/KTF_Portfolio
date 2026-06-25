using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstellationNode : MonoBehaviour
{
    [SerializeField] int _id;
    [SerializeField] Image _nodeImg;
    [SerializeField] GameObject _selectObj;
    ConstellationItemData _data;


    UserInfoData userinfo => Managers.Instance.UserInfo();

    public void SetData()
    {
        _data = userinfo.GetConstellationItemData(_id);
        if (_data == null)
        {
            Debug.LogError($"null data {_id}");
            return;
        }
        Refresh();
    }

    public void Refresh()
    {
        _nodeImg.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.ConstellationAtlas, ReturnNodeImg());
        _nodeImg.SetNativeSize();
        _selectObj.SetActive(!_data._isOpen && _data.data.PreviousNode == 0);
    }

    public void Click()
    {
        Managers.Instance.GetUIManager().UIConstellation._currentItemData = _data;
        Managers.Instance.GetUIManager().UIConstellation.Refresh();
    }

    private string ReturnNodeImg()
    {
        string gradeSt = string.Empty;
        switch (_data._grade)
        {
            case Define.EConstellationGrade.Normal:
                gradeSt = "Green";
                break;
            case Define.EConstellationGrade.Rare:
                gradeSt = "Blue";
                break;
            case Define.EConstellationGrade.Epic:
                gradeSt = "Purple";
                break;
            case Define.EConstellationGrade.Legendary:
                gradeSt = "gold";
                break;
            default:
                break;
        }
        if (!_data._isOpen) gradeSt = "gray";

        return $"{_data.data.StarSize}_{gradeSt}";
    }
}
