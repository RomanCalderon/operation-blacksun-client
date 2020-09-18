using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    public const int TICKS_PER_SECOND = 30;
    public const int MS_PER_TICK = 1000 / TICKS_PER_SECOND;

    // INPUT
    public const int NUM_PLAYER_INPUTS = 8;

    // CLIENT PREDICTION
    // Known good values:
    // Velocity Tolerance - 2.5f
    // Converge Multiplier - 1.02f
    public const float VELOCITY_TOLERANCE = 2.5f;
    public const float PLAYER_CONVERGE_MULTIPLIER = 1.5f;

    // INTERPOLATION
    public const float INTERP_POSITION_SPEED = 18.0f;
    public const float INTERP_ROTATION_SPEED = 12.0f;

    // INVENTORY
    public const int INVENTORY_RIG_SIZE = 6;
    public const int INVENTORY_BACKPACK_SIZE = 10;
    public const int SLOT_MAX_STACK_SIZE = 256;

    public const float PLAYER_RESPAWN_DELAY = 3.0f;

    public const float GUNSHOT_MIN_DISTANCE = 10f;
    public const float GUNSHOT_MAX_DISTANCE = 500f;
}
