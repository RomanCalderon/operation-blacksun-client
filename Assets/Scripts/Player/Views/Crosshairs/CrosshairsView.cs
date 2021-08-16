using UnityEngine;

public class CrosshairsView : MonoBehaviour
{
    private RectTransform m_crosshairRect = null;

    [Header ( "Components" )]
    [SerializeField]
    private CanvasGroup m_linesCanvasGroup;
    [SerializeField]
    private RectTransform m_topLine;
    [SerializeField]
    private RectTransform m_bottomLine;
    [SerializeField]
    private RectTransform m_leftLine;
    [SerializeField]
    private RectTransform m_rightLine;
    [SerializeField]
    private RectTransform m_center;

    [Space]
    [Header ( "Properties" )]
    [SerializeField]
    private float m_restingSize = 150f;
    [SerializeField]
    private float m_maxSize = 300f;
    [SerializeField]
    private float m_sizeLerpRate = 8f;
    [SerializeField]
    private float m_alphaLerpRate = 16f;

    private float m_currentSize, m_targetSize;
    private float m_currentLinesAlpha, m_targetLinesAlpha;

    private void Awake ()
    {
        m_crosshairRect = GetComponent<RectTransform> ();
    }

    private void Start ()
    {
        m_crosshairRect.sizeDelta = Vector2.one * m_maxSize;
        m_targetSize = m_restingSize;
    }

    private void Update ()
    {
        float deltaTime = Time.deltaTime;

        // Lerp currentSize to the target value
        m_currentSize = Mathf.Lerp ( m_currentSize, m_targetSize, deltaTime * m_sizeLerpRate );

        // Set the crosshair's size to the currentSize value
        m_crosshairRect.sizeDelta = Vector2.one * m_currentSize;

        // Lerp currentLinesAlpha to the target value
        m_currentLinesAlpha = Mathf.Lerp ( m_currentLinesAlpha, m_targetLinesAlpha, deltaTime * m_alphaLerpRate );

        // Set the line's alpha value to the currentLinesAlpha value
        m_linesCanvasGroup.alpha = m_currentLinesAlpha;
    }

    /// <summary>
    /// Sets the crosshair's overall size by <paramref name="value"/>, 
    /// where 0 is resting and 1 is max size.
    /// </summary>
    /// <param name="value">Crosshair size value.</param>
    public void SetSize ( float size )
    {
        m_targetSize = Mathf.Lerp ( m_restingSize, m_maxSize, size );
    }

    /// <summary>
    /// Sets the crosshair's center <paramref name="state"/>.
    /// </summary>
    /// <param name="state">The active state of the center crosshair element.</param>
    public void SetCenter ( bool state )
    {
        m_center.gameObject.SetActive ( state );
    }

    /// <summary>
    /// Sets the crosshair's lines <paramref name="state"/>.
    /// </summary>
    /// <param name="state">The active state of the crosshair element.</param>
    public void SetLines ( bool state )
    {
        m_topLine.gameObject.SetActive ( state );
        m_bottomLine.gameObject.SetActive ( state );
        m_leftLine.gameObject.SetActive ( state );
        m_rightLine.gameObject.SetActive ( state );
    }

    /// <summary>
    /// Sets the crosshair's lines alpha value to <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value of the target alpha state.</param>
    public void SetLinesAlpha ( float value )
    {
        m_targetLinesAlpha = Mathf.Clamp01 ( value );
    }

    /// <summary>
    /// Sets the crosshair's lines <paramref name="width"/> and <paramref name="length"/>.
    /// </summary>
    /// <param name="width">Width of the lines.</param>
    /// <param name="length">Length on the lines.</param>
    public void SetLinesScale ( float width, float height )
    {
        Vector2 newSize = new Vector2 ( width, height );
        m_topLine.sizeDelta = newSize;
        m_bottomLine.sizeDelta = newSize;
        m_leftLine.sizeDelta = newSize;
        m_rightLine.sizeDelta = newSize;
    }
}
