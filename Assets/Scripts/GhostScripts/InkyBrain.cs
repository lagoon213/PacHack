using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Arcade-correcte Inky (blauw).
/// Chase target = punt 2 tiles vóór Pac-Man,
/// plus 2x vector (Blinky -> dat punt).
/// </summary>
public class InkyBrain : ScatterChaseBrain
{
    [SerializeField] private Transform blinky;
    [SerializeField] private PacManMovement pacman; // moet CurrentDir (Vector2Int) hebben

    [Header("Arcade instellingen")]
    [SerializeField] private int tilesAhead = 2;

    // Optioneel: originele arcade "up-bug" (Pac-Man omhoog -> ook 2 tiles naar links)
    [SerializeField] private bool emulateArcadeUpBug = false;

    protected override Vector2Int GetChaseTargetTile()
    {
        // 1) Pak Pac-Man tile + richting
        Vector3Int pacCell = walls.WorldToCell(pacman.transform.position);
        Vector2Int pacDir = pacman.CurrentDir; // <-- pas aan als jouw property anders heet

        // Safety: als dir 0 is, gebruik "geen offset"
        if (pacDir == Vector2Int.zero)
            pacDir = Vector2Int.right;

        // 2) Punt 2 tiles vóór Pac-Man (tile coords)
        Vector3Int aheadCell = pacCell + new Vector3Int(pacDir.x, pacDir.y, 0) * tilesAhead;

        // Arcade up-bug (optioneel)
        if (emulateArcadeUpBug && pacDir == Vector2Int.up)
            aheadCell += new Vector3Int(-tilesAhead, 0, 0);

        // 3) Vector van Blinky naar dat punt (in tiles)
        Vector3Int blinkyCell = walls.WorldToCell(blinky.position);
        Vector3Int v = aheadCell - blinkyCell;

        // 4) Verdubbel vector en tel op bij aheadCell
        Vector3Int targetCell = aheadCell + v;

        return new Vector2Int(targetCell.x, targetCell.y);
    }
}