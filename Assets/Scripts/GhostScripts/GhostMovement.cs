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
    [SerializeField] private float houseMultiplier = 0.45f;      // Ghost house/door
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

            // Brain kiest gewenste richting
            Vector2Int desiredDir = brain.GetDesiredDir(this);

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

        // Snelheid = Pac-Man speed * multiplier
        float speed = pacman.MoveSpeed * GetSpeedMultiplier();

        transform.position = Vector3.MoveTowards(
            transform.position,
            TargetWorldPos,
            speed * Time.deltaTime
        );

        /* =========================
         * Animatie updaten
         * ========================= */

        animator.SetFloat("MoveX", CurrentDir.x);
        animator.SetFloat("MoveY", CurrentDir.y);

        // Frightened animatie aan/uit
        animator.SetBool(
            "IsFrightened",
            modeController != null &&
            modeController.CurrentMode == GhostMode.Frightened
        );
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

        // Alleen omkeren als het kan (geen wall-jitter)
        if (CanMove(reversed))
        {
            SetDir(reversed);

            // Direct nieuwe target zodat reactie instant is
            var currentCell = Walls.WorldToCell(transform.position);
            var nextCell = currentCell + new Vector3Int(CurrentDir.x, CurrentDir.y, 0);
            TargetWorldPos = Walls.GetCellCenterWorld(nextCell);
        }
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
    /// Check of de ghost die kant op kan (geen muur/door/room).
    /// </summary>
    public bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;

        var currentCell = Walls.WorldToCell(transform.position);
        var nextCell = currentCell + new Vector3Int(dir.x, dir.y, 0);

        // Block: walls + ghost door + ghost room
        if (Walls.HasTile(nextCell) ||
            (Ghost_Door != null && Ghost_Door.HasTile(nextCell)) ||
            (Ghost_Room != null && Ghost_Room.HasTile(nextCell)))
            return false;

        return true;
    }

    /// <summary>
    /// Bepaalt multiplier op basis van tile + mode:
    /// - house/door
    /// - tunnel
    /// - frightened
    /// - default
    /// </summary>
    private float GetSpeedMultiplier()
    {
        Vector3Int cell = Walls.WorldToCell(transform.position);

        // House/door
        if ((Ghost_Room != null && Ghost_Room.HasTile(cell)) ||
            (Ghost_Door != null && Ghost_Door.HasTile(cell)))
            return houseMultiplier;

        // Tunnel
        if (Tunnel != null && Tunnel.HasTile(cell))
            return tunnelMultiplier;

        // Frightened
        if (modeController != null &&
            modeController.CurrentMode == GhostMode.Frightened)
            return frightenedMultiplier;

        // Default
        return normalMultiplier;
    }
}