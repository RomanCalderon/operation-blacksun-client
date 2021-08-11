using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using PlayerInput;

[RequireComponent ( typeof ( PlayerMovementController ) )]
[RequireComponent ( typeof ( Rigidbody ) )]
public class ClientPredictionHandler : MonoBehaviour
{
    // State caching
    private const uint BUFFER_SIZE = 1024;
    // Minimum position snapping distance
    private const float SNAP_THRESHOLD = 4f;

    // Components
    private PlayerMovementController m_playerMovementController = null;
    private Rigidbody m_rigidbody = null;

    // Input
    public static ClientInputState InputState { get; private set; } = null;

    // Prediction/reconciliation
    private uint m_simulationFrame = 0;
    private readonly SimulationState [] m_simulationStateBuffer = new SimulationState [ BUFFER_SIZE ];
    private readonly ClientInputState [] m_clientInputBuffer = new ClientInputState [ BUFFER_SIZE ];
    private SimulationState m_serverSimulationState;
    private uint m_lastCorrectedFrame;
    private Vector3 m_clientPositionError = Vector3.zero;

    #region Initialization

    private void Awake ()
    {
        m_playerMovementController = GetComponent<PlayerMovementController> ();
        m_rigidbody = GetComponent<Rigidbody> ();
    }

    // Start is called before the first frame update
    void Start ()
    {
        InputState = new ClientInputState ();
    }

    #endregion

    #region Runtime

    private void Update ()
    {
        // Capture client input state
        InputState = SampleInputs ();
    }

    private void FixedUpdate ()
    {
        // Send player input to the server as byte array to be processed
        byte [] inputBytes = StateToBytes ( InputState );
        ClientSend.PlayerInput ( inputBytes );

        // Set current input and simulation states to buffer
        uint bufferIndex = m_simulationFrame % BUFFER_SIZE;
        m_clientInputBuffer [ bufferIndex ] = InputState;
        m_simulationStateBuffer [ bufferIndex ] = CurrentSimulationState ( InputState );

        // Apply the input on the local client
        m_playerMovementController.ProcessInputs ( InputState );

        // Simulate physics
        Physics.Simulate ( Time.fixedDeltaTime );

        // Increment client simulation frame
        ++m_simulationFrame;

        // Reconciliate if there's a message from the server
        if ( HasAvailableStateMessage () )
        {
            Reconciliate ();
        }
    }

    private void Reconciliate ()
    {
        SimulationState serverState = GetServerState ();

        // Sanity check, don't reconciliate for old states
        if ( serverState.SimulationFrame <= m_lastCorrectedFrame ) return;

        // Determine the cache index 
        uint bufferIndex = serverState.SimulationFrame % BUFFER_SIZE;

        // Obtain the cached input and simulation states
        ClientInputState cachedInputState = m_clientInputBuffer [ bufferIndex ];
        SimulationState cachedSimulationState = m_simulationStateBuffer [ bufferIndex ];

        // If there's missing cache data for either input or simulation,
        // snap the player's position to match the server
        if ( cachedInputState == null || cachedSimulationState == null )
        {
            Debug.Assert ( cachedInputState != null, "cachedInputState == null" );
            Debug.Assert ( cachedSimulationState != null, "cachedSimulationState == null" );

            m_rigidbody.position = serverState.Position;
            m_playerMovementController.SetVelocty ( serverState.Velocity );

            // Set the last corrected frame to equal the server's frame
            m_lastCorrectedFrame = serverState.SimulationFrame;
            return;
        }

        // Find the difference between the vector's values
        Vector3 positionError = cachedSimulationState.Position - serverState.Position;

        // A correction is required
        if ( positionError.sqrMagnitude > 0.0000001f ) //CorrectionTolerance
        {
            // Capture the current predicted pos for smoothing
            Vector3 previousPosition = m_rigidbody.position;

            // Set the player's position and velocity to match the server's state
            m_rigidbody.position = serverState.Position;
            m_rigidbody.velocity = serverState.Velocity;

            // Declare the rewindFrame as we're about to resimulate our cached inputs
            uint rewindFrame = serverState.SimulationFrame + 1;

            // Loop through and apply cached inputs until we're 
            // caught up to our current simulation frame
            while ( rewindFrame < m_simulationFrame )
            {
                // Determine the cache index 
                bufferIndex = rewindFrame % BUFFER_SIZE;

                // Replace the simulationStateBuffer index with the new value
                SimulationState rewoundSimulationState = CurrentSimulationState ();
                rewoundSimulationState.SimulationFrame = rewindFrame;
                m_simulationStateBuffer [ bufferIndex ] = rewoundSimulationState;

                // If there's no state to simulate, for whatever reason, 
                // increment the rewindFrame and continue
                if ( m_clientInputBuffer [ bufferIndex ] == null || m_simulationStateBuffer [ bufferIndex ] == null )
                {
                    Debug.Assert ( m_clientInputBuffer [ bufferIndex ] != null, $"m_inputStateCache [ {bufferIndex} ] == null" );
                    Debug.Assert ( m_simulationStateBuffer [ bufferIndex ] != null, $"m_simulationStateCache [ {bufferIndex} ] == null" );
                    ++rewindFrame;
                    continue;
                }

                // Apply cached input
                m_playerMovementController.ProcessInputs ( m_clientInputBuffer [ bufferIndex ] );

                // Simulate physics
                Physics.Simulate ( Time.fixedDeltaTime );

                // Increase the amount of frames that we've rewound
                ++rewindFrame;
            }

            // Player smoothing after correcting a misprediction
            PredictionCorrectionSmoothing ( previousPosition );
        }

        // Once we're complete, update the lastCorrectedFrame to match
        // NOTE: Set this even if there's no correction to be made
        m_lastCorrectedFrame = serverState.SimulationFrame;
    }

