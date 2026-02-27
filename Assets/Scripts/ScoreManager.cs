using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int score;
    public int highScore;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // eerste scene
        RefreshPelletTilemapAndCount();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // na restart / scene reload
        RefreshPelletTilemapAndCount();
    }

    private void RefreshPelletTilemapAndCount()
    {
        // Zoek opnieuw de pellet tilemap als die missing is
        if (pelletTilemap == null)
        {
            var go = GameObject.Find("Pellets"); // zet dit gelijk aan jouw GameObject naam
            if (go != null)
                pelletTilemap = go.GetComponent<Tilemap>();
        }

        if (pelletTilemap != null)
            CountPellets();
        else
            Debug.LogWarning("ScoreManager: pelletTilemap niet gevonden. Check GameObject naam ('Pellets').");
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

    public void PelletEaten(Vector3 worldPosition)
    {
        if (pelletTilemap == null) return;

        Vector3Int cellPos = pelletTilemap.WorldToCell(worldPosition);

        if (pelletTilemap.HasTile(cellPos))
        {
            pelletTilemap.SetTile(cellPos, null);
            pelletsRemaining--;

            if (pelletsRemaining <= 0)
                WinLevel();
        }
    }

    private void WinLevel()
    {
        Debug.Log("YOU WIN");
    }
}