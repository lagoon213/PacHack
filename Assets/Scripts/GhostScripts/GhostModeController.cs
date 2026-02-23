using UnityEngine;

/// <summary>
/// Centrale controller voor ghost modes.
///
/// Regelt:
/// - timing van Scatter/Chase fases
/// - tijdelijke Frightened override
/// - terugzetten naar vorige mode
/// - event bij mode-wissel (o.a. 180° turn)
/// </summary>
public class GhostModeController : MonoBehaviour
{
    /* =========================
     * Fase-configuratie
     * ========================= */

    /// <summary>
    /// Eén Scatter/Chase fase met duur.
    /// </summary>
    [System.Serializable]
    public struct Phase
    {
        public GhostMode mode;   // Scatter of Chase
        public float duration;   // seconden
    }

    /// <summary>
    /// Wordt gefired bij mode-wissel: (oud, nieuw).
    /// </summary>
    public System.Action<GhostMode, GhostMode> OnModeChanged;

    /// <summary>
    /// Volgorde van Scatter/Chase fases.
    /// Na laatste fase blijft het Chase.
    /// </summary>
    [SerializeField] private Phase[] phases;

    /* =========================
     * Huidige state
     * ========================= */

    /// <summary>
    /// Actieve globale ghost mode.
    /// </summary>
    public GhostMode CurrentMode { get; private set; }

    private int phaseIndex;
    private float phaseTimer;

    /* =========================
     * Frightened override
     * ========================= */

    private bool frightenedActive;
    private float frightenedTimer;
    private GhostMode modeBeforeFrightened;

    /* =========================
     * Unity lifecycle
     * ========================= */

    private void Start()
    {
        phaseIndex = 0;
        phaseTimer = 0f;

        // Start in eerste fase, anders direct Chase
        if (phases != null && phases.Length > 0)
            SetMode(phases[0].mode);
        else
            SetMode(GhostMode.Chase);
    }

    private void Update()
    {
        /* =========================
         * Frightened actief
         * ========================= */

        // Tijdens frightened pauzeer je fase-timing
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
         * Scatter/Chase fases
         * ========================= */

        if (phases == null || phases.Length == 0)
            return;

        // Klaar met fases: blijf in Chase
        if (phaseIndex >= phases.Length)
        {
            SetMode(GhostMode.Chase);
            return;
        }

        phaseTimer += Time.deltaTime;

        // Volgende fase als timer voorbij is
        if (phaseTimer >= phases[phaseIndex].duration)
        {
            phaseTimer = 0f;
            phaseIndex++;

            if (phaseIndex < phases.Length)
                SetMode(phases[phaseIndex].mode);
            else
                SetMode(GhostMode.Chase); // permanent na laatste fase
        }
    }

    /* =========================
     * Public API
     * ========================= */

    /// <summary>
    /// Zet Frightened aan voor 'duration' seconden.
    /// Als al actief: timer reset.
    /// </summary>
    public void TriggerFrightened(float duration)
    {
        // Vorige mode alleen 1x opslaan
        if (!frightenedActive)
            modeBeforeFrightened = CurrentMode;

        frightenedActive = true;
        frightenedTimer = duration;
        SetMode(GhostMode.Frightened);
    }

    /* =========================
     * Intern
     * ========================= */

    /// <summary>
    /// Zet mode en fire event als hij veranderd is.
    /// </summary>
    private void SetMode(GhostMode newMode)
    {
        if (CurrentMode == newMode)
            return;

        var oldMode = CurrentMode;
        CurrentMode = newMode;

        OnModeChanged?.Invoke(oldMode, CurrentMode);
    }
}