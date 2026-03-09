using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Knipper-logica voor power pellets (Tilemap).
///
/// Doet:
/// - cachet alle power pellets bij start
/// - laat pellets knipperen
/// - verwijdert pellets permanent bij consumptie
///
/// Script hoort op de PowerPellets Tilemap.
/// </summary>
public class PowerPelletBlinkTilemap : MonoBehaviour
{
    /* =========================
     * Referenties & instellingen
     * ========================= */
    //snake
    [SerializeField] private PacManMovement pacMan; //reference to pacman movement not very clean but as a test
    /// <summary>
    /// Tilemap met power pellets.
    /// </summary>
    [SerializeField] private Tilemap powerPelletTilemap;

    /// <summary>
    /// Tijd tussen knipperen (seconden).
    /// Lager = sneller knipperen.
    /// </summary>
    [SerializeField] private float blinkInterval = 0.25f;

    /* =========================
     * Runtime state
     * ========================= */

    /// <summary>
    /// Cache van actieve pellets (positie → originele tile).
    /// Opgegeten pellets komen niet terug.
    /// </summary>
    private Dictionary<Vector3Int, TileBase> pelletTiles = new();

    /// <summary>
    /// Huidige zichtbaarheid.
    /// </summary>
    private bool visible = true;

    /// <summary>
    /// Timer voor knipper-interval.
    /// </summary>
    private float timer;

    /* =========================
     * Unity lifecycle
     * ========================= */

    private void Start()
    {
        // Cache leegmaken (veilig bij reload)
        pelletTiles.Clear();

        // Alle power pellets cachen
        foreach (var pos in powerPelletTilemap.cellBounds.allPositionsWithin)
        {
            if (powerPelletTilemap.HasTile(pos))
            {
                pelletTiles[pos] = powerPelletTilemap.GetTile(pos);

                powerPelletTilemap.SetTileFlags(pos, TileFlags.None); //try and allow color change
            }
        }
        
    }

    /* =========================
     * Public API
     * ========================= */

    /// <summary>
    /// Verwijdert een power pellet permanent.
    /// Aangeroepen wanneer Pac-Man hem opeet.
    /// </summary>
    public void Consume(Vector3Int cell)
    {
        // Uit cache halen (knippert nooit meer)
        pelletTiles.Remove(cell);

        // Visueel verwijderen
        powerPelletTilemap.SetTile(cell, null);

        if (pacMan != null && pacMan.snakeMode == true)
        {
            //some logic in ghost that allows the ghost to grow
        }
    }

    /* =========================
     * Update loop
     * ========================= */

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= blinkInterval)
        {
            timer = 0f;
            visible = !visible;

            // Zichtbaarheid togglen voor resterende pellets
           foreach (var kvp in pelletTiles)
        {
            powerPelletTilemap.SetColor(
                kvp.Key,
                visible ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0f)
            );
        }
        }
    }
}