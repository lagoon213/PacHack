using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Ghost movement (tile-based).
///
/// Doet:
/// - bewegen van tile naar tile
/// - richting kiezen via GhostBrain
/// - snelheid (multipliers)
/// - 180° turn bij Scatter ↔ Chase switch
/// - animatie params updaten
/// - ghost house state (InHouse/CanExitHouse)
/// </summary>
public class GhostMovement : MonoBehaviour
{
    /* =========================
     * Referenties
     * ========================= */

    [SerializeField] private Animator animator;
    [SerializeField] private PacManMovement pacman;
    [SerializeField] private GhostBrain brain;
    [SerializeField] private GhostModeController modeController;

    /* =========================
     * Ghost house state (per ghost instelbaar)
     * ========================= */

    [Header("Ghost House State")]
    [SerializeField] private bool startInHouse = true;          // Blinky = false
    [SerializeField] private bool startCanExitHouse = false;    // meestal false

    public bool InHouse { get; private set; }
    public bool CanExitHouse { get; set; }

    /* =========================
     * Tilemaps
     * ========================= */

    [Header("Collision Tilemaps")]
    public Tilemap Walls;
    public Tilemap Ghost_Room;
    public Tilemap Ghost_Door;

    [Header("Special Speed Tilemaps")]
    [SerializeField] private Tilemap Tunnel;

    /* =========================
     * Snelheid-multipliers
     * ========================= */

    [Header("Speed Multipliers")]
    [SerializeField] private float normalMultiplier = 0.75f;     // Scatter/Chase
    [SerializeField] private float frightenedMultiplier = 0.5f;  // Frightened
    [SerializeField] private float houseMultiplier = 0.45f;      // In ghost house
    [SerializeField] private float tunnelMultiplier = 0.4f;      // Tunnel

    /* =========================
     * Runtime state
     * ========================= */

    /// <summary>
    /// Huidige richting (tile coords).
    /// </summary>
    public Vector2Int CurrentDir { get; private set; } = Vector2Int.right;

    /// <summary>
    /// Volgende world positie (center van volgende tile).
    /// </summary>
    public Vector3 TargetWorldPos { get; private set; }

    /* =========================
     * Unity lifecycle
     * ========================= */

    private void OnEnable()
    {
        // Luister naar mode-wissels (180° turn)
        if (modeController != null)
            modeController.OnModeChanged += HandleModeChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe (geen leaks)
        if (modeController != null)
            modeController.OnModeChanged -= HandleModeChanged;
    }

    private void Start()
    {
        if (Walls == null)
        {
            Debug.LogError($"{name}: Walls tilemap ontbreekt!");
            enabled = false;
            return;
        }

        // Init house state (per ghost via Inspector)
        InHouse = startInHouse;
        CanExitHouse = startCanExitHouse;

        // Start netjes in het midden van de tile
        var cell = Walls.WorldToCell(transform.position);
        TargetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = TargetWorldPos;
    }

    private void Update()
    {
        /* =========================
         * Tile-logica (richting kiezen)
         * ========================= */

        // Alleen kiezen als we tile-center bereikt hebben
        if (Vector3.Distance(transform.position, TargetWorldPos) < 0.001f)
        {
            transform.position = TargetWorldPos;

            // Check of ghost de ghost house heeft verlaten
            var cell = Walls.WorldToCell(transform.position);
            if (InHouse && Ghost_Room != null && !Ghost_Room.HasTile(cell))
            {
                InHouse = false;

                // Buiten: nooit terug door de deur
                CanExitHouse = false;
            }

            // Brain kiest gewenste richting
            Vector2Int desiredDir = brain != null ? brain.GetDesiredDir(this) : Vector2Int.zero;

            if (CanMove(desiredDir))
                SetDir(desiredDir);

            // Als huidige richting niet meer kan: stop
            if (!CanMove(CurrentDir))
                SetDir(Vector2Int.zero);

            // Volgende tile target zetten
            if (CurrentDir != Vector2Int.zero)
            {
                var currentCell = Walls.WorldToCell(transform.position);
                var nextCell = currentCell + new Vector3Int(CurrentDir.x, CurrentDir.y, 0);
                TargetWorldPos = Walls.GetCellCenterWorld(nextCell);
            }
        }

        /* =========================
         * Beweging uitvoeren
         * ========================= */

        if (pacman != null)
        {
            float speed = pacman.MoveSpeed * GetSpeedMultiplier();

            transform.position = Vector3.MoveTowards(
                transform.position,
                TargetWorldPos,
                speed * Time.deltaTime
            );
        }

        /* =========================
         * Animatie updaten
         * ========================= */

        if (animator != null)
        {
            animator.SetFloat("MoveX", CurrentDir.x);
            animator.SetFloat("MoveY", CurrentDir.y);

            animator.SetBool(
                "IsFrightened",
                modeController != null &&
                modeController.CurrentMode == GhostMode.Frightened
            );
        }
    }

