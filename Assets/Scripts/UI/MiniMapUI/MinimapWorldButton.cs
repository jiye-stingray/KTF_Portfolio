using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinimapWorldButton : MonoBehaviour
{
    [SerializeField] GameObject _gray;
    [SerializeField] GameObject _squadIcon;
    [SerializeField] TMP_Text _nameTxt;

    [SerializeField] int _id;

    FieldInfo _data;
    FieldItemData _fieldItemData;
    UserInfoData UserInfoData => Managers.Instance.UserInfo();

    public void Init()
    {
        _data = ClientLocalDB_Simple.GetData<FieldInfo>(DBKey.FieldInfo, _id);
        if (_data == null) return;
        _fieldItemData = UserInfoData._dicFieldItemData[_id];
        Refresh();

    }

    private void Refresh()
    {
        _nameTxt.text = _data.Name;
        _gray.SetActive(!_fieldItemData.isOpen);
        _squadIcon.SetActive(UserInfoData._fieldId == _id);
    }

    public void Click()
    {

        if (_gray.activeSelf)
        {
            Managers.Instance.GetUIManager().ShowCommonToastMessage("해당 지역으로 이동할 수 없습니다.");
            return;
        }

        if(_id == 1) // main field 일 때
        {
            if (UserInfoData._fieldId == _id)       // mainField 일때 본인이 이미 위치해 있으면 반환
            {
                return;
            }

#if USE_SERVER
            Managers.Instance.GetServerManager().OnPostChangeMap(1, 1, false);     // 초원 
#else

            // 맵 이동
            UserInfoData._fieldId = _id;
        
            Managers.Instance.GetMapManager().UnLoadMap();
            Loading.Load(Loading.Field);
#endif

        }
        else        // 난이도 선택
        {
            Managers.Instance.GetUIManager().UIMinimap.InitUISubWorldDifficulty(_id);
        }
    }
}
