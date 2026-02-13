using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

/// <summary>
/// Handles Pac-Man movement and interaction:
/// - Tile-based grid movement
/// - Input handling (new Input System)
/// - Wall / ghost-room collision
/// - Pellet consumption
/// - Visual rotation and animation control
/// 
/// Pac-Man acts as the speed baseline for all ghosts.
/// </summary>
public class PacManMovement : MonoBehaviour
{
    /* =========================
     * References
     * ========================= */

    [SerializeField] private Animator animator;
    [SerializeField] private Transform visual;

    /* =========================
     * Tilemaps
     * ========================= */

    public Tilemap Walls;
    public Tilemap Ghost_Room;
    public Tilemap Ghost_Door;
    public Tilemap Pellets;

    /* =========================
     * Movement settings
     * ========================= */

    [SerializeField] private float moveSpeed = 5f;

    /// <summary>
    /// Public read-only access to Pac-Man speed.
    /// Used as baseline for ghost movement.
    /// </summary>
    public float MoveSpeed => moveSpeed;

    /* =========================
     * Movement state
     * ========================= */

    /// <summary>
    /// Current movement direction in tile coordinates.
    /// </summary>
    private Vector2Int _currentDir = Vector2Int.right;

    /// <summary>
    /// Direction requested by player input.
    /// </summary>
    private Vector2Int _desiredDir = Vector2Int.right;

    /// <summary>
    /// Raw input vector from the Input System.
    /// </summary>
    private Vector2 _moveInput;

    /// <summary>
    /// World-space position of the next tile center.
    /// </summary>
    private Vector3 _targetWorldPos;

    /* =========================
     * Unity Lifecycle
     * ========================= */

    private void Start()
    {
        // Snap Pac-Man to the center of the starting tile
        var cell = Walls.WorldToCell(transform.position);
        _targetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = _targetWorldPos;
    }

    private void Update()
    {
        ReadInput();

        /* =========================
         * Tile-based movement logic
         * ========================= */

        // Only update direction when we reach the center of a tile
        if (Vector3.Distance(transform.position, _targetWorldPos) < 0.001f)
        {
            transform.position = _targetWorldPos;

            // Try to apply desired input direction
            if (CanMove(_desiredDir))
            {
                _currentDir = _desiredDir;
            }

            // Stop movement if current direction becomes blocked
            if (!CanMove(_currentDir))
            {
                _currentDir = Vector2Int.zero;
            }

            // Set the next tile target
            if (_currentDir != Vector2Int.zero)
            {
                var currentCell = Walls.WorldToCell(transform.position);
                var nextCell = currentCell + new Vector3Int(_currentDir.x, _currentDir.y, 0);
                _targetWorldPos = Walls.GetCellCenterWorld(nextCell);
            }

            /* =========================
             * Pellet consumption
             * ========================= */

            var pelletCell = Pellets.WorldToCell(transform.position);
            if (Pellets.HasTile(pelletCell))
            {
                Pellets.SetTile(pelletCell, null);
            }
        }

        /* =========================
         * Movement execution
         * ========================= */

        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetWorldPos,
            moveSpeed * Time.deltaTime
        );

        /* =========================
         * Visual & animation updates
         * ========================= */

        UpdateFacing();

        // Pause animation when Pac-Man is not moving
        bool isMoving = _currentDir != Vector2Int.zero;
        animator.speed = isMoving ? 1f : 0f;
    }

    /* =========================
     * Input Handling
     * ========================= */

    /// <summary>
    /// Called by the Input System when movement input is received.
    /// </summary>
    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    /// <summary>
    /// Converts raw input into a valid grid direction.
    /// Prevents diagonal movement.
    /// </summary>
    private void ReadInput()
    {
        int x = Mathf.RoundToInt(_moveInput.x);
        int y = Mathf.RoundToInt(_moveInput.y);

        // Prevent diagonal movement (horizontal has priority)
        if (x != 0) y = 0;

        if (x != 0 || y != 0)
        {
            _desiredDir = new Vector2Int(x, y);
        }
    }

    /* =========================
     * Collision & helpers
     * ========================= */

    /// <summary>
    /// Checks whether Pac-Man can move in the given direction.
    /// Blocks walls, ghost room, and ghost door tiles.
    /// </summary>
    private bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;

        var currentCell = Walls.WorldToCell(transform.position);
        var nextCell = currentCell + new Vector3Int(dir.x, dir.y, 0);

        if (Walls.HasTile(nextCell) ||
            Ghost_Door.HasTile(nextCell) ||
            Ghost_Room.HasTile(nextCell))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Rotates the visual child object to face the current movement direction.
    /// </summary>
    private void UpdateFacing()
    {
        if (_currentDir == Vector2Int.right)
            visual.rotation = Quaternion.Euler(0, 0, 0);
        else if (_currentDir == Vector2Int.up)
            visual.rotation = Quaternion.Euler(0, 0, 90);
        else if (_currentDir == Vector2Int.left)
            visual.rotation = Quaternion.Euler(0, 0, 180);
        else if (_currentDir == Vector2Int.down)
            visual.rotation = Quaternion.Euler(0, 0, -90);
    }

    /// <summary>
    /// Exposes Pac-Man's current direction for ghost AI (e.g. Pinky).
    /// </summary>
    public Vector2Int CurrentDir => _currentDir;
}
