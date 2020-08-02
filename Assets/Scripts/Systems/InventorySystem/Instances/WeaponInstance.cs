using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using EZCameraShake;
using InventorySystem.PlayerItems;

public class WeaponInstance : PlayerItemInstance
{
    public Barrel Barrel { get; private set; } = null;
    public Magazine Magazine { get; private set; } = null;
    public Stock Stock { get; private set; } = null;
    public int BulletCount { get; private set; } = 0;

    private CameraController m_cameraController = null;

    [Header ( "Audio" )]
    [SerializeField]
    private AudioMixerGroup m_mixerGroup = null;
    [Space]
    [SerializeField]
    private string m_normalGunshotClip = null;
    [SerializeField]
    private float m_normalGunshotVolume = 0.65f, m_normalSpatialBlend = 0.2f;
    [Space]
    [SerializeField]
    private AudioClip m_suppressedGunshotClip = null;
    [SerializeField]
    private float m_suppressedGunshotVolume = 0.6f, m_suppressedSpatialBlend = 0.2f;
    [Space]
    [SerializeField]
    private AudioClip m_drawClip = null;
    [SerializeField]
    private float m_drawVolume = 0.75f;
    [Space]
    [SerializeField]
    private AudioClip m_boltChargeClip = null;
    [SerializeField]
    private float m_boltChargeVolume = 0.75f;
    [Space]
    [SerializeField]
    private AudioClip m_partialReloadClip = null;
    [SerializeField]
    private float m_partialReloadVolume = 0.75f;
    [Space]
    [SerializeField]
    private AudioClip m_fullReloadClip = null;
    [SerializeField]
    private float m_fullReloadVolume = 0.75f;
    [Space]
    [SerializeField]
    private AudioClip m_holsterClip = null;
    [SerializeField]
    private float m_holsterVolume = 0.75f;
    [Space]
    [SerializeField]
    private AudioClip m_dryFireClip = null;
    [SerializeField]
    private float m_dryfireVolume = 0.75f;

    [Header ( "Reloading" )]
    [SerializeField]
    private float m_partialReloadTime = 0.5f;
    [SerializeField]
    private float m_fullReloadTime = 0.8f;
    private bool m_isReloading = false;
    private bool m_isFullReload = false;
    private Coroutine m_reloadCoroutine = null;

    private float m_fireCooldown = 0f;


    private void OnEnable ()
    {
        WeaponStateBehaviour.OnStateEntered += StartAudioClip;
        WeaponStateBehaviour.OnStateExited += StopAudioClip;
        WeaponStateBehaviour.OnStateEntered += CancelReloadAnimation;
    }

    private void OnDisable ()
    {
        WeaponStateBehaviour.OnStateEntered -= StartAudioClip;
        WeaponStateBehaviour.OnStateExited -= StopAudioClip;
        WeaponStateBehaviour.OnStateEntered -= CancelReloadAnimation;

        // Cancel reload if this gun gets disabled
        CancelReload ();
    }

    // Start is called before the first frame update
    void Start ()
    {
        m_cameraController = CameraController.Instance;
    }

    // Update is called once per frame
    void Update ()
    {
        if ( m_fireCooldown > 0f )
        {
            m_fireCooldown -= Time.deltaTime;
        }
        else
        {
            m_fireCooldown = 0f;
        }
    }

    #region Attachment Equipping/Unequipping

    /// <summary>
    /// Equips a Barrel to this WeaponInstance.
    /// </summary>
    /// <param name="barrel">The Barrel to equip.</param>
    public void EquipAttachment ( Barrel barrel )
    {
        Barrel = barrel;
    }

