using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerInput
{

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

        // KeyCodes
        // Movement
        // TODO: Get keybinds from KeybindManager
        public static KeyCode MoveForwardKey = KeyCode.W;
        public static KeyCode MoveBackwardKey = KeyCode.S;
        public static KeyCode MoveLeftKey = KeyCode.A;
        public static KeyCode MoveRightKey = KeyCode.D;
        public static KeyCode RunKey = KeyCode.LeftShift;
        public static KeyCode JumpKey = KeyCode.Space;
        public static KeyCode CrouchKeyToggle = KeyCode.C;
        public static KeyCode CrouchKeyHold = KeyCode.LeftControl;
        // Actions
        public static KeyCode PrimaryButton = KeyCode.Mouse0;
        public static KeyCode SecondaryButton = KeyCode.Mouse1;
        public static KeyCode ReloadKey = KeyCode.R;
        public static KeyCode InteractKey = KeyCode.E;

        // Input
        // Movement
        public static bool ForwardInput { get; private set; } = false;
        public static bool BackwardInput { get; private set; } = false;
        public static bool RightInput { get; private set; } = false;
        public static bool LeftInput { get; private set; } = false;
        public static bool JumpInput { get; private set; } = false;
        public static bool RunInput { get; private set; } = false;
        public static bool CrouchInput { get; private set; } = false;
        public static bool ShootInput { get; private set; } = false;
        public static bool Aiming { get; private set; } = false;
        public static Vector3 GunDirection { get; private set; }
        public static Vector3 LookDirection { get; private set; }
        public static bool Interact { get; private set; } = false;

        // TODO: Update from KeybindManager
        private InputModes m_crouchInputMode = InputModes.HOLD;
        private static bool m_crouchToggle = false;

        #endregion

        private void Awake ()
        {
            Initialize ();
        }

        public static void Initialize ()
        {
            ForwardInput = false;
            BackwardInput = false;
            RightInput = false;
            LeftInput = false;
            JumpInput = false;
            RunInput = false;
            CrouchInput = m_crouchToggle = false;
            ShootInput = false;
        }

        private void Update ()
        {
            // Forward / Backward / Right / Left
            ForwardInput = Input.GetKey ( MoveForwardKey );
            BackwardInput = Input.GetKey ( MoveBackwardKey );
            RightInput = Input.GetKey ( MoveRightKey );
            LeftInput = Input.GetKey ( MoveLeftKey );

            // Jump
            JumpInput = Input.GetKey ( JumpKey );

            // Run
            RunInput = Input.GetKey ( RunKey );

            // Crouch
            switch ( m_crouchInputMode )
            {
                case InputModes.TOGGLE:
                    if ( Input.GetKeyDown ( CrouchKeyToggle ) )
                    {
                        m_crouchToggle = !m_crouchToggle;
                        CrouchInput = m_crouchToggle;
                    }
                    break;
                case InputModes.HOLD:
                    CrouchInput = Input.GetKey ( CrouchKeyHold );
                    break;
                default:
                    break;
            }

            // Shoot
            ShootInput = !InventoryManager.Instance.IsDisplayed && Input.GetKey ( PrimaryButton );

            // Interact
            Interact = !InventoryManager.Instance.IsDisplayed && Input.GetKey ( InteractKey );
        }

        public static bool GetKey ( KeyCode key )
        {
            return Input.GetKey ( key );
        }

        public static bool GetKeyDown ( KeyCode key )
        {
            return Input.GetKeyDown ( key );
        }

        public static bool GetKeyUp ( KeyCode key )
        {
            return Input.GetKeyUp ( key );
        }

        public static void SetAim ( bool aimState )
        {
            Aiming = aimState;
        }

        public static void SetLookDirection ( Vector3 direction )
        {
            LookDirection = direction;
        }

        public static void SetGunDirection ( Vector3 direction )
        {
            GunDirection = direction;
        }
    }
}

