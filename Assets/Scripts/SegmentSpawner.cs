using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class SegmentSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("References")]
    [SerializeField] private Tilemap walls;
    [SerializeField] private GameObject segmentPrefab;

    private List<GameObject> segments = new();
    private List<Vector3Int> moveHistory = new();

    private Vector3Int previousTile;

    public int SegmentCount => segments.Count;

    private void Start()
    {
        previousTile = walls.WorldToCell(transform.position);
        moveHistory.Add(previousTile);
    }

    public void RecordMove(Vector3Int currentTile)
    {
        if (currentTile == previousTile)
            return;

        moveHistory.Insert(0, currentTile);
        previousTile = currentTile;

        UpdateSegments();

        int maxHistory = segments.Count + 5;
        if (moveHistory.Count > maxHistory)
            moveHistory.RemoveAt(moveHistory.Count - 1);
    }

    public void Grow()
    {
        SpawnSegment();
    }

    private void SpawnSegment()
    {
        Vector3Int spawnTile;

        if (moveHistory.Count > segments.Count + 1)
            spawnTile = moveHistory[segments.Count + 1];
        else
            spawnTile = moveHistory[moveHistory.Count - 1];

        Vector3 pos = walls.GetCellCenterWorld(spawnTile);

        GameObject segment = Instantiate(segmentPrefab, pos, Quaternion.identity, transform);

        segments.Add(segment);
    }

    private void UpdateSegments()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (i + 1 >= moveHistory.Count)
                return;

            Vector3Int tile = moveHistory[i + 1];
            segments[i].transform.position = walls.GetCellCenterWorld(tile);
        }
    }

    public bool CheckSelfCollision()
    {
        Vector3Int headTile = walls.WorldToCell(transform.position);

        foreach (GameObject segment in segments)
        {
            Vector3Int segmentTile = walls.WorldToCell(segment.transform.position);

            if (segmentTile == headTile)
                return true;
        }

        return false;
    }
}
