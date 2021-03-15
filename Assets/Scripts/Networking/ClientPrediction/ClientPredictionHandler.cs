using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using PlayerInput;

[RequireComponent ( typeof ( PlayerMovementController ) )]
public class ClientPredictionHandler : MonoBehaviour
{
    // State caching
    private const int STATE_CACHE_SIZE = 1024;

    private PlayerMovementController m_playerMovementController = null;

    // Input
    public static ClientInputState InputState { get; private set; } = null;

    private int m_simulationFrame;
    private SimulationState [] m_simulationStateCache = new SimulationState [ STATE_CACHE_SIZE ];
    private ClientInputState [] m_inputStateCache = new ClientInputState [ STATE_CACHE_SIZE ];
    private SimulationState m_serverSimulationState;
    private int m_lastCorrectedFrame;

    private void Awake ()
    {
        m_playerMovementController = GetComponent<PlayerMovementController> ();
    }

    // Start is called before the first frame update
    void Start ()
    {
        InputState = new ClientInputState ();
    }

    private void Update ()
    {
        // Capture input
        InputState = new ClientInputState
        {
            MoveForward = PlayerInputController.ForwardInput,
            MoveBackward = PlayerInputController.BackwardInput,
            MoveRight = PlayerInputController.RightInput,
            MoveLeft = PlayerInputController.LeftInput,
            Jump = PlayerInputController.JumpInput,
            Run = PlayerInputController.RunInput,
            Crouch = PlayerInputController.CrouchInput,
            Rotation = transform.rotation,
            DeltaTime = Time.deltaTime
        };
    }

    private void FixedUpdate ()
    {
        // Set the InputState's simulation frame
        InputState.SimulationFrame = m_simulationFrame;

        // Send the input to the server as byte array to be processed
        byte [] inputBytes = StateToBytes ( InputState );
        ClientSend.PlayerInput ( inputBytes );
        
        // Reconciliate if there's a message from the server
        if ( m_serverSimulationState != null )
        {
            Reconciliate ();
        }

        // Process the input on the local client
        m_playerMovementController.ProcessInputs ( InputState );


        // Simulate physics
        Physics.Simulate ( Time.fixedDeltaTime );

        // Get the current simulation state
        SimulationState simulationState = CurrentSimulationState ( InputState );
        // Determine the cache index based on on modulus operator
        int cacheIndex = m_simulationFrame % STATE_CACHE_SIZE;
        m_simulationStateCache [ cacheIndex ] = simulationState;
        m_inputStateCache [ cacheIndex ] = InputState;

        // Increase the client's simulation frame
        m_simulationFrame++;
    }

    public void OnServerSimulationStateReceived ( byte [] simulationState )
    {
        SimulationState message = BytesToState ( simulationState );

        if ( m_serverSimulationState == null )
        {
            Debug.Log ( "m_serverSimulationState is null" );
            m_serverSimulationState = message;
            return;
        }

        // Only register newer SimulationStates
        if ( m_serverSimulationState?.SimulationFrame < message.SimulationFrame )
        {
            m_serverSimulationState = message;
        }
    }

    private void Reconciliate ()
    {
        // Sanity check, don't reconciliate for old states
        if ( m_serverSimulationState.SimulationFrame <= m_lastCorrectedFrame ) return;

        // Determine the cache index 
        int cacheIndex = m_serverSimulationState.SimulationFrame % STATE_CACHE_SIZE;

        // Obtain the cached input and simulation states
        ClientInputState cachedInputState = m_inputStateCache [ cacheIndex ];
        SimulationState cachedSimulationState = m_simulationStateCache [ cacheIndex ];

        // If there's missing cache data for either input or simulation 
        // snap the player's position to match the server
        if ( cachedInputState == null || cachedSimulationState == null )
        {
            transform.position = m_serverSimulationState.Position;
            m_playerMovementController.SetVelocty ( m_serverSimulationState.Velocity );

            // Set the last corrected frame to equal the server's frame
            m_lastCorrectedFrame = m_serverSimulationState.SimulationFrame;
            return;
        }

        // Find the difference between the vector's values
        Vector3 positionError = cachedSimulationState.Position - m_serverSimulationState.Position;

        //  The distance in units that we will allow the client's
        //  prediction to drift from it's position on the server.
        //  Exceeding this threshold will warrant a correction.
        float tolerance = 0.075f;

        // A correction is necessary.
        if ( positionError.magnitude > tolerance )
        {
            //Debug.Log ( $"position error: {positionError.magnitude}" );

            // Set the player's position and velocity to match the server's state
            transform.position = m_serverSimulationState.Position;
            m_playerMovementController.SetVelocty ( m_serverSimulationState.Velocity );

            // Declare the rewindFrame as we're about to resimulate our cached inputs
            int rewindFrame = m_serverSimulationState.SimulationFrame;

            // Loop through and apply cached inputs until we're 
            // caught up to our current simulation frame
            while ( rewindFrame < m_simulationFrame )
            {
                // Determine the cache index 
                int rewindCacheIndex = rewindFrame % STATE_CACHE_SIZE;

                // Obtain the cached input and simulation states
                ClientInputState rewindCachedInputState = m_inputStateCache [ rewindCacheIndex ];
                SimulationState rewindCachedSimulationState = m_simulationStateCache [ rewindCacheIndex ];

                // If there's no state to simulate, for whatever reason, 
                // increment the rewindFrame and continue
                if ( rewindCachedInputState == null || rewindCachedSimulationState == null )
                {
                    rewindFrame++;
                    continue;
                }

                // Process the cached inputs
                m_playerMovementController.ProcessInputs ( rewindCachedInputState );

                // Simulate physics
                Physics.Simulate ( Time.fixedDeltaTime );

                // Replace the simulationStateCache index with the new value
                SimulationState rewoundSimulationState = CurrentSimulationState ();
                rewoundSimulationState.SimulationFrame = rewindFrame;
                m_simulationStateCache [ rewindCacheIndex ] = rewoundSimulationState;

                // Increase the amount of frames that we've rewound
                rewindFrame++;
            }
        }

        // Once we're complete, update the lastCorrectedFrame to match
        // NOTE: Set this even if there's no correction to be made
        m_lastCorrectedFrame = m_serverSimulationState.SimulationFrame;
    }

    #region Util
    
    public SimulationState CurrentSimulationState ( ClientInputState inputState = null )
    {
        return new SimulationState
        {
            Position = transform.position,
            Rotation = transform.rotation,
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
            QuaternionSerializationSurrogate quaternionSS = new QuaternionSerializationSurrogate ();

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
