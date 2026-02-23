using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// AI-brain voor Pinky (roze ghost).
///
/// Gedrag:
/// - Chase: target een paar tiles vóór Pac-Man (ambush)
/// - Scatter/Frightened: via ScatterChaseBrain
///
/// Deze versie gebruikt de vereenvoudigde logica:
/// 4 tiles recht vooruit in Pac-Man zijn richting.
/// </summary>
public class PinkyBrain : ScatterChaseBrain
{
    /* =========================
     * Referenties
     * ========================= */

    /// <summary>
    /// Pac-Man movement (positie + richting).
    /// </summary>
    [SerializeField] private PacManMovement pacman;

    /* =========================
     * Chase logica
     * ========================= */

    /// <summary>
    /// Bepaalt Pinky zijn chase target tile.
    /// </summary>
    protected override Vector2Int GetChaseTargetTile()
    {
        // Pac-Man world → tile-coördinaten
        Vector3Int pacCell3 = walls.WorldToCell(pacman.transform.position);
        Vector2Int pacCell = new Vector2Int(pacCell3.x, pacCell3.y);

        // Richting waarin Pac-Man beweegt
        Vector2Int dir = pacman.CurrentDir;

        // Fallback als Pac-Man stilstaat
        if (dir == Vector2Int.zero)
            dir = Vector2Int.right;

        // Target 4 tiles vooruit (ambush)
        return pacCell + dir * 4;
    }
}