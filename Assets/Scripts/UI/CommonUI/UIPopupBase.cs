public class UIPopupBase : UIBase
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        return true;
    }

    public override void OpenToStack()
    {
        base.Open();
        
        // Sound 
        Managers.Instance.Sound.PlaySFX("Effect", "SE_reward");

    }

    public override void ClickCloseBtn()
    {
        // Sound 
        Managers.Instance.Sound.PlaySFX("Effect", "SE_Inventory_Close_01");

        
        UIManager.ClosePopupUI(this);
    }

    public void DestroyClose()
    {
        DestroyImmediate(gameObject);
    }
}