    /// <summary>
    /// Equips a Magazine to this WeaponInstance.
    /// </summary>
    /// <param name="magazine">The Magazine to equip.</param>
    public void EquipAttachment ( Magazine magazine )
    {
        Magazine = magazine;

        if ( Magazine == null )
        {
            CancelReload ();
        }
        else if ( BulletCount == 0 )
        {
            string ammoId = InventoryManager.Instance.PlayerItemDatabase.GetAmmoByCaliber ( ( PlayerItem as Weapon ).Caliber );
            int inventoryAmmoCount = InventoryManager.Instance.GetItemCount ( ammoId );
            BulletCount = Mathf.Min ( Magazine.AmmoCapacity, inventoryAmmoCount );
        }
    }

    /// <summary>
    /// Equips a Stock to this WeaponInstance.
    /// </summary>
    /// <param name="stock">The Stock to equip.</param>
    public void EquipAttachment ( Stock stock )
    {
        Stock = stock;
    }

    #endregion

    /// <summary>
    /// Fires a single round in a specified direction.
    /// Invoked by WeaponsController.
    /// </summary>
    /// <param name="direction">The direction the weapon is fired.</param>
    public void Shoot ( Vector3 position, Vector3 direction )
    {
        if ( Magazine == null )
        {
            DryFire ();
            return;
        }
        if ( m_fireCooldown > 0f )
        {
            return;
        }

        // Check bullet count
        if ( BulletCount > 0 )
        {
            // Cancel the reload (if reloading)
            CancelReload ();

            // Reset fireCooldown
            m_fireCooldown = ( PlayerItem as Weapon ).FireRate;

            // Subtract one bullet from the magazine
            BulletCount--;

            // Play gunshot Audioclip
            AudioManager.PlaySound ( m_normalGunshotClip, m_normalGunshotVolume, false, m_normalSpatialBlend, transform.position );

            // Perform gunshot
            float damage = ( PlayerItem as Weapon ).BaseDamage;
            ClientSend.PlayerShoot ( direction, damage, m_normalGunshotClip, m_normalGunshotVolume, Constants.GUNSHOT_MIN_DISTANCE, Constants.GUNSHOT_MAX_DISTANCE );
            WeaponsController.OnSetTrigger?.Invoke ( "Shoot" );
            CameraRecoil ();

            if ( BulletCount > 0 )
            {
                WeaponsController.OnSetTrigger?.Invoke ( "BoltCharge" );
            }
        }
        else
        {
            DryFire ();
        }
    }

    private void DryFire ()
    {
        if ( Input.GetKeyDown ( KeyCode.Mouse0 ) )
        {
            AudioManager.PlaySound ( m_dryFireClip, m_mixerGroup, m_dryfireVolume, false );
        }
    }

    #region Reloading

    /// <summary>
    /// Invoked by WeaponsController.
    /// </summary>
    public void Reload ()
    {
        if ( m_isReloading ) // Already reloading
        {
            return;
        }
        if ( Magazine == null ) // No magazine
        {
            return;
        }
        if ( BulletCount == Magazine.AmmoCapacity ) // Full ammo capacity
        {
            return;
        }
        string ammoId = InventoryManager.Instance.PlayerItemDatabase.GetAmmoByCaliber ( ( PlayerItem as Weapon ).Caliber );
        int inventoryAmmoCount = InventoryManager.Instance.GetItemCount ( ammoId );
        if ( inventoryAmmoCount == 0 ) // Out of compatible ammo in inventory
        {
            return;
        }

        // Set reloading flag
        m_isReloading = true;

        // If a full reload is required
        m_isFullReload = BulletCount == 0;

        if ( m_isFullReload )
        {
            WeaponsController.OnSetTrigger?.Invoke ( "ReloadFull" );
            m_reloadCoroutine = StartCoroutine ( ReloadCoroutine ( m_fullReloadTime ) );
        }
        else
        {
            WeaponsController.OnSetTrigger?.Invoke ( "ReloadPartial" );
            m_reloadCoroutine = StartCoroutine ( ReloadCoroutine ( m_partialReloadTime ) );
        }
    }

    private IEnumerator ReloadCoroutine ( float reloadTime )
    {
        yield return new WaitForSeconds ( reloadTime );

        FinishReload ();
    }

