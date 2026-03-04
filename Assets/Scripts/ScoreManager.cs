using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public static event System.Action OnPelletEaten;

    public int score;
    public int highScore;

    [Header("Pellets")]
    public Tilemap pelletTilemap;
    public int pelletsRemaining;

    [Header("WIN FLOW (Sleep in Inspector)")]
    [SerializeField] private WallsFlasher wallsFlasher; // sleep Walls (met WallsFlasher component) hierheen
    [SerializeField] private GameObject winUI;          // sleep WinUI Panel hierheen
    [SerializeField] private float freezeDelayBeforeFlash = 0.05f;

    private bool winTriggered;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        RefreshPelletTilemapAndCount();
        RebindSceneReferences(); // ✅ belangrijk
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        winTriggered = false;

        RefreshPelletTilemapAndCount();
        RebindSceneReferences(); // ✅ pak nieuwe scene refs
    }

    /// <summary>
    /// Rebind WinUI + WallsFlasher bij elke scene load
    /// zodat DontDestroy ScoreManager nooit "Missing" refs houdt.
    /// </summary>
    private void RebindSceneReferences()
    {
        // --- WinUI ---
        // Als je WinUI in de scene zit: sleep hem 1x, maar bij reload wordt dat Missing.
        // Daarom: als winUI missing/null => zoek een object met component WinUI tag of naam? (zonder naam: via Canvas child)
        if (winUI == null)
        {
            // Zoek in alle canvassen een child die "WinUI" heet (fallback)
            // (werkt ook als je meerdere canvassen hebt)
            var canvases = FindObjectsOfType<Canvas>(true);
            foreach (var c in canvases)
            {
                var t = c.transform.Find("WinUI");
                if (t != null)
                {
                    winUI = t.gameObject;
                    break;
                }
            }
        }

        if (winUI != null)
        {
            winUI.SetActive(false);
            Debug.Log("ScoreManager: WinUI gekoppeld ✅");
        }
        else
        {
            Debug.LogWarning("ScoreManager: WinUI is NIET gekoppeld. Sleep je WinUI panel in ScoreManager óf zorg dat hij onder Canvas/WinUI heet.");
        }

        // --- WallsFlasher ---
        if (wallsFlasher == null)
        {
            wallsFlasher = FindObjectOfType<WallsFlasher>(true);
        }

        if (wallsFlasher != null)
        {
            Debug.Log("ScoreManager: WallsFlasher gekoppeld ✅");
        }
        else
        {
            Debug.LogWarning("ScoreManager: WallsFlasher niet gevonden. Zet WallsFlasher op je Walls Tilemap en sleep hem in ScoreManager.");
        }
    }

    private void RefreshPelletTilemapAndCount()
    {
        if (pelletTilemap == null)
        {
            var go = GameObject.Find("Pellets");
            if (go != null)
                pelletTilemap = go.GetComponent<Tilemap>();
        }

        if (pelletTilemap != null)
            CountPellets();
        else
            Debug.LogWarning("ScoreManager: pelletTilemap niet gevonden. Check GameObject naam ('Pellets').");
    }

    public void AddScore(int amount)
    {
        score += amount;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }

    private void CountPellets()
    {
        pelletsRemaining = 0;

        BoundsInt bounds = pelletTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (pelletTilemap.HasTile(pos))
                pelletsRemaining++;
        }

        Debug.Log("Pellets found: " + pelletsRemaining);
    }

    // SINGLE SOURCE OF TRUTH
    public void PelletEaten(Vector3 worldPosition)
    {
        if (winTriggered) return;

        Vector3Int cellPos = pelletTilemap.WorldToCell(worldPosition);

        if (!pelletTilemap.HasTile(cellPos))
            return;

        pelletTilemap.SetTile(cellPos, null);
        pelletsRemaining--;

        AddScore(10);

        OnPelletEaten?.Invoke();

        if (pelletsRemaining <= 0)
            WinLevel();
    }

    private void WinLevel()
    {
        if (winTriggered) return;
        winTriggered = true;

        Debug.Log("YOU WIN -> Start WinSequence");
        StartCoroutine(WinSequence());
    }

    private IEnumerator WinSequence()
    {
        FreezeGameplay(true);

        yield return new WaitForSecondsRealtime(freezeDelayBeforeFlash);

        if (wallsFlasher != null)
            yield return StartCoroutine(wallsFlasher.FlashWhiteBlueRoutine());
        else
            Debug.LogError("ScoreManager: wallsFlasher = NULL, geen flash.");

        if (winUI != null)
        {
            winUI.SetActive(true);
            Debug.Log("ScoreManager: WinUI actief ✅");
        }
        else
        {
            Debug.LogError("ScoreManager: winUI = NULL, geen win UI.");
        }

        Time.timeScale = 0f;
    }

    private void FreezeGameplay(bool freeze)
    {
        var pac = FindObjectOfType<PacManMovement>(true);
        if (pac != null) pac.enabled = !freeze;

        var pacDeath = FindObjectOfType<PacmanDeath>(true);
        if (pacDeath != null) pacDeath.enabled = !freeze;

        foreach (var g in FindObjectsOfType<GhostMovement>(true))
            g.enabled = !freeze;

        foreach (var g in FindObjectsOfType<GhostEatable>(true))
            g.enabled = !freeze;
    }
}