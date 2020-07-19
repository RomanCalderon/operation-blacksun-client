using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Presets;
using InventorySystem.Slots;
using InventorySystem.Slots.Results;
using InventorySystem.PlayerItems;
using System.Collections;

namespace InventorySystem
{
    [Serializable]
    public class Inventory
    {
        // Rig
        [SerializeField]
        private Slot [] m_rigSlots;
        // Backpack
        [SerializeField]
        private Slot [] m_backpackSlots;
        // Primary weapon
        [SerializeField]
        private WeaponSlots m_primaryWeaponSlots;
        // Secondary weapon
        [SerializeField]
        private WeaponSlots m_secondaryWeaponSlots;

        #region Constructors

        public Inventory ()
        {
            m_rigSlots = new Slot [ Constants.INVENTORY_RIG_SIZE ];
            for ( int i = 0; i < m_rigSlots.Length; i++ )
            {
                m_rigSlots [ i ] = new Slot ( "rig-" + i );
            }
            m_backpackSlots = new Slot [ Constants.INVENTORY_BACKPACK_SIZE ];
            for ( int i = 0; i < m_backpackSlots.Length; i++ )
            {
                m_backpackSlots [ i ] = new Slot ( "backpack-" + i );
            }

            m_primaryWeaponSlots = new WeaponSlots ( "primary-weapon", "primary-barrel", "primary-sight", "primary-magazine", "primary-stock" );
            m_secondaryWeaponSlots = new WeaponSlots ( "secondary-weapon", "secondary-barrel", "secondary-sight", "secondary-magazine", "secondary-stock" );

            OnValidate ();
        }

        public Inventory ( Preset preset )
        {
            if ( preset == null )
            {
                return;
            }

            // Initialize Rig slots
            m_rigSlots = new Slot [ Constants.INVENTORY_RIG_SIZE ];
            for ( int i = 0; i < m_rigSlots.Length; i++ )
            {
                if ( i < preset.RigItems.Length )
                {
                    m_rigSlots [ i ] = new Slot ( "rig-" + i, preset.RigItems [ i ] );
                }
                else
                {
                    m_rigSlots [ i ] = new Slot ( "rig-" + i );
                }
            }
            // Initialize Backpack slots
            m_backpackSlots = new Slot [ Constants.INVENTORY_BACKPACK_SIZE ];
            for ( int i = 0; i < m_backpackSlots.Length; i++ )
            {
                m_backpackSlots [ i ] = new Slot ( "backpack-" + i );
            }
            // Add PlayerItems to the backpack
            foreach ( PlayerItemPreset playerItemPreset in preset.BackpackItems )
            {
                Debug.Log ( AddToBackpackAny ( playerItemPreset.PlayerItem, playerItemPreset.Quantity ) );
            }

            // Primary weapon
            m_primaryWeaponSlots = new WeaponSlots ( "primary-weapon", "primary-barrel", "primary-sight", "primary-magazine", "primary-stock",
                preset.PrimaryWeapon.Weapon, preset.PrimaryWeapon.Barrel, preset.PrimaryWeapon.Sight, preset.PrimaryWeapon.Magazine, preset.PrimaryWeapon.Stock );

            // Secondary weapon
            m_secondaryWeaponSlots = new WeaponSlots ( "secondary-weapon", "secondary-barrel", "secondary-sight", "secondary-magazine", "secondary-stock",
                preset.SecondaryWeapon.Weapon, preset.SecondaryWeapon.Barrel, preset.SecondaryWeapon.Sight, preset.SecondaryWeapon.Magazine, preset.SecondaryWeapon.Stock );

            OnValidate ();
        }

        #endregion

        #region Slot access

