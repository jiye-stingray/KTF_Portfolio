using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Utils;

public class UIGaugeWidget : MonoBehaviour
{

    [SerializeField] private Image gauge = null;
    public Image Gauge { get { return gauge; } }

    [SerializeField] private TMP_Text txtGauge = null;


    float percent = 0.0f;
    public float Percent { get { return percent; } }

    /*
     * *
     */
    
    public void SetPercent(float _percent)
    {
        percent = _percent;
    }

    public void Refresh()
    {
        gauge.fillAmount = percent;
        if (txtGauge != null)
            txtGauge.gameObject.SetActive(false);
    }

    public void Refresh(float _percent)
    {
        gauge.fillAmount = _percent;
        if (txtGauge != null)
            txtGauge.gameObject.SetActive(false);
    }

    public void Refresh(float _percent, string value, string max)
    {
        gauge.fillAmount = _percent;
        DrawTxt(value, max);
    }

    public void Refresh(float _percent, string value)
    {
        gauge.fillAmount = _percent;
        DrawTxt(value);
    }

    public void Refresh(float value, float max)
    {
        if (value >= max)
            gauge.fillAmount = 1.0f;
        else
            gauge.fillAmount = value / max;

        /**/
        DrawTxt((int)value, (int)max);
    }
    public void Refresh(float value, float max, float realValue,float realMax)
    {
        if (value >= max)
            gauge.fillAmount = 1.0f;
        else
            gauge.fillAmount = value / max;

        /**/
        DrawTxt((int)realValue, (int)realMax);
    }

    public void DrawTxt(string value, string max)
    {
        if (txtGauge == null)
            return;

        StringMaker.Clear();
        StringMaker.stringBuilder.Append(value);
        StringMaker.stringBuilder.Append("/");
        StringMaker.stringBuilder.Append(max);
        txtGauge.text = StringMaker.stringBuilder.ToString();
    }

    public void DrawTxt(string value)
    {
        if (txtGauge == null)
            return;
        txtGauge.text = value;
    }

    public void DrawTxt(int value, int max)
    {
        if (txtGauge == null)
            return;

        StringMaker.Clear();
        StringMaker.stringBuilder.Append(value);
        StringMaker.stringBuilder.Append("/");
        StringMaker.stringBuilder.Append(max);
        txtGauge.text = StringMaker.stringBuilder.ToString();
    }
}