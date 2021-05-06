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
    private const string REVIVE_PREFIX_COLOR_HEX = "60ef74";
    private const float INTERACT_TIME_THRESHOLD = 0.01f;

    [SerializeField]
    private GameObject m_interactionViewUI = null;
    [SerializeField, Tooltip ( "Text which indiates how to interact." )]
    private Text m_intetractionLabel = null;
    [SerializeField, Tooltip ( "Text which indicates the target interaction context." )]
    private Text m_interactionContextText = null;
    [SerializeField, Tooltip ( "Image displayed for target interactable." )]
    private Image m_interactionContextImage = null;
    [SerializeField, Tooltip ( "Visual indicator for interaction progress." )]
    private Image m_interactionStatusBar = null;

    private InteractionStates m_state = InteractionStates.NONE;
    private IInteractable m_target = null;
    private string m_interactKeybind = null;

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
            UpdateInteractionContext ();

            m_intetractionLabel.text = ( m_target.InteractTime < INTERACT_TIME_THRESHOLD ? "PRESS " : "HOLD " ) + m_interactKeybind;
            m_intetractionLabel.enabled = true;
        }
        else
        {
            m_intetractionLabel.text = string.Empty;
            m_interactionContextText.text = INACCESSIBLE_MESSAGE;

            m_intetractionLabel.enabled = false;
            m_interactionContextText.enabled = true;
            m_interactionContextImage.enabled = false;
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

    private void UpdateInteractionContext ()
    {
        switch ( m_target.InteractionType )
        {
            // Standard interaction
            case 0:
                if ( m_target is PickupInstance pickup )
                {
                    Color labelColor = Constants.RarityToColor ( pickup.PlayerItem.Rarity );
                    SetInteractionContext ( m_target.InteractionContext, labelColor, pickup.PlayerItem.Image );
                }
                else
                {
                    SetInteractionContext ( m_target.InteractionContext );
                }
                break;
            // Revive interaction
            case 1:
                SetInteractionContext ( $"<color=#{REVIVE_PREFIX_COLOR_HEX}>{REVIVE_INTERACTION_PREFIX}</color> {m_target.InteractionContext}" );
                break;
            default:
                break;
        }

        m_interactionContextText.enabled = true;
    }

    private void SetInteractionContext ( string value )
    {
        if ( !string.IsNullOrEmpty ( value ) )
        {
            m_interactionContextText.color = Color.white;
            m_interactionContextText.text = value.ToUpper ();
            m_interactionContextImage.enabled = false;
        }
    }

    private void SetInteractionContext ( string value, Color color, Sprite sprite = null )
    {
        if ( !string.IsNullOrEmpty ( value ) )
        {
            m_interactionContextText.color = color;
            m_interactionContextText.text = value.ToUpper ();
        }

        m_interactionContextImage.enabled = sprite != null;
        m_interactionContextImage.sprite = sprite;
        m_interactionContextImage.color = color;
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
