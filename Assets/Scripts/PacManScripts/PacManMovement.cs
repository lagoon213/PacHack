using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PacManMovement : MonoBehaviour
{
    /* =========================
     * References
     * ========================= */

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visual;
    [SerializeField] private SegmentSpawner segmentSpawner;

    /* =========================
     * Tilemaps
     * ========================= */

    [Header("Tilemaps")]
    public Tilemap Walls;
    public Tilemap Ghost_Room;
    public Tilemap Ghost_Door;
    public Tilemap Pellets;

    /* =========================
     * Movement
     * ========================= */

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    public float MoveSpeed => moveSpeed;

    /* =========================
     * Warp
     * ========================= */

    [Header("Warp")]
    [SerializeField] private Transform warpTileLeft;
    [SerializeField] private Transform warpTileRight;
    [SerializeField] private float warpCooldown = 0.12f;

    private Vector3Int leftWarpCell;
    private Vector3Int rightWarpCell;
    private float nextAllowedWarpTime;

    /* =========================
     * Movement state
     * ========================= */

    private Vector2Int _currentDir = Vector2Int.right;
    private Vector2Int _desiredDir = Vector2Int.right;
    private Vector2 _moveInput;

    private Vector3 _targetWorldPos;

    public Vector3 LastMoveDirection { get; private set; } = Vector3.right;

    /* =========================
     * Snake
     * ========================= */

    private int pelletsUntilGrowth = 10;

    /* =========================
     * Start
     * ========================= */

    private void Start()
    {
        var cell = Walls.WorldToCell(transform.position);

        _targetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = _targetWorldPos;

        CacheWarpCells();
    }

    /* =========================
     * Update
     * ========================= */

    private void Update()
    {
        ReadInput();

        if (Vector3.Distance(transform.position, _targetWorldPos) < 0.001f)
        {
            Vector3Int currentTile = Walls.WorldToCell(transform.position);

            /* ===== Snake tracking ===== */

            if (segmentSpawner != null)
            {
                segmentSpawner.RecordMove(currentTile);

                if (segmentSpawner.CheckSelfCollision())
                {
                    Debug.LogWarning("YOU DIED");
                    return;
                }
            }

            /* ===== Warp ===== */

            if (ApplyWarp())
                return;

            /* ===== Direction ===== */

            if (CanMove(_desiredDir))
                _currentDir = _desiredDir;

            if (!CanMove(_currentDir))
                _currentDir = Vector2Int.zero;

            if (_currentDir != Vector2Int.zero)
            {
                Vector3Int nextCell =
                    currentTile + new Vector3Int(_currentDir.x, _currentDir.y, 0);

                _targetWorldPos = Walls.GetCellCenterWorld(nextCell);

                LastMoveDirection = new Vector3(_currentDir.x, _currentDir.y, 0);
            }

            /* ===== Pellets ===== */

            HandlePellets();
        }

        /* ===== Movement ===== */

        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetWorldPos,
            moveSpeed * Time.deltaTime
        );

        UpdateFacing();

        bool isMoving = _currentDir != Vector2Int.zero;

        if (animator != null)
            animator.speed = isMoving ? 1f : 0f;
    }

    /* =========================
     * Pellets
     * ========================= */

    private void HandlePellets()
    {
        Vector3Int pelletCell = Pellets.WorldToCell(transform.position);

        if (Pellets.HasTile(pelletCell))
        {
            ScoreManager.Instance.AddScore(10);
            ScoreManager.Instance.PelletEaten(transform.position);

            pelletsUntilGrowth--;

            if (pelletsUntilGrowth <= 0)
            {
                pelletsUntilGrowth = 10;

                if (segmentSpawner != null)
                    segmentSpawner.Grow();
            }
        }
    }

    /* =========================
     * Input
     * ========================= */

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    private void ReadInput()
    {
        int x = Mathf.RoundToInt(_moveInput.x);
        int y = Mathf.RoundToInt(_moveInput.y);

        if (x != 0)
            y = 0;

        if (x != 0 || y != 0)
            _desiredDir = new Vector2Int(x, y);
    }

    /* =========================
     * Warp
     * ========================= */

    private void CacheWarpCells()
    {
        if (warpTileLeft != null)
            leftWarpCell = Walls.WorldToCell(warpTileLeft.position);

        if (warpTileRight != null)
            rightWarpCell = Walls.WorldToCell(warpTileRight.position);
    }

    private bool ApplyWarp()
    {
        if (Time.time < nextAllowedWarpTime)
            return false;

        Vector3Int cell = Walls.WorldToCell(transform.position);

        if (cell == leftWarpCell)
            cell = rightWarpCell;
        else if (cell == rightWarpCell)
            cell = leftWarpCell;
        else
            return false;

        nextAllowedWarpTime = Time.time + warpCooldown;

        _targetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = _targetWorldPos;

        return true;
    }

    /* =========================
     * Movement checks
     * ========================= */

    private bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero)
            return false;

        Vector3Int currentCell = Walls.WorldToCell(transform.position);

        Vector3Int nextCell =
            currentCell + new Vector3Int(dir.x, dir.y, 0);

        if ((Walls != null && Walls.HasTile(nextCell)) ||
            (Ghost_Door != null && Ghost_Door.HasTile(nextCell)) ||
            (Ghost_Room != null && Ghost_Room.HasTile(nextCell)))
            return false;

        return true;
    }

    /* =========================
     * Visual facing
     * ========================= */

    private void UpdateFacing()
    {
        if (visual == null)
            return;

        if (_currentDir == Vector2Int.right)
            visual.rotation = Quaternion.Euler(0, 0, 0);
        else if (_currentDir == Vector2Int.up)
            visual.rotation = Quaternion.Euler(0, 0, 90);
        else if (_currentDir == Vector2Int.left)
            visual.rotation = Quaternion.Euler(0, 0, 180);
        else if (_currentDir == Vector2Int.down)
            visual.rotation = Quaternion.Euler(0, 0, -90);
    }

    /* =========================
     * Ghost AI access
     * ========================= */

    public Vector2Int CurrentDir => _currentDir;
}