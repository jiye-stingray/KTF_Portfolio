using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager
{
    private Camera _uicam;
    public Camera UICam
    {
        get
        {
            if (_uicam == null)
                _uicam = GameObject.Find("UICamera").GetComponent<Camera>();

            return _uicam;
        }
    }

    private FollowCamera _followCam;
    public FollowCamera FollowCam
    {
        get
        {
            if (_followCam == null)
                _followCam = Camera.main.GetComponent<FollowCamera>();

            return _followCam;
        }
    }
}
