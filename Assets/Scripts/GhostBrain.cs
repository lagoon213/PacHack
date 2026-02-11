using UnityEngine;

public abstract class GhostBrain : MonoBehaviour
{
    public abstract Vector2Int GetDesiredDir(GhostMovement motor);
}