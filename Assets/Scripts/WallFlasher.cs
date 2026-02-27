using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class WallsFlasher : MonoBehaviour
{
    [SerializeField] private Tilemap wallsTilemap;

    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private int flashes = 8;
    [SerializeField] private float interval = 0.12f;

    private Color originalColor;

    private void Awake()
    {
        if (wallsTilemap == null)
            wallsTilemap = GetComponent<Tilemap>();

        if (wallsTilemap != null)
            originalColor = wallsTilemap.color;
    }

    public IEnumerator FlashRoutine()
    {
        if (wallsTilemap == null) yield break;

        for (int i = 0; i < flashes; i++)
        {
            wallsTilemap.color = flashColor;
            yield return new WaitForSecondsRealtime(interval);

            wallsTilemap.color = originalColor;
            yield return new WaitForSecondsRealtime(interval);
        }

        wallsTilemap.color = originalColor;
    }
}