    /* =========================
     * Mode-wissel handling
     * ========================= */

    /// <summary>
    /// 180° turn bij Scatter ↔ Chase (classic gedrag).
    /// </summary>
    private void HandleModeChanged(GhostMode oldMode, GhostMode newMode)
    {
        bool scatterChaseSwitch =
            (oldMode == GhostMode.Scatter && newMode == GhostMode.Chase) ||
            (oldMode == GhostMode.Chase && newMode == GhostMode.Scatter);

        if (!scatterChaseSwitch) return;
        if (CurrentDir == Vector2Int.zero) return;

        Vector2Int reversed = -CurrentDir;

        // In classic Pac-Man: always reverse (ook als het "terug" is)
        SetDir(reversed);

        // Gebruik de tile waar je NAARTOE ging als basis (stabieler dan transform)
        var currentCell = Walls.WorldToCell(TargetWorldPos);
        var nextCell = currentCell + new Vector3Int(CurrentDir.x, CurrentDir.y, 0);
        TargetWorldPos = Walls.GetCellCenterWorld(nextCell);
    }

    /* =========================
     * Helpers
     * ========================= */

    /// <summary>
    /// Zet de huidige richting.
    /// </summary>
    private void SetDir(Vector2Int dir)
    {
        CurrentDir = dir;
    }

    /// <summary>
    /// Check of de ghost die kant op kan (walls/door/room regels).
    /// </summary>
    public bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;
        if (Walls == null) return false;

        var currentCell = Walls.WorldToCell(transform.position);
        var nextCell = currentCell + new Vector3Int(dir.x, dir.y, 0);

        // Walls blokkeren altijd
        if (Walls.HasTile(nextCell)) return false;

        // Ghost room: alleen blokkeren als je buiten bent
        if (!InHouse && Ghost_Room != null && Ghost_Room.HasTile(nextCell))
            return false;

        // Ghost door:
        if (Ghost_Door != null && Ghost_Door.HasTile(nextCell))
        {
            // In house: alleen door als release actief is
            if (InHouse && !CanExitHouse) return false;

            // Buiten: nooit terug door de deur
            if (!InHouse) return false;
        }

        return true;
    }

    /// <summary>
    /// Bepaalt multiplier op basis van tile + mode:
    /// - in ghost house
    /// - tunnel
    /// - frightened
    /// - default
    /// </summary>
    private float GetSpeedMultiplier()
    {
        if (Walls == null) return normalMultiplier;

        Vector3Int cell = Walls.WorldToCell(transform.position);

        // In ghost house (alleen room, niet deur)
        if (Ghost_Room != null && Ghost_Room.HasTile(cell))
            return houseMultiplier;

        // Tunnel
        if (Tunnel != null && Tunnel.HasTile(cell))
            return tunnelMultiplier;

        // Frightened
        if (modeController != null && modeController.CurrentMode == GhostMode.Frightened)
            return frightenedMultiplier;

        // Default
        return normalMultiplier;
    }
}