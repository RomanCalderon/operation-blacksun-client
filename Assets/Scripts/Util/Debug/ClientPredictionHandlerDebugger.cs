using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientPredictionHandlerDebugger : MonoBehaviour
{
    [Header ( "CPH Reference" )]
    [SerializeField]
    private ClientPredictionHandler m_clientPredictionHalder = null;

    [Header ( "UI" )]
    [SerializeField]
    private InputField m_correctionToleranceInputField = null;
    [SerializeField]
    private InputField m_correctionLerpRateInputField = null;


    private void Start ()
    {
        // Init input field values
        m_correctionToleranceInputField.text = m_clientPredictionHalder.CorrectionTolerance.ToString ();
        m_correctionLerpRateInputField.text = m_clientPredictionHalder.CorrectionLerpRate.ToString ();
    }

    public void UpdateValues ()
    {
        if ( float.TryParse ( m_correctionToleranceInputField.text, out float correctionTolerance ) )
        {
            m_clientPredictionHalder.CorrectionTolerance = correctionTolerance;
        }
        if ( float.TryParse ( m_correctionLerpRateInputField.text, out float correctionLerpRate ) )
        {
            m_clientPredictionHalder.CorrectionLerpRate = correctionLerpRate;
        }
    }
}
