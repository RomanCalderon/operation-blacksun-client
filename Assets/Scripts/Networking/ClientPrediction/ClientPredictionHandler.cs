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
    private const uint STATE_CACHE_SIZE = 1024;
    // Correction tolerance
    private const float CORRECTION_TOLERANCE = 0.001f;
    private const float CORRECTION_LERP_RATE = 0.4f;
    // Minimum position snapping distance
    private const float SNAP_THRESHOLD = 4f;

    private PlayerMovementController m_playerMovementController = null;
    private Rigidbody m_rigidbody = null;

    // Input
    public static ClientInputState InputState { get; private set; } = null;

    private uint m_simulationFrame = 0;
    private SimulationState [] m_simulationStateCache = new SimulationState [ STATE_CACHE_SIZE ];
    private ClientInputState [] m_inputStateCache = new ClientInputState [ STATE_CACHE_SIZE ];
    private SimulationState m_serverSimulationState;
    private uint m_lastCorrectedFrame;
    private Vector3 m_clientPositionError;

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

    private void Update ()
    {
        // Capture input
        InputState = SampleInputs ();
    }

    private void FixedUpdate ()
    {
        // Set the InputState's simulation frame
        InputState.SimulationFrame = m_simulationFrame;
        InputState.ServerTick = Client.instance.ServerTick;

        // Send player input to the server as byte array to be processed
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
        uint cacheIndex = m_simulationFrame % STATE_CACHE_SIZE;
        m_simulationStateCache [ cacheIndex ] = simulationState;
        m_inputStateCache [ cacheIndex ] = InputState;

        // Increment client simulation frame
        m_simulationFrame++;
    }

    public void OnServerSimulationStateReceived ( byte [] simulationState )
    {
        SimulationState message = BytesToState ( simulationState );

        // Update client with last received server tick
        Client.instance.ServerTick = message.ServerTick;

        if ( m_serverSimulationState == null )
        {
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
        uint cacheIndex = m_serverSimulationState.SimulationFrame % STATE_CACHE_SIZE;

        // Obtain the cached input and simulation states
        ClientInputState cachedInputState = m_inputStateCache [ cacheIndex ];
        SimulationState cachedSimulationState = m_simulationStateCache [ cacheIndex ];

        // If there's missing cache data for either input or simulation 
        // snap the player's position to match the server
        if ( cachedInputState == null || cachedSimulationState == null )
        {
            m_rigidbody.position = m_serverSimulationState.Position;
            m_playerMovementController.SetVelocty ( m_serverSimulationState.Velocity );

            // Set the last corrected frame to equal the server's frame
            m_lastCorrectedFrame = m_serverSimulationState.SimulationFrame;
            return;
        }

        // Find the difference between the vector's values
        Vector3 positionError = cachedSimulationState.Position - m_serverSimulationState.Position;

        // A correction is necessary.
        if ( positionError.sqrMagnitude > CORRECTION_TOLERANCE )
        {
            // Capture the current predicted pos for smoothing
            Vector3 previousPosition = m_rigidbody.position + m_clientPositionError;

            // Set the player's position and velocity to match the server's state
            m_rigidbody.position = m_serverSimulationState.Position;
            m_playerMovementController.SetVelocty ( m_serverSimulationState.Velocity );

            // Declare the rewindFrame as we're about to resimulate our cached inputs
            uint rewindFrame = m_serverSimulationState.SimulationFrame;

            // Loop through and apply cached inputs until we're 
            // caught up to our current simulation frame
            while ( rewindFrame < m_simulationFrame )
            {
                // Determine the cache index 
                uint rewindCacheIndex = rewindFrame % STATE_CACHE_SIZE;

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

            // If client is more than snap threshold, snap
            if ( ( previousPosition - m_rigidbody.position ).sqrMagnitude >= SNAP_THRESHOLD )
            {
                m_clientPositionError = Vector3.zero;
            }
            else
            {
                m_clientPositionError = previousPosition - m_rigidbody.position;
            }

            // Player position correction smoothing
            m_clientPositionError *= CORRECTION_LERP_RATE;
            transform.position = m_rigidbody.position + m_clientPositionError;
        }

        // Once we're complete, update the lastCorrectedFrame to match
        // NOTE: Set this even if there's no correction to be made
        m_lastCorrectedFrame = m_serverSimulationState.SimulationFrame;
    }

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
            DeltaTime = Time.deltaTime
        };
    }

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
