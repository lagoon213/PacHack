using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public Tilemap Walls;
    public Tilemap Ghost_Room;
    public Tilemap Ghost_Door;

    [SerializeField] private GhostBrain brain;
    [SerializeField] private GhostModeController modeController;

    public float moveSpeed = 5f;

    public Vector2Int CurrentDir { get; private set; } = Vector2Int.right;
    public Vector3 TargetWorldPos { get; private set; }

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
        var cell = Walls.WorldToCell(transform.position);
        TargetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = TargetWorldPos;
    }

    private void Update()
    {
        // Alleen beslissen op tile-centers
        if (Vector3.Distance(transform.position, TargetWorldPos) < 0.001f)
        {
            transform.position = TargetWorldPos;

            // Vraag aan de brain welke richting we willen
            Vector2Int desiredDir = brain.GetDesiredDir(this);

            if (CanMove(desiredDir))
                SetDir(desiredDir);

            if (!CanMove(CurrentDir))
                SetDir(Vector2Int.zero);

            if (CurrentDir != Vector2Int.zero)
            {
                var currentCell = Walls.WorldToCell(transform.position);
                var nextCell = currentCell + new Vector3Int(CurrentDir.x, CurrentDir.y, 0);
                TargetWorldPos = Walls.GetCellCenterWorld(nextCell);
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, TargetWorldPos, moveSpeed * Time.deltaTime);

        // Anim params
        animator.SetFloat("MoveX", CurrentDir.x);
        animator.SetFloat("MoveY", CurrentDir.y);

        animator.SetBool(
         "IsFrightened",
        modeController != null &&
        modeController.CurrentMode == GhostMode.Frightened
        );
    }

    private void HandleModeChanged(GhostMode oldMode, GhostMode newMode)
    {
        // Alleen omdraaien bij Scatter <-> Chase (classic Pac-Man)
        bool scatterChaseSwitch =
            (oldMode == GhostMode.Scatter && newMode == GhostMode.Chase) ||
            (oldMode == GhostMode.Chase && newMode == GhostMode.Scatter);

        if (!scatterChaseSwitch) return;

        if (CurrentDir == Vector2Int.zero) return;

        Vector2Int reversed = -CurrentDir;

        // Alleen draaien als die richting ook mogelijk is (voorkomt rare situaties)
        if (CanMove(reversed))
        {
            SetDir(reversed);

            // Forceer direct nieuwe target tile zodat hij meteen “snapt”
            var currentCell = Walls.WorldToCell(transform.position);
            var nextCell = currentCell + new Vector3Int(CurrentDir.x, CurrentDir.y, 0);
            TargetWorldPos = Walls.GetCellCenterWorld(nextCell);
        }
    }

    private void SetDir(Vector2Int dir)
    {
        CurrentDir = dir;
    }

    public bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;

        var currentCell = Walls.WorldToCell(transform.position);
        var nextCell = currentCell + new Vector3Int(dir.x, dir.y, 0);

        if (Walls.HasTile(nextCell) ||
            (Ghost_Door != null && Ghost_Door.HasTile(nextCell)) ||
            (Ghost_Room != null && Ghost_Room.HasTile(nextCell)))
            return false;

        return true;
    }
}