    #endregion

    #region Main Methods

    public void OnServerSimulationStateReceived ( byte [] simulationState )
    {
        if ( simulationState == null ) return;

        SimulationState message = BytesToState ( simulationState );

        if ( message != null )
        {
            // Update client with last received server tick
            Client.instance.ServerTick = message.ServerTick;

            if ( m_serverSimulationState == null )
            {
                m_serverSimulationState = message;
            }
            // Only register newer SimulationStates
            else if ( m_serverSimulationState.SimulationFrame < message.SimulationFrame )
            {
                m_serverSimulationState = message;
            }
        }
    }

    public bool HasAvailableStateMessage ()
    {
        return m_serverSimulationState != null;
    }

    private SimulationState GetServerState ()
    {
        return m_serverSimulationState;
    }

    private void PredictionCorrectionSmoothing ( Vector3 previousPosition )
    {
        // If client is more than snap threshold, snap
        if ( ( previousPosition - m_rigidbody.position ).sqrMagnitude >= SNAP_THRESHOLD )
        {
            m_clientPositionError = Vector3.zero;
        }
        else
        {
            m_clientPositionError = m_rigidbody.position - previousPosition;
        }
/*
        // Calculate a force that attempts to reach target velocity
        Vector3 velocity = m_rigidbody.velocity;
        Vector3 smoothTargetVelocity = Vector3.Lerp ( velocity, m_clientPositionError, fixedDeltaTime );
        //float smoothTargetVelocityX = Mathf.Lerp ( velocity.x, m_clientPositionError.x, fixedDeltaTime );
        //float smoothTargetVelocityZ = Mathf.Lerp ( velocity.z, m_clientPositionError.z, fixedDeltaTime );
        Vector3 velocityChange = ( smoothTargetVelocity/*new Vector3 ( smoothTargetVelocityX, 0, smoothTargetVelocityZ )*//* - velocity );
        //velocityChange.y = 0;
*/
        // Apply correction force
        m_rigidbody.AddForce ( m_clientPositionError, ForceMode.Force );
    }

    #endregion

    #region Util

    private ClientInputState SampleInputs ()
    {
        return new ClientInputState ()
        {
            MoveForward = PlayerInputController.ForwardInput,
            MoveBackward = PlayerInputController.BackwardInput,
            MoveRight = PlayerInputController.RightInput,
            MoveLeft = PlayerInputController.LeftInput,
            Jump = PlayerInputController.JumpInput,
            Run = PlayerInputController.RunInput,
            Crouch = PlayerInputController.CrouchInput,
            Shoot = PlayerInputController.ShootInput,
            Aiming = PlayerInputController.Aiming,
            GunDirection = PlayerInputController.GunDirection,
            LookDirection = PlayerInputController.LookDirection,
            CameraPitch = PlayerInputController.CameraPitch,
            Interact = PlayerInputController.Interact,
            Rotation = m_rigidbody.rotation,
            SimulationFrame = m_simulationFrame,
            ServerTick = Client.instance.ServerTick,
            DeltaTime = Time.deltaTime,
        };
    }

    public SimulationState CurrentSimulationState ( ClientInputState inputState = null )
    {
        return new SimulationState
        {
            Position = m_rigidbody.position,
            Rotation = m_rigidbody.rotation,
            Velocity = m_playerMovementController.Velocity,
            SimulationFrame = inputState != null ? inputState.SimulationFrame : 0,
            DeltaTime = inputState != null ? inputState.DeltaTime : Time.deltaTime
        };
    }

    private byte [] StateToBytes ( ClientInputState inputState )
    {
        using ( var ms = new MemoryStream () )
        {
            BinaryFormatter formatter = new BinaryFormatter ();
            SurrogateSelector surrogateSelector = new SurrogateSelector ();
            Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate ();
            QuaternionSerializationSurrogate quaternionSS = new QuaternionSerializationSurrogate ();

            surrogateSelector.AddSurrogate ( typeof ( Vector3 ), new StreamingContext ( StreamingContextStates.All ), vector3SS );
            surrogateSelector.AddSurrogate ( typeof ( Quaternion ), new StreamingContext ( StreamingContextStates.All ), quaternionSS );
            formatter.SurrogateSelector = surrogateSelector;
            formatter.Serialize ( ms, inputState );
            return ms.ToArray ();
        }
    }

    private SimulationState BytesToState ( byte [] arr )
    {
        using ( var memStream = new MemoryStream () )
        {
            BinaryFormatter formatter = new BinaryFormatter ();
            SurrogateSelector surrogateSelector = new SurrogateSelector ();
            Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate ();
            QuaternionSerializationSurrogate quaternionSS = new QuaternionSerializationSurrogate ();

            surrogateSelector.AddSurrogate ( typeof ( Vector3 ), new StreamingContext ( StreamingContextStates.All ), vector3SS );
            surrogateSelector.AddSurrogate ( typeof ( Quaternion ), new StreamingContext ( StreamingContextStates.All ), quaternionSS );
            formatter.SurrogateSelector = surrogateSelector;
            memStream.Write ( arr, 0, arr.Length );
            memStream.Seek ( 0, SeekOrigin.Begin );
            SimulationState obj = ( SimulationState ) formatter.Deserialize ( memStream );
            return obj;
        }
    }

    #endregion
}
