using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimController : MonoBehaviour
{
    public delegate void AimHandler ( bool state );
    public static AimHandler OnAimUpdated;
    public delegate void FOVHandler ( float fov );
    public static FOVHandler OnFOVUpdated;

    private static bool m_aimState = false;
    public static bool AimState
    {
        get
        {
            return m_aimState;
        }
        set
        {
            if ( value != m_aimState )
            {
                OnAimUpdated?.Invoke ( m_aimState = value );
            }
        }
    }

    public void UpdateFOVParameters ( float aimFOV )
    {
        OnFOVUpdated?.Invoke ( aimFOV );
    }
}
