using UnityEngine;
using UnityEngine.Tilemaps;

public class PinkyBrain : GhostBrain
{
    [SerializeField] private PacManMovement pacman;
    [SerializeField] private Tilemap walls;

    public override Vector2Int GetDesiredDir(GhostMovement motor)
    {
        Vector3Int pacCell3 = walls.WorldToCell(pacman.transform.position);
        Vector2Int pacCell = new Vector2Int(pacCell3.x, pacCell3.y);

        Vector2Int dir = pacman.CurrentDir;
        if (dir == Vector2Int.zero) dir = Vector2Int.right;

        Vector2Int target = pacCell + dir * 4;

        return ChooseDirTowardTarget(motor, walls, target);
    }
}

