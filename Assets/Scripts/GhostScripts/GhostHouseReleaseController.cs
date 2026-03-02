using UnityEngine;

public class GhostHouseReleaseController : MonoBehaviour
{
    [System.Serializable]
    public class ReleaseRule
    {
        public GhostMovement ghost;
        public int pelletsNeeded;

        [HideInInspector]
        public bool released;
    }

    [Header("Release rules (pellets eaten)")]
    [SerializeField] private ReleaseRule[] rules;

    private int pelletsEaten;

    private void OnEnable()
    {
        ScoreManager.OnPelletEaten += HandlePellet;
    }

    private void OnDisable()
    {
        ScoreManager.OnPelletEaten -= HandlePellet;
    }

    private void HandlePellet()
    {
        pelletsEaten++;

        foreach (var r in rules)
        {
            if (r == null || r.ghost == null) 
                continue;

            if (r.released) 
                continue;

            if (pelletsEaten >= r.pelletsNeeded)
            {
                r.released = true;
                r.ghost.CanExitHouse = true;
            }
        }
    }
}