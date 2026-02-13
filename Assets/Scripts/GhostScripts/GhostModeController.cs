using UnityEngine;

/// <summary>
/// Central controller that manages the global ghost behavior modes.
///
/// Responsibilities:
/// - Handle the Scatter / Chase timing phases
/// - Temporarily override behavior with Frightened mode
/// - Restore the correct mode after Frightened ends
/// - Broadcast mode changes to all ghosts (e.g. for 180° turns)
///
/// This controller acts as the single source of truth for ghost modes.
/// </summary>
public class GhostModeController : MonoBehaviour
{
    /* =========================
     * Phase configuration
     * ========================= */

    /// <summary>
    /// Represents a timed Scatter or Chase phase.
    /// Only these modes are configured in the inspector.
    /// </summary>
    [System.Serializable]
    public struct Phase
    {
        public GhostMode mode;     // Scatter or Chase
        public float duration;     // Duration in seconds
    }

    /// <summary>
    /// Event fired whenever the ghost mode changes.
    /// Arguments: (oldMode, newMode)
    /// Used by GhostMovement for 180° turn behavior.
    /// </summary>
    public System.Action<GhostMode, GhostMode> OnModeChanged;

    /// <summary>
    /// Ordered list of Scatter / Chase phases for the current level.
    /// After the final phase, ghosts remain in Chase permanently.
    /// </summary>
    [SerializeField] private Phase[] phases;

    /* =========================
     * Current state
     * ========================= */

    /// <summary>
    /// The currently active global ghost mode.
    /// </summary>
    public GhostMode CurrentMode { get; private set; }

    private int phaseIndex;
    private float phaseTimer;

    /* =========================
     * Frightened override state
     * ========================= */

    private bool frightenedActive;
    private float frightenedTimer;
    private GhostMode modeBeforeFrightened;

    /* =========================
     * Unity Lifecycle
     * ========================= */

    private void Start()
    {
        phaseIndex = 0;
        phaseTimer = 0f;

        // Initialize starting mode
        if (phases != null && phases.Length > 0)
            SetMode(phases[0].mode);
        else
            SetMode(GhostMode.Chase);
    }

    private void Update()
    {
        /* =========================
         * Frightened override logic
         * ========================= */

        // While frightened is active, suspend normal phase switching
        if (frightenedActive)
        {
            frightenedTimer -= Time.deltaTime;

            if (frightenedTimer <= 0f)
            {
                frightenedActive = false;
                SetMode(modeBeforeFrightened);
            }

            return;
        }

        /* =========================
         * Scatter / Chase phase logic
         * ========================= */

        if (phases == null || phases.Length == 0)
            return;

        // If all phases are completed, remain in Chase permanently
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
                SetMode(GhostMode.Chase); // permanent Chase after final phase
        }
    }

    /* =========================
     * Public API
     * ========================= */

    /// <summary>
    /// Triggers Frightened mode for the given duration.
    /// If already frightened, the timer is reset.
    /// </summary>
    public void TriggerFrightened(float duration)
    {
        // Store previous mode only the first time frightened is triggered
        if (!frightenedActive)
        {
            modeBeforeFrightened = CurrentMode;
        }

        frightenedActive = true;
        frightenedTimer = duration;
        SetMode(GhostMode.Frightened);
    }

    /* =========================
     * Internal helpers
     * ========================= */

    /// <summary>
    /// Sets the current ghost mode and notifies listeners
    /// if the mode has changed.
    /// </summary>
    private void SetMode(GhostMode newMode)
    {
        if (CurrentMode == newMode)
            return;

        var oldMode = CurrentMode;
        CurrentMode = newMode;

        // Notify all subscribers of the mode change
        OnModeChanged?.Invoke(oldMode, CurrentMode);
    }
}
