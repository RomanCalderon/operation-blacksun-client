using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerInput
{
    public struct Frame
    {
        // Frame timestamp
        public float timestamp;
        // Interpolation time on client
        public short lerp_msec;
        // Duration in ms of command
        public float deltaTime;
        // Position of player
        public Vector3 position;
        // Delta position
        public Vector3 deltaPosition;
        // Velocity this frame
        public Vector3 velocity;
        // Player movement inputs
        [MarshalAs ( UnmanagedType.ByValArray, SizeConst = Constants.NUM_PLAYER_INPUTS )]
        public bool [] inputs;
        // Player rotation
        public Quaternion rot;

        #region Constructors

        public Frame (
            float timestamp,
            short lerp_msec,
            float deltaTime,
            Vector3 position,
            Vector3 deltaPosition,
            Vector3 velocity,
            bool [] inputs,
            Quaternion rot
        )
        {
            this.timestamp = timestamp;
            this.lerp_msec = lerp_msec;
            this.deltaTime = deltaTime;
            this.position = position;
            this.deltaPosition = deltaPosition;
            this.velocity = velocity;
            this.inputs = inputs;
            this.rot = rot;
        }

        public Frame ( Frame f )
        {
            timestamp = f.timestamp;
            lerp_msec = f.lerp_msec;
            deltaTime = f.deltaTime;
            position = f.position;
            deltaPosition = f.deltaPosition;
            velocity = f.velocity;
            inputs = f.inputs;
            rot = f.rot;
        }

        #endregion
    }

    /// <summary>
    /// Controlls player input and handles sending these sampled inputs
    /// to the server and the local movement controller.
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        #region Models

        public enum InputModes
        {
            TOGGLE,
            HOLD
        }

        #endregion

        #region Members

        private PlayerMovementController m_playerMovementController = null;

        public static bool CrouchInput { get; private set; } = false;
        public static bool ProneInput { get; private set; } = false;

        private InputModes m_crouchInputMode = InputModes.HOLD; // TODO: Update from KeybindManager
        private static bool m_crouchToggle = false;
        private InputModes m_proneInputMode = InputModes.TOGGLE; // TODO: Update from KeybindManager
        private static bool m_proneToggle = false;

        private Vector3 m_previousPosition; // Used for calculating position delta

        #endregion


        private void Awake ()
        {
            m_playerMovementController = GetComponent<PlayerMovementController> ();

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
                    CrouchInput = Input.GetKey ( KeyCode.LeftControl ); // TODO: Switch to KeybindManager as input
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
                    if ( Input.GetKeyDown ( KeyCode.Z ) ) // TODO: Switch to KeybindManager as input
                    {
                        m_proneToggle = !m_proneToggle;
                        ProneInput = m_proneToggle;
                    }
                    break;
                case InputModes.HOLD:
                    ProneInput = Input.GetKey ( KeyCode.Z ); // TODO: Switch to KeybindManager as input
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Samples the current inputs and returns a PlayerInputs struct.
        /// </summary>
        /// <returns>PlayerInput struct of the sampled inputs at this given frame.</returns>
        public Frame SamplePlayerInputs ( float deltaTime )
        {
            float timestamp = Time.time;
            short lerpMilliseconds = ( short ) ( Time.deltaTime * 1000 );
            Vector3 position = transform.position;
            Vector3 deltaPosition = position - m_previousPosition;
            m_previousPosition = transform.position;
            Vector3 velocity = m_playerMovementController.Velocity;
            bool [] inputs = new bool [ Constants.NUM_PLAYER_INPUTS ]
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

            return new Frame ( timestamp, lerpMilliseconds, deltaTime, position, deltaPosition, velocity, inputs, rotation );
        }
    }
}

