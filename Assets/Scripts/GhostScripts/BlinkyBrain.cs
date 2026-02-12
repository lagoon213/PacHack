using UnityEngine;
using UnityEngine.Tilemaps;

public class BlinkyBrain : GhostBrain
{
    [SerializeField] private PacManMovement pacman;
    [SerializeField] private Tilemap walls;

    public override Vector2Int GetDesiredDir(GhostMovement motor)
    {
        Vector3Int pacCell3 = walls.WorldToCell(pacman.transform.position);
        Vector2Int target = new Vector2Int(pacCell3.x, pacCell3.y);

        return ChooseDirTowardTarget(motor, walls, target);
    }
}
