using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsController : MonoBehaviour
{
    public enum Weapons
    {
        Primary,
        Secondary
    }

    public delegate void WeaponAnimatorHandler_Trigger ( string parameterName );
    public static WeaponAnimatorHandler_Trigger OnSetTrigger;
    public delegate void WeaponAnimatorHandler_Bool ( string parameterName, bool value );
    public static WeaponAnimatorHandler_Bool OnSetBool;
    public delegate void WeaponSwitchHandler ( /*Action<float> finishCallback*/ );
    public static WeaponSwitchHandler OnSwitchWeapon;

    [Header ( "Weapons" )]
    private Weapons m_activeWeapon = Weapons.Primary;
    [SerializeField]
    private GameObject m_primaryWeaponHolder = null;
    [SerializeField]
    private GameObject m_secondaryWeaponHolder = null;

    private Coroutine m_weaponSwitchCoroutine = null;

    [SerializeField]
    private Transform m_cameraTransform = null;

    private void OnEnable ()
    {
        WeaponStateBehaviour.OnStateEntered += SwitchWeapons;
    }

    private void OnDisable ()
    {
        WeaponStateBehaviour.OnStateEntered -= SwitchWeapons;
    }

    // Start is called before the first frame update
    void Start ()
    {
        m_primaryWeaponHolder.SetActive ( true );
        m_secondaryWeaponHolder.SetActive ( false );
    }

    // Update is called once per frame
    void Update ()
    {
        // Weapon switching - Primary
        if ( Input.GetKeyDown ( KeyCode.Alpha1 ) && m_activeWeapon != Weapons.Primary )
        {
            OnSetTrigger?.Invoke ( "Holster" );
            //OnSwitchWeapon?.Invoke ( SwitchWeapons );
        }
        // Weapon switching - Secondary
        if ( Input.GetKeyDown ( KeyCode.Alpha2 ) && m_activeWeapon != Weapons.Secondary )
        {
            OnSetTrigger?.Invoke ( "Holster" );
            //OnSwitchWeapon?.Invoke ( SwitchWeapons );
        }

        // Shooting
        if ( Input.GetKeyDown ( KeyCode.Mouse0 ) )
        {
            ClientSend.PlayerShoot ( m_cameraTransform.forward );

            // This will be changed
            OnSetTrigger?.Invoke ( "Shoot" );
            OnSetTrigger?.Invoke ( "BoltCharge" );
        }


        if ( Input.GetKeyDown ( KeyCode.R ) )
        {
            OnSetTrigger?.Invoke ( "ReloadFull" );
        }
        if ( Input.GetKeyDown ( KeyCode.H ) )
        {
            OnSetTrigger?.Invoke ( "Holster" );
        }

        // TODO: send shoot logic to server and handle response somewhere here
    }

    private void SwitchWeapons ( AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( m_weaponSwitchCoroutine != null )
        {
            StopCoroutine ( m_weaponSwitchCoroutine );
        }
        m_weaponSwitchCoroutine = StartCoroutine ( SwitchWeaponsDelay ( stateInfo.length ) );
    }

    private IEnumerator SwitchWeaponsDelay ( float delay )
    {
        yield return new WaitForSeconds ( delay );

        DisableWeapons ();
        if ( m_activeWeapon == Weapons.Primary )
        {
            m_secondaryWeaponHolder.SetActive ( true );
            m_activeWeapon = Weapons.Secondary;
        }
        else
        {
            m_primaryWeaponHolder.SetActive ( true );
            m_activeWeapon = Weapons.Primary;
        }
    }

    private void DisableWeapons ()
    {
        m_primaryWeaponHolder.SetActive ( false );
        m_secondaryWeaponHolder.SetActive ( false );
    }

}
