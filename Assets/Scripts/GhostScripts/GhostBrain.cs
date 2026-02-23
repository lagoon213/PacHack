using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Basis-klasse voor alle ghost AI brains.
///
/// Taken:
/// - Contract: bepaal gewenste richting
/// - Helper: kies beste richting naar een target tile
/// </summary>
public abstract class GhostBrain : MonoBehaviour
{
    /* =========================
     * Richting-prioriteit
     * ========================= */

    /// <summary>
    /// Tie-break volgorde bij gelijke afstand:
    /// Up → Left → Down → Right (klassieke Pac-Man).
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
    /// Wordt aangeroepen als de ghost het midden van een tile bereikt.
    /// Geeft de gewenste bewegingsrichting terug.
    /// </summary>
    public abstract Vector2Int GetDesiredDir(GhostMovement motor);

    /* =========================
     * Gedeelde keuze-logica
     * ========================= */

    /// <summary>
    /// Kiest de richting die het dichtst bij de target tile komt.
    ///
    /// Regels:
    /// - geen directe 180° omkering (tenzij nodig)
    /// - alleen geldige richtingen (CanMove)
    /// - kies kleinste squared distance (snel)
    /// - PriorityDirs bepaalt tie-break
    /// </summary>
    protected Vector2Int ChooseDirTowardTarget(
        GhostMovement motor,
        Tilemap walls,
        Vector2Int targetTile
    )
    {
        // Huidige tile van de ghost
        Vector3Int myCell3 = walls.WorldToCell(motor.transform.position);
        Vector2Int myCell = new Vector2Int(myCell3.x, myCell3.y);

        Vector2Int bestDir = Vector2Int.zero;
        int bestDist = int.MaxValue;

        foreach (var dir in PriorityDirs)
        {
            // Vermijd directe omkering
            if (motor.CurrentDir != Vector2Int.zero && dir == -motor.CurrentDir)
                continue;

            // Skip als je daar niet heen kan
            if (!motor.CanMove(dir))
                continue;

            // Volgende tile in deze richting
            Vector2Int next = myCell + dir;

            // Squared distance naar target
            int dx = next.x - targetTile.x;
            int dy = next.y - targetTile.y;
            int dist = dx * dx + dy * dy;

            // Beste (kleinste) afstand bewaren
            if (dist < bestDist)
            {
                bestDist = dist;
                bestDir = dir;
            }
        }

        return bestDir;
    }
}