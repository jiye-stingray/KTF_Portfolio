using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ConstellationCostButton : UICostButton
{
    [SerializeField] GameObject _maxGradeObj;
    ConstellationItemData _data;

    bool _isNotyet;     // 기존 노드가 해금되지 않아서 아직 해금 불가 상태

    UserInfoData userinfo => Managers.Instance.UserInfo();
    UIManager uiManager => Managers.Instance.GetUIManager();

    public void Init(ConstellationItemData data)
    {
        _data = data;


        _maxGradeObj.SetActive(_data._grade == Define.EConstellationGrade.Legendary);
        if (_maxGradeObj.activeSelf) return;

        ConstellationPrice price = ClientLocalDB_Simple.GetData<ConstellationPrice>(DBKey.ConstellationPrice, _data.data.StarSize);

        base.Init(new ECurrency[] { ECurrency.StarPiece },new int[] { price.CostValue });

    }

    public override void Refresh()
    {
        base.Refresh();

        _descTxt.text = _data._isOpen ? "재설정" : "해금";

        // 기존 노드가 해금되지 않아서 해금이 불가능한 경우
        _isNotyet = (!_data._isOpen &&_data.data.PreviousNode != 0 && !userinfo.GetConstellationItemData(_data.data.PreviousNode)._isOpen);
        if(!_gray.activeSelf)
            _gray.SetActive(_isNotyet);


    }

    public override void Click()
    {
        base.Click();
        if (_maxGradeObj.activeSelf)
        {
            // 토스트 메시지
            uiManager.ShowUIToast<UIToastBase>("이미 최고 등급입니다", "ToastMessage");
            return;
        }
        else if(_isNotyet)
        {
            // 토스트 메시지
            uiManager.ShowUIToast<UIToastBase>("해금을 실행 할 수 없습니다", "ToastMessage");

            return;
        }
        else if(_gray.activeSelf)
        {
            // 토스트 메시지
            uiManager.ShowUIToast<UIToastBase>("재화가 부족합니다", "ToastMessage");
            return;
        }

        // 성공
        SuccessAction();
    }

}
