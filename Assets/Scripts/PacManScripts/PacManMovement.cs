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
     * Movement settings
     * ========================= */

    [SerializeField] private float moveSpeed = 5f;

    /// <summary>
    /// Pac-Man snelheid (ghosts gebruiken dit als baseline).
    /// </summary>
    public float MoveSpeed => moveSpeed;

    /* =========================
     * Movement state
     * ========================= */

    /// <summary>
    /// Huidige richting (tile coords).
    /// </summary>
    private Vector2Int _currentDir = Vector2Int.right;

    /// <summary>
    /// Richting die de speler probeert te pakken.
    /// </summary>
    private Vector2Int _desiredDir = Vector2Int.right;

    /// <summary>
    /// Raw input vector (Input System).
    /// </summary>
    private Vector2 _moveInput;

    /// <summary>
    /// Volgende world target (center van volgende tile).
    /// </summary>
    private Vector3 _targetWorldPos;

    /* =========================
     * Unity lifecycle
     * ========================= */

    private void Start()
    {
        // Start in het midden van de tile
        var cell = Walls.WorldToCell(transform.position);
        _targetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = _targetWorldPos;
    }

    private void Update()
    {
        ReadInput();

        /* =========================
         * Tile-logica
         * ========================= */

        // Alleen wisselen als we tile-center bereikt hebben
        if (Vector3.Distance(transform.position, _targetWorldPos) < 0.001f)
        {
            transform.position = _targetWorldPos;

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

            /* =========================
             * Pellets opeten
             * ========================= */

            var pelletCell = Pellets.WorldToCell(transform.position);
            if (Pellets.HasTile(pelletCell))
            {
                //Pellets.SetTile(pelletCell, null);
                ScoreManager.Instance.AddScore(10); //feel like this logic should be in pellet
                ScoreManager.Instance.PelletEaten(transform.position);
            }
        }

        /* =========================
         * Beweging uitvoeren
         * ========================= */

        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetWorldPos,
            moveSpeed * Time.deltaTime
        );

        /* =========================
         * Visual + animatie
         * ========================= */

        UpdateFacing();

        // Animatie pauze als je stilstaat
        bool isMoving = _currentDir != Vector2Int.zero;
        animator.speed = isMoving ? 1f : 0f;
    }

    /* =========================
     * Input
     * ========================= */

    /// <summary>
    /// Input System callback.
    /// </summary>
    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    /// <summary>
    /// Input → grid richting (geen diagonalen).
    /// </summary>
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
     * Helpers
     * ========================= */

    /// <summary>
    /// Check of Pac-Man die kant op mag (walls/door/room blokkeren).
    /// </summary>
    private bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;

        var currentCell = Walls.WorldToCell(transform.position);
        var nextCell = currentCell + new Vector3Int(dir.x, dir.y, 0);

        if (Walls.HasTile(nextCell) ||
            Ghost_Door.HasTile(nextCell) ||
            Ghost_Room.HasTile(nextCell))
            return false;

        return true;
    }

    /// <summary>
    /// Draai de visual naar de huidige richting.
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
    /// Huidige richting (voor ghost AI zoals Pinky).
    /// </summary>
    public Vector2Int CurrentDir => _currentDir;
}