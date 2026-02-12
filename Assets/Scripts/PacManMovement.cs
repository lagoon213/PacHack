using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PacManMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visual;
    

    public Tilemap Walls;
    public Tilemap Ghost_Room;
    public Tilemap Ghost_Door;
    public Tilemap Pellets;

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

            var cell = Pellets.WorldToCell(transform.position);
            if (Pellets.HasTile(cell)){
                Pellets.SetTile(cell, null);
                ScoreManager.Instance.AddScore(10);
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, _targetWorldPos, moveSpeed * Time.deltaTime);

        UpdateFacing();

        bool isMoving = _currentDir != Vector2Int.zero;
        animator.speed = isMoving ? 1f : 0f;
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

    private void UpdateFacing()
    {
        if (_currentDir == Vector2Int.right) visual.rotation = Quaternion.Euler(0, 0, 0);
        else if (_currentDir == Vector2Int.up) visual.rotation = Quaternion.Euler(0, 0, 90);
        else if (_currentDir == Vector2Int.left) visual.rotation = Quaternion.Euler(0, 0, 180);
        else if (_currentDir == Vector2Int.down) visual.rotation = Quaternion.Euler(0, 0, -90);
    }

    public Vector2Int CurrentDir => _currentDir;
}