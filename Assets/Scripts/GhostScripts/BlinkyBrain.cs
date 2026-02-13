using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// AI brain for Blinky (the red ghost).
///
/// Blinky's behavior:
/// - In Chase mode, he directly targets Pac-Man's current tile
/// - In Scatter mode, behavior is handled by ScatterChaseBrain
/// - In Frightened mode, behavior is handled by ScatterChaseBrain
///
/// This makes Blinky the most aggressive ghost,
/// as he always homes in on Pac-Man's exact position.
/// </summary>
public class BlinkyBrain : ScatterChaseBrain
{
    /* =========================
     * References
     * ========================= */

    /// <summary>
    /// Reference to Pac-Man's transform.
    /// Used to determine Pac-Man's current tile position.
    /// </summary>
    [SerializeField] private Transform pacman;

    /* =========================
     * Chase Logic
     * ========================= */

    /// <summary>
    /// Returns the target tile Blinky should chase.
    /// 
    /// Blinky always targets Pac-Man's current tile,
    /// making him the most direct and aggressive ghost.
    /// </summary>
    protected override Vector2Int GetChaseTargetTile()
    {
        // Convert Pac-Man's world position to tile coordinates
        Vector3Int pacCell3 = walls.WorldToCell(pacman.position);

        // Use Pac-Man's exact tile as chase target
        return new Vector2Int(pacCell3.x, pacCell3.y);
    }
}
