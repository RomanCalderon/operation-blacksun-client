using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( Camera ) )]
public class FOVListener : MonoBehaviour
{
    private Camera m_camera = null;
    private float m_smoothDamp = 6f;
    private float m_originalFOV, m_aimFOV, m_targetFOV = 0f;

    [SerializeField]
    private float m_FOVMultiplier = 1.0f;


    private void OnEnable ()
    {
        AimController.OnAimStateUpdated += SetTargetFOV;
        AimController.OnAimSpeedUpdated += SetAimSpeed;
        AimController.OnTargetFOVUpdated += SetAimFOV;
    }

    private void OnDisable ()
    {
        AimController.OnAimStateUpdated -= SetTargetFOV;
        AimController.OnAimSpeedUpdated -= SetAimSpeed;
        AimController.OnTargetFOVUpdated -= SetAimFOV;
    }

    private void Awake ()
    {
        m_camera = GetComponent<Camera> ();
        m_originalFOV = m_targetFOV = m_camera.fieldOfView;
    }

    private void Update ()
    {
        m_camera.fieldOfView = Mathf.Lerp ( m_camera.fieldOfView, m_targetFOV, Time.deltaTime * m_smoothDamp );
    }

    private void SetAimSpeed ( float aimSpeed )
    {
        m_smoothDamp = aimSpeed;
    }

    private void SetAimFOV ( float fov )
    {
        m_aimFOV = fov * m_FOVMultiplier;
    }

    private void SetTargetFOV ( bool aimState )
    {
        m_targetFOV = aimState ? m_aimFOV : m_originalFOV;
    }
}
