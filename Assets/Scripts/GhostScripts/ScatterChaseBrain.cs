using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Basis-AI voor Pac-Man ghosts met modes:
/// - Scatter: vaste hoek target
/// - Chase: ghost-specifieke target
/// - Frightened: semi-random beweging
///
/// Afgeleide classes implementeren alleen GetChaseTargetTile().
/// </summary>
public abstract class ScatterChaseBrain : GhostBrain
{
    /* =========================
     * Referenties
     * ========================= */

    /// <summary>
    /// Bepaalt de huidige ghost mode (Scatter / Chase / Frightened).
    /// </summary>
    [SerializeField] protected GhostModeController modeController;

    /// <summary>
    /// Wall tilemap voor grid-logica en WorldToCell conversies.
    /// </summary>
    [SerializeField] protected Tilemap walls;

    /// <summary>
    /// Tilemap met exact één tile:
    /// de scatter-hoek van deze ghost.
    /// </summary>
    [SerializeField] protected Tilemap scatterTargetTilemap;

    /* =========================
     * Scatter target cache
     * ========================= */

    /// <summary>
    /// Gecachte scatter target tile.
    /// </summary>
    private Vector2Int _cachedScatterTarget;

    /// <summary>
    /// Geeft aan of de scatter target al is ingelezen.
    /// </summary>
    private bool _scatterCached;

    /* =========================
     * Hoofdlogica
     * ========================= */

    /// <summary>
    /// Wordt aangeroepen wanneer de ghost het midden van een tile bereikt.
    /// Bepaalt de volgende bewegingsrichting.
    /// </summary>
    public override Vector2Int GetDesiredDir(GhostMovement motor)
    {
        // Frightened: willekeurige veilige richting
        if (modeController.CurrentMode == GhostMode.Frightened)
            return GetFrightenedDir(motor);

        // Scatter of Chase: bepaal doel-tile
        Vector2Int targetTile =
            (modeController.CurrentMode == GhostMode.Scatter)
            ? GetScatterTargetTile()
            : GetChaseTargetTile();

        // Kies richting die het beste naar het doel leidt
        return ChooseDirTowardTarget(motor, walls, targetTile);
    }

    /* =========================
     * Scatter target
     * ========================= */

    /// <summary>
    /// Leest éénmalig de scatter-hoek uit de tilemap
    /// en cached het resultaat.
    /// </summary>
    protected Vector2Int GetScatterTargetTile()
    {
        if (_scatterCached)
            return _cachedScatterTarget;

        foreach (var pos in scatterTargetTilemap.cellBounds.allPositionsWithin)
        {
            if (scatterTargetTilemap.HasTile(pos))
            {
                _cachedScatterTarget = new Vector2Int(pos.x, pos.y);
                _scatterCached = true;
                return _cachedScatterTarget;
            }
        }

        Debug.LogError($"{name}: scatterTargetTilemap bevat geen tile!");
        return Vector2Int.zero;
    }

    /* =========================
     * Chase target
     * ========================= */

    /// <summary>
    /// Geeft het chase-doel terug.
    /// Wordt per ghost anders geïmplementeerd.
    /// </summary>
    protected abstract Vector2Int GetChaseTargetTile();

    /* =========================
     * Frightened gedrag
     * ========================= */

    /// <summary>
    /// Frightened gedrag:
    /// - probeer richtingen in willekeurige volgorde
    /// - vermijd direct omkeren als dat kan
    /// </summary>
    protected virtual Vector2Int GetFrightenedDir(GhostMovement motor)
    {
        int start = Random.Range(0, PriorityDirs.Length);

        for (int i = 0; i < PriorityDirs.Length; i++)
        {
            Vector2Int dir = PriorityDirs[(start + i) % PriorityDirs.Length];

            // Vermijd 180° omkering
            if (motor.CurrentDir != Vector2Int.zero && dir == -motor.CurrentDir)
                continue;

            if (motor.CanMove(dir))
                return dir;
        }

        // Geen geldige richting gevonden
        return Vector2Int.zero;
    }
}