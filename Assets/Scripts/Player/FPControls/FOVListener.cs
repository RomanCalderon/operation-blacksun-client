using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( Camera ) )]
public class FOVListener : MonoBehaviour
{
    private Camera m_camera = null;

    [SerializeField]
    private float m_smoothDamp = 6f;
    private float m_originalFOV, m_aimFOV, m_targetFOV = 0f;

    private void OnEnable ()
    {
        AimController.OnFOVUpdated += UpdateAimFOV;
        AimController.OnAimUpdated += AimUpdated;
    }

    private void OnDisable ()
    {
        AimController.OnFOVUpdated -= UpdateAimFOV;
        AimController.OnAimUpdated -= AimUpdated;
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

    private void UpdateAimFOV ( float fov )
    {
        m_aimFOV = fov;
    }

    private void AimUpdated ( bool state )
    {
        m_targetFOV = state ? m_aimFOV : m_originalFOV;
    }
}