        /// <summary>
        /// Get the slot with ID <paramref name="slotId"/>.
        /// </summary>
        /// <param name="slotId">The ID of the slot.</param>
        /// <returns>The Slot with ID <paramref name="slotId"/>.</returns>
        public Slot GetSlot ( string slotId )
        {
            // Search rig
            for ( int i = 0; i < m_rigSlots.Length; i++ )
            {
                if ( m_rigSlots [ i ].Id == slotId )
                {
                    return m_rigSlots [ i ];
                }
            }

            // Search backpack
            for ( int i = 0; i < m_backpackSlots.Length; i++ )
            {
                if ( m_backpackSlots [ i ].Id == slotId )
                {
                    return m_backpackSlots [ i ];
                }
            }

            // Search primary weapon slots
            if ( m_primaryWeaponSlots.ContainsSlot ( slotId ) )
            {
                return m_primaryWeaponSlots.GetSlot ( slotId );
            }

            // Search secondary weapon slots
            if ( m_secondaryWeaponSlots.ContainsSlot ( slotId ) )
            {
                return m_secondaryWeaponSlots.GetSlot ( slotId );
            }

            return null;
        }

        public InsertionResult SetSlot ( string slotId, PlayerItem playerItem, int quantity )
        {
            Slot slot = GetSlot ( slotId );

            if ( slot != null )
            {
                if ( playerItem == null ) // Empty slot
                {
                    slot.Clear ();
                    return new InsertionResult ( slot, InsertionResult.Results.SUCCESS );
                }
                slot.Clear ();
                return slot.Insert ( playerItem, quantity ); // Insert PlayerItem
            }
            Debug.LogWarning ( $"Slot [{slotId}] is null." );
            return new InsertionResult ( playerItem, InsertionResult.Results.INSERTION_FAILED );
        }

        public int GetItemCount ( string playerItemId )
        {
            int count = 0;

            // Rig slots
            List<Slot> rigSlots = m_rigSlots.Where ( s => s.PlayerItem && s.PlayerItem.Id == playerItemId ).ToList();
            foreach ( Slot slot in rigSlots )
            {
                count += slot.StackSize;
            }
            // Backpack slots
            List<Slot> backpackSlots = m_backpackSlots.Where ( s => s.PlayerItem && s.PlayerItem.Id == playerItemId ).ToList();
            foreach ( Slot slot in backpackSlots )
            {
                count += slot.StackSize;
            }

            // Weapon slots
            count += m_primaryWeaponSlots.GetItemCount ( playerItemId );
            count += m_secondaryWeaponSlots.GetItemCount ( playerItemId );

            return count;
        }

        #region Rig

        /// <summary>
        /// Add one instance of <paramref name="playerItem"/> into slot <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to add to a rig slot.</param>
        /// <param name="slotIndex">The rig slot index.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToRig ( PlayerItem playerItem, int slotIndex )
        {
            return AddToRig ( playerItem, 1, slotIndex );
        }

        /// <summary>
        /// Add <paramref name="quantity"/> amount of <paramref name="playerItem"/> into slot <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to add to a rig slot.</param>
        /// <param name="quantity">The amount of items to add.</param>
        /// <param name="slotIndex">The rig slot index.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToRig ( PlayerItem playerItem, int quantity, int slotIndex )
        {
            if ( slotIndex < 0 || slotIndex >= m_rigSlots.Length || playerItem == null || quantity <= 0 )
            {
                return new InsertionResult ( playerItem, InsertionResult.Results.INSERTION_FAILED );
            }

            Slot slot = m_rigSlots.FirstOrDefault ( s => s.IsAvailable ( playerItem ) );

            return slot.Insert ( playerItem, quantity );
        }

        /// <summary>
        /// Get the slot at <paramref name="index"/> in the rig.
        /// </summary>
        /// <param name="index">The index of the slot.</param>
        /// <returns>The Slot at <paramref name="index"/> in the rig.</returns>
        public Slot GetFromRig ( int index )
        {
            index = Mathf.Clamp ( index, 0, m_rigSlots.Length - 1 );
            return m_rigSlots [ index ];
        }

        #endregion

        #region Backpack

