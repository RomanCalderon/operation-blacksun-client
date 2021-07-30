using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerModelController : MonoBehaviour
{
    #region Constants

    private const int WEAPONS_LAYER_INDEX = 1;
    private const float CHEST_CONSTRAINT_OFFSET_INTERP_MULTIPLIER = 38f;

    // Animator Controller parameter references
    private const string ANIMATOR_HORIZONTAL_MOVEMENT = "HorizontalMovement";
    private const string ANIMATOR_VERTICAL_MOVEMENT = "VerticalMovement";
    private const string ANIMATOR_MOVEMENT_SPEED = "MovementSpeed";
    private const string ANIMATOR_IS_RUNNING = "IsRunning";
    private const string ANIMATOR_IS_CROUCHING = "IsCrouching";
    private const string ANIMATOR_TOGGLE_CROUCH = "ToggleCrouch";
    private const string ANIMATOR_IS_GROUNDED = "IsGrounded";
    private const string ANIMATOR_JUMP = "Jump";

    #endregion

    #region Members

    [ SerializeField]
    private GameObject m_model = null;
    [SerializeField]
    private Animator m_animator = null;

    [SerializeField]
    private float m_minRunSpeed = 4.1f;
    [SerializeField]
    private float m_movementTransitionSpeed = 2.0f;
    [SerializeField, Tooltip ( "Verticle crouch displacement amount" )]
    private float m_crouchTargetHeight = 0.5f;
    [SerializeField, Tooltip ( "Model's verticle displacement when crouching" )]
    private float m_modelCrouchOffset = 0.1f;
    [SerializeField, Tooltip ( "Chest constraint crouch offset amount in degrees" )]
    private Vector3 m_chestConstraintCrouchOffset = Vector3.zero;
    [SerializeField,
        Tooltip ( "Model-related components' crouch duration (sec). This value should" +
        " match the time it take for the model be be in the crouch state" ),
        Range ( 0.01f, 3.0f )]
    private float m_crouchInterpDuration = 0.1f;
    [SerializeField, Tooltip ( "Chest IK Target Container rotation interpolation speed" )]
    private float m_chestIKRotationInterpSpeed = 16f;
    [SerializeField, Tooltip ( "Maximum animation movement speed" )]
    private float m_maxMoveSpeed = 10f;
    [SerializeField, Tooltip ( "In-air movement speed" )]
    private float m_jumpMovementSpeed = 1f;

    private Vector2 m_currentMovementDirection = Vector2.zero;
    private Vector2 m_targetMovementDirection = Vector2.zero;
    private float m_movementSpeed = 0f;
    private bool m_running = false;
    private bool m_crouchState = false;
    private bool m_prevCrouchState = false;
    private bool m_jumpState = false;
    private bool m_prevJumpState = false;
    private bool m_isGrounded = false;
    private Vector3 m_modelCrouchOffset_Default;
    private Vector3 m_modelOffsetTarget;

    [Header ( "IK" )]
    [SerializeField]
    private MultiAimConstraint m_chestConstraint = null;
    [SerializeField]
    private Transform m_chestIKTargetContainer = null;

    private Vector3 m_chestConstraintOffset_Default;
    private Vector3 m_chestConstraintOffset_Target;
    private Vector3 m_chestIKTargetContainer_Offset;
    private Vector3 m_chestIKTargetContainer_TargetPos;
    private Quaternion m_chestIKTargetContainer_TargetRot;

    #endregion

    #region Initialization

    // Start is called before the first frame update
    void Start ()
    {
        Debug.Assert ( m_model != null, "Player model is null." );

        m_prevCrouchState = m_crouchState;
        m_prevJumpState = m_jumpState;
        m_modelOffsetTarget = m_modelCrouchOffset_Default = m_model.transform.localPosition;
        m_chestConstraintOffset_Target = m_chestConstraintOffset_Default = m_chestConstraint.data.offset;
        m_chestIKTargetContainer_TargetPos = m_chestIKTargetContainer_Offset = m_chestIKTargetContainer.localPosition;
    }

    #endregion

    #region Runtime

    private void Update ()
    {
        float deltaTime = Time.deltaTime;

        // Handles model translations between standing and crouching
        m_model.transform.localPosition = Vector3.MoveTowards (
            m_model.transform.localPosition,
            m_modelOffsetTarget,
            deltaTime / m_crouchInterpDuration );

        // Handles chest constraint offset interpolation
        m_chestConstraint.data.offset = Vector3.MoveTowards (
            m_chestConstraint.data.offset,
            m_chestConstraintOffset_Target,
            deltaTime / m_crouchInterpDuration * CHEST_CONSTRAINT_OFFSET_INTERP_MULTIPLIER );

        // Handles chest IK target container position interpolation
        m_chestIKTargetContainer.localPosition = Vector3.MoveTowards (
            m_chestIKTargetContainer.localPosition,
            m_chestIKTargetContainer_TargetPos,
            deltaTime / m_crouchInterpDuration );

        // Handles chest IK target container rotation interpolation
        m_chestIKTargetContainer.localRotation = Quaternion.Slerp (
            m_chestIKTargetContainer.localRotation,
            m_chestIKTargetContainer_TargetRot,
            deltaTime * m_chestIKRotationInterpSpeed );
    }

    private void FixedUpdate ()
    {
        UpdateAnimator ();
    }

    private void UpdateAnimator ()
    {
        if ( m_animator != null )
        {
            float fixedDeltaTime = Time.fixedDeltaTime;

            // X/Z movement
            m_currentMovementDirection = Vector3.Lerp ( m_currentMovementDirection, m_targetMovementDirection, fixedDeltaTime * m_movementTransitionSpeed );
            m_animator.SetFloat ( ANIMATOR_HORIZONTAL_MOVEMENT, m_currentMovementDirection.x );
            m_animator.SetFloat ( ANIMATOR_VERTICAL_MOVEMENT, m_currentMovementDirection.y );
            m_animator.SetFloat ( ANIMATOR_MOVEMENT_SPEED, m_movementSpeed );

            // Running
            m_animator.SetBool ( ANIMATOR_IS_RUNNING, m_running && m_movementSpeed >= m_minRunSpeed );

            // Crouching
            m_animator.SetBool ( ANIMATOR_IS_CROUCHING, m_crouchState );
            // Trigger ToggleCrouch animator param on new crouch state
            if ( m_prevCrouchState != m_crouchState )
            {
                m_animator.SetTrigger ( ANIMATOR_TOGGLE_CROUCH );
                m_prevCrouchState = m_crouchState;
            }

            // Jumping
            m_animator.SetBool ( ANIMATOR_IS_GROUNDED, m_isGrounded );
            // Trigger Jump animator param on jump
            if ( m_prevJumpState != m_jumpState )
            {
                m_prevJumpState = m_jumpState;
                if ( m_jumpState )
                {
                    m_animator.SetTrigger ( ANIMATOR_JUMP );
                }
            }
        }
    }

    #endregion

    #region Setters

    public void ShowModel ( bool activeState )
    {
        m_model.SetActive ( activeState );
    }

    public void SetAnimatorParams ( int moveInputX, int moveInputY, float moveSpeed, float cameraPitch )
    {
        m_targetMovementDirection = new Vector2 ( moveInputX, moveInputY );
        // Clamp movement speed param to [1,maxMovementSpeed] to maintain normalized speed animation states
        float maxMovementSpeed = m_isGrounded ? m_maxMoveSpeed : m_jumpMovementSpeed;
        m_movementSpeed = Mathf.Clamp ( moveSpeed, 1f, maxMovementSpeed );
        m_chestIKTargetContainer_TargetRot = Quaternion.Euler ( Vector3.right * cameraPitch );
    }

    public void SetRun ( bool value )
    {
        m_running = value;
        int weaponsLayerWeight = value ? 0 : 1;
        m_animator.SetLayerWeight ( WEAPONS_LAYER_INDEX, weaponsLayerWeight );
    }

    public void SetCrouch ( bool value )
    {
        m_crouchState = value;

        // Set target model offset
        m_modelOffsetTarget = value ? m_modelCrouchOffset_Default + Vector3.up * m_modelCrouchOffset : m_modelCrouchOffset_Default;

        // Set target chest constraint offset
        m_chestConstraintOffset_Target = value ? m_chestConstraintOffset_Default + m_chestConstraintCrouchOffset : m_chestConstraintOffset_Default;

        // Set target height for chest IK target container
        m_chestIKTargetContainer_TargetPos = value ? m_chestIKTargetContainer_Offset + ( m_crouchTargetHeight * Vector3.down ) : m_chestIKTargetContainer_Offset;
    }


    public void SetJump ( bool jumpInput, bool isGrounded )
    {
        m_isGrounded = isGrounded;
        m_jumpState = jumpInput;
    }

    #endregion

    #region Debug

    private void OnDrawGizmos ()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere ( m_chestIKTargetContainer_TargetPos, 0.1f );
    }

    #endregion
}
