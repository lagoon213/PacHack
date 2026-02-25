using UnityEngine;
using UnityEngine.Tilemaps;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int score;
    public int highScore; // track the high score
    public Tilemap pelletTilemap;
    public int pelletsRemaining;
    void Start()
    {
        CountPellets();
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // make highscore persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        // Load high score from previous sessions
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    public void AddScore(int amount)
    {
        score += amount;
        // Update high score if broken
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore); // save it
        }
    }

    public void ResetScore()
    {
        score = 0;
    }

    void CountPellets()
    {
        pelletsRemaining = 0;

        BoundsInt bounds = pelletTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (pelletTilemap.HasTile(pos))
            {
                pelletsRemaining++;
            }
        }

        Debug.Log("Pellets found: " + pelletsRemaining);
    }

    public void PelletEaten(Vector3 worldPosition)
    {
        Vector3Int cellPos = pelletTilemap.WorldToCell(worldPosition);

        if (pelletTilemap.HasTile(cellPos))
        {
            pelletTilemap.SetTile(cellPos, null); // remove tile
            pelletsRemaining--;

            if (pelletsRemaining <= 0)
            {
                WinLevel();
            }
        }
    }

    void WinLevel()
    {
        Debug.Log("YOU WIN");
    }
    
}
