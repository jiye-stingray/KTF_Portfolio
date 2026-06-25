using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UISpeechBubble : UIBase
{
    [SerializeField] TMP_Text _Txt;

    protected BaseBuilding _rootBuilding;
    protected RectTransform _rectTrans;

    TimeData timeData;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _rectTrans = GetComponent<RectTransform>();

        return true;
    }

    public virtual void InitData(BaseBuilding building, string talkKey, int duration)
    {
        _rootBuilding = building;
        _Txt.text = talkKey;
        timeData = new TimeData();
        timeData.SetByDuration(duration);

        SetPosition();
    }

    protected virtual void Update()
    {
        SetPosition();

        if (timeData.GetRemain() <= 0)
            Close();
    }

    private void SetPosition()
    {
        if (_rootBuilding == null)
            return;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_rootBuilding.transform.position) + new Vector3(0, 150, 0);
        _rectTrans.position = (Vector2)Managers.Instance.GetCameraManager().UICam.ScreenToWorldPoint(screenPosition);
    }

    public override void Close()
    {
        Destroy(gameObject);
    }
}