    private void FinishReload ()
    {
        string ammoId = InventoryManager.Instance.PlayerItemDatabase.GetAmmoByCaliber ( ( PlayerItem as Weapon ).Caliber );
        int inventoryAmmoCount = InventoryManager.Instance.GetItemCount ( ammoId );
        int shotsFired = Magazine.AmmoCapacity - BulletCount;
        int refillAmount = Mathf.Min ( shotsFired, inventoryAmmoCount );
        ClientSend.PlayerInventoryReduceItem ( ammoId, shotsFired );
        BulletCount += refillAmount;

        // Reset reload flags
        m_isReloading = false;
        m_isFullReload = false;
    }

    private void CancelReload ()
    {
        if ( m_isReloading )
        {
            // Stop the reload animation
            WeaponsController.OnSetTrigger?.Invoke ( "CancelReload" );

            // Stop the reload coroutine
            if ( m_reloadCoroutine != null )
            {
                StopCoroutine ( m_reloadCoroutine );
                m_reloadCoroutine = null;
            }

            // Stop the reload audio clip
            string clipName = m_isFullReload ? m_fullReloadClip.name : m_partialReloadClip.name;
            AudioManager.Stop ( clipName );

            // Reset reload flags
            m_isReloading = false;
            m_isFullReload = false;
        }
    }

    #endregion

    private void CameraRecoil ()
    {
        // Camera recoil
        float recoilStrength = WeaponsController.CalculateRecoilStrength ( ( PlayerItem as Weapon ).WeaponClass, ( PlayerItem as Weapon ).Caliber, out float aspect );
        if ( Stock != null ) // Use Stock to reduce recoil
        {
            recoilStrength *= 1f - Stock.RecoilReductionModifier;
        }
        m_cameraController.AddRecoil ( recoilStrength, recoilStrength * Random.Range ( -aspect, aspect ) );

        // Camera shake
        CameraShaker.Instance.ShakeOnce ( 0.1f, 6f, 0.01f, 0.16f );
    }

    #region WeaponStateBehaviour Listeners

    private void StartAudioClip ( AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( stateInfo.IsName ( "Draw" ) )
        {
            AudioManager.PlaySound ( m_drawClip, m_mixerGroup, m_drawVolume, false );
        }
        if ( stateInfo.IsName ( "BoltCharge" ) )
        {
            AudioManager.PlaySound ( m_boltChargeClip, m_mixerGroup, m_boltChargeVolume, false );
        }
        if ( stateInfo.IsName ( "ReloadPartial" ) )
        {
            AudioManager.PlaySound ( m_partialReloadClip, m_mixerGroup, m_partialReloadVolume, false );
        }
        if ( stateInfo.IsName ( "ReloadFull" ) )
        {
            AudioManager.PlaySound ( m_fullReloadClip, m_mixerGroup, m_fullReloadVolume, false );
        }
        if ( stateInfo.IsName ( "Holster" ) )
        {
            AudioManager.PlaySound ( m_holsterClip, m_mixerGroup, m_holsterVolume, false );
        }
    }

    private void StopAudioClip ( AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( stateInfo.IsName ( "Draw" ) )
        {
            AudioManager.Stop ( m_drawClip.name );
        }
        if ( stateInfo.IsName ( "BoltCharge" ) )
        {
            AudioManager.Stop ( m_boltChargeClip.name );
        }
        if ( stateInfo.IsName ( "ReloadPartial" ) )
        {
            AudioManager.Stop ( m_partialReloadClip.name );
        }
        if ( stateInfo.IsName ( "ReloadFull" ) )
        {
            AudioManager.Stop ( m_fullReloadClip.name );
        }
        if ( stateInfo.IsName ( "Holster" ) )
        {
            AudioManager.Stop ( m_holsterClip.name );
        }
    }

    private void CancelReloadAnimation ( AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( stateInfo.IsName ( "Holster" ) )
        {
            CancelReload ();
        }
    }

    #endregion

}
