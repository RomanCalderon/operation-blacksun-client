using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBobber : MonoBehaviour
{
    private const float TRANSITION_SPEED = 6f;
    private const float PLAYER_SPEED_MULTIPLIER = 2f;
    private const float BOB_AMOUNT_MULTIPLIER = 0.008f;

    [SerializeField]
    private PlayerMovementController m_playerMovementController = null;

    // Local position where your camera would rest when it's not bobbing
    private Vector3 m_restPosition;
    private float m_time = 0f;
    private Vector3 m_camPos;


    void Awake ()
    {
        m_camPos = transform.localPosition;
    }

    void Update ()
    {
        float deltaTime = Time.deltaTime;
        float playerSpeed = m_playerMovementController.Velocity.magnitude * PLAYER_SPEED_MULTIPLIER;
        bool grounded = m_playerMovementController.IsGrounded;

        if ( playerSpeed > 0f && grounded ) // Moving
        {
            m_time += playerSpeed * deltaTime;

            // Use the timer value to set the position
            float bobAmount = Mathf.Sqrt ( playerSpeed ) * BOB_AMOUNT_MULTIPLIER;
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
}
