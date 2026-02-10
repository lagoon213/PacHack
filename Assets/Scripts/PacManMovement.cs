using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PacManMovement : MonoBehaviour
{
    public Tilemap Walls;
    public Tilemap Ghost_Room;
    public Tilemap Ghost_Door;

    public float moveSpeed = 5f; //Later this will be a "hackable" speed 

    private Vector2Int _currentDir = Vector2Int.right;
    private Vector2Int _desiredDir = Vector2Int.right;

    private Vector2 _moveInput;

    public Vector3 _targetWorldPos;

    void Start()
    {
        var cell = Walls.WorldToCell(transform.position);
        _targetWorldPos = Walls.GetCellCenterWorld(cell);
        transform.position = _targetWorldPos;
    }

    void Update()
    {
        ReadInput();

        if (Vector3.Distance(transform.position, _targetWorldPos) < 0.001f){
            transform.position = _targetWorldPos;

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
    }

    public void OnMove(InputValue value){
        _moveInput = value.Get<Vector2>();
    }

    private void ReadInput(){
        int x = Mathf.RoundToInt(_moveInput.x);
        int y = Mathf.RoundToInt(_moveInput.y);

        if (x != 0) y=0;

        if (x != 0 || y != 0){
            _desiredDir = new Vector2Int(x,y);
        }
    }

    private bool CanMove(Vector2Int dir){
        if (dir == Vector2Int.zero) return false;

        var currentCell = Walls.WorldToCell(transform.position);
        var nextCell = currentCell + new Vector3Int(dir.x,dir.y,0);

        if (Walls.HasTile(nextCell) || Ghost_Door.HasTile(nextCell) || Ghost_Room.HasTile(nextCell)) return false;

        return true;
    }
}