        /// <summary>
        /// Add <paramref name="quantity"/> amount of <paramref name="playerItem"/> into any available backpack slot or slots.
        /// </summary>
        /// <param name="playerItem">The PlayerItem being added to the backpack.</param>
        /// <param name="quantity">The amount of items being added.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToBackpackAny ( PlayerItem playerItem, int quantity )
        {
            if ( playerItem == null || quantity <= 0 )
            {
                return new InsertionResult ( InsertionResult.Results.INSERTION_FAILED );
            }

            Slot slot = null;
            InsertionResult insertionResult = null;
            do
            {
                slot = m_backpackSlots.FirstOrDefault ( s => s.IsAvailable ( playerItem ) );

                if ( slot != null )
                {
                    // Insert
                    insertionResult = slot.Insert ( playerItem, quantity );

                    // Update quantity
                    quantity = insertionResult.OverflowAmount;

                }
            } while ( slot != null && insertionResult.Result == InsertionResult.Results.OVERFLOW );

            return insertionResult;
        }

        /// <summary>
        /// Add one instance of <paramref name="playerItem"/> into slot <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to add to a backpack slot.</param>
        /// <param name="slotIndex">The backpack slot index.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToBackpack ( PlayerItem playerItem, int slotIndex )
        {
            return AddToBackpack ( playerItem, 1, slotIndex );
        }

        /// <summary>
        /// Add <paramref name="quantity"/> amount of <paramref name="playerItem"/> into slot <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to add to a backpack slot.</param>
        /// <param name="quantity">The amount of items to add.</param>
        /// <param name="slotIndex">The backpack slot index.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToBackpack ( PlayerItem playerItem, int quantity, int slotIndex )
        {
            if ( slotIndex < 0 || slotIndex >= m_rigSlots.Length || playerItem == null || quantity <= 0 )
            {
                return new InsertionResult ( playerItem, InsertionResult.Results.INSERTION_FAILED );
            }

            Slot slot = m_backpackSlots.FirstOrDefault ( s => s.IsAvailable ( playerItem ) );

            return slot.Insert ( playerItem, quantity );
        }

        /// <summary>
        /// Get the slot at <paramref name="index"/> in the backpack.
        /// </summary>
        /// <param name="index">The index of the slot.</param>
        /// <returns>The Slot at <paramref name="index"/> in the backpack.</returns>
        public Slot GetFromBackpack ( int index )
        {
            index = Mathf.Clamp ( index, 0, m_rigSlots.Length - 1 );
            return m_backpackSlots [ index ];
        }

        #endregion

        #endregion

        /// <summary>
        /// Updates the inspector when a change to the inventory is made.
        /// </summary>
        public void OnValidate ()
        {
            // Rig slots
            foreach ( Slot slot in m_rigSlots )
            {
                if ( slot != null )
                {
                    slot.OnValidate ();
                }
            }

            // Backpack slots
            foreach ( Slot slot in m_backpackSlots )
            {
                if ( slot != null )
                {
                    slot.OnValidate ();
                }
            }

            // Primary weapon and attachment slots
            if ( m_primaryWeaponSlots != null )
            {
                if ( m_primaryWeaponSlots.WeaponSlot != null )
                {
                    m_primaryWeaponSlots.WeaponSlot.OnValidate ();
                }
                if ( m_primaryWeaponSlots.BarrelSlot != null )
                {
                    m_primaryWeaponSlots.BarrelSlot.OnValidate ();
                }
                if ( m_primaryWeaponSlots.SightSlot != null )
                {
                    m_primaryWeaponSlots.SightSlot.OnValidate ();
                }
                if ( m_primaryWeaponSlots.MagazineSlot != null )
                {
                    m_primaryWeaponSlots.MagazineSlot.OnValidate ();
                }
                if ( m_primaryWeaponSlots.StockSlot != null )
                {
                    m_primaryWeaponSlots.StockSlot.OnValidate ();
                }
            }

            // Secondary weapon and attachment slots
            if ( m_secondaryWeaponSlots != null )
            {
                if ( m_secondaryWeaponSlots.WeaponSlot != null )
                {
                    m_secondaryWeaponSlots.WeaponSlot.OnValidate ();
                }
                if ( m_secondaryWeaponSlots.BarrelSlot != null )
                {
                    m_secondaryWeaponSlots.BarrelSlot.OnValidate ();
                }
                if ( m_secondaryWeaponSlots.SightSlot != null )
                {
                    m_secondaryWeaponSlots.SightSlot.OnValidate ();
                }
                if ( m_secondaryWeaponSlots.MagazineSlot != null )
                {
                    m_secondaryWeaponSlots.MagazineSlot.OnValidate ();
                }
                if ( m_secondaryWeaponSlots.StockSlot != null )
                {
                    m_secondaryWeaponSlots.StockSlot.OnValidate ();
                }
            }
        }

