using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class WallsFlasher : MonoBehaviour
{
    [SerializeField] private Tilemap wallsTilemap;

    [Header("Flash Colors")]
    [SerializeField] private Color colorA = Color.white;
    [SerializeField] private Color colorB = new Color(0f, 0.3f, 1f, 1f);

    [Header("Flash Settings")]
    [SerializeField] private int flashes = 8;
    [SerializeField] private float interval = 0.12f;

    private Color originalColor;

    private void Awake()
    {
        if (wallsTilemap == null)
            wallsTilemap = GetComponent<Tilemap>();

        if (wallsTilemap != null)
            originalColor = wallsTilemap.color;
        else
            Debug.LogError("WallsFlasher: wallsTilemap is NULL (geen Tilemap gevonden/gekoppeld).");
    }

    public IEnumerator FlashWhiteBlueRoutine()
    {
        if (wallsTilemap == null)
        {
            Debug.LogError("WallsFlasher: Flash gestart maar wallsTilemap is NULL.");
            yield break;
        }

        Debug.Log("WallsFlasher: Flash start!");

        for (int i = 0; i < flashes; i++)
        {
            wallsTilemap.color = colorA;
            yield return new WaitForSecondsRealtime(interval);

            wallsTilemap.color = colorB;
            yield return new WaitForSecondsRealtime(interval);
        }

        wallsTilemap.color = originalColor;
        Debug.Log("WallsFlasher: Flash klaar.");
    }
}