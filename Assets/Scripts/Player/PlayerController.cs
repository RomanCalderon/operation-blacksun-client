using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum InputModes
    {
        TOGGLE,
        HOLD
    }

    public static bool CrouchInput { get; private set; } = false;
    public static bool ProneInput { get; private set; } = false;

    private InputModes m_crouchInputMode = InputModes.HOLD; // TODO: Update from KeybindManager
    private static bool m_crouchToggle = false;
    private InputModes m_proneInputMode = InputModes.TOGGLE; // TODO: Update from KeybindManager
    private static bool m_proneToggle = false;


    private void Awake ()
    {
        Initialize ();
    }

    public static void Initialize ()
    {
        CrouchInput = m_crouchToggle = false;
        ProneInput = m_proneToggle = false;
    }

    private void Update ()
    {
        // CROUCH INPUT
        switch ( m_crouchInputMode )
        {
            case InputModes.TOGGLE:
                if ( Input.GetKeyDown ( KeyCode.C ) ) // TODO: Switch to KeybindManager as input
                {
                    m_crouchToggle = !m_crouchToggle;
                    CrouchInput = m_crouchToggle;
                }
                break;
            case InputModes.HOLD:
                CrouchInput = Input.GetKey ( KeyCode.C ); // TODO: Switch to KeybindManager as input
                break;
            default:
                break;
        }

        // Switch prone to false if crouch is true
        if ( CrouchInput && ProneInput )
        {
            ProneInput = m_proneToggle = false;
        }

        // PRONE INPUT
        switch ( m_proneInputMode )
        {
            case InputModes.TOGGLE:
                if ( Input.GetKeyDown ( KeyCode.LeftAlt ) ) // TODO: Switch to KeybindManager as input
                {
                    m_proneToggle = !m_proneToggle;
                    ProneInput = m_proneToggle;
                }
                break;
            case InputModes.HOLD:
                ProneInput = Input.GetKey ( KeyCode.LeftAlt ); // TODO: Switch to KeybindManager as input
                break;
            default:
                break;
        }
    }

    private void FixedUpdate ()
    {
        SendInputToServer ();
    }

    private void SendInputToServer ()
    {
        bool [] _inputs = new bool []
        {
            Input.GetKey(KeyCode.W),            // [0] Forward
            Input.GetKey(KeyCode.S),            // [1] Backward
            Input.GetKey(KeyCode.A),            // [2] Left
            Input.GetKey(KeyCode.D),            // [3] Right
            Input.GetKey(KeyCode.LeftShift),    // [4] Run
            Input.GetKey(KeyCode.Space),        // [5] Jump
            CrouchInput,                      // [6] Crouch
            ProneInput                        // [7] Prone
        };

        ClientSend.PlayerMovement ( _inputs );
    }
}