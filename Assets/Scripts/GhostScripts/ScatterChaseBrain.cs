using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Shared brain base for ghosts that use the classic Pac-Man mode system:
/// - Scatter: target a fixed "corner" tile (per ghost)
/// - Chase: target depends on the ghost type (Blinky/Pinky/etc.)
/// - Frightened: choose a semi-random valid direction
///
/// Responsibilities:
/// - Resolve the current mode from GhostModeController
/// - Select the appropriate target tile for Scatter/Chase
/// - Provide a default frightened movement strategy
///
/// Concrete ghost brains only implement GetChaseTargetTile().
/// </summary>
public abstract class ScatterChaseBrain : GhostBrain
{
    /* =========================
     * References
     * ========================= */

    /// <summary>
    /// Global controller that determines the current ghost mode (Scatter/Chase/Frightened).
    /// </summary>
    [SerializeField] protected GhostModeController modeController;

    /// <summary>
    /// Tilemap used for converting world positions to tile coordinates.
    /// (Also used by ChooseDirTowardTarget for consistent grid logic.)
    /// </summary>
    [SerializeField] protected Tilemap walls;

    /// <summary>
    /// Per-ghost scatter target tilemap.
    /// This tilemap should contain exactly one tile that represents the ghost's
    /// scatter corner/target position.
    /// </summary>
    [SerializeField] protected Tilemap scatterTargetTilemap;

    /* =========================
     * Cached scatter target
     * ========================= */

    /// <summary>
    /// Cached scatter target tile coordinates, read once from the scatterTargetTilemap.
    /// </summary>
    private Vector2Int _cachedScatterTarget;

    /// <summary>
    /// Indicates whether the scatter target has been cached already.
    /// </summary>
    private bool _scatterCached;

    /* =========================
     * Main decision entry point
     * ========================= */

    /// <summary>
    /// Called by GhostMovement whenever the ghost reaches the center of a tile.
    /// Decides which direction the ghost should take next.
    /// </summary>
    public override Vector2Int GetDesiredDir(GhostMovement motor)
    {
        // 1) Frightened mode: override normal targeting with random-ish movement
        if (modeController.CurrentMode == GhostMode.Frightened)
            return GetFrightenedDir(motor);

        // 2) Scatter/Chase: choose which target tile to steer toward
        Vector2Int targetTile =
            (modeController.CurrentMode == GhostMode.Scatter)
            ? GetScatterTargetTile()
            : GetChaseTargetTile();

        // Use shared helper from GhostBrain to choose the best direction toward target
        return ChooseDirTowardTarget(motor, walls, targetTile);
    }

    /* =========================
     * Scatter target
     * ========================= */

    /// <summary>
    /// Returns the scatter target tile coordinates for this ghost.
    /// Reads the target from scatterTargetTilemap once and caches it.
    /// </summary>
    private Vector2Int GetScatterTargetTile()
    {
        if (_scatterCached)
            return _cachedScatterTarget;

        // Search the tilemap bounds for the single scatter tile
        foreach (var pos in scatterTargetTilemap.cellBounds.allPositionsWithin)
        {
            if (scatterTargetTilemap.HasTile(pos))
            {
                _cachedScatterTarget = new Vector2Int(pos.x, pos.y);
                _scatterCached = true;
                return _cachedScatterTarget;
            }
        }

        // Fallback: no tile found (misconfigured scene)
        Debug.LogError($"{name}: scatterTargetTilemap has no tile!");
        return Vector2Int.zero;
    }

    /* =========================
     * Chase target (implemented per ghost type)
     * ========================= */

    /// <summary>
    /// Returns the chase target tile for the specific ghost type.
    /// Example:
    /// - Blinky: Pac-Man's current tile
    /// - Pinky: 4 tiles ahead of Pac-Man
    /// - Inky/Clyde: custom rules
    /// </summary>
    protected abstract Vector2Int GetChaseTargetTile();

    /* =========================
     * Frightened behavior
     * ========================= */

    /// <summary>
    /// Default frightened behavior:
    /// Choose a valid direction in a randomized order, avoiding immediate reversal
    /// when possible. This mimics the "erratic" movement during frightened mode.
    /// 
    /// You can override this per ghost if you want more specific frightened logic.
    /// </summary>
    protected virtual Vector2Int GetFrightenedDir(GhostMovement motor)
    {
        // Start at a random index so the evaluation order changes each decision
        int start = Random.Range(0, PriorityDirs.Length);

        for (int i = 0; i < PriorityDirs.Length; i++)
        {
            Vector2Int dir = PriorityDirs[(start + i) % PriorityDirs.Length];

            // Avoid immediate reversal unless forced
            if (motor.CurrentDir != Vector2Int.zero && dir == -motor.CurrentDir)
                continue;

            // Pick the first valid direction found
            if (motor.CanMove(dir))
                return dir;
        }

        // If no direction is valid, stop (should be rare in a well-formed maze)
        return Vector2Int.zero;
    }
}
