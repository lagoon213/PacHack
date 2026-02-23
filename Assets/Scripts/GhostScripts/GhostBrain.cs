using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Basis-klasse voor alle ghost AI brains.
///
/// Taken:
/// - Contract: bepaal gewenste richting
/// - Helper: kies beste richting naar een target tile
/// - House-logica: gedrag in ghost house (wachten / naar deur)
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
    /// Handelt eerst ghost-house af, daarna normale AI.
    /// </summary>
    public virtual Vector2Int GetDesiredDir(GhostMovement motor)
    {
        // In ghost house → house gedrag
        if (motor.InHouse)
            return GetHouseDir(motor);

        // Buiten → normale AI
        return GetNormalDir(motor);
    }

    /// <summary>
    /// Normale AI (buiten de ghost house).
    /// Elke ghost (of base brain) implementeert dit zelf.
    /// </summary>
    protected abstract Vector2Int GetNormalDir(GhostMovement motor);

    /* =========================
     * Ghost house gedrag
     * ========================= */

    /// <summary>
    /// Gedrag in ghost house:
    /// - Niet vrij: simpel bouncen (up/down)
    /// - Wel vrij: richting de deur tile
    /// </summary>
    protected virtual Vector2Int GetHouseDir(GhostMovement motor)
    {
        // Nog niet vrij → blijf bewegen in house
        if (!motor.CanExitHouse)
        {
            if (motor.CanMove(Vector2Int.up))
                return Vector2Int.up;

            if (motor.CanMove(Vector2Int.down))
                return Vector2Int.down;

            return Vector2Int.zero;
        }

        // Vrijgegeven → ga naar de deur
        Vector2Int doorTile = FindSingleTile(motor.Ghost_Door);
        return ChooseDirTowardTarget(motor, motor.Walls, doorTile);
    }

    /// <summary>
    /// Vindt de eerste/eenige tile in een tilemap (bv. ghost door).
    /// </summary>
    protected Vector2Int FindSingleTile(Tilemap map)
    {
        if (map == null) return Vector2Int.zero;

        foreach (var pos in map.cellBounds.allPositionsWithin)
        {
            if (map.HasTile(pos))
                return new Vector2Int(pos.x, pos.y);
        }

        return Vector2Int.zero;
    }

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