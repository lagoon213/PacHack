using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private FadeIn fadeIn;

    private bool isGameOver = false;

    public void ShowGameOver()
    {
        isGameOver = true;

        Time.timeScale = 0f;

        if (fadeIn != null)
            fadeIn.ShowAndFadeIn();
        else
            Debug.LogError("GameOverMenu: FadeIn reference ontbreekt!");
    }

    private void Update()
    {
        if (!isGameOver) return;

        // "any key" op het toetsenbord
        if (Input.anyKeyDown)
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}