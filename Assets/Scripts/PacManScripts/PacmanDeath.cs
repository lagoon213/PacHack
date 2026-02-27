using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PacmanDeath : MonoBehaviour
{
    [Header("Lives / Respawn")]
    public int lives = 3;
    public float respawnDelay = 1f;
    public Transform respawnPoint;

    [Header("Game Over")]
    public FadeIn gameOverFade;

    private bool isDead = false;
    private bool isGameOver = false;

    
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private PacManMovement movement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        movement = GetComponent<PacManMovement>();
    }

    private void Update()
    {
        if (!isGameOver) return;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
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

        Debug.Log($"Pac-Man dood! Lives over: {lives}");

        if (lives <= 0)
        {
            GameOver();
            return;
        }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        DisablePacman();

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

    private void GameOver()
    {
        
        DisablePacman();

        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverFade != null)
            gameOverFade.ShowAndFadeIn();
        else
            Debug.LogError("GameOverFade is niet gekoppeld in de Inspector!");
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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