public class Constants
{
    #region Timestep

    public const int TICKS_PER_SECOND = 30;
    public const int MS_PER_TICK = 1000 / TICKS_PER_SECOND;

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

    #region Client Prediction



    #endregion

    #region Interpolation

    // Remote (other) players
    public const float INTERP_POSITION_SPEED = 18.0f;
    public const float INTERP_ROTATION_SPEED = 12.0f;

    #endregion

    #region Inventory

    public const int INVENTORY_RIG_SIZE = 6;
    public const int INVENTORY_BACKPACK_SIZE = 10;
    public const int SLOT_MAX_STACK_SIZE = 256;

    #endregion

    #region Player

    // Respawn
    public const float PLAYER_RESPAWN_DELAY = 3.0f;

    #endregion

    #region Audio

    public const float GUNSHOT_MIN_DISTANCE = 10f;
    public const float GUNSHOT_MAX_DISTANCE = 500f;

    #endregion
}
