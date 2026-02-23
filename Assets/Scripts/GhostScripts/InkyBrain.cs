using UnityEngine;

public class InkyBrain : ScatterChaseBrain
{
    [SerializeField] private Transform pacman;
    [SerializeField] private Transform blinky;

    protected override Vector2Int GetChaseTargetTile()
    {
        Vector2 pacPos = pacman.position;
        Vector2 blinkyPos = blinky.position;

        // Calculate the vector from Blinky
        Vector2 vectorFromBlinky = pacPos - blinkyPos;
        Vector2 targetPos = pacPos + vectorFromBlinky * 2f;

        Vector3Int targetCell3 = walls.WorldToCell(targetPos);
        return new Vector2Int(targetCell3.x, targetCell3.y);
    }
}
