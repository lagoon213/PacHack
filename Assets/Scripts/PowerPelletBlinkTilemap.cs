using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PowerPelletBlinkTilemap : MonoBehaviour
{
    [SerializeField] private Tilemap powerPelletTilemap;
    [SerializeField] private float blinkInterval = 0.25f;

    private Dictionary<Vector3Int, TileBase> pelletTiles = new();
    private bool visible = true;
    private float timer;

    private void Start()
    {
        pelletTiles.Clear();

        foreach (var pos in powerPelletTilemap.cellBounds.allPositionsWithin)
        {
            if (powerPelletTilemap.HasTile(pos))
            {
                pelletTiles[pos] = powerPelletTilemap.GetTile(pos);
            }
        }
    }

    public void Consume(Vector3Int cell)
    {
        // haal uit cache zodat hij nooit meer terugkomt
        pelletTiles.Remove(cell);
        // zorg dat hij nu ook echt weg is
        powerPelletTilemap.SetTile(cell, null);
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
                powerPelletTilemap.SetTile(kvp.Key, visible ? kvp.Value : null);
            }
        }
    }
}
