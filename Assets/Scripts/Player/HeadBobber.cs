using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBobber : MonoBehaviour
{
    private const float TRANSITION_SPEED = 6f;
    private const float AIM_BOB_MULTIPLIER = 0.1f;
    private const float BOB_REDUCTION = 100f;

    [SerializeField]
    private PlayerMovementController m_playerMovementController = null;

    [Header ( "Bob Amount" )]
    [SerializeField]
    private float m_bobAmount = 0.05f;
    [SerializeField, Range ( 0f, 1f )]
    private float m_playerSpeedBobInfluence = 0f;
    [Header ( "Bob Speed" )]
    [SerializeField]
    private float m_speedMultiplier = 8f;
    [SerializeField, Range ( 0f, 1f )]
    private float m_playerSpeedInfluence = 0f;

    // Local position where your camera would rest when it's not bobbing
    private Vector3 m_restPosition;
    private float m_time = 0f;
    private float m_aimingModifier = 1f;
    private Vector3 m_camPos;


    private void OnEnable ()
    {
        AimController.OnAimStateUpdated += AimUpdated;
    }

    private void OnDisable ()
    {
        AimController.OnAimStateUpdated -= AimUpdated;
    }

    void Awake ()
    {
        m_camPos = transform.localPosition;
    }

    void Update ()
    {
        float deltaTime = Time.deltaTime;
        float playerSpeed = m_playerMovementController.Velocity.magnitude;
        bool playerSliding = m_playerMovementController.IsSliding;
        bool grounded = m_playerMovementController.IsGrounded;

        if ( playerSpeed > 0f && grounded ) // Moving
        {
            m_time += deltaTime * ( m_speedMultiplier + m_playerSpeedInfluence * playerSpeed );

            // Use the timer value to set the position
            float playerSlideMultiplier = ( playerSliding ? 0.1f : 1f );
            float bobAmount = ( m_bobAmount + m_playerSpeedBobInfluence * playerSpeed * playerSpeed ) / BOB_REDUCTION * m_aimingModifier * playerSlideMultiplier;
            float deltaX = Mathf.Cos ( m_time ) * bobAmount / 4f;
            float deltaY = m_restPosition.y + Mathf.Abs ( Mathf.Sin ( m_time ) * bobAmount ) - ( bobAmount / 1.5f );
            Vector3 newPosition = new Vector3 ( deltaX, deltaY, m_restPosition.z );
            m_camPos = Vector3.Lerp ( m_camPos, newPosition, deltaTime * TRANSITION_SPEED );
        }
        else
        {
            // Reinitialize
            m_time = Mathf.PI / 2;

            // Transition smoothly from walking to stopping
            Vector3 newPosition = Vector3.Lerp ( m_camPos, m_restPosition, deltaTime * TRANSITION_SPEED );
            m_camPos = newPosition;
        }

        // Set transform to cam position
        transform.localPosition = m_camPos;

        // Completed a full cycle on the unit circle
        // Reset to 0 to avoid bloated values
        if ( m_time > Mathf.PI * 2 )
        {
            m_time = 0;
        }
    }

    private void AimUpdated ( bool aimState )
    {
        m_aimingModifier = aimState ? AIM_BOB_MULTIPLIER : 1f;
    }
}
