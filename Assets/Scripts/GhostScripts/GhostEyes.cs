using UnityEngine;

public class GhostEyes : MonoBehaviour
{
    [Header("Body (Visual SpriteRenderer)")]
    [SerializeField] private SpriteRenderer body;

    [Header("Eyes Sprites")]
    [SerializeField] private SpriteRenderer eyesUp;
    [SerializeField] private SpriteRenderer eyesDown;
    [SerializeField] private SpriteRenderer eyesLeft;
    [SerializeField] private SpriteRenderer eyesRight;

    private GhostMovement movement;
    private bool eyesActive;

    private void Awake()
    {
        movement = GetComponent<GhostMovement>();
        ShowBody();
    }

    private void LateUpdate()
    {
        if (!eyesActive || movement == null) return;

        Vector2Int dir = movement.CurrentDir;

        HideAllEyes();

        if (dir == Vector2Int.up) eyesUp.enabled = true;
        else if (dir == Vector2Int.down) eyesDown.enabled = true;
        else if (dir == Vector2Int.left) eyesLeft.enabled = true;
        else if (dir == Vector2Int.right) eyesRight.enabled = true;
        else eyesUp.enabled = true;
    }

    public void ShowEyes()
    {
        eyesActive = true;

        if (body != null)
            body.enabled = false;

        HideAllEyes();
        if (eyesUp != null) eyesUp.enabled = true;
    }

    public void ShowBody()
    {
        eyesActive = false;

        if (body != null)
            body.enabled = true;

        HideAllEyes();
    }

    private void HideAllEyes()
    {
        if (eyesUp != null) eyesUp.enabled = false;
        if (eyesDown != null) eyesDown.enabled = false;
        if (eyesLeft != null) eyesLeft.enabled = false;
        if (eyesRight != null) eyesRight.enabled = false;
    }
}