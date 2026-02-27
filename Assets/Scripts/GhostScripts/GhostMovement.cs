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
/// - tunnel warp via 2 markers (GameObjects) op warp tile posities
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
    [SerializeField] private Tilemap Tunnel; // voor speed multiplier (mag hele tunnelstrook zijn)

    /* =========================
     * Warp markers (GameObjects)
     * ========================= */

    [Header("Warp Markers (GameObjects on tile centers)")]
    [SerializeField] private Transform warpTileLeft;
    [SerializeField] private Transform warpTileRight;

    /* =========================
     * Tunnel Warp
     * ========================= */

    [Header("Tunnel Warp Settings")]
    [SerializeField] private float warpCooldown = 0.12f;

    private Vector3Int leftWarpCell = new Vector3Int(int.MinValue, int.MinValue, 0);
    private Vector3Int rightWarpCell = new Vector3Int(int.MinValue, int.MinValue, 0);
    private float nextAllowedWarpTime;

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

        // Init house state (per ghost via Inspector)
        InHouse = startInHouse;
        CanExitHouse = startCanExitHouse;

        // Start netjes in het midden van de tile
        var cell = Walls.WorldToCell(transform.position);
        TargetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = TargetWorldPos;

        CacheWarpCellsFromMarkers();
    }

    private void Update()
    {
        /* =========================
         * Tile-logica (richting kiezen)
         * ========================= */

        if (Vector3.Distance(transform.position, TargetWorldPos) < 0.001f)
        {
            transform.position = TargetWorldPos;

            // ✅ Warp toepassen op tile-center moment
            // Als we warpen: frame stoppen zodat target berekening schoon opnieuw gebeurt.
            if (ApplyWarpIfOnWarpTile())
                return;

            // Check of ghost de ghost house heeft verlaten
            var cell = Walls.WorldToCell(transform.position);
            if (InHouse && Ghost_Room != null && !Ghost_Room.HasTile(cell))
            {
                InHouse = false;
                CanExitHouse = false; // Buiten: nooit terug door de deur
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

    private void HandleModeChanged(GhostMode oldMode, GhostMode newMode)
    {
        bool scatterChaseSwitch =
            (oldMode == GhostMode.Scatter && newMode == GhostMode.Chase) ||
            (oldMode == GhostMode.Chase && newMode == GhostMode.Scatter);

        if (!scatterChaseSwitch) return;
        if (CurrentDir == Vector2Int.zero) return;

        SetDir(-CurrentDir);

        var currentCell = Walls.WorldToCell(TargetWorldPos);
        var nextCell = currentCell + new Vector3Int(CurrentDir.x, CurrentDir.y, 0);
        TargetWorldPos = Walls.GetCellCenterWorld(nextCell);
    }

    /* =========================
     * Helpers
     * ========================= */

    private void SetDir(Vector2Int dir) => CurrentDir = dir;

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

        leftWarpCell = Walls.WorldToCell(warpTileLeft.position);
        rightWarpCell = Walls.WorldToCell(warpTileRight.position);

        if (leftWarpCell == rightWarpCell)
        {
            Debug.LogError($"{name}: Left/Right warp markers zitten op dezelfde cell: {leftWarpCell}. Zet ze op verschillende tiles.");
        }
    }

    /// <summary>
    /// Als de ghost op een warp tile staat (tile-center moment), teleport naar de andere warp tile.
    /// </summary>
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

        TargetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = TargetWorldPos;

        return true;
    }

    /// <summary>
    /// Check of de ghost die kant op kan (walls/door/room regels + warp exit).
    /// </summary>
    public bool CanMove(Vector2Int dir)
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

        // Walls blokkeren altijd
        if (Walls.HasTile(nextCell)) return false;

        // Ghost room: alleen blokkeren als je buiten bent
        if (!InHouse && Ghost_Room != null && Ghost_Room.HasTile(nextCell))
            return false;

        // Ghost door:
        if (Ghost_Door != null && Ghost_Door.HasTile(nextCell))
        {
            if (InHouse && !CanExitHouse) return false; // In house: alleen door als release actief is
            if (!InHouse) return false;                 // Buiten: nooit terug door de deur
        }

        return true;
    }

    /// <summary>
    /// Bepaalt multiplier op basis van tile + mode.
    /// </summary>
    private float GetSpeedMultiplier()
    {
        if (Walls == null) return normalMultiplier;

        Vector3Int cell = Walls.WorldToCell(transform.position);

        if (Ghost_Room != null && Ghost_Room.HasTile(cell))
            return houseMultiplier;

        if (Tunnel != null && Tunnel.HasTile(cell))
            return tunnelMultiplier;

        if (modeController != null && modeController.CurrentMode == GhostMode.Frightened)
            return frightenedMultiplier;

        return normalMultiplier;
    }
}