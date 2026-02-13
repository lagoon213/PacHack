using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class ScatterChaseBrain : GhostBrain
{
    [SerializeField] protected GhostModeController modeController;
    [SerializeField] protected Tilemap walls;

    [SerializeField] protected Tilemap scatterTargetTilemap;

    public override Vector2Int GetDesiredDir(GhostMovement motor)
    {
        Vector2Int targetTile;

        if (modeController.CurrentMode == GhostMode.Scatter)
        {
            targetTile = GetScatterTargetTile();
        }
        else
        {
            targetTile = GetChaseTargetTile();
        }

        return ChooseDirTowardTarget(motor, walls, targetTile);
    }

    private Vector2Int GetScatterTargetTile()
    {
        foreach (var pos in scatterTargetTilemap.cellBounds.allPositionsWithin)
        {
            if (scatterTargetTilemap.HasTile(pos))
            {
                return new Vector2Int(pos.x, pos.y);
            }
        }

        return Vector2Int.zero;
    }

    protected abstract Vector2Int GetChaseTargetTile();
}
