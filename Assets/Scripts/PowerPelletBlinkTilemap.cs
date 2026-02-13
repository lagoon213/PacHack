using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Controls the blinking behavior of power pellets that are stored in a Tilemap.
///
/// Responsibilities:
/// - Cache all power pellet tiles at startup
/// - Periodically toggle their visibility (blink effect)
/// - Permanently remove pellets when they are consumed by Pac-Man
///
/// This script should be attached to the PowerPellets Tilemap GameObject.
/// </summary>
public class PowerPelletBlinkTilemap : MonoBehaviour
{
    /* =========================
     * References & Settings
     * ========================= */

    /// <summary>
    /// Tilemap that contains the power pellet tiles.
    /// </summary>
    [SerializeField] private Tilemap powerPelletTilemap;

    /// <summary>
    /// Time (in seconds) between visibility toggles.
    /// Lower values result in faster blinking.
    /// </summary>
    [SerializeField] private float blinkInterval = 0.25f;

    /* =========================
     * Runtime State
     * ========================= */

    /// <summary>
    /// Cache of all active power pellet tiles and their original TileBase.
    /// Used to prevent eaten pellets from reappearing.
    /// </summary>
    private Dictionary<Vector3Int, TileBase> pelletTiles = new();

    /// <summary>
    /// Current visibility state of the pellets.
    /// </summary>
    private bool visible = true;

    /// <summary>
    /// Timer used to control blinking intervals.
    /// </summary>
    private float timer;

    /* =========================
     * Unity Lifecycle
     * ========================= */

    private void Start()
    {
        // Clear cache in case of scene reload or reinitialization
        pelletTiles.Clear();

        // Cache all power pellet tiles at startup
        foreach (var pos in powerPelletTilemap.cellBounds.allPositionsWithin)
        {
            if (powerPelletTilemap.HasTile(pos))
            {
                pelletTiles[pos] = powerPelletTilemap.GetTile(pos);
            }
        }
    }

    /* =========================
     * Public API
     * ========================= */

    /// <summary>
    /// Permanently removes a power pellet from the tilemap and cache.
    /// Called when Pac-Man consumes a power pellet.
    /// </summary>
    public void Consume(Vector3Int cell)
    {
        // Remove from cache so it will never blink again
        pelletTiles.Remove(cell);

        // Ensure the tile is removed visually
        powerPelletTilemap.SetTile(cell, null);
    }

    /* =========================
     * Update Loop
     * ========================= */

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= blinkInterval)
        {
            timer = 0f;
            visible = !visible;

            // Toggle visibility for all remaining power pellets
            foreach (var kvp in pelletTiles)
            {
                powerPelletTilemap.SetTile(
                    kvp.Key,
                    visible ? kvp.Value : null
                );
            }
        }
    }
}
