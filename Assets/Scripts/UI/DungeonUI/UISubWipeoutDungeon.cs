using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UISubWipeoutDungeon : UISubBase
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] TMP_Text _wipeoutCntTxt;
    [SerializeField] CountSwitchDataButton _leftSwitchBtn;
    [SerializeField] CountSwitchDataButton _rightSwitchBtn;
    [SerializeField] UICostButton _costButton;

    Dungeon _dungeonData;

    int _maxKeyCnt;

    IndexWrapper _index = new IndexWrapper();
    private const int MaxWipeCount = 100;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public void InitData(Dungeon dungeon)
    {
        _dungeonData = dungeon;

        switch (_dungeonData.DungeonType)
        {
            case EDungeonType.Equipment:
                _maxKeyCnt = UserInfoData.GetCurrencyValue(ECurrency.AdmissionTicket_EquipmentDungeon);
                break;
            case EDungeonType.Gold:
                _maxKeyCnt = UserInfoData.GetCurrencyValue(ECurrency.AdmissionTicket_GoldDungeon);
                break;

        }

        _index._index = 1;
        _leftSwitchBtn.Init(_index, 1, Mathf.Min(_maxKeyCnt, MaxWipeCount) ,SwitchDataBtnClick);
        _rightSwitchBtn.Init(_index, 1, Mathf.Min(_maxKeyCnt, MaxWipeCount) ,SwitchDataBtnClick);

        base.OpenToStack();
        Refresh();
    }


    public override void Refresh()
    {
        if(_dungeonData.DungeonType == EDungeonType.Constellation)
            _nameTxt.text = $"{_dungeonData.Name}";
        else
            _nameTxt.text = $"{_dungeonData.Name} <color=#6D4421>{_dungeonData.DungeonLevel}</color>층";
        _wipeoutCntTxt.text = _index._index.ToString();
        _costButton.Init(new ECurrency[] { GetTicketCurrency() }, new int[] { _index._index });
    }

    public void MinBtnClick()
    {
        _index._index = 1;
        Refresh();
    }

    public void MaxBtnClick()
    {
        _index._index = Mathf.Min(_maxKeyCnt, MaxWipeCount);
        Refresh();
    }

    public void SwitchDataBtnClick(int index)
    {
        switch (_dungeonData.DungeonType)
        {
            case EDungeonType.Equipment:
                _maxKeyCnt = UserInfoData.GetCurrencyValue(ECurrency.AdmissionTicket_EquipmentDungeon);
                break;
            case EDungeonType.Gold:
                _maxKeyCnt = UserInfoData.GetCurrencyValue(ECurrency.AdmissionTicket_GoldDungeon);
                break;

        }
        // 한쪽 전환시 양쪽 다 update 필요
        _rightSwitchBtn.UpdateGray();
        _leftSwitchBtn.UpdateGray();

        Refresh();
    }

    ECurrency GetTicketCurrency()
    {
        return _dungeonData.DungeonType switch
        {
            EDungeonType.Equipment => ECurrency.AdmissionTicket_EquipmentDungeon,
            EDungeonType.Gold => ECurrency.AdmissionTicket_GoldDungeon,
            _ => ECurrency.None
        };
    }

    public void Click()
    {
        //장비 던전 인 경우 count 체크 후 접근 불가 팝업 띄우기
        if (_dungeonData.DungeonType == EDungeonType.Equipment)
        {
            RewardData reward = ClientLocalDB_Simple.GetData<RewardData>(DBKey.DungeonReward, _dungeonData.RewardID);

            int count = 0;
            for (int i = 0; i < reward.RewardType.Length; i++)
            {
                if (reward.RewardType[i] == ERewardType.EquipmentBox)
                    count += reward.RewardValue[i];

            }
            count *= _index._index;
            if (UserInfoData._dicEquipmentItemData.Count + count > 300)      // 갯수 db에 따로 X 
            {
                // 팝업 띄우기
                Managers.Instance.GetUIManager().ShowConfirmPopUp("가방의 공간이 부족합니다", "( 클릭 시 장비 화면으로 이동합니다 )",
                () =>
                {
                    Managers.Instance.GetUIManager().UIEquipmentSetting.OpenToStack();
                });

                return;
            }
        }

        EContent contentType = Utils.ParseEnum<EContent>(_dungeonData.DungeonType.ToString());
        int point = contentType == EContent.Constellation ? UserInfoData._maxConstellationDungeonMonsterCnt : 0;
        BestHttp_GameManager.OnPostDungeonClear(contentType, EFactionType.None, _dungeonData.DungeonLevel, point, 0, _index._index);
        ClickCloseBtn();
    }
}
