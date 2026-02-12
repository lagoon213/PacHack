using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class GhostBrain : MonoBehaviour
{
    protected static readonly Vector2Int[] PriorityDirs =
    {
        Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right
    };

    public abstract Vector2Int GetDesiredDir(GhostMovement motor);

    protected Vector2Int ChooseDirTowardTarget(GhostMovement motor, Tilemap walls, Vector2Int targetTile)
    {
        Vector3Int myCell3 = walls.WorldToCell(motor.transform.position);
        Vector2Int myCell = new Vector2Int(myCell3.x, myCell3.y);

        Vector2Int bestDir = Vector2Int.zero;
        int bestDist = int.MaxValue;

        foreach (var dir in PriorityDirs)
        {
            if (motor.CurrentDir != Vector2Int.zero && dir == -motor.CurrentDir)
                continue;

            if (!motor.CanMove(dir))
                continue;

            Vector2Int next = myCell + dir;

            int dx = next.x - targetTile.x;
            int dy = next.y - targetTile.y;
            int dist = dx * dx + dy * dy;

            if (dist < bestDist)
            {
                bestDist = dist;
                bestDir = dir;
            }
        }

        return bestDir;
    }
}