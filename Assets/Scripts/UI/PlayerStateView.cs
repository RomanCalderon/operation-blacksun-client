using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateView : MonoBehaviour
{
    [Header ( "Healthbar" )]
    [SerializeField]
    private Slider m_healthbarSlider = null;
    [SerializeField]
    private CanvasGroupFader m_lowHealthIndicator = null;

    private void OnEnable ()
    {
        PlayerManager.OnHealthUpdated += UpdateHealthbar;
    }

    private void OnDisable ()
    {
        PlayerManager.OnHealthUpdated -= UpdateHealthbar;
    }

    // Start is called before the first frame update
    void Start ()
    {

    }

    private void UpdateHealthbar ( int playerId, float healthValue, float maxHealthValue )
    {
        // Update the healthbar state for this player
        if ( Client.instance.myId == playerId )
        {
            float healthPercentage = healthValue / maxHealthValue;

            m_healthbarSlider.value = healthPercentage;

            m_lowHealthIndicator.SetActive ( healthPercentage < 0.5f );
        }
    }
}
