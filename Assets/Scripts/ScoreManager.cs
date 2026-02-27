using UnityEngine;
using UnityEngine.Tilemaps;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    // Global pellet event (ghosts listen to this)
    public static event System.Action OnPelletEaten;

    public int score;
    public int highScore;

    [Header("Pellet Tilemap")]
    public Tilemap pelletTilemap;
    public int pelletsRemaining;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Start()
    {
        CountPellets();
    }

    public void AddScore(int amount)
    {
        score += amount;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }

    public void ResetScore()
    {
        score = 0;
    }

    private void CountPellets()
    {
        pelletsRemaining = 0;

        BoundsInt bounds = pelletTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (pelletTilemap.HasTile(pos))
                pelletsRemaining++;
        }

        Debug.Log("Pellets found: " + pelletsRemaining);
    }

    // 🟡 SINGLE SOURCE OF TRUTH
    public void PelletEaten(Vector3 worldPosition)
    {
        Vector3Int cellPos = pelletTilemap.WorldToCell(worldPosition);

        if (!pelletTilemap.HasTile(cellPos))
            return;

        // Remove pellet
        pelletTilemap.SetTile(cellPos, null);
        pelletsRemaining--;

        // Add score
        AddScore(10);

        // 🔔 Notify listeners (ghost release, etc)
        OnPelletEaten?.Invoke();

        if (pelletsRemaining <= 0)
            WinLevel();
    }

    private void WinLevel()
    {
        Debug.Log("YOU WIN");
    }
}