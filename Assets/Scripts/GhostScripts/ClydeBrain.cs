using UnityEngine;
using UnityEngine.Tilemaps;

public class ClydeBrain : ScatterChaseBrain
{
    [SerializeField] private Transform pacman;

    protected override Vector2Int GetChaseTargetTile()
    {
        // Werk in tiles (niet world units)
        Vector3Int pacCell = walls.WorldToCell(pacman.position);
        Vector3Int clydeCell = walls.WorldToCell(transform.position);

        // Pac-Man gebruikt Manhattan distance op grid-achtig gedrag
        int tileDist =
            Mathf.Abs(pacCell.x - clydeCell.x) +
            Mathf.Abs(pacCell.y - clydeCell.y);

        // Clyde rule:
        // - far: target Pac-Man tile
        // - near: target scatter corner tile
        if (tileDist > 8)
        {
            return new Vector2Int(pacCell.x, pacCell.y);
        }
        else
        {
            // Gebruik de scatter target tile uit de base class
            return GetScatterTargetTile();
        }
    }
}