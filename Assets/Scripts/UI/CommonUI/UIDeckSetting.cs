using System;
using static Define;

public class UIDeckSetting : UIBase
{
    [Serializable]
    public struct Stab
    {
        public UIDeckSettingPage deckSettingPage;
    }
    public Stab stab;
    EContent _contentType;

    private bool _isInternalChange = false;
    /*
     * *
     */

    public void InitContentType(EContent contentType, int level = 0, EFactionType factionType = EFactionType.All)
    {
        _contentType = contentType;
        stab.deckSettingPage.InitContentType(contentType, level, factionType);
    }

    private bool ChangeFieldDeckCheck(Action changeAction)
    {
        if (stab.deckSettingPage.ChangeFieldDeckCheck())
        {
            _isInternalChange = true;
            _isInternalChange = false;
            
            UIManager.ShowConfirmPopUp("필드덱이 변경되었습니다.", "저장 하시겠습니까?",
                () =>
                {
                    stab.deckSettingPage.SaveFieldDeck();
                },
                () =>
                {
                    stab.deckSettingPage.ResetDeckData();
                    changeAction();
                });
            return true;
        }

        return false;
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        stab.deckSettingPage.Init();
       
        return true;
    }

    public override void Open()
    {
        base.Open();
        SettingTab();
        Refresh();
    }
    public override void Refresh()
    {
        base.Refresh();
        
        stab.deckSettingPage.Refresh();
    }

    void SettingTab()
    {
        if(!stab.deckSettingPage.isActiveAndEnabled)
            stab.deckSettingPage.Open();
    }

    public override void ClickCloseBtn()
    {
        if (ChangeFieldDeckCheck(ClickCloseBtn))
            return;
        
        base.ClickCloseBtn();
    }
}
