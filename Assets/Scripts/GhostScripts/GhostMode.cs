using UnityEngine;

/// <summary>
/// Represents the current behavioral mode of a ghost.
/// 
/// The active GhostMode determines:
/// - How the ghost selects its movement target
/// - Which speed multipliers are applied
/// - Which animations are played
/// </summary>
public enum GhostMode
{
    /// <summary>
    /// Scatter mode:
    /// Ghosts target their individual corner tiles.
    /// Used to periodically relieve pressure on the player.
    /// </summary>
    Scatter,

    /// <summary>
    /// Chase mode:
    /// Ghosts actively pursue Pac-Man using their unique AI logic.
    /// This is the primary threat state.
    /// </summary>
    Chase,

    /// <summary>
    /// Frightened mode:
    /// Triggered when Pac-Man eats a power pellet.
    /// Ghosts move erratically at reduced speed and can be eaten.
    /// </summary>
    Frightened,

    /// <summary>
    /// Returning mode (Eyes):
    /// Activated after a ghost is eaten.
    /// The ghost returns to the ghost house at high speed.
    /// </summary>
    Returning
}
