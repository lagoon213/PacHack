using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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
     * Warp markers
     * ========================= */

    [Header("Warp Markers")]
    [SerializeField] private Transform warpTileLeft;
    [SerializeField] private Transform warpTileRight;

    /* =========================
     * Movement
     * ========================= */

    [SerializeField] private float moveSpeed = 5f;
    public float MoveSpeed => moveSpeed;

    /* =========================
     * Warp
     * ========================= */

    [SerializeField] private float warpCooldown = 0.12f;

    private Vector3Int leftWarpCell = new Vector3Int(int.MinValue, int.MinValue, 0);
    private Vector3Int rightWarpCell = new Vector3Int(int.MinValue, int.MinValue, 0);
    private float nextAllowedWarpTime;

    /* =========================
     * Movement state
     * ========================= */

    private Vector2Int _currentDir = Vector2Int.right;
    private Vector2Int _desiredDir = Vector2Int.right;
    private Vector2 _moveInput;

    private Vector3 _targetWorldPos;

    /* =========================
     * Snake system
     * ========================= */

    [Header("Snake")]
    [SerializeField] private GameObject snakeBodyPrefab;

    private List<GameObject> snakeSegments = new List<GameObject>();
    private List<Vector3Int> moveHistory = new List<Vector3Int>();

    private Vector3Int previousTile;

    private int pelletsUntilGrowth = 10;

    public Vector3 LastMoveDirection { get; private set; } = Vector3.right;
    public bool snakeMode;

    /* =========================
     * Start
     * ========================= */

    private void Start()
    {
        if (Walls == null)
        {
            Debug.LogError($"{name}: Walls tilemap missing!");
            enabled = false;
            return;
        }

        var cell = Walls.WorldToCell(transform.position);
        _targetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = _targetWorldPos;

        CacheWarpCellsFromMarkers();

        previousTile = cell;
        moveHistory.Add(previousTile);
    }

    /* =========================
     * Update
     * ========================= */

    private void Update()
    {
        ReadInput();

        if (Vector3.Distance(transform.position, _targetWorldPos) < 0.001f)
        {
            var currentTile = Walls.WorldToCell(transform.position);

            /* ===== Snake path tracking ===== */

            if (currentTile != previousTile)
            {
                moveHistory.Insert(0, currentTile);
                previousTile = currentTile;

                UpdateSnakeSegments();
                CheckSelfCollision();

                int maxHistory = snakeSegments.Count + 5;

                if (moveHistory.Count > maxHistory)
                    moveHistory.RemoveAt(moveHistory.Count - 1);
            }           

            /* ===== Warp ===== */

            if (ApplyWarpIfOnWarpTile())
                return;

            /* ===== Direction ===== */

            if (CanMove(_desiredDir))
                _currentDir = _desiredDir;

            if (!CanMove(_currentDir))
                _currentDir = Vector2Int.zero;

            if (_currentDir != Vector2Int.zero)
            {
                var currentCell = Walls.WorldToCell(transform.position);
                var nextCell = currentCell + new Vector3Int(_currentDir.x, _currentDir.y, 0);

                _targetWorldPos = Walls.GetCellCenterWorld(nextCell);

                LastMoveDirection = new Vector3(_currentDir.x, _currentDir.y, 0).normalized;
            }

            /* ===== Pellets ===== */

            var pelletCell = Pellets.WorldToCell(transform.position);

            if (Pellets.HasTile(pelletCell))
            {
                ScoreManager.Instance.AddScore(10);
                ScoreManager.Instance.PelletEaten(transform.position);

                pelletsUntilGrowth--;

                if (pelletsUntilGrowth <= 0)
                {
                    pelletsUntilGrowth = 10;
                    SpawnNewSegment();
                }
            }
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
     * Snake
     * ========================= */
    private void CheckSelfCollision()
{
    Vector3Int headTile = Walls.WorldToCell(transform.position);

    foreach (GameObject segment in snakeSegments)
    {
        Vector3Int segmentTile = Walls.WorldToCell(segment.transform.position);

        if (segmentTile == headTile)
        {
            //death logic method
            Debug.LogWarning("YOU DIED");
            return;
        }
    }
}
    private void UpdateSnakeSegments()
    {
        for (int i = 0; i < snakeSegments.Count; i++)
        {
            if (i + 1 >= moveHistory.Count)
                return;

            Vector3Int tile = moveHistory[i + 1];
            Vector3 pos = Walls.GetCellCenterWorld(tile);

            snakeSegments[i].transform.position = pos;
        }
    }

    private void SpawnNewSegment()
    {
        Vector3Int spawnTile;

        if (moveHistory.Count > snakeSegments.Count + 1)
            spawnTile = moveHistory[snakeSegments.Count + 1];
        else
            spawnTile = moveHistory[moveHistory.Count - 1];

        Vector3 pos = Walls.GetCellCenterWorld(spawnTile);

        GameObject segment = Instantiate(snakeBodyPrefab, pos, Quaternion.identity);
        snakeSegments.Add(segment);
    }

    /* =========================
     * Input
     * ========================= */

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }
/*
    private void ReadInput()
    {
        int x = Mathf.RoundToInt(_moveInput.x);
        int y = Mathf.RoundToInt(_moveInput.y);

        if (x != 0) y = 0;

        if (x != 0 || y != 0)
            _desiredDir = new Vector2Int(x, y);

    }
    */
   private void ReadInput()
{
    int x = Mathf.RoundToInt(_moveInput.x);
    int y = Mathf.RoundToInt(_moveInput.y);

    if (x != 0) y = 0;

    Vector2Int newDir = new Vector2Int(x, y);

    if (newDir == Vector2Int.zero)
        return;

    if (snakeMode)
    {
        Vector2Int lastDir = new Vector2Int(
            Mathf.RoundToInt(LastMoveDirection.x),
            Mathf.RoundToInt(LastMoveDirection.y)
        );

        if (newDir == -lastDir)
            return;
    }

    _desiredDir = newDir;
}

    /* =========================
     * Warp
     * ========================= */

    private void CacheWarpCellsFromMarkers()
    {
        if (warpTileLeft == null || warpTileRight == null)
            return;

        leftWarpCell = Walls.WorldToCell(warpTileLeft.position);
        rightWarpCell = Walls.WorldToCell(warpTileRight.position);
    }

    private bool ApplyWarpIfOnWarpTile()
    {
        if (Time.time < nextAllowedWarpTime)
            return false;

        var cell = Walls.WorldToCell(transform.position);

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
     * Movement
     * ========================= */

    private bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero)
            return false;

        var currentCell = Walls.WorldToCell(transform.position);

        if (leftWarpCell.x != int.MinValue && currentCell == leftWarpCell && dir == Vector2Int.left)
            return true;

        if (rightWarpCell.x != int.MinValue && currentCell == rightWarpCell && dir == Vector2Int.right)
            return true;

        var nextCell = currentCell + new Vector3Int(dir.x, dir.y, 0);

        if ((Walls != null && Walls.HasTile(nextCell)) ||
            (Ghost_Door != null && Ghost_Door.HasTile(nextCell)) ||
            (Ghost_Room != null && Ghost_Room.HasTile(nextCell)))
            return false;

        return true;
    }

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

    public Vector2Int CurrentDir => _currentDir;
}