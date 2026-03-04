using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GhostEatable : MonoBehaviour
{
    [SerializeField] private GhostModeController ghostModeController;
    [SerializeField] private int eatenPoints = 200;

    private Collider2D col;
    private GhostMovement movement;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        movement = GetComponent<GhostMovement>();

        if (ghostModeController == null)
            ghostModeController = FindObjectOfType<GhostModeController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        HandleHitPlayer(other.gameObject);
    }

    private void HandleHitPlayer(GameObject player)
    {
        if (movement != null && movement.IsEyes)
            return;

        bool frightened =
            ghostModeController != null &&
            ghostModeController.CurrentMode == GhostMode.Frightened;

        if (frightened)
            EatGhost();
        else
        {
            var death = player.GetComponent<PacmanDeath>();
            if (death != null) death.Die();
            else player.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void EatGhost()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(eatenPoints);

        // ✅ eyes start exact op death tile
        if (movement != null)
            movement.EnterEyesMode(transform.position);

        // collider uit tot body terug is
        if (col != null)
            col.enabled = false;

        CancelInvoke(nameof(ReEnableColliderWhenBack));
        InvokeRepeating(nameof(ReEnableColliderWhenBack), 0.1f, 0.1f);
    }

    private void ReEnableColliderWhenBack()
    {
        if (movement == null)
        {
            CancelInvoke(nameof(ReEnableColliderWhenBack));
            return;
        }

        if (!movement.IsEyes)
        {
            if (col != null)
                col.enabled = true;

            CancelInvoke(nameof(ReEnableColliderWhenBack));
        }
    }
}