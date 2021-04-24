using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using EZCameraShake;
using InventorySystem.PlayerItems;


public class WeaponInstance : PlayerItemInstance
{
    public delegate void AimSpeedHandler ( float aimSpeed );
    public static AimSpeedHandler OnUpdatedAimSpeed;
    public delegate void AimZoomHandler ( float zoomAmount );
    public static AimZoomHandler OnUpdatedAimZoomAmount;

    private Player m_player = null;

    public Barrel Barrel { get; private set; } = null;
    public Magazine Magazine { get; private set; } = null;
    public Sight Sight { get; private set; } = null;
    public Stock Stock { get; private set; } = null;

    [Space]
    [SerializeField]
    private int m_ironsightIndex = 0;
    [SerializeField]
    private AttachmentsController m_attachmentsController = null;

    private CameraController m_cameraController = null;
    private AimController m_aimController = null;
    public AimListener AimListener { get => m_aimListener; }
    [SerializeField]
    private AimListener m_aimListener = null;

    // Ammo
    public int BulletCount { get; private set; } = 0;
    private string m_ammoId;

    #region Audio Properties

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

    #endregion

    [Header ( "Reloading" )]
    [SerializeField]
    private float m_partialReloadTime = 0.5f;
    [SerializeField]
    private float m_fullReloadTime = 0.8f;
    private bool m_isReloading = false;
    private bool m_isFullReload = false;
    private Coroutine m_reloadCoroutine = null;

    private float m_fireCooldown = 0f;

    private Vector3 m_lookDirection;


    private void OnEnable ()
    {
        WeaponStateBehaviour.OnStateEntered += StartAudioClip;
        WeaponStateBehaviour.OnStateExited += StopAudioClip;
        WeaponStateBehaviour.OnStateEntered += CancelReloadAnimation;

        // Update aim speed and zoom amount
        UpdateAimController ();
    }

    private void OnDisable ()
    {
        WeaponStateBehaviour.OnStateEntered -= StartAudioClip;
        WeaponStateBehaviour.OnStateExited -= StopAudioClip;
        WeaponStateBehaviour.OnStateEntered -= CancelReloadAnimation;

        // Cancel reload if this gun gets disabled
        CancelReload ();

        // Reset aim FOV target
        AimController.CanAim = false;
    }

    // Start is called before the first frame update
    void Start ()
    {
        m_cameraController = CameraController.Instance;
        m_attachmentsController.Initialize ( m_ironsightIndex );
    }

    public void Initialize ( Player player, AimController aimController )
    {
        m_player = player;

        m_ammoId = m_player.InventoryManager.PlayerItemDatabase.GetAmmoByCaliber ( ( PlayerItem as Weapon ).Caliber );

        m_aimController = aimController;
        UpdateAttachments ();
        UpdateAimController ();
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

        // Update look direction every frame
        m_lookDirection = m_aimListener.GetAimVector ();
        PlayerInput.PlayerInputController.SetLookDirection ( m_lookDirection );
    }

    public void UpdateAimController ()
    {
        if ( m_aimController != null )
        {
            if ( Sight != null )
            {
                float aimSpeed = 10f / ( Mathf.Sqrt ( Sight.SightZoomStrength ) * 0.5f );
                float aimZoomAmount = 72f / Sight.PlayerZoomModifier;
                m_aimController.UpdateAimSpeed ( aimSpeed );
                m_aimController.UpdateAimFOV ( aimZoomAmount );
            }
            else
            {
                m_aimController.UpdateAimSpeed ( 20f );
                m_aimController.UpdateAimFOV ( 72f );
            }
        }
    }

    #region Attachment Equipping/Unequipping

    public void UpdateAttachments ()
    {
        m_attachmentsController.Initialize ( m_ironsightIndex );
        m_attachmentsController.UpdateAttachment ( Barrel );
        m_attachmentsController.UpdateAttachment ( Magazine );
        m_attachmentsController.UpdateAttachment ( Sight );
        m_attachmentsController.UpdateAttachment ( Stock );
    }

