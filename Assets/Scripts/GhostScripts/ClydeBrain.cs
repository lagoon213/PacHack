using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// AI-brain voor Clyde (oranje ghost).
///
/// Gedrag:
/// - Chase: jaagt op Pac-Man als hij ver weg is
/// - Near: vlucht naar zijn scatter-hoek
/// - Scatter/Frightened: via ScatterChaseBrain
///
/// Clyde zorgt voor onvoorspelbaar gedrag.
/// </summary>
public class ClydeBrain : ScatterChaseBrain
{
    /* =========================
     * Referenties
     * ========================= */

    /// <summary>
    /// Transform van Pac-Man.
    /// </summary>
    [SerializeField] private Transform pacman;

    /* =========================
     * Chase logica
     * ========================= */

    /// <summary>
    /// Bepaalt Clyde zijn chase target tile.
    /// </summary>
    protected override Vector2Int GetChaseTargetTile()
    {
        // Zet world posities om naar tile-coördinaten
        Vector3Int pacCell = walls.WorldToCell(pacman.position);
        Vector3Int clydeCell = walls.WorldToCell(transform.position);

        // Manhattan distance (aantal tiles)
        int tileDist =
            Mathf.Abs(pacCell.x - clydeCell.x) +
            Mathf.Abs(pacCell.y - clydeCell.y);

        // Clyde-regel:
        // ver weg → jaag Pac-Man
        // dichtbij → vlucht naar scatter-hoek
        if (tileDist > 8)
        {
            return new Vector2Int(pacCell.x, pacCell.y);
        }
        else
        {
            return GetScatterTargetTile();
        }
    }
}