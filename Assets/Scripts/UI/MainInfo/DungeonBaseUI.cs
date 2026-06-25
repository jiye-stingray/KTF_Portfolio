using TMPro;
using UnityEngine;
using static Define;

public class DungeonBaseUI : UIBase
{
    [SerializeField] private TMP_Text _time;
    [SerializeField] public TMP_Text _levelLabel;
    [SerializeField] private TMP_Text _count;

    public Transform _timeTxtTrans => _time.transform;
    
    protected EDungeonType _dungeonType;
    protected double _damageScale;
    
    public UIBattleWinPopup _uiBattleWinPopup = null;
    public UIBattleLosePopup _uiBattleLosePopup = null;

    public virtual void DungeonExit()
    {
        bool onlyWinType = _dungeonType == EDungeonType.Constellation || _dungeonType == EDungeonType.Ranking || _dungeonType == EDungeonType.Field ||
                           _dungeonType == EDungeonType.GuildBoss; 
        string description = onlyWinType ? "(진행중인 전투 기록이 초기화 됩니다.)" : "( 입장권이 소모되지 않습니다. )";  
        UIManager.ShowConfirmPopUp("나가시겠습니까?", description,
            () =>
            {
                if(onlyWinType)
                    Managers.Instance._dungeonFieldBase.GoField();
                else
                    Managers.Instance._dungeonFieldBase.GameOver().Forget();
            });
    }

    public void EnableTime(bool state)
    {
        if(_time != null)
            _time.gameObject.SetActive(state);
    }

    public void UpdateTime(int value)
    {
        if(_time != null)
            _time.text = $"{value}s";
    }
    
    public void UpdateCount(int count)
    {
        if(_dungeonType == EDungeonType.Constellation)
            _levelLabel.text = $"처치한 몬스터 수: {count}";
        else
            _count.text = $"x {count}";
    }

    public void OpenBattleLosePopup(EContent contentType, EFactionType factionType, int index)
    {   
        _uiBattleLosePopup = UIManager.ShowUISubBase<UIBattleLosePopup>(this, "UIBattleLosePopup");
        _uiBattleLosePopup.Init(contentType, factionType, index);
    }

    //일반던전
    public void OpenBattleWinPopup(EContent contentType, EFactionType factionType, int level, double point, bool isWipeOut, RewardBundleDto rewardBundle)
    {
        _uiBattleWinPopup = UIManager.ShowUISubBase<UIBattleWinPopup>(this, "UIBattleWinPopup");
        _uiBattleWinPopup.Init(contentType, factionType, level, point, isWipeOut, rewardBundle);
    }
    
    //랭킹던전
    public void OpenBattleWinPopup(EContent contentType, double point, double userPoint, bool isBestScore,  int step, RewardBundleDto rewardBundle)
    {
        _uiBattleWinPopup = UIManager.ShowUISubBase<UIBattleWinPopup>(this, "UIBattleWinPopup");
        _uiBattleWinPopup.Init(contentType, point, userPoint, isBestScore, step, rewardBundle);
    }

    // 별자리 던전
    public void OpenBattleWinPopup(EContent contentType, double point, double userPoint, bool isBestScore, RewardBundleDto rewardBundle)
    {
        _uiBattleWinPopup = UIManager.ShowUISubBase<UIBattleWinPopup>(this, "UIBattleWinPopup");
        _uiBattleWinPopup.Init(contentType, point, userPoint, isBestScore,rewardBundle);
    }

    //길드 보스 던전
    public void OpenBattleWinPopup(EContent contentType, double point, bool isBestScore, RewardBundleDto rewardBundle = null)
    {
        _uiBattleWinPopup = UIManager.ShowUISubBase<UIBattleWinPopup>(this, "UIBattleWinPopup");
        _uiBattleWinPopup.Init(contentType, point, isBestScore);
    }
    
    //안개 던전 
    public void OpenFogDungeonBattleWinPopup()
    {
        _uiBattleWinPopup = UIManager.ShowUISubBase<UIBattleWinPopup>(this, "UIBattleWinPopup");
        _uiBattleWinPopup.FogDungeon();
    }
    
    public void OpenFieldDungeonBattleWinPopup(RewardBundleDto rewardBundle)
    {
        _uiBattleWinPopup = UIManager.ShowUISubBase<UIBattleWinPopup>(this, "UIBattleWinPopup");
        _uiBattleWinPopup.FieldDungeon(rewardBundle);
    }
    
    public void OpenGuildBossAlreadyClearedPopup()
    {
        _uiBattleWinPopup = UIManager.ShowUISubBase<UIBattleWinPopup>(this, "UIBattleWinPopup");
        _uiBattleWinPopup.OpenGuildBossAlreadyClearedPopup();
    }
    
    public override void ClickCloseBtn()
    {
        if (!Managers.Instance._dungeonFieldBase._isPlaying)
            return;
        
        DungeonExit();
    }
}