    /// <summary>
    /// Equips a Barrel to this WeaponInstance.
    /// </summary>
    /// <param name="barrel">The Barrel to equip.</param>
    public void EquipAttachment ( Barrel barrel )
    {
        Barrel = barrel;

        // Update attachment instance
        m_attachmentsController.UpdateAttachment ( Barrel );
    }

    /// <summary>
    /// Equips a Magazine to this WeaponInstance.
    /// </summary>
    /// <param name="magazine">The Magazine to equip.</param>
    public void EquipAttachment ( Magazine magazine )
    {
        Magazine = magazine;

        // Update attachment instance
        m_attachmentsController.UpdateAttachment ( Magazine );

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

    public void EquipAttachment ( Sight sight )
    {
        Sight = sight;
        Debug.Log ( $"Weapon [{PlayerItem}] - Sight equipped [{Sight}]" );

        // Update attachment instance
        m_attachmentsController.UpdateAttachment ( Sight );

        // Update ADS point
        m_aimListener.UpdateADSPointTarget ();

        // Update aim speed and zoom amount
        UpdateAimController ();
    }

    /// <summary>
    /// Equips a Stock to this WeaponInstance.
    /// </summary>
    /// <param name="stock">The Stock to equip.</param>
    public void EquipAttachment ( Stock stock )
    {
        Stock = stock;

        // Update attachment instance
        m_attachmentsController.UpdateAttachment ( Stock );
    }

    #endregion

    #region Shooting

    /// <summary>
    /// Fires a single round in a specified direction.
    /// Invoked by WeaponsController.
    /// </summary>
    /// <param name="direction">The direction the weapon is fired.</param>
    public void Shoot ()
    {
        if ( m_isReloading && !m_isFullReload )
        {
            CancelReload ();
            return;
        }
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
            // Reset fireCooldown
            m_fireCooldown = ( PlayerItem as Weapon ).FireRate;

            // Subtract one bullet from the magazine
            BulletCount--;

            // Play gunshot Audioclip
            AudioManager.PlaySound ( m_normalGunshotClip, m_normalGunshotVolume, false, m_normalSpatialBlend, transform.position );

            // Perform gunshot

            // Send shoot command to server
            ClientSend.WeaponShoot ( Client.instance.ServerTick, m_player.PositionLerpProgress );

            // Play animation
            if ( BulletCount == 0 )
            {
                WeaponsController.OnSetTrigger?.Invoke ( "ShootFinal" );
            }
            else
            {
                WeaponsController.OnSetTrigger?.Invoke ( "Shoot" );
                WeaponsController.OnSetTrigger?.Invoke ( "BoltCharge" );
            }
            CameraRecoil ();
        }
        else
        {
            DryFire ();
        }
    }

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
        CameraShaker.Instance.ShakeOnce ( recoilStrength * 0.045f, 4f, 0.01f, 0.16f );
    }

    private void DryFire ()
    {
        if ( Input.GetKeyDown ( PlayerInput.PlayerInputController.PrimaryButton ) )
        {
            AudioManager.PlaySound ( m_dryFireClip, m_mixerGroup, m_dryfireVolume, false );
        }
    }

    #endregion

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
        int inventoryAmmoCount = InventoryManager.Instance.GetItemCount ( m_ammoId );
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
            m_reloadCoroutine = StartCoroutine ( ReloadCoroutine ( m_fullReloadTime, inventoryAmmoCount ) );
        }
        else
        {
            WeaponsController.OnSetTrigger?.Invoke ( "ReloadPartial" );
            m_reloadCoroutine = StartCoroutine ( ReloadCoroutine ( m_partialReloadTime, inventoryAmmoCount ) );
        }
    }

    private IEnumerator ReloadCoroutine ( float reloadTime, int inventoryAmmoCount )
    {
        yield return new WaitForSeconds ( reloadTime );

        FinishReload ( inventoryAmmoCount );
    }

    private void FinishReload ( int inventoryAmmoCount )
    {
        if ( !m_isReloading )
        {
            return;
        }

        int shotsFired = Magazine.AmmoCapacity - BulletCount;
        int refillAmount = Mathf.Min ( shotsFired, inventoryAmmoCount );
        BulletCount += refillAmount;

        // Reset reload flags
        m_isReloading = false;
        m_isFullReload = false;
    }

    private void CancelReload ()
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

    #endregion

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
