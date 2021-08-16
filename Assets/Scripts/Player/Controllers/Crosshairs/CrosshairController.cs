using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    private const float ALPHA_LERP_RATE = 16f;

    [SerializeField]
    private bool m_enabled = true;
    [SerializeField]
    private CrosshairsView m_activeCrosshair = null;

    [SerializeField]
    private CanvasGroup m_canvasGroup = null;

    private float m_targetAlpha = 1f;
    private float m_currentAlpha;

    // Start is called before the first frame update
    void Start ()
    {
        m_currentAlpha = m_targetAlpha;
    }

    #region Runtime

    private void Update ()
    {
        // Return if not enabled
        if ( !m_enabled ) return;

        m_currentAlpha = Mathf.Lerp ( m_currentAlpha, m_targetAlpha, Time.deltaTime * ALPHA_LERP_RATE );
        m_canvasGroup.alpha = m_currentAlpha;
    }

    #endregion

    #region Interface

    public void EnableCrosshairs ( bool state )
    {
        m_enabled = state;

        if ( m_enabled )
        {
            m_canvasGroup.alpha = m_targetAlpha = 1f;
        }
        else
        {
            m_canvasGroup.alpha = m_targetAlpha = 0f;
        }
    }

    /// <summary>
    /// Sets the crosshair's overall alpha <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value of the target alpha state.</param>
    public void SetAlpha ( float value )
    {
        m_targetAlpha = value;
    }

    /// <summary>
    /// Sets the crosshair's overall size by <paramref name="value"/>, 
    /// where 0 is resting and 1 is max size.
    /// </summary>
    /// <param name="value">Crosshair size value.</param>
    public void SetSize ( float value )
    {
        // Return if active crosshair is null
        if ( m_activeCrosshair == null ) return;

        // Set active crosshair size
        m_activeCrosshair.SetSize ( Mathf.Clamp01 ( value ) );
    }

    /// <summary>
    /// Sets the crosshair's center <paramref name="state"/>.
    /// </summary>
    /// <param name="state">The active state of the crosshair element</param>
    public void SetCenter ( bool state )
    {
        m_activeCrosshair.SetCenter ( state );
    }

    /// <summary>
    /// Sets the crosshair's lines <paramref name="state"/>.
    /// </summary>
    /// <param name="state">The active state of the crosshair element.</param>
    public void SetLines ( bool state )
    {
        m_activeCrosshair.SetLines ( state );
    }

    /// <summary>
    /// Sets the crosshair's lines alpha value to <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value of the target alpha state.</param>
    public void SetLinesAlpha ( float value )
    {
        m_activeCrosshair.SetLinesAlpha ( value );
    }

    /// <summary>
    /// Sets the crosshair's lines <paramref name="width"/> and <paramref name="length"/>.
    /// </summary>
    /// <param name="width">Width of the lines</param>
    /// <param name="length">Length on the lines</param>
    public void SetLinesScale ( float width, float length )
    {
        m_activeCrosshair.SetLinesScale ( width, length );
    }

    #endregion
}
