using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelController : MonoBehaviour
{
    private const int WEAPONS_LAYER_INDEX = 1;

    [SerializeField]
    private GameObject m_model = null;
    [SerializeField]
    private Animator m_animator = null;

    [SerializeField]
    private float m_movementTransitionSpeed = 2.0f;
    private Vector2 m_targetMovementDirection = Vector2.zero;
    private float m_movementSpeed = 0f;
    private bool m_running = false;
    private bool m_crouching = false;
    private bool m_prone = false;

    // Start is called before the first frame update
    void Start ()
    {
        Debug.Assert ( m_model != null, "Player model is null." );
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
            float currentHorizontalMovement = m_animator.GetFloat ( "HorizontalMovement" );
            float currentVerticalMovement = m_animator.GetFloat ( "VerticalMovement" );
            float smoothHorizontalMovement = Mathf.Lerp ( currentHorizontalMovement, m_targetMovementDirection.x, fixedDeltaTime * m_movementTransitionSpeed );
            float smoothVerticalMovement = Mathf.Lerp ( currentVerticalMovement, m_targetMovementDirection.y, fixedDeltaTime * m_movementTransitionSpeed );
            m_animator.SetFloat ( "HorizontalMovement", smoothHorizontalMovement );
            m_animator.SetFloat ( "VerticalMovement", smoothVerticalMovement );

            // Running
            m_animator.SetBool ( "IsRunning", m_running );

            // Crouching
            m_animator.SetBool ( "IsCrouching", m_crouching );
            
            // Crouching
            m_animator.SetBool ( "IsProning", m_prone );
        }
    }

    public void ShowModel ( bool activeState )
    {
        m_model.SetActive ( activeState );
    }

    public void SetMovementVector ( Vector2 movementVector )
    {
        m_targetMovementDirection = movementVector;
        m_movementSpeed = movementVector.magnitude;
    }

    public void SetRun ( bool value )
    {
        m_running = value;
        int weaponsLayerWeight = value ? 0 : 1;
        m_animator.SetLayerWeight ( WEAPONS_LAYER_INDEX, weaponsLayerWeight );
    }

    public void SetCrouch ( bool value )
    {
        m_crouching = value;
    }

    public void SetProne ( bool value )
    {
        m_prone = value;
    }
}
