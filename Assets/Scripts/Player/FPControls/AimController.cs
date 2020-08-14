using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimController : MonoBehaviour
{
    public delegate void AimStateHandler ( bool state );
    public static AimStateHandler OnAimStateUpdated;
    public delegate void AimSpeedHandler ( float aimSpeed );
    public static AimSpeedHandler OnAimSpeedUpdated;
    public delegate void FOVHandler ( float fov );
    public static FOVHandler OnTargetFOVUpdated;

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
                OnAimStateUpdated?.Invoke ( m_aimState = value );
            }
        }
    }

    public void UpdateAimSpeed ( float aimSpeed )
    {
        OnAimSpeedUpdated?.Invoke ( aimSpeed );
    }

    public void UpdateAimFOV ( float aimFOV )
    {
        OnTargetFOVUpdated?.Invoke ( aimFOV );
    }
}
