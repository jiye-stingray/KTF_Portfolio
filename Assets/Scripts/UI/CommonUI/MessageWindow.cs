using TMPro;

public class MessageWindow : BaseWindow
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;
    
    public void Init(string title, string content)
    {
        titleText.text = title;
        contentText.text = content;
    }
}