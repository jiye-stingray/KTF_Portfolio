using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIJoystick : UIBase, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public static int JoystickPointerId { get; private set; } = -9999;
    
    private Vector2 moveDir;
    public Vector2 MoveDir
    {
        get => moveDir;
        set
        {
            moveDir = value;
        }
    }
    [SerializeField] private GameObject _handler;
    [SerializeField] private GameObject _bg;
    [SerializeField] private RectTransform _handlerRect;
    [SerializeField] private RectTransform _bgRect;
    [SerializeField] private RectTransform _myRectTransform;
    [SerializeField] private Image _touchImage;

    [SerializeField] bool _enable;
    
    private JoystickController _joystickController;
    private Camera _uiCamera; 
    private Vector2 _touchPos;
    private Vector2 _originPos;
    private float _handleRadius;
    private bool _touch;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _originPos = transform.localPosition;
        _handleRadius = (_bgRect.rect.width - _handlerRect.rect.width) * 0.3435f;      // hangle 가동 범위

        _enable = true;
        _touch = false;
        return true;
    }

    public void SetJoystick(JoystickController joystickController, Camera uiCamera)
    {
        _joystickController = joystickController;
        _uiCamera = uiCamera;
        
        _touchImage.enabled = _joystickController.joystickType == Define.EJoystickType.Flexible;
    }

    // Event

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_enable)
            return;
        
        JoystickPointerId = eventData.pointerId;
        
        _touch = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_myRectTransform, eventData.position, _uiCamera, out _touchPos);

        if (_joystickController.joystickType == Define.EJoystickType.Flexible)
        {
            _bgRect.localPosition = _touchPos;
            _handlerRect.localPosition =_touchPos;
        }

        _joystickController.JoystickState = Define.EJoystickState.PointDown;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_touch)
            return;

        JoyStickReset();
    }

    void JoyStickReset()
    {
        JoystickPointerId = -9999;
        
        _touch = false;
        moveDir = Vector2.zero;

        _handlerRect.anchoredPosition = Vector3.zero;
        _bgRect.anchoredPosition = Vector3.zero;

        _joystickController.SetJoystickStateValue(Define.EJoystickState.PointUp, moveDir);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_touch) return;

        // BG/Handle의 공통 부모 기준으로 좌표를 뽑는다 (핵심)
        RectTransform parentRect = (RectTransform)_bgRect.parent;

        Vector2 dragPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, _uiCamera, out dragPos);

        // 중심점: Fixed는 BG의 원래 위치, Flexible은 터치 시작 위치
        Vector2 center = (_joystickController.joystickType == Define.EJoystickType.Fixed)
            ? _bgRect.anchoredPosition
            : _touchPos; // OnPointerDown에서 parentRect 기준으로 저장해야 함(아래 참고)

        Vector2 offset = dragPos - center;
        Vector2 clamped = Vector2.ClampMagnitude(offset, _handleRadius);

        moveDir = (offset.sqrMagnitude > 0.0001f) ? offset.normalized : Vector2.zero;

        // Handle은 중심 + 제한된 오프셋
        _handlerRect.anchoredPosition = center + clamped;

        _joystickController.SetJoystickStateValue(Define.EJoystickState.Dragging, moveDir);
    }

    public override void Close()
    {
        EnableJoystick(false);
        base.Close();
    }

    public override void Open()
    {
        EnableJoystick(true);
        base.Open();
    }

    public void EnableJoystick(bool state)
    {
        if(!state)
            JoyStickReset();
        _enable = state;
    }
}
