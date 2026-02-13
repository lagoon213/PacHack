using UnityEngine;
using UnityEngine.Tilemaps;

public class BlinkyBrain : ScatterChaseBrain
{
    [SerializeField] private Transform pacman;

    protected override Vector2Int GetChaseTargetTile()
    {
        Vector3Int pacCell3 = walls.WorldToCell(pacman.position);
        return new Vector2Int(pacCell3.x, pacCell3.y);
    }
}
