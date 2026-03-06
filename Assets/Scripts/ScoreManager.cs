using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public static event Action OnPelletEaten;
    public static event Action OnSnakeGrow;

    public int score;
    public int highScore;

    public Tilemap pelletTilemap;
    public int pelletsRemaining;

    private int pelletsSinceGrow = 0;

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
        RefreshPelletTilemapAndCount();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshPelletTilemapAndCount();
        pelletsSinceGrow = 0;
    }

    private void RefreshPelletTilemapAndCount()
    {
        if (pelletTilemap == null)
        {
            var go = GameObject.Find("Pellets");
            if (go != null)
                pelletTilemap = go.GetComponent<Tilemap>();
        }

        if (pelletTilemap != null)
            CountPellets();
        else
            Debug.LogWarning("ScoreManager: pelletTilemap not found. Check GameObject name ('Pellets').");
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
        pelletsSinceGrow = 0;
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
        Vector3Int cellPos = pelletTilemap.WorldToCell(worldPosition);

        if (!pelletTilemap.HasTile(cellPos))
            return;

        pelletTilemap.SetTile(cellPos, null);
        pelletsRemaining--;

        AddScore(10);

        OnPelletEaten?.Invoke();

        // Snake growth every 10 pellets
        pelletsSinceGrow++;
        if (pelletsSinceGrow >= 10)
        {
            pelletsSinceGrow = 0;
            OnSnakeGrow?.Invoke();
        }

        if (pelletsRemaining <= 0)
            WinLevel();
    }

    private void WinLevel()
    {
        Debug.Log("YOU WIN");
    }
}