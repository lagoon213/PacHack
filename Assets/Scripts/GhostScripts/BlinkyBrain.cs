using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// AI-brain voor Blinky (rode ghost).
///
/// Gedrag:
/// - Chase: target altijd Pac-Man zijn huidige tile
/// - Scatter: via ScatterChaseBrain
/// - Frightened: via ScatterChaseBrain
///
/// Blinky is de meest agressieve ghost.
/// </summary>
public class BlinkyBrain : ScatterChaseBrain
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
    /// Geeft de chase target tile terug.
    /// Blinky jaagt direct op Pac-Man.
    /// </summary>
    protected override Vector2Int GetChaseTargetTile()
    {
        // Zet Pac-Man world positie om naar tile-coördinaten
        Vector3Int pacCell = walls.WorldToCell(pacman.position);

        // Gebruik Pac-Man zijn huidige tile als target
        return new Vector2Int(pacCell.x, pacCell.y);
    }
}