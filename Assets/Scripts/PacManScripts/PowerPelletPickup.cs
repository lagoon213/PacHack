using UnityEngine;
using UnityEngine.Tilemaps;

public class PowerPelletPickup : MonoBehaviour
{
    [SerializeField] private Tilemap powerPelletTilemap;
    [SerializeField] private PowerPelletBlinkTilemap blink;
    [SerializeField] private GhostModeController ghostModeController;
    [SerializeField] private float frightenedDuration = 6f;

    private void Update()
    {
        Vector3Int cell = powerPelletTilemap.WorldToCell(transform.position);

        if (powerPelletTilemap.HasTile(cell))
        {
            blink.Consume(cell);
            ghostModeController.TriggerFrightened(frightenedDuration);
        }
    }
}
