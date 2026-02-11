using UnityEngine;
using UnityEngine.Tilemaps;

public class PinkyBrain : GhostBrain
{
    [SerializeField] private PacManMovement pacman;
    [SerializeField] private Tilemap walls; 

    private static readonly Vector2Int[] PriorityDirs =
    {
        Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right
    };

    public override Vector2Int GetDesiredDir(GhostMovement motor)
    {
        Vector2Int targetTile = GetPinkyChaseTargetTile();
        return ChooseDirTowardTarget(motor, targetTile);
    }

    private Vector2Int GetPinkyChaseTargetTile()
    {
        Vector3Int pacCell3 = walls.WorldToCell(pacman.transform.position);
        Vector2Int pacCell = new Vector2Int(pacCell3.x, pacCell3.y);

        Vector2Int pacDir = pacman.CurrentDir;
        if (pacDir == Vector2Int.zero) pacDir = Vector2Int.right;

        return pacCell + pacDir * 4;
    }

    private Vector2Int ChooseDirTowardTarget(GhostMovement motor, Vector2Int targetTile)
    {
        Vector3Int myCell3 = walls.WorldToCell(motor.transform.position);
        Vector2Int myCell = new Vector2Int(myCell3.x, myCell3.y);

        Vector2Int bestDir = Vector2Int.zero;
        int bestDist = int.MaxValue;

        foreach (var dir in PriorityDirs)
        {
            // niet omdraaien tenzij het moet
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

        // als alleen reverse kan
        if (bestDir == Vector2Int.zero && motor.CurrentDir != Vector2Int.zero && motor.CanMove(-motor.CurrentDir))
            return -motor.CurrentDir;

        return bestDir;
    }
}

