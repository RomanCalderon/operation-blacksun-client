using UnityEngine;
using InventorySystem.PlayerItems;

public class Constants
{
    #region Audio

    public const float GUNSHOT_MIN_DISTANCE = 10f;
    public const float GUNSHOT_MAX_DISTANCE = 500f;

    #endregion

    #region Input

    /// <summary>
    /// Total number of input that is being
    /// sent across the network from this client:
    /// - Move forward
    /// - Move backward
    /// - Move right
    /// - Move left
    /// - Jump
    /// - Run
    /// - Crouch
    /// - Rotational orientation
    /// </summary>
    public const int NUM_PLAYER_INPUTS = 8;

    // Player camera control
    public const string MOUSE_HORIZONTAL_INPUT = "Mouse X";
    public const string MOUSE_VERTICAL_INPUT = "Mouse Y";

    #endregion

    #region Inventory

    public const int INVENTORY_RIG_SIZE = 6;
    public const int INVENTORY_BACKPACK_SIZE = 10;
    public const int SLOT_MAX_STACK_SIZE = 256;

    #endregion

    #region Interpolation

    // Remote (other) players
    public const float INTERP_POSITION_SPEED = 18.0f;
    public const float INTERP_ROTATION_SPEED = 12.0f;

    #endregion

    #region Items

    public static Color StandardRarityColor { get => Color.white; }
    public static Color AdvancedRarityColor { get => new Color ( 0.298f, 0.49f, 0.603f, 1f ); }
    public static Color EpicRarityColor { get => new Color ( 0.945f, 0.878f, 0.439f, 1f ); }
    public static Color CosmicRarityColor { get => new Color ( 0.725f, 0.266f, 0.388f, 1f ); }

    public static Color RarityToColor ( Rarity rarity )
    {
        switch ( rarity )
        {
            case Rarity.STANDARD:
                return StandardRarityColor;
            case Rarity.ADVANCED:
                return AdvancedRarityColor;
            case Rarity.EPIC:
                return EpicRarityColor;
            case Rarity.COSMIC:
                return CosmicRarityColor;
            default:
                return StandardRarityColor;
        }
    }

    public static string RarityToColorHex ( Rarity rarity )
    {
        switch ( rarity )
        {
            case Rarity.STANDARD:
                return ColorUtility.ToHtmlStringRGBA ( StandardRarityColor );
            case Rarity.ADVANCED:
                return ColorUtility.ToHtmlStringRGBA ( AdvancedRarityColor );
            case Rarity.EPIC:
                return ColorUtility.ToHtmlStringRGBA ( EpicRarityColor );
            case Rarity.COSMIC:
                return ColorUtility.ToHtmlStringRGBA ( CosmicRarityColor );
            default:
                return ColorUtility.ToHtmlStringRGBA ( Color.white );
        }
    }

    #endregion

    #region Player

    // Respawn
    public const float PLAYER_RESPAWN_DELAY = 3.0f;

    #endregion

    #region Timestep

    public const int TICKS_PER_SECOND = 60;
    public const int MS_PER_TICK = 1000 / TICKS_PER_SECOND;

    #endregion
}
