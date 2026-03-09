using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Ghost movement (tile-based) with optional snake growth logic.
/// Ghost segments follow correctly and grow when Pac-Man eats a power pellet.
/// </summary>
public class GhostMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PacManMovement pacman;
    [SerializeField] private GhostBrain brain;
    [SerializeField] private GhostModeController modeController;

    [Header("Snake/Growth")]
    [SerializeField] private SegmentSpawner segmentSpawner; // optional for ghosts

    [Header("Ghost House")]
    [SerializeField] private bool startInHouse = true;
    [SerializeField] private bool startCanExitHouse = false;

    public bool InHouse { get; private set; }
    public bool CanExitHouse { get; set; }

    [Header("Tilemaps")]
    public Tilemap Walls;
    public Tilemap Ghost_Room;
    public Tilemap Ghost_Door;
    [SerializeField] private Tilemap Tunnel;

    [Header("Warp")]
    [SerializeField] private Transform warpTileLeft;
    [SerializeField] private Transform warpTileRight;
    [SerializeField] private float warpCooldown = 0.12f;

    [Header("Speed Multipliers")]
    [SerializeField] private float normalMultiplier = 0.75f;
    [SerializeField] private float frightenedMultiplier = 0.5f;
    [SerializeField] private float houseMultiplier = 0.45f;
    [SerializeField] private float tunnelMultiplier = 0.4f;

    private Vector2Int currentDir = Vector2Int.right;
    private Vector3 targetWorldPos;
    private Vector3Int leftWarpCell, rightWarpCell;
    private float nextAllowedWarpTime;

    private void OnEnable()
    {
        if (modeController != null)
            modeController.OnModeChanged += HandleModeChanged;
    }

    private void OnDisable()
    {
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

        InHouse = startInHouse;
        CanExitHouse = startCanExitHouse;

        var cell = Walls.WorldToCell(transform.position);
        targetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = targetWorldPos;

        CacheWarpCells();

        // Subscribe to power pellet consumption event
        PowerPelletBlinkTilemap pelletBlink = Object.FindFirstObjectByType<PowerPelletBlinkTilemap>();
        if (pelletBlink != null)
            pelletBlink.OnPowerPelletConsumed += HandlePowerPelletConsumed;
    }

    private void HandlePowerPelletConsumed(Vector3 worldPos)
    {
        if (segmentSpawner != null)
            segmentSpawner.Grow();
    }

    private void Update()
    {
        var currentTile = Walls.WorldToCell(transform.position);

        // Record ghost movement for snake segment tracking
        if (segmentSpawner != null)
            segmentSpawner.RecordMove(currentTile);

        // Tile-center logic
        if (Vector3.Distance(transform.position, targetWorldPos) < 0.001f)
        {
            transform.position = targetWorldPos;

            if (ApplyWarpIfOnWarpTile()) return;

            if (InHouse && Ghost_Room != null && !Ghost_Room.HasTile(currentTile))
            {
                InHouse = false;
                CanExitHouse = false;
            }

            Vector2Int desiredDir = brain != null ? brain.GetDesiredDir(this) : Vector2Int.zero;
            if (CanMove(desiredDir)) currentDir = desiredDir;
            if (!CanMove(currentDir)) currentDir = Vector2Int.zero;

            if (currentDir != Vector2Int.zero)
            {
                var nextCell = currentTile + new Vector3Int(currentDir.x, currentDir.y, 0);
                targetWorldPos = Walls.GetCellCenterWorld(nextCell);
            }
        }

        // Movement
        if (pacman != null)
        {
            float speed = pacman.MoveSpeed * GetSpeedMultiplier();
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, speed * Time.deltaTime);
        }

        // Animation
        if (animator != null)
        {
            animator.SetFloat("MoveX", currentDir.x);
            animator.SetFloat("MoveY", currentDir.y);
            animator.SetBool("IsFrightened", modeController != null && modeController.CurrentMode == GhostMode.Frightened);
        }
    }

    // ===== Helpers =====

    private void HandleModeChanged(GhostMode oldMode, GhostMode newMode)
    {
        bool scatterChaseSwitch =
            (oldMode == GhostMode.Scatter && newMode == GhostMode.Chase) ||
            (oldMode == GhostMode.Chase && newMode == GhostMode.Scatter);

        if (!scatterChaseSwitch || currentDir == Vector2Int.zero) return;

        currentDir = -currentDir;
        var currentCell = Walls.WorldToCell(targetWorldPos);
        var nextCell = currentCell + new Vector3Int(currentDir.x, currentDir.y, 0);
        targetWorldPos = Walls.GetCellCenterWorld(nextCell);
    }

    private void CacheWarpCells()
    {
        if (warpTileLeft != null) leftWarpCell = Walls.WorldToCell(warpTileLeft.position);
        if (warpTileRight != null) rightWarpCell = Walls.WorldToCell(warpTileRight.position);
    }

    private bool ApplyWarpIfOnWarpTile()
    {
        if (Time.time < nextAllowedWarpTime) return false;

        var cell = Walls.WorldToCell(transform.position);
        if (cell == leftWarpCell) cell = rightWarpCell;
        else if (cell == rightWarpCell) cell = leftWarpCell;
        else return false;

        nextAllowedWarpTime = Time.time + warpCooldown;
        targetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = targetWorldPos;
        return true;
    }

    public bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;
        var cell = Walls.WorldToCell(transform.position);
        var nextCell = cell + new Vector3Int(dir.x, dir.y, 0);

        if (Walls.HasTile(nextCell)) return false;
        if (!InHouse && Ghost_Room != null && Ghost_Room.HasTile(nextCell)) return false;
        if (Ghost_Door != null && Ghost_Door.HasTile(nextCell))
        {
            if (InHouse && !CanExitHouse) return false;
            if (!InHouse) return false;
        }

        return true;
    }

    private float GetSpeedMultiplier()
    {
        var cell = Walls.WorldToCell(transform.position);

        if (Ghost_Room != null && Ghost_Room.HasTile(cell)) return houseMultiplier;
        if (Tunnel != null && Tunnel.HasTile(cell)) return tunnelMultiplier;
        if (modeController != null && modeController.CurrentMode == GhostMode.Frightened) return frightenedMultiplier;
        return normalMultiplier;
    }

    public Vector2Int CurrentDir => currentDir;
}