using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class TowerDungeonEntranceItem : MonoBehaviour
{
    [SerializeField] TMP_Text _levelTxt;
    [SerializeField] Image[] _icons;
    [SerializeField] Transform _rewardIconArea;
    [SerializeField] GameObject _currentGo;
    [SerializeField] GameObject _clearImg;
    //[SerializeField] TMP_Text _grayTxt;

    int _clearLevel;

    EFactionType _factionType;
    int _level;
    TowerDungeon _towerDungeonData;
    List<RewardItem> _rewardItemList = new List<RewardItem>();

    UserInfoData Userinfo => Managers.Instance.UserInfo();

    public void Init(EFactionType type, int level)
    {
        _factionType = type;
        _level = level;

        _towerDungeonData = ClientLocalDB_Simple.GetData<TowerDungeon>(DBKey.TowerDungeon, $"{_factionType}_{_level}");

        switch (_factionType)
        {
            case EFactionType.Celestial:
                _clearLevel = Userinfo._clearCelestialTowerDungeonLevel;
                break;
            case EFactionType.Crusher:
                _clearLevel = Userinfo._clearCrusherTowerDungeonLevel;
                break;
            case EFactionType.Guardian:
                _clearLevel = Userinfo._clearGuardianTowerDungeonLevel;
                break;
            case EFactionType.Human:
                _clearLevel = Userinfo._clearHumanTowerDungeonLevel;
                break;
            case EFactionType.All:
                _clearLevel = Userinfo._clearAllTowerDungeonLevel;
                break;
        }


        Refresh();
    }

    void Refresh()
    {
        _levelTxt.text = $"{_towerDungeonData.DungeonLevel} 층";

        // Icon
        SpawnPointInfoData infoData = ClientLocalDB_Simple.GetData<SpawnPointInfoData>(DBKey.SpawnPointInfo, _towerDungeonData.SpawnPointInfo);
        SpawnObjectGroupData groupData = ClientLocalDB_Simple.GetData<SpawnObjectGroupData>(DBKey.SpawnObjectGroup, infoData.SpawnObjectGroup);

        // 초기화
        for (int i = 0; i < _icons.Length; i++)
        {
            _icons[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < 3; i++)
        {
            if (groupData.ObjectList.Length < i) break;

            UnitData monsterData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.MonsterCharacter, groupData.ObjectList[i]);
            _icons[i].gameObject.SetActive(true);
            _icons[i].sprite = Managers.Instance.GetAtlasManager().GetCharacterIcon(monsterData.UnitType, monsterData.Resource);
        }


        for (int i = 0; i < _rewardItemList.Count; i++)
        {
            Destroy(_rewardItemList[i].gameObject);
        }
        _rewardItemList.Clear();

        RewardData dungeonReward = ClientLocalDB_Simple.GetData<RewardData>(DBKey.DungeonReward, _towerDungeonData.RewardID);
        for (int i = 0; i < dungeonReward.RewardType.Length; i++)
        {
            // reward 갯수만큼 반복
            RewardItem icon = Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/Common/RewardItem_68", _rewardIconArea).GetComponent<RewardItem>();

            // icon Init
            icon.Init(dungeonReward.RewardType[i], dungeonReward.RewardId[i], dungeonReward.RewardValue[i]);
            _rewardItemList.Add(icon);
        }


        // Cleard
        _clearImg.SetActive(_clearLevel >= _level);  

        _currentGo.SetActive( !_clearImg.activeSelf &&_clearLevel + 1 == _level);
    }

}
