using UnityEngine;
using UnityEngine.Tilemaps;

public class PinkyBrain : ScatterChaseBrain
{
    [SerializeField] private PacManMovement pacman; 

    protected override Vector2Int GetChaseTargetTile()
    {
        Vector3Int pacCell3 = walls.WorldToCell(pacman.transform.position);
        Vector2Int pacCell = new Vector2Int(pacCell3.x, pacCell3.y);

        Vector2Int dir = pacman.CurrentDir;
        if (dir == Vector2Int.zero) dir = Vector2Int.right;

        return pacCell + dir * 4;
    }
}
