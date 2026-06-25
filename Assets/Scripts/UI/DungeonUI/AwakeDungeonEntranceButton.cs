using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static Utils;

public class AwakeDungeonEntranceButton : MonoBehaviour
{
    [SerializeField] bool _wipeOut;
    UserInfoData UserInfo => Managers.Instance.UserInfo();
    UIManager UIManager => Managers.Instance.GetUIManager();
    AtlasManager AtlasManager => Managers.Instance.GetAtlasManager();

    public void Init()
    {
    }

    public void Click()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        if (_wipeOut)
        {
            // 처음 도전 하는 경우
            if(UserInfo._maxConstellationDungeonMonsterCnt <= 0)
            {
                UIManager.ShowCommonToastMessage("소탕이 불가능 합니다.");
                return;
            }

            if (UIManager.UIDungeonEntrance._uiSubWipeoutDungeon != null)
            {
                UIManager.UIDungeonEntrance._uiSubWipeoutDungeon.ClickCloseBtn();
                UIManager.UIDungeonEntrance._uiSubWipeoutDungeon = null;
            }

            UIManager.UIDungeonEntrance._uiSubWipeoutDungeon = UIManager.ShowUISubBase<UISubWipeoutDungeon>(UIManager.UIAwakeDungeonEntrance,
                "UISubWipeoutDungeon");
            UIManager.UIDungeonEntrance._uiSubWipeoutDungeon.InitData(UIManager.UIAwakeDungeonEntrance._dungeonData);
        }
        else
        {   
            // 던전 입장
            UIManager.UIDeckSetting.InitContentType(EContent.Constellation);
            UIManager.UIDeckSetting.OpenToStack();   
        }
    }
}