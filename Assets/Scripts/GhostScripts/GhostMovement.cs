using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PacManMovement pacman;
    [SerializeField] private GhostBrain brain;
    [SerializeField] private GhostModeController modeController;
    [SerializeField] private GhostEyes eyes;

    [Header("Ghost House State")]
    [SerializeField] private bool startInHouse = true;
    [SerializeField] private bool startCanExitHouse = false;

    public bool InHouse { get; private set; }
    public bool CanExitHouse { get; set; }

    [Header("Collision Tilemaps")]
    public Tilemap Walls;
    public Tilemap Ghost_Room;
    public Tilemap Ghost_Door;

    [Header("Special Speed Tilemaps")]
    [SerializeField] private Tilemap Tunnel;

    [Header("Warp Markers (GameObjects on tile centers)")]
    [SerializeField] private Transform warpTileLeft;
    [SerializeField] private Transform warpTileRight;

    [Header("Tunnel Warp Settings")]
    [SerializeField] private float warpCooldown = 0.12f;

    private Vector3Int leftWarpCell = new Vector3Int(int.MinValue, int.MinValue, 0);
    private Vector3Int rightWarpCell = new Vector3Int(int.MinValue, int.MinValue, 0);
    private float nextAllowedWarpTime;

    [Header("Speed Multipliers")]
    [SerializeField] private float normalMultiplier = 0.75f;
    [SerializeField] private float frightenedMultiplier = 0.5f;
    [SerializeField] private float houseMultiplier = 0.45f;
    [SerializeField] private float tunnelMultiplier = 0.4f;

    [Header("Eyes Mode")]
    [SerializeField] private float eyesMultiplier = 1.8f;

    [Tooltip("Tile-center net buiten de ghost door (buiten de box).")]
    [SerializeField] private Transform eyesDoorTarget;

    [Tooltip("Tile-center binnen in de ghost house (home).")]
    [SerializeField] private Transform eyesHomeTarget;

    [Tooltip("Hoelang in house blijven na terugkomen.")]
    [SerializeField] private float respawnHoldInHouseSeconds = 1.0f;

    public bool IsEyes { get; private set; }
    private bool holdInHouseActive;

    public Vector2Int CurrentDir { get; private set; } = Vector2Int.right;
    public Vector3 TargetWorldPos { get; private set; }

    // ✅ onthoud waar hij dood ging (tile cell)
    private Vector3Int deathCell = new Vector3Int(int.MinValue, int.MinValue, 0);

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

        if (eyes == null)
            eyes = GetComponent<GhostEyes>();

        InHouse = startInHouse;
        CanExitHouse = startCanExitHouse;

        SnapToCellCenter();

        CacheWarpCellsFromMarkers();

        if (eyes != null)
            eyes.ShowBody();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, TargetWorldPos) < 0.001f)
        {
            transform.position = TargetWorldPos;

            // ✅ HARD: eyes warpen NOOIT
            if (!IsEyes)
            {
                if (ApplyWarpIfOnWarpTile())
                    return;
            }

            var cell = Walls.WorldToCell(transform.position);

            // ✅ Eyes terug? check home cell
            if (IsEyes && eyesHomeTarget != null)
            {
                var homeCell = Walls.WorldToCell(eyesHomeTarget.position);
                if (cell == homeCell)
                    ExitEyesModeInHouse();
            }

            // house verlaten (alleen als geen eyes)
            if (!IsEyes && InHouse && Ghost_Room != null && !Ghost_Room.HasTile(cell))
            {
                InHouse = false;
                CanExitHouse = false;
            }

            Vector2Int desiredDir;

            if (IsEyes)
                desiredDir = GetEyesDirToTargets_NoUTurn();
            else
                desiredDir = brain != null ? brain.GetDesiredDir(this) : Vector2Int.zero;

            if (CanMove(desiredDir))
                CurrentDir = desiredDir;

            if (!CanMove(CurrentDir))
                CurrentDir = Vector2Int.zero;

            if (CurrentDir != Vector2Int.zero)
            {
                var currentCell = Walls.WorldToCell(transform.position);
                var nextCell = currentCell + new Vector3Int(CurrentDir.x, CurrentDir.y, 0);
                TargetWorldPos = Walls.GetCellCenterWorld(nextCell);
            }
        }

        if (pacman != null)
        {
            float speed = pacman.MoveSpeed * GetSpeedMultiplier();
            transform.position = Vector3.MoveTowards(transform.position, TargetWorldPos, speed * Time.deltaTime);
        }

        if (animator != null)
        {
            animator.SetFloat("MoveX", CurrentDir.x);
            animator.SetFloat("MoveY", CurrentDir.y);

            animator.SetBool(
                "IsFrightened",
                modeController != null &&
                modeController.CurrentMode == GhostMode.Frightened &&
                !IsEyes
            );
        }
    }

    /// <summary>
    /// Wordt aangeroepen als Pac-Man deze ghost opeet.
    /// We geven de deathWorldPos mee zodat eyes EXACT op die tile starten.
    /// </summary>
    public void EnterEyesMode(Vector3 deathWorldPos)
    {
        IsEyes = true;

        InHouse = false;
        CanExitHouse = true;
        holdInHouseActive = false;

        // ✅ onthoud death cell en snap eyes EXACT op die cell
        deathCell = Walls.WorldToCell(deathWorldPos);
        Vector3 start = Walls.GetCellCenterWorld(deathCell);
        transform.position = start;
        TargetWorldPos = start;

        // start dir
        if (CurrentDir == Vector2Int.zero)
            CurrentDir = Vector2Int.up;

        // warp cooldown (maar eyes warpen toch niet)
        nextAllowedWarpTime = Time.time + 0.05f;

        if (eyes != null)
            eyes.ShowEyes();
    }

    private void ExitEyesModeInHouse()
    {
        IsEyes = false;

        InHouse = true;

        // even vasthouden
        CanExitHouse = false;
        holdInHouseActive = true;
        StopAllCoroutines();
        StartCoroutine(HoldInHouseRoutine(respawnHoldInHouseSeconds));

        SnapToCellCenter();
        CurrentDir = Vector2Int.zero;

        if (eyes != null)
            eyes.ShowBody();
    }

    private IEnumerator HoldInHouseRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        holdInHouseActive = false;
        CanExitHouse = true;
    }

    private void HandleModeChanged(GhostMode oldMode, GhostMode newMode)
    {
        if (IsEyes) return;

        bool scatterChaseSwitch =
            (oldMode == GhostMode.Scatter && newMode == GhostMode.Chase) ||
            (oldMode == GhostMode.Chase && newMode == GhostMode.Scatter);

        if (!scatterChaseSwitch) return;
        if (CurrentDir == Vector2Int.zero) return;

        CurrentDir = -CurrentDir;

        var currentCell = Walls.WorldToCell(TargetWorldPos);
        var nextCell = currentCell + new Vector3Int(CurrentDir.x, CurrentDir.y, 0);
        TargetWorldPos = Walls.GetCellCenterWorld(nextCell);
    }

    private void SnapToCellCenter()
    {
        var cell = Walls.WorldToCell(transform.position);
        TargetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = TargetWorldPos;
    }

    private Vector2Int GetEyesDirToTargets_NoUTurn()
    {
        if (Walls == null || eyesHomeTarget == null)
            return Vector2Int.up;

        var hereCell = Walls.WorldToCell(transform.position);
        var homeCell = Walls.WorldToCell(eyesHomeTarget.position);

        // Als we geen doorTarget hebben: direct naar home
        bool useDoor = (eyesDoorTarget != null);
        Vector3Int doorCell = useDoor ? Walls.WorldToCell(eyesDoorTarget.position) : new Vector3Int(int.MinValue, int.MinValue, 0);

        // ✅ Belangrijk:
        // - Zolang we NIET op de doorCell zijn: ga naar door
        // - Zodra we OP de doorCell zijn: ga direct naar home
        Vector3 goalPos;
        if (useDoor && hereCell != doorCell)
            goalPos = eyesDoorTarget.position;
        else
            goalPos = eyesHomeTarget.position;

        Vector2Int[] dirs = new[]
        {
        Vector2Int.up,
        Vector2Int.left,
        Vector2Int.down,
        Vector2Int.right
    };

        // reverse blokkeren voorkomt flippen
        Vector2Int reverse = -CurrentDir;

        float best = float.MaxValue;
        Vector2Int bestDir = Vector2Int.zero;

        // 1) zonder U-turn
        foreach (var d in dirs)
        {
            if (d == reverse) continue;
            if (!CanMove(d)) continue;

            var nextCell = hereCell + new Vector3Int(d.x, d.y, 0);
            Vector3 nextWorld = Walls.GetCellCenterWorld(nextCell);

            float dist = (nextWorld - goalPos).sqrMagnitude;
            if (dist < best)
            {
                best = dist;
                bestDir = d;
            }
        }

        // 2) als niets: U-turn toestaan
        if (bestDir == Vector2Int.zero)
        {
            foreach (var d in dirs)
            {
                if (!CanMove(d)) continue;

                var nextCell = hereCell + new Vector3Int(d.x, d.y, 0);
                Vector3 nextWorld = Walls.GetCellCenterWorld(nextCell);

                float dist = (nextWorld - goalPos).sqrMagnitude;
                if (dist < best)
                {
                    best = dist;
                    bestDir = d;
                }
            }
        }

        // ✅ Anti-stuck:
        // Als we op de doorCell zijn en de beste richting brengt ons niet dichter bij home,
        // forceer dan alsnog een stap richting home (als mogelijk).
        if (useDoor && hereCell == doorCell)
        {
            // probeer een richting die afstand naar home verlaagt
            float bestHome = float.MaxValue;
            Vector2Int bestHomeDir = Vector2Int.zero;

            foreach (var d in dirs)
            {
                if (!CanMove(d)) continue;
                var nextCell = hereCell + new Vector3Int(d.x, d.y, 0);
                float distHome = (Walls.GetCellCenterWorld(nextCell) - eyesHomeTarget.position).sqrMagnitude;

                if (distHome < bestHome)
                {
                    bestHome = distHome;
                    bestHomeDir = d;
                }
            }

            if (bestHomeDir != Vector2Int.zero)
                return bestHomeDir;
        }

        return bestDir;
    }

    private void CacheWarpCellsFromMarkers()
    {
        if (warpTileLeft == null || warpTileRight == null)
        {
            Debug.LogWarning($"{name}: Warp markers missen. Sleep warpTileLeft en warpTileRight in de inspector.");
            return;
        }

        leftWarpCell = Walls.WorldToCell(warpTileLeft.position);
        rightWarpCell = Walls.WorldToCell(warpTileRight.position);
    }

    private bool ApplyWarpIfOnWarpTile()
    {
        // ✅ extra safety
        if (IsEyes) return false;

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

    public bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;
        if (Walls == null) return false;

        var currentCell = Walls.WorldToCell(transform.position);
        var nextCell = currentCell + new Vector3Int(dir.x, dir.y, 0);

        // ✅ Walls blokkeren altijd (ook eyes!)
        if (Walls.HasTile(nextCell)) return false;

        // ✅ Eyes negeert alleen room/door regels, maar NIET walls
        if (IsEyes) return true;

        if (!InHouse && Ghost_Room != null && Ghost_Room.HasTile(nextCell))
            return false;

        if (Ghost_Door != null && Ghost_Door.HasTile(nextCell))
        {
            if (InHouse && (holdInHouseActive || !CanExitHouse)) return false;
            if (!InHouse) return false;
        }

        return true;
    }

    private float GetSpeedMultiplier()
    {
        if (IsEyes) return eyesMultiplier;

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