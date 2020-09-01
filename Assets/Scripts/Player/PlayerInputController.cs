using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Controlls player input and handles sending these sampled inputs
/// to the server and the local movement controller.
/// </summary>
public class PlayerInputController : MonoBehaviour
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
        // Request number
        public uint request;
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

    #region Members

    // Movement Controller reference
    [SerializeField]
    private PlayerMovementController m_playerMovementController = null;

    public static bool CrouchInput { get; private set; } = false;
    public static bool ProneInput { get; private set; } = false;

    private InputModes m_crouchInputMode = InputModes.HOLD; // TODO: Update from KeybindManager
    private static bool m_crouchToggle = false;
    private InputModes m_proneInputMode = InputModes.TOGGLE; // TODO: Update from KeybindManager
    private static bool m_proneToggle = false;

    private PlayerInputs m_currentInput;
    private uint m_requestRef = 0;

    #endregion


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
        m_currentInput = SamplePlayerInputs ();

        // Send sampled inputs to the server
        SendInputToServer ( m_currentInput );
        
        // Send sampled inputs to the movement controller
        SendInputsToMovementController ( m_currentInput );
    }

    /// <summary>
    /// Samples the current inputs and returns a PlayerInputs struct.
    /// </summary>
    /// <returns>PlayerInput struct of the sampled inputs at this given frame.</returns>
    private PlayerInputs SamplePlayerInputs ()
    {
        m_requestRef++;
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
            request = m_requestRef,
            lerp_msec = lerpMilliseconds,
            msec = milliseconds,
            inputs = _inputs,
            rot = rotation
        };
        return playerInput;
    }

    private void SendInputToServer ( PlayerInputs inputs )
    {
        byte [] inputByteArr = GetBytes ( inputs );
        ClientSend.PlayerInput ( inputByteArr );
    }

    private void SendInputsToMovementController ( PlayerInputs inputs )
    {
        // Movement / Run / Jump / Crouch / Prone
        Vector2 inputDirection = Vector2.zero;
        if ( inputs.inputs [ 0 ] )
        {
            inputDirection.y += 1;
        }
        if ( inputs.inputs [ 1 ] )
        {
            inputDirection.y -= 1;
        }
        if ( inputs.inputs [ 2 ] )
        {
            inputDirection.x -= 1;
        }
        if ( inputs.inputs [ 3 ] )
        {
            inputDirection.x += 1;
        }
        bool runInput = inputs.inputs [ 4 ];
        bool jumpInput = inputs.inputs [ 5 ];
        bool crouchInput = inputs.inputs [ 6 ];
        bool proneInput = inputs.inputs [ 7 ];
        m_playerMovementController.Movement ( inputDirection, runInput, jumpInput, crouchInput, proneInput );
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