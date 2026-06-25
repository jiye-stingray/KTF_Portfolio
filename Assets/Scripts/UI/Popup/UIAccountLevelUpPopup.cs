using TMPro;
using UnityEngine;

public class UIAccountLevelUpPopup : UIPopupBase
{
    [SerializeField] private TMP_Text _levelTxt;
    [SerializeField] private GameObject _contentsRoot;
    [SerializeField] private TMP_Text _contentsTxt;

    int _level;

    public void SetData(int level)
    {
        // SFX Account Levelup
        Managers.Instance.Sound.PlaySFX("Effect", "SE_accoutleveup");
        
        _level = level;
        UserLevelData userLevelData = ClientLocalDB_Simple.GetData<UserLevelData>(DBKey.UserLevel, level);
        _levelTxt.text = $"Lv.{userLevelData.Level}";
        bool contentsOpen = !userLevelData.LevelUpMessage.IsNull();
        _contentsRoot.SetActive(contentsOpen);
        _contentsTxt.text = contentsOpen ? userLevelData.LevelUpMessage : "";
    }

    public override void ClickCloseBtn()
    {
        // 추후 튜토리얼 아이디로 변경할거임
        Managers.Instance.GetTutorialManager().CheckQuestTutorial();
        base.ClickCloseBtn();
    }
}
