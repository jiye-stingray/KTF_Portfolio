using System;
using UnityEngine;

public class JoystickController
{
    // Event
    public event Action<Vector2> OnMoveDirChange;
    public event Action<Define.EJoystickState> OnJoystickTypeChange;

    // Define
    public Define.EJoystickType joystickType = Define.EJoystickType.Flexible;
    public Define.EJoystickState joystickState = Define.EJoystickState.PointUp;
    
    private Vector2 moveDir;
    public Vector2 MoveDir
    {
        get => moveDir;
        set
        {
            moveDir = value;
            OnMoveDirChange?.Invoke(moveDir);
        }
    }

    public Define.EJoystickState JoystickState
    {
        get => joystickState;

        set
        {
            joystickState = value;
            OnJoystickTypeChange?.Invoke(joystickState);
        }
    }

    
    public void SetJoystickStateValue(Define.EJoystickState _state, Vector2 _dir)
    {
        JoystickState = _state;
        MoveDir = _dir;
    }
}
