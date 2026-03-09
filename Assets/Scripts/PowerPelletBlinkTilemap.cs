using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Knipper-logica voor power pellets (Tilemap).
/// 
/// - cachet alle power pellets bij start
/// - laat pellets knipperen
/// - verwijdert pellets permanent bij consumptie
/// - trigger event voor consumptie (bijv. ghosts kunnen hierop reageren)
/// </summary>
public class PowerPelletBlinkTilemap : MonoBehaviour
{
    [Header("Power Pellets")]
    [SerializeField] private Tilemap powerPelletTilemap;
    [SerializeField] private float blinkInterval = 0.25f;

    private Dictionary<Vector3Int, TileBase> pelletTiles = new();
    private bool visible = true;
    private float timer;

    /// <summary>
    /// Event fired when a power pellet is consumed.
    /// Passes the world position of the consumed pellet.
    /// </summary>
    public event System.Action<Vector3> OnPowerPelletConsumed;

    private void Start()
    {
        pelletTiles.Clear();

        foreach (var pos in powerPelletTilemap.cellBounds.allPositionsWithin)
        {
            if (!powerPelletTilemap.HasTile(pos)) continue;

            pelletTiles[pos] = powerPelletTilemap.GetTile(pos);
            powerPelletTilemap.SetTileFlags(pos, TileFlags.None);
        }
    }

    public void Consume(Vector3Int cell)
    {
        if (!pelletTiles.ContainsKey(cell)) return;

        // Remove tile visually and from cache
        pelletTiles.Remove(cell);
        powerPelletTilemap.SetTile(cell, null);

        // Fire consumption event
        OnPowerPelletConsumed?.Invoke(powerPelletTilemap.GetCellCenterWorld(cell));
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= blinkInterval)
        {
            timer = 0f;
            visible = !visible;

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