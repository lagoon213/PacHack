using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class GhostMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visual;
    [SerializeField] private PacManMovement pacman;
    

    public Tilemap Walls;
    public Tilemap Ghost_Room;
    public Tilemap Ghost_Door;

    public float moveSpeed = 5f; //Later this will be a "hackable" speed 

    private Vector2Int _currentDir = Vector2Int.right;
    private Vector2Int _desiredDir = Vector2Int.right;

    private Vector2 _moveInput;

    public Vector3 _targetWorldPos;

    private static readonly Vector2Int[] PriorityDirs = new Vector2Int[]
    {
        Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right
    };

    void Start()
    {
        var cell = Walls.WorldToCell(transform.position);
        _targetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = _targetWorldPos;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, _targetWorldPos) < 0.001f){
            transform.position = _targetWorldPos;

            Vector2Int targetTile = GetPinkyChaseTargetTile();
            _desiredDir = ChooseDirTowardTarget(targetTile);

            if (CanMove(_desiredDir)){
                _currentDir = _desiredDir;
            }

            if (!CanMove(_currentDir)){
                _currentDir = Vector2Int.zero;
            }

            if (_currentDir != Vector2Int.zero){
                var currentCell = Walls.WorldToCell(transform.position);
                var nextCell = currentCell + new Vector3Int(_currentDir.x, _currentDir.y, 0);
                _targetWorldPos = Walls.GetCellCenterWorld(nextCell);
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, _targetWorldPos, moveSpeed * Time.deltaTime);

        animator.SetFloat("MoveX", _currentDir.x);
        animator.SetFloat("MoveY", _currentDir.y);
    }

        private Vector2Int GetPinkyChaseTargetTile()
    {
        // Pac-Man tile
        Vector3Int pacCell3 = Walls.WorldToCell(pacman.transform.position);
        Vector2Int pacCell = new Vector2Int(pacCell3.x, pacCell3.y);

        // Pac-Man direction
        Vector2Int pacDir = pacman.CurrentDir;
        if (pacDir == Vector2Int.zero) pacDir = Vector2Int.right; // fallback

        // Target 4 tiles ahead
        Vector2Int target = pacCell + pacDir * 4;

        // (OPTIONEEL) Classic Pinky bug: wanneer Pac-Man UP kijkt, target verschuift ook 4 naar links.
        // Uncomment voor “echte” arcade feel:
        // if (pacDir == Vector2Int.up) target += Vector2Int.left * 4;

        return target;
    }

    private Vector2Int ChooseDirTowardTarget(Vector2Int targetTile)
    {
        Vector3Int myCell3 = Walls.WorldToCell(transform.position);
        Vector2Int myCell = new Vector2Int(myCell3.x, myCell3.y);

        Vector2Int bestDir = Vector2Int.zero;
        int bestDist = int.MaxValue;

        foreach (var dir in PriorityDirs)
        {
            // niet omdraaien tenzij je echt moet
            if (_currentDir != Vector2Int.zero && dir == -_currentDir)
                continue;

            if (!CanMove(dir))
                continue;

            Vector2Int next = myCell + dir;

            int dx = next.x - targetTile.x;
            int dy = next.y - targetTile.y;
            int dist = dx * dx + dy * dy; // squared distance

            if (dist < bestDist)
            {
                bestDist = dist;
                bestDir = dir;
            }
        }

        // Als alles geblokkeerd is behalve reverse, pak reverse
        if (bestDir == Vector2Int.zero && _currentDir != Vector2Int.zero && CanMove(-_currentDir))
            return -_currentDir;

        return bestDir;
    }

    private bool CanMove(Vector2Int dir){
        if (dir == Vector2Int.zero) return false;

        var currentCell = Walls.WorldToCell(transform.position);
        var nextCell = currentCell + new Vector3Int(dir.x,dir.y,0);

        if (Walls.HasTile(nextCell) || Ghost_Door.HasTile(nextCell) || Ghost_Room.HasTile(nextCell)) return false;

        return true;
    }
}
