using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UISpeechBox : UIBase
{
    public Vector3 worldOffset = new Vector3(0f, 5f, 0f);

    protected RectTransform _rectTrans;

    private Button _button;
    private Button _lockButton;
    protected BaseBuilding _rootBuilding;

    private Camera _worldCam;
    private Camera _uiCam;
    private Canvas _canvas;
    private RectTransform _canvasRect;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _button = GetComponentsInChildren<Button>(true)
            .FirstOrDefault(b => b.name == "SpeechButton");

        _lockButton = GetComponentsInChildren<Button>(true)
            .FirstOrDefault(b => b.name == "LockButton");

        _rectTrans = GetComponent<RectTransform>();

        _worldCam = Camera.main;
        _uiCam = Managers.Instance.GetCameraManager().UICam;

        _canvas = GetComponentInParent<Canvas>();
        _canvasRect = _canvas.GetComponent<RectTransform>();

        return true;
    }

    public virtual void InitData(int idx, BaseBuilding building)
    {
        _rootBuilding = building;

        _button.onClick.RemoveListener(OnClickSpeechButton);
        _button.onClick.AddListener(OnClickSpeechButton);

        Refresh();
    }

    protected virtual void LateUpdate()
    {
        if (_rootBuilding == null)
            return;

        if (!gameObject.activeInHierarchy)
            return;

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_worldCam == null)
            _worldCam = Camera.main;

        if (_uiCam == null)
            _uiCam = Managers.Instance.GetCameraManager().UICam;

        Vector3 worldPos = _rootBuilding.transform.position + worldOffset;

        Vector3 screenPos = _worldCam.WorldToScreenPoint(worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPos,
            _uiCam,
            out Vector2 localPos
        );

        _rectTrans.anchoredPosition = localPos;
    }

    public virtual void OnClickSpeechButton()
    {
        _rootBuilding.SuccessSpeechBtnClick();
    }

    public virtual void EnableButtonCheck(bool state)
    {
        _button.gameObject.SetActive(state);
        _lockButton.gameObject.SetActive(!state);
    }

    protected void DisableButton()
    {
        _button.gameObject.SetActive(false);
        _lockButton.gameObject.SetActive(false);
    }
}