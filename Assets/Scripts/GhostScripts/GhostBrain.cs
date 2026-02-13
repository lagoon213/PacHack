using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Base class for all ghost AI brains.
///
/// Responsibilities:
/// - Define the contract for deciding a movement direction
/// - Provide shared helper logic for choosing a direction toward a target tile
///
/// Concrete ghost brains (Blinky, Pinky, Inky, Clyde) should inherit from this
/// and only implement their specific targeting logic.
/// </summary>
public abstract class GhostBrain : MonoBehaviour
{
    /* =========================
     * Direction priority
     * ========================= */

    /// <summary>
    /// Direction priority order used when multiple paths have equal distance.
    /// 
    /// This order mimics classic Pac-Man behavior:
    /// Up → Left → Down → Right.
    /// </summary>
    protected static readonly Vector2Int[] PriorityDirs =
    {
        Vector2Int.up,
        Vector2Int.left,
        Vector2Int.down,
        Vector2Int.right
    };

    /* =========================
     * Public API
     * ========================= */

    /// <summary>
    /// Determines the desired movement direction for the ghost.
    /// 
    /// This method is called by GhostMovement whenever the ghost
    /// reaches the center of a tile.
    /// </summary>
    /// <param name="motor">
    /// Reference to the GhostMovement component, providing
    /// information such as current direction and valid movement checks.
    /// </param>
    /// <returns>
    /// The direction the ghost would like to move in.
    /// </returns>
    public abstract Vector2Int GetDesiredDir(GhostMovement motor);

    /* =========================
     * Shared pathfinding logic
     * ========================= */

    /// <summary>
    /// Chooses the best direction that moves the ghost closer to a target tile.
    /// 
    /// Algorithm:
    /// - Evaluate all valid directions except immediate reversal
    /// - Measure squared distance from the next tile to the target tile
    /// - Select the direction with the smallest distance
    /// - Use PriorityDirs to break ties deterministically
    /// 
    /// This matches the original Pac-Man ghost decision logic.
    /// </summary>
    protected Vector2Int ChooseDirTowardTarget(
        GhostMovement motor,
        Tilemap walls,
        Vector2Int targetTile
    )
    {
        // Current ghost tile position
        Vector3Int myCell3 = walls.WorldToCell(motor.transform.position);
        Vector2Int myCell = new Vector2Int(myCell3.x, myCell3.y);

        Vector2Int bestDir = Vector2Int.zero;
        int bestDist = int.MaxValue;

        foreach (var dir in PriorityDirs)
        {
            // Prevent immediate reversal unless forced
            if (motor.CurrentDir != Vector2Int.zero && dir == -motor.CurrentDir)
                continue;

            // Skip directions blocked by walls or invalid tiles
            if (!motor.CanMove(dir))
                continue;

            // Evaluate next tile in this direction
            Vector2Int next = myCell + dir;

            // Squared distance to target (faster than Vector2.Distance)
            int dx = next.x - targetTile.x;
            int dy = next.y - targetTile.y;
            int dist = dx * dx + dy * dy;

            // Keep the direction that minimizes distance
            if (dist < bestDist)
            {
                bestDist = dist;
                bestDir = dir;
            }
        }

        return bestDir;
    }
}
