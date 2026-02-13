using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// AI brain for Pinky (the pink ghost).
///
/// Pinky's behavior:
/// - In Chase mode, Pinky targets a tile a few steps ahead of Pac-Man
///   to "ambush" instead of directly following.
/// - In Scatter mode, behavior is handled by ScatterChaseBrain.
/// - In Frightened mode, behavior is handled by ScatterChaseBrain.
///
/// Note:
/// The original arcade version has a known quirk where targeting while moving "Up"
/// shifts the target to the left as well. This implementation uses the common
/// simplified version: 4 tiles directly ahead of Pac-Man's current direction.
/// </summary>
public class PinkyBrain : ScatterChaseBrain
{
    /* =========================
     * References
     * ========================= */

    /// <summary>
    /// Reference to Pac-Man movement, used for both position and current direction.
    /// </summary>
    [SerializeField] private PacManMovement pacman;

    /* =========================
     * Chase Logic
     * ========================= */

    /// <summary>
    /// Returns Pinky's chase target tile.
    /// 
    /// Pinky aims for the tile 4 steps ahead of Pac-Man's current direction.
    /// This produces a predictive, ambush-style chase pattern.
    /// </summary>
    protected override Vector2Int GetChaseTargetTile()
    {
        // Convert Pac-Man's world position to tile coordinates
        Vector3Int pacCell3 = walls.WorldToCell(pacman.transform.position);
        Vector2Int pacCell = new Vector2Int(pacCell3.x, pacCell3.y);

        // Use Pac-Man's current direction as prediction vector
        Vector2Int dir = pacman.CurrentDir;

        // Fallback when Pac-Man is idle (prevents targeting "no direction")
        if (dir == Vector2Int.zero)
            dir = Vector2Int.right;

        // Target 4 tiles ahead (predictive chase)
        return pacCell + dir * 4;
    }
}
