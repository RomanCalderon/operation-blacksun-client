using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoilController : MonoBehaviour
{
    [Header ( "Recoil Settings" )]
    [SerializeField]
    private float m_rotationSpeed = 6f;
    [SerializeField]
    private float m_returnSpeed = 25f;

    [Header ( "Hipfire" )]
    [SerializeField]
    private Vector3 m_recoilRotation = new Vector3 ( 2f, 2f, 2f );

    [Header ( "Aiming" )]
    [SerializeField]
    private Vector3 m_recoilRotationAiming = new Vector3 ( 0.5f, 0.5f, 0.5f );

    private bool m_aiming = false;

    private Vector3 m_currentRotation;
    private Vector3 m_rotation;

    #region Runtime

    private void Update ()
    {
        float deltaTime = Time.deltaTime;
        m_currentRotation = Vector3.Lerp ( m_currentRotation, Vector3.zero, m_returnSpeed * deltaTime );
        m_rotation = Vector3.Slerp ( m_rotation, m_currentRotation, m_rotationSpeed * deltaTime );
        transform.localRotation = Quaternion.Euler ( m_rotation );
    }

    #endregion

    public void Fire ()
    {
        if ( m_aiming )
        {
            float recoilPitch = -m_recoilRotationAiming.x;
            float recoilYaw = Random.Range ( -m_recoilRotationAiming.y, m_recoilRotationAiming.y );
            float recoilRoll = Random.Range ( -m_recoilRotationAiming.z, m_recoilRotationAiming.z );
            m_currentRotation += new Vector3 ( recoilPitch, recoilYaw, recoilRoll );
        }
        else
        {
            float recoilPitch = -m_recoilRotation.x;
            float recoilYaw = Random.Range ( -m_recoilRotation.y, m_recoilRotation.y );
            float recoilRoll = Random.Range ( -m_recoilRotation.z, m_recoilRotation.z );
            m_currentRotation += new Vector3 ( recoilPitch, recoilYaw, recoilRoll );
        }
    }

    #region Event Listeners

    public void SetAimState ( bool state )
    {
        m_aiming = state;
    }

    #endregion
}
