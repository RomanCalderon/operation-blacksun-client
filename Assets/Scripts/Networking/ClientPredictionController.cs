using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using PlayerInput;

[RequireComponent ( typeof ( PlayerInputController ) )]
[RequireComponent ( typeof ( PlayerMovementController ) )]
public class ClientPredictionController : MonoBehaviour
{
    private PlayerInputController m_playerInputController = null;
    private PlayerMovementController m_playerMovementController = null;

    private static PlayerInputBuffer InputBuffer = null;
    private Frame m_currentInput;
    private Frame m_serverState;
    private Frame m_predictedState;
    private float m_latency;
    private float m_historyDuration;
    private Vector3 m_targetPosition;

    // DEBUG
    [SerializeField]
    private Text m_debugText = null;


    private void Awake ()
    {
        m_playerInputController = GetComponent<PlayerInputController> ();
        m_playerMovementController = GetComponent<PlayerMovementController> ();

        Initialize ();
    }


    private void Initialize ()
    {
        InputBuffer = new PlayerInputBuffer ();

        m_latency = 0.1f; // Init value for position calculation
    }

    // Update is called once per frame
    void Update ()
    {
        // DEBUG
        m_debugText.text = "input_buffer_size: " + InputBuffer.Size.ToString ();

        // Get input
        float deltaTime = Time.deltaTime;
        m_currentInput = m_playerInputController.SamplePlayerInputs ( deltaTime );

        // Send input to the server
        SendInputToServer ( m_currentInput );

        UpdateFrame ( deltaTime, m_currentInput );
    }

    private void UpdateFrame ( float deltaTime, Frame input )
    {
        // Run player controller to get new prediction
        Frame newState = m_playerMovementController.Movement ( m_predictedState, deltaTime, input.inputs );
        // Add input to input buffer
        InputBuffer.Add ( m_playerInputController.SamplePlayerInputs ( deltaTime ) );

        // Extrapolate predicted position
        Vector3 extrapolatedPosition = m_predictedState.position + newState.velocity * m_latency * Constants.PLAYER_CONVERGE_MULTIPLIER;

        // Interpolate client position towards extrapolated position
        float t = deltaTime / ( m_latency * Constants.PLAYER_CONVERGE_MULTIPLIER );
        m_historyDuration += deltaTime;

        transform.position += ( extrapolatedPosition - transform.position ) * t;
    }

    private void SendInputToServer ( Frame inputs )
    {
        byte [] inputByteArr = FrameToBytes ( inputs );
        ClientSend.PlayerInput ( inputByteArr );
    }

    /// <summary>
    /// Called when client receives a player state update from the server.
    /// </summary>
    public void OnServerFrame ( byte [] processedRequest )
    {
        //Debug.Log ( "OnServerFrame()" );

        // Get server frame
        Frame serverFrame = BytesToFrame ( processedRequest );
        m_latency = Time.time - serverFrame.timestamp;
        m_historyDuration = InputBuffer.BufferDuration;

        // Remove frames from input buffer until its duration is equal to the latency
        float dt = Mathf.Max ( 0, m_historyDuration - m_latency );
        m_historyDuration -= dt;

        while ( InputBuffer.Size > 0 && dt > 0 )
        {
            if ( dt >= InputBuffer.Frames [ 0 ].deltaTime )
            {
                dt -= InputBuffer.Frames [ 0 ].deltaTime;
                InputBuffer.RemoveAt ( 0 );
            }
            else
            {
                float t = 1 - dt / InputBuffer.Frames [ 0 ].deltaTime;
                InputBuffer.Frames [ 0 ].deltaTime -= dt;
                InputBuffer.Frames [ 0 ].deltaPosition *= t;
                break;
            }
        }

        m_serverState = serverFrame;

        // If predicated and server velocity difference exceeds
        // the tolerance, replay inputs. This is only needed if
        // the velocity for one frame depends on the velocity
        // of the previous frame.
        if ( ( m_serverState.velocity - InputBuffer.Frames [ 0 ].velocity ).magnitude > Constants.VELOCITY_TOLERANCE )
        {
            m_predictedState = m_serverState;

            for ( int i = 0; i < InputBuffer.Size; i++ )
            {
                Debug.Log ( "Server frame movement update" );
                Frame newState = m_playerMovementController.Movement ( m_predictedState, InputBuffer.Frames [ i ].deltaTime, InputBuffer.Frames [ i ].inputs );
                InputBuffer.Frames [ i ].deltaPosition = newState.position - m_predictedState.position;
                InputBuffer.Frames [ i ].velocity = newState.velocity;
                m_predictedState = newState;
            }
            Debug.Log ( "Exceeded velocity tolerance" );
            CheckPosition ( transform.position - m_serverState.position );
        }
        else
        {
            // Add deltas from input buffer to server state to get predicted state
            m_predictedState.position = m_serverState.position;

            foreach ( Frame frame in InputBuffer.Frames )
            {
                m_predictedState.position += frame.deltaPosition;
            }
        }
    }

    #region Util

    private void CheckPosition ( Vector3 positionDelta )
    {
        if ( positionDelta.magnitude > Constants.POSITION_CORRECTION_TOLERANCE )
        {
            Debug.Log ( "Correct position!" );
            transform.position = m_predictedState.position;
        }
    }

    private byte [] FrameToBytes ( Frame str )
    {
        int size = Marshal.SizeOf ( str );
        byte [] arr = new byte [ size ];

        IntPtr ptr = Marshal.AllocHGlobal ( size );
        Marshal.StructureToPtr ( str, ptr, true );
        Marshal.Copy ( ptr, arr, 0, size );
        Marshal.FreeHGlobal ( ptr );
        return arr;
    }

    private Frame BytesToFrame ( byte [] arr )
    {
        Frame str = new Frame ();

        int size = Marshal.SizeOf ( str );
        IntPtr ptr = Marshal.AllocHGlobal ( size );

        Marshal.Copy ( arr, 0, ptr, size );

        str = ( Frame ) Marshal.PtrToStructure ( ptr, str.GetType () );
        Marshal.FreeHGlobal ( ptr );

        return str;
    }

    #endregion
}
