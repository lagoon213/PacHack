using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Handles interaction between Pac-Man and power pellets.
/// 
/// Responsibilities:
/// - Detect when Pac-Man enters a power pellet tile
/// - Remove the power pellet from the tilemap
/// - Notify the GhostModeController to trigger Frightened mode
/// 
/// This script should be placed on the Pac-Man GameObject.
/// </summary>
public class PowerPelletPickup : MonoBehaviour
{
    /* =========================
     * References
     * ========================= */

    /// <summary>
    /// Tilemap that contains all power pellets.
    /// </summary>
    [SerializeField] private Tilemap powerPelletTilemap;

    /// <summary>
    /// Reference to the blink controller responsible for
    /// visually toggling power pellet tiles.
    /// Used to permanently remove eaten pellets from the blink cache.
    /// </summary>
    [SerializeField] private PowerPelletBlinkTilemap blink;

    /// <summary>
    /// Central controller that manages ghost modes
    /// (Scatter / Chase / Frightened).
    /// </summary>
    [SerializeField] private GhostModeController ghostModeController;

    /// <summary>
    /// Duration (in seconds) that ghosts remain in Frightened mode
    /// after a power pellet is eaten.
    /// </summary>
    [SerializeField] private float frightenedDuration = 6f;

    /* =========================
     * Unity Lifecycle
     * ========================= */

    private void Update()
    {
        // Determine which tile Pac-Man is currently standing on
        Vector3Int cell = powerPelletTilemap.WorldToCell(transform.position);

        // Check if a power pellet exists on this tile
        if (powerPelletTilemap.HasTile(cell))
        {
            // Remove the pellet visually and from the blink cache
            blink.Consume(cell);

            // Trigger Frightened mode for all ghosts
            ghostModeController.TriggerFrightened(frightenedDuration);
        }
    }
}
