using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Constants

    private const int NUM_BOOL_INPUTS = 8;

    #endregion

    #region Models

    public enum InputModes
    {
        TOGGLE,
        HOLD
    }

    public struct PlayerInputs
    {
        // Interpolation time on client
        public short lerp_msec;
        // Duration in ms of command
        public byte msec;
        // Player movement inputs
        [MarshalAs ( UnmanagedType.ByValArray, SizeConst = NUM_BOOL_INPUTS )]
        public bool [] inputs;
        // Player rotation
        public Quaternion rot;
    }

    #endregion

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
        short lerpMilliseconds = ( short ) ( Time.deltaTime * 1000 );
        byte milliseconds = ( byte ) ( Time.fixedDeltaTime * 1000 );
        bool [] _inputs = new bool [ NUM_BOOL_INPUTS ]
        {
            Input.GetKey(KeyCode.W),            // [0] Forward
            Input.GetKey(KeyCode.S),            // [1] Backward
            Input.GetKey(KeyCode.A),            // [2] Left
            Input.GetKey(KeyCode.D),            // [3] Right
            Input.GetKey(KeyCode.LeftShift),    // [4] Run
            Input.GetKey(KeyCode.Space),        // [5] Jump
            CrouchInput,                        // [6] Crouch
            ProneInput                          // [7] Prone
        };
        Quaternion rotation = transform.rotation;
        PlayerInputs playerInput = new PlayerInputs ()
        {
            lerp_msec = lerpMilliseconds,
            msec = milliseconds,
            inputs = _inputs,
            rot = rotation
        };
        byte [] inputByteArr = GetBytes ( playerInput );
        ClientSend.PlayerInput ( inputByteArr );
    }

    private byte [] GetBytes ( PlayerInputs str )
    {
        int size = Marshal.SizeOf ( str );
        byte [] arr = new byte [ size ];

        IntPtr ptr = Marshal.AllocHGlobal ( size );
        Marshal.StructureToPtr ( str, ptr, true );
        Marshal.Copy ( ptr, arr, 0, size );
        Marshal.FreeHGlobal ( ptr );
        return arr;
    }
}