using UnityEngine;

public class GhostModeController : MonoBehaviour
{
    [System.Serializable]
    public struct Phase
    {
        public GhostMode mode;
        public float duration;
    }

    [SerializeField] private Phase[] phases;

    public GhostMode CurrentMode { get; private set; }

    private int phaseIndex;
    private float timer;

    private void Start()
    {
        phaseIndex = 0;
        timer = 0f;
        CurrentMode = phases[0].mode;
    }

    private void Update()
    {
        // Als we bij de laatste phase zijn (∞ Chase)
        if (phaseIndex >= phases.Length)
            return;

        timer += Time.deltaTime;

        if (timer >= phases[phaseIndex].duration)
        {
            timer = 0f;
            phaseIndex++;

            if (phaseIndex < phases.Length)
            {
                CurrentMode = phases[phaseIndex].mode;
            }
        }
    }
}
