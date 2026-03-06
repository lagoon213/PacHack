using System.Collections.Generic;
using UnityEngine;

public class PacSnakeBody : MonoBehaviour
{
    [Header("Segment Settings")]
    [SerializeField] private Transform segmentPrefab;
    [SerializeField] private PacManMovement pacMan; // Should have LastMoveDirection

    [Header("Movement Settings")]
    [SerializeField] private float segmentSpacing = 0.5f; // distance between segments

    private List<Transform> segments = new List<Transform>();

    private void OnEnable()
    {
        ScoreManager.OnSnakeGrow += Grow;
    }

    private void OnDisable()
    {
        ScoreManager.OnSnakeGrow -= Grow;
    }

    /// <summary>
    /// Call this every time Pac-Man moves to a new tile
    /// </summary>
    public void MoveBody()
    {
        Vector3 previousPosition = pacMan.transform.position;

        // Move each segment to follow the one in front
        foreach (Transform segment in segments)
        {
            Vector3 targetPosition = segment.position;
            float distance = Vector3.Distance(previousPosition, segment.position);

            if (distance > segmentSpacing)
            {
                // Move segment towards the previous position
                segment.position = previousPosition;
            }

            previousPosition = targetPosition; // update previousPosition for next segment
        }
    }

    /// <summary>
    /// Adds a new segment immediately behind Pac-Man or last segment
    /// </summary>
    public void Grow()
    {
        Vector3 spawnPos;

        if (segments.Count == 0)
        {
            // First segment spawns behind Pac-Man
            spawnPos = pacMan.transform.position - pacMan.LastMoveDirection.normalized * segmentSpacing;
        }
        else
        {
            // Spawn behind last segment, opposite of direction from previous segment
            Transform lastSeg = segments[segments.Count - 1];
            Vector3 dir = segments.Count > 1 ? 
                (lastSeg.position - segments[segments.Count - 2].position).normalized : 
                pacMan.LastMoveDirection.normalized;

            spawnPos = lastSeg.position - dir * segmentSpacing;
        }

        spawnPos.z = -0.1f; // render above tilemap

        Transform newSeg = Instantiate(segmentPrefab, spawnPos, Quaternion.identity);
        newSeg.parent = transform;

        // Optional: sorting order for visuals
        var sr = newSeg.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 10 - segments.Count;

        segments.Add(newSeg);

        Debug.Log($"Snake segment spawned at {spawnPos}");
    }
}