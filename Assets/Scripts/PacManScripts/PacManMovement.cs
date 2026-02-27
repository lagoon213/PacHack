using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

/// <summary>
/// Pac-Man movement (tile-based).
///
/// Doet:
/// - grid movement + input (New Input System)
/// - botsing met walls/ghost house
/// - pellets opeten
/// - visual draaien + animatie pauzeren
/// - tunnel warp via 2 markers (GameObjects) op warp tile posities
///
/// Pac-Man speed is de basis voor ghost speed.
/// </summary>
public class PacManMovement : MonoBehaviour
{
    /* =========================
     * Referenties
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
     * Warp markers (GameObjects)
     * ========================= */

    [Header("Warp Markers (GameObjects on tile centers)")]
    [SerializeField] private Transform warpTileLeft;
    [SerializeField] private Transform warpTileRight;

    /* =========================
     * Movement settings
     * ========================= */

    [SerializeField] private float moveSpeed = 5f;

    /// <summary>
    /// Pac-Man snelheid (ghosts gebruiken dit als baseline).
    /// </summary>
    public float MoveSpeed => moveSpeed;

    /* =========================
     * Warp settings
     * ========================= */

    [Header("Tunnel Warp Settings")]
    [SerializeField] private float warpCooldown = 0.12f;

    // Deze cells zijn in WALLS cell-space
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
     * Unity lifecycle
     * ========================= */

    private void Start()
    {
        if (Walls == null)
        {
            Debug.LogError($"{name}: Walls tilemap ontbreekt!");
            enabled = false;
            return;
        }

        // Start in het midden van de tile
        var cell = Walls.WorldToCell(transform.position);
        _targetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = _targetWorldPos;

        CacheWarpCellsFromMarkers();
    }

    private void Update()
    {
        ReadInput();

        // Alleen wisselen als we tile-center bereikt hebben
        if (Vector3.Distance(transform.position, _targetWorldPos) < 0.001f)
        {
            transform.position = _targetWorldPos;

            // ✅ Warp toepassen op tile-center moment
            // Als we warpen: frame stoppen zodat target berekening schoon opnieuw gebeurt.
            if (ApplyWarpIfOnWarpTile())
                return;

            // Probeer gewenste richting toe te passen
            if (CanMove(_desiredDir))
                _currentDir = _desiredDir;

            // Als je niet meer vooruit kan: stop
            if (!CanMove(_currentDir))
                _currentDir = Vector2Int.zero;

            // Volgende tile target zetten
            if (_currentDir != Vector2Int.zero)
            {
                var currentCell = Walls.WorldToCell(transform.position);
                var nextCell = currentCell + new Vector3Int(_currentDir.x, _currentDir.y, 0);
                _targetWorldPos = Walls.GetCellCenterWorld(nextCell);
            }

            // Pellets opeten
             var pelletCell = Pellets.WorldToCell(transform.position);
            if (Pellets.HasTile(pelletCell))
            {
                //Pellets.SetTile(pelletCell, null);
                ScoreManager.Instance.AddScore(10); //feel like this logic should be in pellet
                ScoreManager.Instance.PelletEaten(transform.position);
            }
        }

        // Beweging uitvoeren
        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetWorldPos,
            moveSpeed * Time.deltaTime
        );

        // Visual + animatie
        UpdateFacing();

        bool isMoving = _currentDir != Vector2Int.zero;
        if (animator != null)
            animator.speed = isMoving ? 1f : 0f;
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

        // Geen diagonaal (horizontaal wint)
        if (x != 0) y = 0;

        if (x != 0 || y != 0)
            _desiredDir = new Vector2Int(x, y);
    }

    /* =========================
     * Warp helpers
     * ========================= */

    private void CacheWarpCellsFromMarkers()
    {
        if (warpTileLeft == null || warpTileRight == null)
        {
            Debug.LogWarning($"{name}: Warp markers missen. Sleep warpTileLeft en warpTileRight in de inspector.");
            return;
        }

        // We nemen de marker worldpos en zetten die om naar Walls cell-space
        leftWarpCell = Walls.WorldToCell(warpTileLeft.position);
        rightWarpCell = Walls.WorldToCell(warpTileRight.position);

        // Extra veilig: snap markers naar cell centers (optioneel)
        // warpTileLeft.position = Walls.GetCellCenterWorld(leftWarpCell);
        // warpTileRight.position = Walls.GetCellCenterWorld(rightWarpCell);

        if (leftWarpCell == rightWarpCell)
        {
            Debug.LogError($"{name}: Left/Right warp markers zitten op dezelfde cell: {leftWarpCell}. Zet ze op verschillende tiles.");
        }
        else
        {
            Debug.Log($"{name}: WarpCells (Walls) Left={leftWarpCell}, Right={rightWarpCell}");
        }
    }

    private bool ApplyWarpIfOnWarpTile()
    {
        if (Time.time < nextAllowedWarpTime) return false;
        if (leftWarpCell.x == int.MinValue || rightWarpCell.x == int.MinValue) return false;

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
     * Movement helpers
     * ========================= */

    private bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;
        if (Walls == null) return false;

        var currentCell = Walls.WorldToCell(transform.position);

        // ✅ Warp-exit toestaan (classic)
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
        if (visual == null) return;

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