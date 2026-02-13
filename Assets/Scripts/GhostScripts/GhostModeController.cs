using UnityEngine;

public class GhostModeController : MonoBehaviour
{
    [System.Serializable]
    public struct Phase
    {
        public GhostMode mode;     // alleen Scatter/Chase in inspector
        public float duration;
    }

    public System.Action<GhostMode, GhostMode> OnModeChanged;

    [SerializeField] private Phase[] phases;

    public GhostMode CurrentMode { get; private set; }

    private int phaseIndex;
    private float phaseTimer;

    private bool frightenedActive;
    private float frightenedTimer;
    private GhostMode modeBeforeFrightened;

    private void Start()
    {
        phaseIndex = 0;
        phaseTimer = 0f;

        if (phases != null && phases.Length > 0)
            SetMode(phases[0].mode);
        else
            SetMode(GhostMode.Chase);
    }

    private void Update()
    {
        // 1) Frightened override
        if (frightenedActive)
        {
            frightenedTimer -= Time.deltaTime;
            if (frightenedTimer <= 0f)
            {
                frightenedActive = false;
                SetMode(modeBeforeFrightened);
            }
            return; // zolang frightened: geen fase-switching
        }

        // 2) Normale Scatter/Chase phases
        if (phases == null || phases.Length == 0) return;

        // Als schema klaar is: permanent chase
        if (phaseIndex >= phases.Length)
        {
            SetMode(GhostMode.Chase);
            return;
        }

        phaseTimer += Time.deltaTime;
        if (phaseTimer >= phases[phaseIndex].duration)
        {
            phaseTimer = 0f;
            phaseIndex++;

            if (phaseIndex < phases.Length)
                SetMode(phases[phaseIndex].mode);
            else
                SetMode(GhostMode.Chase); // permanent
        }
    }

    public void TriggerFrightened(float duration)
    {
        // Als we al frightened zijn: reset timer (Pac-Man gedrag)
        if (!frightenedActive)
        {
            modeBeforeFrightened = CurrentMode;
        }

        frightenedActive = true;
        frightenedTimer = duration;
        SetMode(GhostMode.Frightened);
    }

    private void SetMode(GhostMode newMode)
    {
        if (CurrentMode == newMode) return;

        var oldMode = CurrentMode;
        CurrentMode = newMode;
        OnModeChanged?.Invoke(oldMode, CurrentMode);
    }
}
