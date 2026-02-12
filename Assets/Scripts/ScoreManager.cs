using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int score;
    public int highScore; // track the high score

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
}
