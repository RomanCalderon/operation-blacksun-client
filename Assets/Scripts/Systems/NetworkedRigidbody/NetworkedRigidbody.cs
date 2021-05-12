using System.IO;
using UnityEngine;

public class NetworkedRigidbody : MonoBehaviour
{
    #region Models

    [System.Serializable]
    public struct NetworkData
    {
        public string Id;
        public Vector3 Position;
        public Vector3 Rotation;

        public NetworkData ( string id, Vector3 position, Vector3 rotation )
        {
            Id = id;
            Position = position;
            Rotation = rotation;
        }

        public byte [] ToArray ()
        {
            MemoryStream stream = new MemoryStream ();
            BinaryWriterExtended writer = new BinaryWriterExtended ( stream );

            writer.Write ( Id );
            writer.Write ( Position );
            writer.Write ( Rotation );

            return stream.ToArray ();
        }

        public static NetworkData FromArray ( byte [] bytes )
        {
            BinaryReaderExtended reader = new BinaryReaderExtended ( new MemoryStream ( bytes ) );
            NetworkData d = default;

            d.Id = reader.ReadString ();
            d.Position = reader.ReadVector3 ();
            d.Rotation = reader.ReadVector3 ();

            return d;
        }
    }

    #endregion

    #region Members

    private const float INTERP_POSITION_SPEED = 10f;
    private const float INTERP_ROTATION_SPEED = 10f;

    private string m_id = null;
    private bool m_isInitialized = false;

    private Vector3 m_targetPosition;
    private Vector3 m_targetRotation;

    #endregion

    #region Methods

    #region Initialization

    public void Initialize ( string id )
    {
        if ( !m_isInitialized )
        {
            m_isInitialized = true;
            NetworkedRigidbodyManager.Instance.AddBody ( m_id = id, this );
        }
    }

    #endregion

    #region Runtime

    private void Update ()
    {
        InterpolateTransform ( Time.deltaTime );
    }

    private void InterpolateTransform ( float deltaTime )
    {
        transform.position = Vector3.Lerp ( transform.position, m_targetPosition, deltaTime * INTERP_POSITION_SPEED );
        transform.eulerAngles = Vector3.Lerp ( transform.eulerAngles, m_targetRotation, deltaTime * INTERP_ROTATION_SPEED );
    }

    #endregion

    public byte [] GetData ()
    {
        NetworkData instanceData = new NetworkData ( m_id, transform.position, transform.eulerAngles );
        return instanceData.ToArray ();
    }

    public void SetData ( NetworkData data )
    {
        if ( data.Id == m_id )
        {
            m_targetPosition = data.Position;
            m_targetRotation = data.Rotation;
        }
    }

    private void OnDestroy ()
    {
        if ( m_isInitialized )
        {
            NetworkedRigidbodyManager.Instance.RemoveBody ( m_id );
        }
    }

    #endregion
}
