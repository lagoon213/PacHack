using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PacmanDeath : MonoBehaviour
{
    [Header("Lives / Respawn")]
    public int lives = 3;
    public float respawnDelay = 1f;
    public Transform respawnPoint;

    [Header("Death Animation")]
    public string deathStateName = "PacManDeath"; // exact naam van je Animator STATE

    [Header("Game Over")]
    public FadeIn gameOverFade;

    [Header("UI (Game Over)")]
    public Text scoreNumberText; // <-- koppel hier TextScore (alleen het nummer)

    private bool isDead = false;
    private bool isGameOver = false;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private PacManMovement movement;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        movement = GetComponent<PacManMovement>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Any key restart bij Game Over (New Input System)
        if (isGameOver &&
            Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || isGameOver) return;

        if (other.CompareTag("Ghost"))
            Die();
    }

    private void Die()
    {
        isDead = true;
        lives--;

        Debug.Log("Pac-Man dood! Lives over: " + lives);

        // Stop beweging/collision, maar laat sprite zichtbaar voor animatie
        DisablePacmanForDeath();

        // Speel death animatie direct (geen triggers nodig)
        if (animator != null && !string.IsNullOrEmpty(deathStateName))
            animator.Play(deathStateName, 0, 0f);

        if (lives <= 0)
        {
            StartCoroutine(GameOverAfterDelay());
        }
        else
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        // wacht zodat death animatie even te zien is
        yield return new WaitForSeconds(respawnDelay);

        rb.simulated = false;
        transform.position = respawnPoint.position;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Physics2D.SyncTransforms();
        rb.simulated = true;

        EnablePacman();
        isDead = false;
    }

    private IEnumerator GameOverAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        GameOver();
    }

    private void GameOver()
    {
        // Pacman mag nu uit beeld
        DisablePacman();

        isGameOver = true;
        Time.timeScale = 0f;

        // Zet alleen het score-getal (zonder "Your score:")
        if (scoreNumberText != null && ScoreManager.Instance != null)
        {
            scoreNumberText.text = ScoreManager.Instance.score.ToString();
        }

        if (gameOverFade != null)
            gameOverFade.ShowAndFadeIn();
        else
            Debug.LogError("GameOverFade niet gekoppeld!");
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;

        // (optioneel) reset score bij restart:
        // if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Tijdens death animatie: sprite moet AAN blijven
    private void DisablePacmanForDeath()
    {
        if (movement != null) movement.enabled = false;
        if (col != null) col.enabled = false;
        if (sr != null) sr.enabled = true;
    }

    private void DisablePacman()
    {
        if (movement != null) movement.enabled = false;
        if (col != null) col.enabled = false;
        if (sr != null) sr.enabled = false;
    }

    private void EnablePacman()
    {
        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;
        if (movement != null) movement.enabled = true;
    }
}