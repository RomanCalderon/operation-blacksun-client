using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using InventorySystem.Slots;
using System.Linq;

[RequireComponent ( typeof ( AimController ) )]
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
    public delegate void WeaponAnimatorHandler_Float ( string parameterName, float value );
    public static WeaponAnimatorHandler_Float OnSetFloat;
    public delegate void WeaponSwitchHandler ();
    public static WeaponSwitchHandler OnSwitchWeapon;

    private AimController m_aimController = null;
    private bool m_canAim = false;

    [Header ( "Weapons" )]
    private Weapons m_activeWeapon = Weapons.Primary;
    [Header ( "Primary" ), SerializeField]
    private GameObject m_primaryWeaponHolder = null;
    [SerializeField]
    private List<PlayerItemInstance> m_primaryWeapons = new List<PlayerItemInstance> ();
    [Header ( "Secondary" ), SerializeField]
    private GameObject m_secondaryWeaponHolder = null;
    [SerializeField]
    private List<PlayerItemInstance> m_secondaryWeapons = new List<PlayerItemInstance> ();

    private Coroutine m_weaponSwitchCoroutine = null;

    [SerializeField]
    private Transform m_cameraTransform = null;

    private void OnEnable ()
    {
        InventoryManager.OnSlotUpdated += EquipWeapon;

        WeaponStateBehaviour.OnStateEntered += SwitchWeapons;
        WeaponStateBehaviour.OnStateEntered += EnteredAnimatorState;
        WeaponStateBehaviour.OnStateUpdated += UpdatedAnimatorState;
        WeaponStateBehaviour.OnStateExited += ExitedAnimatorState;

        AimController.OnAimUpdated += UpdateIdleSpeed;
    }

    private void OnDisable ()
    {
        InventoryManager.OnSlotUpdated -= EquipWeapon;

        WeaponStateBehaviour.OnStateEntered -= SwitchWeapons;
        WeaponStateBehaviour.OnStateEntered -= EnteredAnimatorState;
        WeaponStateBehaviour.OnStateUpdated -= UpdatedAnimatorState;
        WeaponStateBehaviour.OnStateExited -= ExitedAnimatorState;

        AimController.OnAimUpdated -= UpdateIdleSpeed;
    }

    private void Awake ()
    {
        m_aimController = GetComponent<AimController> ();
    }

    // Start is called before the first frame update
    void Start ()
    {
        m_primaryWeaponHolder.SetActive ( true );
        m_secondaryWeaponHolder.SetActive ( false );

        // This needs to update based on equipped sight parameters
        m_aimController.UpdateFOVParameters ( 30f );
    }

    // Update is called once per frame
    void Update ()
    {
        if ( !InventoryManager.Instance.IsDisplayed )
        {
            // Weapon switching - Primary
            if ( Input.GetKeyDown ( KeyCode.Alpha1 ) && m_activeWeapon != Weapons.Primary )
            {
                OnSetTrigger?.Invoke ( "Holster" );
            }
            // Weapon switching - Secondary
            if ( Input.GetKeyDown ( KeyCode.Alpha2 ) && m_activeWeapon != Weapons.Secondary )
            {
                OnSetTrigger?.Invoke ( "Holster" );
            }
            // Shooting
            if ( Input.GetKeyDown ( KeyCode.Mouse0 ) )
            {
                ClientSend.PlayerShoot ( m_cameraTransform.forward );

                // This will be changed
                OnSetTrigger?.Invoke ( "Shoot" );
                OnSetTrigger?.Invoke ( "BoltCharge" );
                CameraShaker.Instance.ShakeOnce ( 0.5f, 6f, 0.01f, 0.16f );
                CameraController.Instance.AddRecoil ( 0.75f );
            }

            // Aiming
            AimController.AimState = m_canAim && Input.GetKey ( KeyCode.Mouse1 );

            // Reloading
            if ( Input.GetKeyDown ( KeyCode.R ) )
            {
                OnSetTrigger?.Invoke ( "ReloadFull" );
            }
        }
        // TODO: send input to server and handle response somewhere here
    }

    #region AnimatorState handlers

    /// <summary>
    /// EnteredAnimatorState is called when a transition starts and the state machine starts to evaluate this state.
    /// </summary>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>
    private void EnteredAnimatorState ( AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( stateInfo.IsName ( "Shoot" ) && !AimController.AimState )
        {
            m_canAim = false;
        }
        if ( stateInfo.IsName ( "BoltCharge" ) && !AimController.AimState )
        {
            m_canAim = false;
        }
        if ( stateInfo.IsName ( "ReloadPartial" ) )
        {
            m_canAim = false;
        }
        if ( stateInfo.IsName ( "ReloadFull" ) )
        {
            m_canAim = false;
        }
        if ( stateInfo.IsName ( "Holster" ) )
        {
            m_canAim = false;
        }
    }

    /// <summary>
    /// UpdatedAnimatorState is called on each Update frame between OnStateEnter and OnStateExit callbacks.
    /// </summary>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>
    private void UpdatedAnimatorState ( AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( stateInfo.IsName ( "Shoot" ) && !AimController.AimState )
        {
            m_canAim = false;
        }
        if ( stateInfo.IsName ( "BoltCharge" ) && !AimController.AimState )
        {
            m_canAim = false;
        }
    }

    /// <summary>
    /// ExitedAnimatorState is called when a transition ends and the state machine finishes evaluating this state.
    /// </summary>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>
    private void ExitedAnimatorState ( AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( stateInfo.IsName ( "Draw" ) )
        {
            m_canAim = true;
        }
        if ( stateInfo.IsName ( "Shoot" ) )
        {
            m_canAim = true;
        }
        if ( stateInfo.IsName ( "BoltCharge" ) )
        {
            m_canAim = true;
        }
        if ( stateInfo.IsName ( "ReloadPartial" ) )
        {
            m_canAim = true;
        }
        if ( stateInfo.IsName ( "ReloadFull" ) )
        {
            m_canAim = true;
        }
    }

    #endregion

    #region Weapon switching

    private void SwitchWeapons ( AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( stateInfo.IsName ( "Holster" ) )
        {
            if ( m_weaponSwitchCoroutine != null )
            {
                StopCoroutine ( m_weaponSwitchCoroutine );
            }
            m_weaponSwitchCoroutine = StartCoroutine ( SwitchWeaponsDelay ( stateInfo.length ) );
        }
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

    #endregion

    private void UpdateIdleSpeed ( bool aimState )
    {
        float idleSpeed = aimState ? 0.3f : 1f;
        OnSetFloat?.Invoke ( "IdleSpeed", idleSpeed );
    }

    /// <summary>
    /// Invoked when a slot has been updated in the Inventory.
    /// If the updated slot is a weapon slot (primary/secondary),
    /// then activate that object from the list.
    /// </summary>
    /// <param name="slotId"></param>
    private void EquipWeapon ( string slotId )
    {
        if ( string.IsNullOrEmpty ( slotId ) )
        {
            Debug.LogWarning ( "slotId is null or empty." );
            return;
        }

        if ( slotId == "primary-weapon" )
        {
            // Get the Slot
            Slot slot = InventoryManager.Instance.Inventory.GetSlot ( slotId );
            if ( slot == null )
            {
                Debug.LogWarning ( $"Slot with id [{slotId}] is missing." );
                return;
            }

            ResetWeapons ( Weapons.Primary );
            PlayerItemInstance weapon = m_primaryWeapons.FirstOrDefault ( w => w.PlayerItem.Id == slot.PlayerItem.Id );
            if ( weapon != null )
            {
                weapon.SetActive ( true );
            }
            else
            {
                Debug.LogWarning ( $"Primary weapon with PlayerItem Id [{slot.PlayerItem.Id}] does not exists in the list." );
            }
        }
        else if ( slotId == "secondary-weapon" )
        {
            // Get the Slot
            Slot slot = InventoryManager.Instance.Inventory.GetSlot ( slotId );
            if ( slot == null )
            {
                Debug.LogWarning ( $"Slot with id [{slotId}] is missing." );
                return;
            }

            ResetWeapons ( Weapons.Secondary );
            PlayerItemInstance weapon = m_secondaryWeapons.FirstOrDefault ( w => w.PlayerItem.Id == slot.PlayerItem.Id );
            if ( weapon != null )
            {
                weapon.SetActive ( true );
            }
            else
            {
                Debug.LogWarning ( $"Secondary weapon with PlayerItem Id [{slot.PlayerItem.Id}] does not exists in the list." );
            }
        }
    }

    private void ResetWeapons ( Weapons weaponType )
    {
        switch ( weaponType )
        {
            case Weapons.Primary:
                foreach ( PlayerItemInstance playerItemInstance in m_primaryWeapons )
                {
                    playerItemInstance.SetActive ( false );
                }
                break;
            case Weapons.Secondary:
                foreach ( PlayerItemInstance playerItemInstance in m_secondaryWeapons )
                {
                    playerItemInstance.SetActive ( false );
                }
                break;
            default:
                break;
        }
    }

    private void ResetWeaponsAll ()
    {
        ResetWeapons ( Weapons.Primary );
        ResetWeapons ( Weapons.Secondary );
    }
}
