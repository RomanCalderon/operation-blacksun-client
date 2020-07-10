using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsController : MonoBehaviour
{
    public delegate void WeaponAnimatorHandler ( string parameterName );
    public static WeaponAnimatorHandler OnSetTrigger;

    [SerializeField]
    private GameObject m_primaryWeaponHolder = null;
    [SerializeField]
    private GameObject m_secondaryWeaponHolder = null;

    [SerializeField]
    private Transform m_cameraTransform = null;

    // Start is called before the first frame update
    void Start ()
    {
        m_primaryWeaponHolder.SetActive ( true );
        m_secondaryWeaponHolder.SetActive ( false );
    }

    // Update is called once per frame
    void Update ()
    {
        if ( Input.GetKeyDown ( KeyCode.Mouse0 ) )
        {
            ClientSend.PlayerShoot ( m_cameraTransform.forward );

            // This will be changed
            OnSetTrigger?.Invoke ( "Shoot" );
        }

        if ( Input.GetKeyDown ( KeyCode.R ) )
        {
            OnSetTrigger?.Invoke ( "ReloadFull" );
        }

        // TODO: send shoot logic to server and handle response somewhere here
    }
}
