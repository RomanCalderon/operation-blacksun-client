using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionView : MonoBehaviour
{
    #region Models

    public enum InteractionStates
    {
        NONE,
        HOVER,
        INTERACTING
    }

    #endregion

    #region Members

    private const string INACCESSIBLE_MESSAGE = "LOCKED";
    private const string REVIVE_INTERACTION_PREFIX = "REVIVE";
    private const float INTERACT_TIME_THRESHOLD = 0.01f;

    [SerializeField]
    private GameObject m_interactionViewUI = null;
    [SerializeField, Tooltip ( "Text which indiates how to interact." )]
    private Text m_intetractableText = null;
    [SerializeField, Tooltip ( "Text which indicates the target interaction." )]
    private Text m_interactionLabel = null;
    [SerializeField, Tooltip ( "Visual indicator for interaction progress." )]
    private Image m_interactionStatusBar = null;

    private InteractionStates m_state = InteractionStates.NONE;
    private IInteractable m_target = null;
    private string m_interactKeybind = null;
    private string m_interactionLabelColor = null;

    // Runtime
    private float m_interactionTime = 0f;
    private float m_interactionProgress = 0f;

    #endregion

    // Start is called before the first frame update
    private void Start ()
    {
        ClearView ();
    }

    public void SetTarget ( IInteractable newTarget, string interactKeybind )
    {
        m_target = newTarget;
        m_interactKeybind = interactKeybind;
    }

    #region State Management

    public void UpdateInteractionState ( InteractionStates state )
    {
        if ( m_target == null || !m_target.IsInteractable )
        {
            m_state = InteractionStates.NONE;
            return;
        }

        m_state = state;

        switch ( m_state )
        {
            case InteractionStates.NONE:
                ClearView ();
                break;
            case InteractionStates.HOVER:
                ClearView ();
                DisplayHoverState ();
                break;
            case InteractionStates.INTERACTING:
                DisplayStartInteractionState ();
                break;
            default:
                ClearView ();
                break;
        }
    }

    private void DisplayHoverState ()
    {
        if ( m_target == null || !m_target.IsInteractable )
        {
            return;
        }

        // Show entire interaction UI
        m_interactionViewUI.SetActive ( true );

        if ( m_target.HasAccessKey )
        {
            UpdateInteractionLabel ();

            m_intetractableText.text = ( m_target.InteractTime < INTERACT_TIME_THRESHOLD ? "PRESS " : "HOLD " ) + m_interactKeybind;
            m_intetractableText.enabled = true;
        }
        else
        {
            m_intetractableText.text = string.Empty;
            m_interactionLabel.text = INACCESSIBLE_MESSAGE;

            m_intetractableText.enabled = false;
            m_interactionLabel.enabled = true;
        }
    }

    public void DisplayStartInteractionState ()
    {
        if ( m_target == null || !m_target.IsInteractable )
        {
            return;
        }

        m_interactionStatusBar.enabled = true;
        m_interactionTime = m_target.InteractTime;
        m_interactionProgress = 0f;
    }

    private void ClearView ()
    {
        // Hide entire interaction UI
        m_interactionViewUI.SetActive ( false );

        // Reset status bar and progress
        m_interactionStatusBar.enabled = false;
        m_interactionProgress = 0f;
    }

    private void UpdateInteractionLabel ()
    {
        switch ( m_target.InteractionType )
        {
            case 0: // Standard interaction
                if ( m_target is PickupInstance pickup )
                {
                    string labelColor = Constants.RarityColorToHex ( pickup.PlayerItem.Rarity );
                    string label = $"<color=#{labelColor}>{m_target.InteractionLabel}</color>";
                    m_interactionLabel.text = label.ToUpper ();
                }
                else
                {
                    m_interactionLabel.text = $"<color=#b94463>{m_target.InteractionLabel}</color>";
                }
                break;
            case 1: // Revive interaction
                m_interactionLabel.text = $"{REVIVE_INTERACTION_PREFIX} - {m_target.InteractionLabel}";
                break;
            default:
                break;
        }

        m_interactionLabel.enabled = true;
    }

    #endregion

    #region Runtime

    private void Update ()
    {
        UpdateInteractionState ();
    }

    private void UpdateInteractionState ()
    {
        if ( m_state == InteractionStates.INTERACTING && m_target != null )
        {
            if ( m_interactionTime < INTERACT_TIME_THRESHOLD || m_interactionProgress >= m_interactionTime )
            {
                UpdateInteractionState ( InteractionStates.NONE );
                return;
            }

            float deltaTime = Time.deltaTime;
            m_interactionProgress += deltaTime;
            m_interactionStatusBar.fillAmount = m_interactionProgress / m_interactionTime;
        }
    }

    #endregion
}
