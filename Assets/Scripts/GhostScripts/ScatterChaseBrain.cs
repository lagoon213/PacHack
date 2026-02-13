using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public abstract class ScatterChaseBrain : GhostBrain
{
    [SerializeField] protected GhostModeController modeController;
    [SerializeField] protected Tilemap walls;

    [SerializeField] protected Tilemap scatterTargetTilemap;

    private Vector2Int _cachedScatterTarget;
    private bool _scatterCached;

    public override Vector2Int GetDesiredDir(GhostMovement motor)
    {
        // 1) Frightened: random richting
        if (modeController.CurrentMode == GhostMode.Frightened)
            return GetFrightenedDir(motor);

        // 2) Scatter/Chase: target tile bepalen
        Vector2Int targetTile =
            (modeController.CurrentMode == GhostMode.Scatter)
            ? GetScatterTargetTile()
            : GetChaseTargetTile();

        return ChooseDirTowardTarget(motor, walls, targetTile);
    }

    private Vector2Int GetScatterTargetTile()
    {
        if (_scatterCached) return _cachedScatterTarget;

        foreach (var pos in scatterTargetTilemap.cellBounds.allPositionsWithin)
        {
            if (scatterTargetTilemap.HasTile(pos))
            {
                _cachedScatterTarget = new Vector2Int(pos.x, pos.y);
                _scatterCached = true;
                return _cachedScatterTarget;
            }
        }

        Debug.LogError($"{name}: scatterTargetTilemap has no tile!");
        return Vector2Int.zero;
    }

    protected abstract Vector2Int GetChaseTargetTile();

    protected virtual Vector2Int GetFrightenedDir(GhostMovement motor)
    {
        // Random volgorde proberen
        int start = Random.Range(0, PriorityDirs.Length);

        for (int i = 0; i < PriorityDirs.Length; i++)
        {
            Vector2Int dir = PriorityDirs[(start + i) % PriorityDirs.Length];

            // niet meteen omdraaien
            if (motor.CurrentDir != Vector2Int.zero && dir == -motor.CurrentDir)
                continue;

            if (motor.CanMove(dir))
                return dir;
        }

        return Vector2Int.zero;
    }
}
