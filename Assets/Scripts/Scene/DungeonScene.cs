using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class DungeonScene : MonoBehaviour
{
    [SerializeField] private Button _clearButton;
    [SerializeField] private Button _failButton;
    private void Start()
    {
        // BGM
        Managers.Instance.Sound.PlayBGM("BGM_BATTLE_01");
        //
        BattleData battleData = BattleData.Get();
        EContent contentType = battleData._contentType;
        DungeonFieldBase dungeonFieldBase = null;
        if(contentType == EContent.Gold || contentType == EContent.Equipment || contentType == EContent.Tower || contentType == EContent.Fog)
        {
            dungeonFieldBase = gameObject.AddComponent<TimeAttackDungeonManager>();
            dungeonFieldBase.Init(battleData).Forget();
        }
        else if (contentType == EContent.FieldDungeon)
        {
            dungeonFieldBase = gameObject.AddComponent<FieldDungeonManager>();
            dungeonFieldBase.Init(battleData).Forget();
        }
        else if(contentType == EContent.Ranking)
        {
            dungeonFieldBase = gameObject.AddComponent<RankingDungeonManager>();
            dungeonFieldBase.Init(battleData).Forget();
        }
        else if (contentType == EContent.Constellation)
        {
            dungeonFieldBase = gameObject.AddComponent<AwakeDungeonManager>();
            dungeonFieldBase.Init(battleData).Forget();
        }
        else if (contentType == EContent.GuildBoss)
        {
            dungeonFieldBase = gameObject.AddComponent<GuildBossDungeonManager>();
            dungeonFieldBase.Init(battleData).Forget();
        }

        if (dungeonFieldBase == null)
            return;

        Managers.Instance._dungeonFieldBase = dungeonFieldBase;
        _clearButton.onClick.AddListener(dungeonFieldBase.GameWinClicked);
        _failButton.onClick.AddListener(dungeonFieldBase.GameOverClicked);
    }
}
