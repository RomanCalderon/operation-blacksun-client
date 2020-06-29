using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelController : MonoBehaviour
{
    [SerializeField]
    private Animator m_animator = null;
    private GameObject m_model = null;
    [SerializeField]
    private float m_movementTransitionSpeed = 2.0f;
    private Vector2 m_targetMovementDirection = Vector2.zero;

    // Start is called before the first frame update
    void Start ()
    {
        m_model = m_animator.gameObject;
    }

    // Update is called once per frame
    void Update ()
    {

    }

    private void FixedUpdate ()
    {
        float currentHorizontalMovement = m_animator.GetFloat ( "HorizontalMovement" );
        float currentVerticalMovement = m_animator.GetFloat ( "VerticalMovement" );
        float smoothHorizontalMovement = Mathf.Lerp ( currentHorizontalMovement, m_targetMovementDirection.x, Time.deltaTime * m_movementTransitionSpeed );
        float smoothVerticalMovement = Mathf.Lerp ( currentVerticalMovement, m_targetMovementDirection.y, Time.deltaTime * m_movementTransitionSpeed );
        m_animator.SetFloat ( "HorizontalMovement", smoothHorizontalMovement );
        m_animator.SetFloat ( "VerticalMovement", smoothVerticalMovement );
    }

    public void HideModel ( bool activeState )
    {
        m_model.SetActive ( activeState );
    }

    public void SetMovementVector ( Vector2 movement )
    {
        m_targetMovementDirection = movement;
    }
}
