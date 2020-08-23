using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using InventorySystem.Slots;
using InventorySystem.PlayerItems;

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

    private int m_activeHotbarIndex = 0;

    public Weapons ActiveWeaponSlot { get; private set; } = Weapons.Primary;
    public WeaponInstance ActiveWeapon
    {
        get
        {
            return ActiveWeaponSlot == Weapons.Primary ? m_primaryEquipped : m_secondaryEquipped;
        }
    }

    [Space]
    [Header ( "Primary" ), SerializeField]
    private GameObject m_primaryWeaponHolder = null;
    [SerializeField]
    private List<WeaponInstance> m_primaryWeapons = new List<WeaponInstance> ();
    private WeaponInstance m_primaryEquipped = null;
    [Header ( "Secondary" ), SerializeField]
    private GameObject m_secondaryWeaponHolder = null;
    [SerializeField]
    private List<WeaponInstance> m_secondaryWeapons = new List<WeaponInstance> ();
    private WeaponInstance m_secondaryEquipped = null;

    private Coroutine m_weaponSwitchCoroutine = null;

    [SerializeField]
    private Transform m_cameraTransform = null;

    private void OnEnable ()
    {
        AimController.OnAimStateUpdated += UpdateIdleSpeed;

        InventoryManager.OnSlotUpdated += EquipWeapon;
        InventoryManager.OnSlotUpdated += EquipAttachment;

        WeaponStateBehaviour.OnStateEntered += SwitchWeapons;
        WeaponStateBehaviour.OnStateEntered += EnteredAnimatorState;
        WeaponStateBehaviour.OnStateUpdated += UpdatedAnimatorState;
        WeaponStateBehaviour.OnStateExited += ExitedAnimatorState;
    }

    private void OnDisable ()
    {
        AimController.OnAimStateUpdated -= UpdateIdleSpeed;

        InventoryManager.OnSlotUpdated -= EquipWeapon;
        InventoryManager.OnSlotUpdated -= EquipAttachment;

        WeaponStateBehaviour.OnStateEntered -= SwitchWeapons;
        WeaponStateBehaviour.OnStateEntered -= EnteredAnimatorState;
        WeaponStateBehaviour.OnStateUpdated -= UpdatedAnimatorState;
        WeaponStateBehaviour.OnStateExited -= ExitedAnimatorState;
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
    }

    // Update is called once per frame
    void Update ()
    {
        if ( !InventoryManager.Instance.IsDisplayed )
        {
            // Weapon switching - Primary
            if ( Input.GetKeyDown ( KeyCode.Alpha1 ) && ActiveWeaponSlot != Weapons.Primary )
            {
                if ( m_activeHotbarIndex == 1 ) // Switching from secondary weapon
                {
                    if ( m_secondaryEquipped != null ) // Play weapon holster animation to switch
                    {
                        OnSetTrigger?.Invoke ( "Holster" );
                    }
                    else
                    {
                        ActivateWeapon ( Weapons.Primary );
                    }
                }
            }
            // Weapon switching - Secondary
            if ( Input.GetKeyDown ( KeyCode.Alpha2 ) && ActiveWeaponSlot != Weapons.Secondary )
            {
                if ( m_activeHotbarIndex == 0 ) // Switching from primary weapon
                {
                    if ( m_primaryEquipped != null ) // Play weapon holster animation to switch
                    {
                        OnSetTrigger?.Invoke ( "Holster" );
                    }
                    else
                    {
                        ActivateWeapon ( Weapons.Secondary );
                    }
                }
            }

            // Shooting
            if ( ActiveWeapon != null )
            {
                switch ( ( ActiveWeapon.PlayerItem as Weapon ).FireMode )
                {
                    case Weapon.FireModes.SemiAuto:
                        if ( Input.GetKeyDown ( KeyCode.Mouse0 ) )
                        {
                            ActiveWeapon.Shoot ();
                        }
                        break;
                    case Weapon.FireModes.FullAuto:
                        if ( Input.GetKey ( KeyCode.Mouse0 ) )
                        {
                            ActiveWeapon.Shoot ();
                        }
                        break;
                    default:
                        break;
                }
            }

            // Aiming
            AimController.AimState = AimController.CanAim && Input.GetKey ( KeyCode.Mouse1 );

            // Reloading
            if ( Input.GetKeyDown ( KeyCode.R ) )
            {
                if ( ActiveWeapon != null )
                {
                    ActiveWeapon.Reload ();
                }
            }
        }
    }

    /// <summary>
    /// Uses <paramref name="weaponClass"/> and <paramref name="caliber"/>
    /// to calculate a base recoil strength of a weapon.
    /// </summary>
    /// <returns>The base recoil strength of a weapon.</returns>
    public static float CalculateRecoilStrength ( Weapon.WeaponClasses weaponClass, Ammunition.Calibers caliber, out float aspect )
    {
        float strength = 0f;
        aspect = UnityEngine.Random.Range ( 0.45f, 0.55f );

        switch ( weaponClass )
        {
            case Weapon.WeaponClasses.Rifle:
                strength += 0.4f;
                break;
            case Weapon.WeaponClasses.SMG:
                strength += 0.6f;
                break;
            case Weapon.WeaponClasses.Shotgun:
                strength += 5.0f;
                break;
            case Weapon.WeaponClasses.Pistol:
                strength += 0.3f;
                break;
            case Weapon.WeaponClasses.Sniper:
                strength += 2.0f;
                break;
            default:
                break;
        }

        switch ( caliber )
        {
            case Ammunition.Calibers.Wilson_9MM:
                strength += UnityEngine.Random.Range ( 0.0f, 0.08f );
                break;
            case Ammunition.Calibers.ACP_Ultra:
                strength += UnityEngine.Random.Range ( 0.0f, 0.1f );
                break;
            case Ammunition.Calibers.NATO_556:
                strength += 0.0f;
                break;
            case Ammunition.Calibers.AAC:
                strength += 0.3f;
                break;
            case Ammunition.Calibers.G12:
                strength += UnityEngine.Random.Range ( 0.0f, 2.0f );
                break;
            case Ammunition.Calibers.Boar_75:
                strength += UnityEngine.Random.Range ( 1.0f, 3.5f );
                break;
            case Ammunition.Calibers.C3:
                strength += UnityEngine.Random.Range ( 0.0f, 0.1f );
                break;
            case Ammunition.Calibers.Vanquisher:
                strength += 1.5f;
                break;
            default:
                break;
        }

        return strength;
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
            AimController.CanAim = false;
        }
        if ( stateInfo.IsName ( "BoltCharge" ) && !AimController.AimState )
        {
            AimController.CanAim = false;
        }
        if ( stateInfo.IsName ( "ReloadPartial" ) )
        {
            AimController.CanAim = false;
        }
        if ( stateInfo.IsName ( "ReloadFull" ) )
        {
            AimController.CanAim = false;
        }
        if ( stateInfo.IsName ( "Holster" ) )
        {
            AimController.CanAim = false;
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
            AimController.CanAim = false;
        }
        if ( stateInfo.IsName ( "BoltCharge" ) && !AimController.AimState )
        {
            AimController.CanAim = false;
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
            AimController.CanAim = true;
        }
        if ( stateInfo.IsName ( "Shoot" ) )
        {
            AimController.CanAim = true;
        }
        if ( stateInfo.IsName ( "BoltCharge" ) )
        {
            AimController.CanAim = true;
        }
        if ( stateInfo.IsName ( "ReloadPartial" ) )
        {
            AimController.CanAim = true;
        }
        if ( stateInfo.IsName ( "ReloadFull" ) )
        {
            AimController.CanAim = true;
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
        Weapons activeWeapon = ActiveWeaponSlot;
        yield return new WaitForSeconds ( delay );

        ActivateWeapon ( activeWeapon == Weapons.Primary ? Weapons.Secondary : Weapons.Primary );
    }

    private void ActivateWeapon ( Weapons weapon )
    {
        DisableWeapons ();

        ActiveWeaponSlot = weapon;
        m_activeHotbarIndex = ( int ) weapon;

        if ( weapon == Weapons.Primary )
        {
            m_primaryWeaponHolder.SetActive ( true );
        }
        else if ( weapon == Weapons.Secondary )
        {
            m_secondaryWeaponHolder.SetActive ( true );
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
        float idleSpeed = aimState ? 0f : 1f;
        if ( aimState && ActiveWeapon.BulletCount > 0 )
        {
            OnSetTrigger?.Invoke ( "ResetIdle" );
        }
        OnSetFloat?.Invoke ( "IdleSpeed", idleSpeed );
    }

    #region Weapon and Attachment Equipping

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
            // Get the Slot from the Inventory
            Slot slot = InventoryManager.Instance.Inventory.GetSlot ( slotId );
            if ( slot == null )
            {
                Debug.LogWarning ( $"Slot with id [{slotId}] is missing." );
                return;
            }
            if ( slot.PlayerItem == null ) // No weapon
            {
                ClearWeapon ( Weapons.Primary );
                return;
            }

            WeaponInstance weapon = m_primaryWeapons.FirstOrDefault ( w => w.PlayerItem.Id == slot.PlayerItem.Id );
            if ( weapon != null )
            {
                ClearWeapon ( Weapons.Primary );
                m_primaryEquipped = weapon;
                m_primaryEquipped.SetActive ( true );
                m_primaryEquipped.Initialize ( m_aimController );
            }
            else
            {
                Debug.LogWarning ( $"Primary weapon with PlayerItem Id [{slot.PlayerItem.Id}] does not exist in the list." );
            }
        }
        else if ( slotId == "secondary-weapon" )
        {
            // Get the Slot from the Inventory
            Slot slot = InventoryManager.Instance.Inventory.GetSlot ( slotId );
            if ( slot == null )
            {
                Debug.LogWarning ( $"Slot with id [{slotId}] is missing." );
                return;
            }
            if ( slot.PlayerItem == null ) // No weapon
            {
                ClearWeapon ( Weapons.Secondary );
                return;
            }

            WeaponInstance weapon = m_secondaryWeapons.FirstOrDefault ( w => w.PlayerItem.Id == slot.PlayerItem.Id );
            if ( weapon != null )
            {
                ClearWeapon ( Weapons.Secondary );
                m_secondaryEquipped = weapon;
                m_secondaryEquipped.SetActive ( true );
                m_secondaryEquipped.Initialize ( m_aimController );
            }
            else
            {
                Debug.LogWarning ( $"Secondary weapon with PlayerItem Id [{slot.PlayerItem.Id}] does not exist in the list." );
            }
        }

        // Update AimController based on active weapon
        if ( ActiveWeapon != null )
        {
            ActiveWeapon.UpdateAimController ();
        }
    }

    /// <summary>
    /// Invoked when a slot has been updated in the Inventory.
    /// If the updated slot is an attachment slot,
    /// then equip that attachment to its corresponding weapon (primary/secondary).
    /// </summary>
    /// <param name="slotId"></param>
    private void EquipAttachment ( string slotId )
    {
        if ( string.IsNullOrEmpty ( slotId ) )
        {
            Debug.LogWarning ( "slotId is null or empty." );
            return;
        }

        if ( slotId.Contains ( "primary" ) ) // Primary weapon attachment
        {
            // Get the Slot from the Inventory
            Slot slot = InventoryManager.Instance.Inventory.GetSlot ( slotId );
            if ( slot == null )
            {
                Debug.LogWarning ( $"Slot with id [{slotId}] is missing." );
                return;
            }
            if ( m_primaryEquipped == null )
            {
                return;
            }

            // Equip the attachment
            if ( slotId.Contains ( "barrel" ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Barrel ) slot.PlayerItem );
            }
            else if ( slotId.Contains ( "magazine" ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Magazine ) slot.PlayerItem );
            }
            else if ( slotId.Contains ( "sight" ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Sight ) slot.PlayerItem );
            }
            else if ( slotId.Contains ( "stock" ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Stock ) slot.PlayerItem );
            }
        }
        else if ( slotId.Contains ( "secondary" ) ) // Secondary weapon attachment
        {
            // Get the Slot from the Inventory
            Slot slot = InventoryManager.Instance.Inventory.GetSlot ( slotId );
            if ( slot == null )
            {
                Debug.LogWarning ( $"Slot with id [{slotId}] is missing." );
                return;
            }
            if ( m_secondaryEquipped == null )
            {
                return;
            }

            // Equip the attachment
            if ( slotId.Contains ( "barrel" ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Barrel ) slot.PlayerItem );
            }
            else if ( slotId.Contains ( "magazine" ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Magazine ) slot.PlayerItem );
            }
            else if ( slotId.Contains ( "sight" ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Sight ) slot.PlayerItem );
            }
            else if ( slotId.Contains ( "stock" ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Stock ) slot.PlayerItem );
            }
        }
    }

    #endregion

    private void ClearWeapon ( Weapons weaponType )
    {
        switch ( weaponType )
        {
            case Weapons.Primary:
                if ( m_primaryEquipped != null )
                {
                    m_primaryEquipped.SetActive ( false );
                }
                m_primaryEquipped = null;
                break;
            case Weapons.Secondary:
                if ( m_secondaryEquipped != null )
                {
                    m_secondaryEquipped.SetActive ( false );
                }
                m_secondaryEquipped = null;
                break;
            default:
                break;
        }
    }

    private void ResetWeaponsAll ()
    {
        ClearWeapon ( Weapons.Primary );
        ClearWeapon ( Weapons.Secondary );
    }
}