        #region Models

        /// <summary>
        /// Weapon and attachment slots.
        /// </summary>
        [Serializable]
        public class WeaponSlots
        {
            public WeaponSlot WeaponSlot;
            public BarrelSlot BarrelSlot;
            public SightSlot SightSlot;
            public MagazineSlot MagazineSlot;
            public StockSlot StockSlot;

            public WeaponSlots ( string weaponSlotId, string barrelSlotId, string sightSlotId, string magazineSlotId, string stockSlotId )
            {
                WeaponSlot = new WeaponSlot ( weaponSlotId );
                BarrelSlot = new BarrelSlot ( barrelSlotId );
                SightSlot = new SightSlot ( sightSlotId );
                MagazineSlot = new MagazineSlot ( magazineSlotId );
                StockSlot = new StockSlot ( stockSlotId );
            }

            public WeaponSlots ( string weaponSlotId, string barrelSlotId, string sightSlotId, string magazineSlotId, string stockSlotId,
                Weapon weapon, Barrel barrel, Sight sight, Magazine magazine, Stock stock )
            {
                WeaponSlot = new WeaponSlot ( weaponSlotId, weapon );
                BarrelSlot = new BarrelSlot ( barrelSlotId, barrel );
                SightSlot = new SightSlot ( sightSlotId, sight );
                MagazineSlot = new MagazineSlot ( magazineSlotId, magazine );
                StockSlot = new StockSlot ( stockSlotId, stock );
            }

            public bool ContainsSlot ( string slotId )
            {
                if ( WeaponSlot.Id == slotId )
                {
                    return true;
                }
                if ( BarrelSlot.Id == slotId )
                {
                    return true;
                }
                if ( SightSlot.Id == slotId )
                {
                    return true;
                }
                if ( MagazineSlot.Id == slotId )
                {
                    return true;
                }
                if ( StockSlot.Id == slotId )
                {
                    return true;
                }
                return false;
            }

            public Slot GetSlot ( string slotId )
            {
                if ( WeaponSlot.Id == slotId )
                {
                    return WeaponSlot;
                }
                if ( BarrelSlot.Id == slotId )
                {
                    return BarrelSlot;
                }
                if ( SightSlot.Id == slotId )
                {
                    return SightSlot;
                }
                if ( MagazineSlot.Id == slotId )
                {
                    return MagazineSlot;
                }
                if ( StockSlot.Id == slotId )
                {
                    return StockSlot;
                }
                return null;
            }

            public int GetItemCount ( string playerItemId )
            {
                int count = 0;

                if ( WeaponSlot.PlayerItem && WeaponSlot.PlayerItem.Id == playerItemId )
                {
                    count++;
                }
                if ( BarrelSlot.PlayerItem && BarrelSlot.PlayerItem.Id == playerItemId )
                {
                    count++;
                }
                if ( SightSlot.PlayerItem && SightSlot.PlayerItem.Id == playerItemId )
                {
                    count++;
                }
                if ( MagazineSlot.PlayerItem && MagazineSlot.PlayerItem.Id == playerItemId )
                {
                    count++;
                }
                if ( StockSlot.PlayerItem && StockSlot.PlayerItem.Id == playerItemId )
                {
                    count++;
                }

                return count;
            }
        }

        #endregion
    }
}
