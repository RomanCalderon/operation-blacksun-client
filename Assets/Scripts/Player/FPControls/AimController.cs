using UnityEngine;
using PlayerInput;

public class AimController : MonoBehaviour
{
    public delegate void AimStateHandler ( bool state );
    public static AimStateHandler OnAimStateUpdated;
    public delegate void AimSpeedHandler ( float aimSpeed );
    public static AimSpeedHandler OnAimSpeedUpdated;
    public delegate void FOVHandler ( float fov );
    public static FOVHandler OnTargetFOVUpdated;

    public static bool CanAim
    {
        get
        {
            return m_canAim;
        }
        set
        {
            if ( value == false && m_aimState == true )
            {
                AimState = false;
            }
            m_canAim = value;
        }
    }
    private static bool m_canAim = false;

    public static bool AimState
    {
        get
        {
            return m_aimState;
        }
        set
        {
            if ( m_canAim && value != m_aimState )
            {
                m_aimState = value;
                OnAimStateUpdated?.Invoke ( value );
                PlayerInputController.SetAim ( value );
            }
        }
    }
    private static bool m_aimState = false;

    public void UpdateAimSpeed ( float aimSpeed )
    {
        OnAimSpeedUpdated?.Invoke ( aimSpeed );
    }

    public void UpdateAimFOV ( float aimFOV )
    {
        OnTargetFOVUpdated?.Invoke ( aimFOV );
    }
}
