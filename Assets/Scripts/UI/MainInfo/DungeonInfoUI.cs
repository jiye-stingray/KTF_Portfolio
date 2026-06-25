using System;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static Define;

public class DungeonInfoUI : DungeonBaseUI
{
    [Header("Root")]
    [SerializeField] private GameObject _monsterCountRoot;
    [SerializeField] private GameObject _rankingDamageRoot;
    
    [Header("Label")]
    [SerializeField] private TMP_Text _nameLabel;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _totalDamageLabel;
    [SerializeField] private TMP_Text _damageLabel;
    [SerializeField] private TMP_Text _nextPhaseLabel;
    
    [Header("Auto")]
    [SerializeField] private GameObject _autoButton;
    [SerializeField] private GameObject _autoIcon;
    
    [SerializeField] private Slider _damageSlider;
    private Action<bool> _autoAction;

    public void InitDungeon(Dungeon dungeon)
    {
        _dungeonType = dungeon.DungeonType;
        if (_dungeonType != EDungeonType.Constellation && _dungeonType != EDungeonType.Fog)
            _levelLabel.text = $"{dungeon.DungeonLevel} 단계";
        
        _nameLabel.text = dungeon.Name;
        _rankingDamageRoot.SetActive(false);
        _monsterCountRoot.SetActive(_dungeonType != EDungeonType.Constellation);
        _description.text = "제한 시간 내 모든 몬스터를 처치하세요!";
    }
    
    public void InitDungeon(FogDungeon dungeon)
    {
        _dungeonType = EDungeonType.Fog;
        
        _nameLabel.text = dungeon.Name;
        _levelLabel.text = "-";
        _rankingDamageRoot.SetActive(false);
        _monsterCountRoot.SetActive(true);
        _description.text = "제한 시간 내 모든 몬스터를 처치하세요!";
    }
    
    public void InitDungeon(FieldDungeon dungeon)
    {
        _dungeonType = EDungeonType.Field;
        
        _nameLabel.text = dungeon.Name;
        _levelLabel.text = "-";
        _rankingDamageRoot.SetActive(false);
        _monsterCountRoot.SetActive(true);
        _description.text = "모든 몬스터를 처치하고 보물상자를 획득 하세요!";
    }
    
    public void InitDungeon(TowerDungeon dungeon)
    {
        _dungeonType = EDungeonType.Tower;
        _levelLabel.text = $"{dungeon.DungeonLevel} 층";
        
        _nameLabel.text = Define.TowerDungeonName[dungeon.Faction];
        _rankingDamageRoot.SetActive(false);
        _monsterCountRoot.SetActive(true);
        _description.text = "제한 시간 내 모든 몬스터를 처치하세요!";
    }
    
    public void InitRankingDungeon(RankingDungeon dungeon)
    {
        _dungeonType = EDungeonType.Ranking;
        _nameLabel.text = "랭킹 던전";
        UpdateRankingDungeonData(dungeon);
        
        _rankingDamageRoot.SetActive(true);
        _monsterCountRoot.SetActive(false);
        _description.text = "제한 시간 내 몬스터에게 최대한 많은 대미지를 주세요!";
    }
    public void InitGuildBossDungeon(GuildBossDto guildBossDto)
    {
        _dungeonType = EDungeonType.GuildBoss;
        _nameLabel.text = "연합 괴수 토벌";
        
        _levelLabel.text = $"{guildBossDto.step} 단계";

        _rankingDamageRoot.SetActive(true);
        _monsterCountRoot.SetActive(false);
        _description.text = "제한 시간 내 몬스터에게 최대한 많은 대미지를 주세요!";
    }

    public void UpdateRankingDungeonData(RankingDungeon dungeon)
    {
        _levelLabel.text = $"{dungeon.Phase} 단계";
        _damageScale = dungeon.DamageScale;
    }
    
    public void UpdateBossHp(double currentHp, double maxHp,double totalDamage)
    {
        _damageLabel.text = currentHp.ToString("N0");
        _nextPhaseLabel.text = maxHp.ToString("N0");
        _damageSlider.value = (float)(currentHp / maxHp);
        _totalDamageLabel.text = totalDamage.ToString("N0");
    }

    public void UpdateDamagePoint(double damage, double totalDamage)
    {
        _totalDamageLabel.text = totalDamage.ToString("N0");
        _damageSlider.value = (float)(damage / _damageScale);
        _damageLabel.text = damage.ToString("N0");
        _nextPhaseLabel.text = _damageScale.ToString("N0");
    }

    public void ActiveAutoButton(bool isOn)
    {
        _autoButton.gameObject.SetActive(isOn);
    }
    
    public void SetAutoMode(bool isOn)
    {
        bool autoMode = isOn;
        _autoIcon.transform.DOKill();

        if (autoMode)
            _autoIcon.transform.DOLocalRotate(new Vector3(0f, 0f, 360f), 1f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
        else
            _autoIcon.transform.localRotation = Quaternion.identity;
        
        if(_autoAction != null)
            _autoAction(autoMode);
    }

    public void SwitchAutoMode()
    {
        MapManager mapManager = Managers.Instance.GetMapManager();
        mapManager.IsDungeonAutoMode = !mapManager.IsDungeonAutoMode;
        SetAutoMode(mapManager.IsDungeonAutoMode);
    }

    public void SetAutoAction(Action<bool> action)
    {
        _autoAction = action;
    }
}