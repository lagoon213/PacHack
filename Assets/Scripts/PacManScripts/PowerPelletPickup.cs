using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Afhandeling van power pellets.
/// Script hoort op Pac-Man.
/// </summary>
public class PowerPelletPickup : MonoBehaviour
{
    public int points = 50;

    [SerializeField] private Tilemap powerPelletTilemap;
    [SerializeField] private PowerPelletBlinkTilemap blink;
    [SerializeField] private GhostModeController ghostModeController;
    [SerializeField] private float frightenedDuration = 6f;

    private void Update()
    {
        if (powerPelletTilemap == null || blink == null || ghostModeController == null)
            return;

        Vector3Int cell = powerPelletTilemap.WorldToCell(transform.position);

        if (powerPelletTilemap.HasTile(cell))
        {
            blink.Consume(cell);
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(points);

            ghostModeController.TriggerFrightened(frightenedDuration);
        }
    }
}