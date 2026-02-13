using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Handles all ghost movement logic:
/// - Tile-based movement
/// - Direction updates based on GhostBrain
/// - Speed calculation relative to Pac-Man
/// - Mode-based behavior (Scatter / Chase / Frightened)
/// - 180-degree turn on Scatter ↔ Chase transitions
/// - Animation parameter updates
/// </summary>
public class GhostMovement : MonoBehaviour
{
    /* =========================
     * References
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
     * Speed Multipliers
     * (relative to Pac-Man speed)
     * ========================= */

    [Header("Speed Multipliers")]
    [SerializeField] private float normalMultiplier = 0.75f;     // Scatter / Chase
    [SerializeField] private float frightenedMultiplier = 0.5f;  // Frightened
    [SerializeField] private float houseMultiplier = 0.45f;      // Ghost room & door
    [SerializeField] private float tunnelMultiplier = 0.4f;      // Tunnel slowdown

    /* =========================
     * Runtime State
     * ========================= */

    /// <summary>
    /// Current movement direction in tile coordinates.
    /// </summary>
    public Vector2Int CurrentDir { get; private set; } = Vector2Int.right;

    /// <summary>
    /// World position of the next tile center the ghost is moving toward.
    /// </summary>
    public Vector3 TargetWorldPos { get; private set; }

    /* =========================
     * Unity Lifecycle
     * ========================= */

    private void OnEnable()
    {
        // Subscribe to mode change events (for 180° turn logic)
        if (modeController != null)
            modeController.OnModeChanged += HandleModeChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        if (modeController != null)
            modeController.OnModeChanged -= HandleModeChanged;
    }

    private void Start()
    {
        // Snap ghost to the center of its starting tile
        var cell = Walls.WorldToCell(transform.position);
        TargetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = TargetWorldPos;
    }

    private void Update()
    {
        /* =========================
         * Tile-based movement logic
         * ========================= */

        // Only choose a new direction when we reach the center of a tile
        if (Vector3.Distance(transform.position, TargetWorldPos) < 0.001f)
        {
            transform.position = TargetWorldPos;

            // Ask the brain which direction it wants to go
            Vector2Int desiredDir = brain.GetDesiredDir(this);

            if (CanMove(desiredDir))
                SetDir(desiredDir);

            // If current direction becomes invalid, stop movement
            if (!CanMove(CurrentDir))
                SetDir(Vector2Int.zero);

            // Set the next tile target
            if (CurrentDir != Vector2Int.zero)
            {
                var currentCell = Walls.WorldToCell(transform.position);
                var nextCell = currentCell + new Vector3Int(CurrentDir.x, CurrentDir.y, 0);
                TargetWorldPos = Walls.GetCellCenterWorld(nextCell);
            }
        }

        /* =========================
         * Movement execution
         * ========================= */

        // Ghost speed is derived from Pac-Man speed and current multipliers
        float speed = pacman.MoveSpeed * GetSpeedMultiplier();

        transform.position = Vector3.MoveTowards(
            transform.position,
            TargetWorldPos,
            speed * Time.deltaTime
        );

        /* =========================
         * Animation parameters
         * ========================= */

        animator.SetFloat("MoveX", CurrentDir.x);
        animator.SetFloat("MoveY", CurrentDir.y);

        // Frightened animation override
        animator.SetBool(
            "IsFrightened",
            modeController != null &&
            modeController.CurrentMode == GhostMode.Frightened
        );
    }

    /* =========================
     * Mode Change Handling
     * ========================= */

    /// <summary>
    /// Handles 180-degree turn when switching between Scatter and Chase.
    /// This mimics classic Pac-Man behavior.
    /// </summary>
    private void HandleModeChanged(GhostMode oldMode, GhostMode newMode)
    {
        bool scatterChaseSwitch =
            (oldMode == GhostMode.Scatter && newMode == GhostMode.Chase) ||
            (oldMode == GhostMode.Chase && newMode == GhostMode.Scatter);

        if (!scatterChaseSwitch) return;
        if (CurrentDir == Vector2Int.zero) return;

        Vector2Int reversed = -CurrentDir;

        // Only reverse if the direction is valid (prevents wall jitter)
        if (CanMove(reversed))
        {
            SetDir(reversed);

            // Force immediate retarget so the ghost reacts instantly
            var currentCell = Walls.WorldToCell(transform.position);
            var nextCell = currentCell + new Vector3Int(CurrentDir.x, CurrentDir.y, 0);
            TargetWorldPos = Walls.GetCellCenterWorld(nextCell);
        }
    }

    /* =========================
     * Helpers
     * ========================= */

    /// <summary>
     /// Updates the current movement direction.
     /// </summary>
    private void SetDir(Vector2Int dir)
    {
        CurrentDir = dir;
    }

    /// <summary>
     /// Checks whether the ghost can move in the given direction.
     /// </summary>
    public bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;

        var currentCell = Walls.WorldToCell(transform.position);
        var nextCell = currentCell + new Vector3Int(dir.x, dir.y, 0);

        // Block movement on walls, ghost doors, and ghost room tiles
        if (Walls.HasTile(nextCell) ||
            (Ghost_Door != null && Ghost_Door.HasTile(nextCell)) ||
            (Ghost_Room != null && Ghost_Room.HasTile(nextCell)))
            return false;

        return true;
    }

    /// <summary>
     /// Determines the correct speed multiplier based on:
     /// - Ghost house / door tiles
     /// - Tunnel tiles
     /// - Current ghost mode
     /// </summary>
    private float GetSpeedMultiplier()
    {
        Vector3Int cell = Walls.WorldToCell(transform.position);

        // 1) Ghost house & door slowdown
        if ((Ghost_Room != null && Ghost_Room.HasTile(cell)) ||
            (Ghost_Door != null && Ghost_Door.HasTile(cell)))
        {
            return houseMultiplier;
        }

        // 2) Tunnel slowdown
        if (Tunnel != null && Tunnel.HasTile(cell))
        {
            return tunnelMultiplier;
        }

        // 3) Frightened slowdown
        if (modeController != null &&
            modeController.CurrentMode == GhostMode.Frightened)
        {
            return frightenedMultiplier;
        }

        // 4) Default (Scatter / Chase)
        return normalMultiplier;
    }
}
