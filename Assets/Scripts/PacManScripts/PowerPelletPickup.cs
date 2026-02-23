using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Afhandeling van power pellets.
///
/// Doet:
/// - check of Pac-Man op een power pellet staat
/// - verwijdert de pellet
/// - zet ghosts in Frightened mode
///
/// Script hoort op Pac-Man.
/// </summary>
public class PowerPelletPickup : MonoBehaviour
{
    /* =========================
     * Referenties
     * ========================= */

    /// <summary>
    /// Tilemap met alle power pellets.
    /// </summary>
    [SerializeField] private Tilemap powerPelletTilemap;

    /// <summary>
    /// Regelt het knipperen van power pellets.
    /// Verwijdert opgegeten pellets definitief.
    /// </summary>
    [SerializeField] private PowerPelletBlinkTilemap blink;

    /// <summary>
    /// Centrale ghost mode controller.
    /// </summary>
    [SerializeField] private GhostModeController ghostModeController;

    /// <summary>
    /// Duur van Frightened mode (seconden).
    /// </summary>
    [SerializeField] private float frightenedDuration = 6f;

    /* =========================
     * Unity lifecycle
     * ========================= */

    private void Update()
    {
        // Tile waar Pac-Man nu op staat
        Vector3Int cell = powerPelletTilemap.WorldToCell(transform.position);

        // Staat hier een power pellet?
        if (powerPelletTilemap.HasTile(cell))
        {
            // Pellet verwijderen (ook uit blink-cache)
            blink.Consume(cell);

            // Ghosts in Frightened mode zetten
            ghostModeController.TriggerFrightened(frightenedDuration);
        }
    }
}