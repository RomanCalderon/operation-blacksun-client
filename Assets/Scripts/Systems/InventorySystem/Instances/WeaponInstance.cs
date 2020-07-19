using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using InventorySystem.PlayerItems;

public class WeaponInstance : PlayerItemInstance
{
    public Barrel Barrel { get; private set; } = null;
    public Magazine Magazine { get; private set; } = null;
    public Stock Stock { get; private set; } = null;
    public int BulletCount { get; private set; } = 0;

    [Header ( "Audio" )]
    [SerializeField]
    private AudioClip m_normalGunshotClip = null;
    [SerializeField]
    private float m_normalGunshotVolume = 0.65f, m_normalSpatialBlend = 0.2f;
    [Space]
    [SerializeField]
    private AudioClip m_suppressedGunshotClip = null;
    [SerializeField]
    private float m_suppressedGunshotVolume = 0.6f, m_suppressedSpatialBlend = 0.2f;
    [Space]
    [SerializeField]
    private AudioClip m_boltChargeClip = null;
    [SerializeField]
    private float m_boltChargeVolume = 0.75f;
    [SerializeField]
    private AudioClip m_partialReloadClip = null;
    [SerializeField]
    private float m_partialReloadVolume = 0.75f;
    [SerializeField]
    private AudioClip m_fullReloadClip = null;
    [SerializeField]
    private float m_fullReloadVolume = 0.75f;

    private float m_fireCooldown = 0f;

    [Header ( "Reloading" )]
    [SerializeField]
    private float m_partialReloadTime = 0.5f;
    [SerializeField]
    private float m_fullReloadTime = 0.8f;
    private bool m_isReloading = false;
    private bool m_isFullReload = false;
    private bool m_cancelReload = false;
    private Coroutine m_reloadCoroutine = null;


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

    /// <summary>
    /// Invoked by WeaponsController.
    /// </summary>
    /// <param name="attachment"></param>
    public void EquipAttachment ( Attachment attachment )
    {
        if ( attachment is Barrel barrel )
        {
            Barrel = barrel;
            Debug.Log ( $"Weapon [{PlayerItem}] - Barrel [{Barrel}]" );
        }
        else if ( attachment is Magazine magazine )
        {
            Magazine = magazine;
            BulletCount = Magazine.AmmoCapacity;
            Debug.Log ( $"Weapon [{PlayerItem}] - Magazine [{Magazine}]" );
        }
        else if ( attachment is Stock stock )
        {
            Stock = stock;
            Debug.Log ( $"Weapon [{PlayerItem}] - Stock [{Stock}]" );
        }
    }

    /// <summary>
    /// Fires a single round in a specified direction.
    /// Invoked by WeaponsController.
    /// </summary>
    /// <param name="direction">The direction the weapon is fired.</param>
    public void Shoot ( Vector3 direction )
    {
        if ( m_fireCooldown <= 0f )
        {
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
                ClientSend.PlayerShoot ( direction );
                WeaponsController.OnSetTrigger?.Invoke ( "Shoot" );
                CameraShaker.Instance.ShakeOnce ( 0.1f, 6f, 0.01f, 0.16f );
                CameraController.Instance.AddRecoil ( 0.35f, Random.Range ( -0.25f, 0.25f ) );

                if ( BulletCount > 0 )
                {
                    WeaponsController.OnSetTrigger?.Invoke ( "BoltCharge" );
                }
            }
        }
    }

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
        // TODO: Implement this
        //if (  ) // Out of compatible ammo in inventory
        //{
        //    return;
        //}

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

    #region WeaponStateBehaviour Listeners

    private void StartAudioClip ( AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( stateInfo.IsName ( "BoltCharge" ) )
        {
            AudioManager.PlaySound ( m_boltChargeClip, m_boltChargeVolume, false );
        }
        if ( stateInfo.IsName ( "ReloadPartial" ) )
        {
            AudioManager.PlaySound ( m_partialReloadClip, m_partialReloadVolume, false );
        }
        if ( stateInfo.IsName ( "ReloadFull" ) )
        {
            AudioManager.PlaySound ( m_fullReloadClip, m_fullReloadVolume, false );
        }
    }

    private void StopAudioClip ( AnimatorStateInfo stateInfo, int layerIndex )
    {
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
    }

    private void CancelReloadAnimation ( AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( stateInfo.IsName ( "Holster" ) )
        {
            CancelReload ();

            // Stop the reload audio clip
            string clipName = m_isFullReload ? m_fullReloadClip.name : m_partialReloadClip.name;
            AudioManager.Stop ( clipName );
        }
    }

    #endregion

    private IEnumerator ReloadCoroutine ( float reloadTime )
    {
        yield return new WaitForSeconds ( reloadTime );

        // TODO: Only provide a minimum amount between the mags ammo
        // capacity and the amount of compatible bullets in the player's inventory
        // Example: BulletCount = Mathf.Min ( Magazine.AmmoCapacity, Inventory.ItemCount ( AmmoType ) );
        BulletCount = Magazine.AmmoCapacity;

        // Reset reload flags
        m_isReloading = false;
        m_isFullReload = false;
    }

    private void CancelReload ()
    {
        if ( m_isReloading || m_reloadCoroutine != null )
        {
            // Stop the reload coroutine
            StopCoroutine ( m_reloadCoroutine );
            m_reloadCoroutine = null;

            // Reset reload flags
            m_isReloading = false;
            m_isFullReload = false;
        }
    }
}
