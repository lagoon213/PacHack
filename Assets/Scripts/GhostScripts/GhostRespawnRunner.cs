using System.Collections;
using UnityEngine;

public class GhostRespawnRunner : MonoBehaviour
{
    public static GhostRespawnRunner Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RunRespawn(GameObject ghost, Transform respawnPoint, float delay, System.Action onRespawned)
    {
        StartCoroutine(RespawnRoutine(ghost, respawnPoint, delay, onRespawned));
    }

    private IEnumerator RespawnRoutine(GameObject ghost, Transform respawnPoint, float delay, System.Action onRespawned)
    {
        yield return new WaitForSeconds(delay);

        if (ghost == null) yield break;

        // 1) positie zetten terwijl hij nog uit staat (belangrijk!)
        if (respawnPoint != null)
            ghost.transform.position = respawnPoint.position;

        // 2) physics resetten (zodat hij niet "doorschiet")
        var rb = ghost.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            Physics2D.SyncTransforms();
        }

        // 3) nu pas aanzetten
        ghost.SetActive(true);

        // 4) movement state resetten (Fix 2)
        ghost.SendMessage("OnRespawned", SendMessageOptions.DontRequireReceiver);

        onRespawned?.Invoke();
    }
}