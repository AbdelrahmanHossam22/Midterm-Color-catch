using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public float totalTime = 60f;
    public int totalCoins = 9;
    public GameObject collectiblePrefab;

    [Header("Spawn Options")]
    public string groundTag = "Ground";
    public int maxSpawnAttempts = 50;
    public float avoidOverlapRadius = 0.6f;
    public float minSpacingBetweenCoins = 2.5f;
    public float spawnSurfaceOffset = 0.2f;

    [Header("Audio (optional)")]
    public AudioClip correctSfx;
    public AudioClip wrongSfx;
    public AudioClip caughtSfx;

    [Tooltip("Reference to your SFXManager or background music AudioSource")]
    public AudioSource sfxManager;

    [Header("Enemy Catch Settings")]
    public bool endOnCatch = true;
    public int catchScorePenalty = 10;
    public float catchTimePenalty = 5f;

    private float timeLeft;
    private bool gameActive = true;
    private int score = 0;

    private readonly List<Collectible> allCollectibles = new();
    private readonly List<Vector3> spawnPositions = new();
    private GameObject spawnedParent;

    [Header("Runtime Debug Info")]
    public Collectible.ColorType targetColor;
    private Collectible.ColorType lastColor;

    public event Action<int> OnScoreChanged;
    public event Action<float> OnTimeChanged;
    public event Action<Collectible.ColorType> OnTargetColorChanged;

    public int CurrentScore => score;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        timeLeft = totalTime;
        score = 0;
        spawnedParent = new GameObject("SpawnedCollectibles");

        SpawnRandomCoins(totalCoins);
        ChooseTargetColor();

        OnScoreChanged?.Invoke(score);
        OnTimeChanged?.Invoke(timeLeft);
    }

    void Update()
    {
        if (!gameActive) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft < 0f) timeLeft = 0f;
        OnTimeChanged?.Invoke(timeLeft);

        if (timeLeft <= 0f)
            EndGame();
    }

    // ---------------- ENEMY CATCH ----------------
    public void HandleEnemyCatch()
    {
        if (!gameActive) return;

        if (caughtSfx && Camera.main)
            AudioSource.PlayClipAtPoint(caughtSfx, Camera.main.transform.position);

        if (catchScorePenalty != 0)
        {
            score -= catchScorePenalty;
            OnScoreChanged?.Invoke(score);
        }

        if (catchTimePenalty > 0f)
        {
            timeLeft = Mathf.Max(0f, timeLeft - catchTimePenalty);
            OnTimeChanged?.Invoke(timeLeft);
        }

        if (endOnCatch || timeLeft <= 0f)
            EndGame();
    }

    // ---------------- COIN COLLECTION ----------------
    public void HandleCollectible(Collectible c)
    {
        if (!gameActive || c == null) return;

        // Save current target color before it changes
        Collectible.ColorType currentTarget = targetColor;
        bool isCorrect = (c.colorType == currentTarget);

        // Correct or wrong color logic
        if (isCorrect)
        {
            score += 10;
            if (correctSfx && Camera.main)
                AudioSource.PlayClipAtPoint(correctSfx, Camera.main.transform.position);
        }
        else
        {
            score -= 5;
            if (wrongSfx && Camera.main)
                AudioSource.PlayClipAtPoint(wrongSfx, Camera.main.transform.position);
        }

        // Update UI
        OnScoreChanged?.Invoke(score);

        // Remove collected coin
        allCollectibles.Remove(c);
        spawnPositions.Remove(c.transform.position);
        Destroy(c.gameObject);

        // Change target color for next coin
        ChooseTargetColor();

        // End game if no coins left
        if (allCollectibles.Count == 0)
            EndGame();
    }

    // ---------------- SPAWN COINS ----------------
    private void SpawnRandomCoins(int total)
    {
        if (collectiblePrefab == null)
        {
            Debug.LogError("❌ No collectible prefab assigned!");
            return;
        }

        GameObject groundObj = GameObject.FindWithTag(groundTag);
        if (groundObj == null)
        {
            Debug.LogError("❌ No object tagged as 'Ground' found!");
            return;
        }

        Collider groundCollider = groundObj.GetComponent<Collider>();
        if (groundCollider == null)
        {
            Debug.LogError("❌ Ground object has no collider!");
            return;
        }

        Bounds spawnBounds = groundCollider.bounds;

        for (int i = 0; i < total; i++)
        {
            bool spawned = false;
            int attempts = 0;

            while (!spawned && attempts < maxSpawnAttempts)
            {
                attempts++;

                float x = UnityEngine.Random.Range(spawnBounds.min.x, spawnBounds.max.x);
                float z = UnityEngine.Random.Range(spawnBounds.min.z, spawnBounds.max.z);
                float rayStartY = spawnBounds.max.y + 50f;
                Vector3 rayStart = new(x, rayStartY, z);

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 100f))
                {
                    if (!hit.collider.CompareTag(groundTag))
                        continue;

                    Vector3 spawnPos = hit.point + Vector3.up * spawnSurfaceOffset;

                    bool tooClose = false;
                    foreach (var existing in spawnPositions)
                    {
                        if (Vector3.Distance(spawnPos, existing) < minSpacingBetweenCoins)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose) continue;

                    Collider[] nearby = Physics.OverlapSphere(spawnPos, avoidOverlapRadius);
                    bool overlap = false;
                    foreach (var n in nearby)
                    {
                        if (n.CompareTag("Coin"))
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (overlap) continue;

                    GameObject obj = Instantiate(collectiblePrefab, spawnPos, Quaternion.identity, spawnedParent.transform);
                    var colScript = obj.GetComponent<Collectible>();
                    if (colScript != null)
                    {
                        int randomColor = UnityEngine.Random.Range(0, Enum.GetValues(typeof(Collectible.ColorType)).Length);
                        colScript.colorType = (Collectible.ColorType)randomColor;
                        colScript.SetColorVisual();

                        obj.tag = "Coin";
                        allCollectibles.Add(colScript);
                        spawnPositions.Add(spawnPos);
                    }

                    spawned = true;
                }
            }

            if (!spawned)
                Debug.LogWarning($"⚠️ Could not find valid position for coin #{i + 1}");
        }
    }

    // ---------------- COLOR SELECTION ----------------
    private bool HasColorLeft(Collectible.ColorType color)
    {
        foreach (var c in allCollectibles)
        {
            if (c != null && c.colorType == color)
                return true;
        }
        return false;
    }

    private void ChooseTargetColor()
    {
        List<Collectible.ColorType> availableColors = new();

        foreach (Collectible.ColorType color in Enum.GetValues(typeof(Collectible.ColorType)))
        {
            if (HasColorLeft(color))
                availableColors.Add(color);
        }

        if (availableColors.Count == 0)
            return;

        Collectible.ColorType newColor;
        do
        {
            newColor = availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
        } while (newColor == lastColor && availableColors.Count > 1);

        targetColor = newColor;
        lastColor = newColor;
        OnTargetColorChanged?.Invoke(targetColor);

        Debug.Log($"🎯 Target Color: {targetColor}");
    }

    // ---------------- END GAME ----------------
    private void EndGame()
    {
        if (!gameActive) return;
        gameActive = false;

        timeLeft = Mathf.Max(0f, timeLeft);
        OnTimeChanged?.Invoke(timeLeft);

        Debug.Log($"⏰ Game Over! Final Score: {score}");

        //  Stop background music using the assigned SFX Manager
        if (sfxManager != null && sfxManager.isPlaying)
            sfxManager.Stop();
        else
            Debug.Log("No SFX Manager assigned or already stopped.");

        //  Show Game Over panel and final score
        var ui = UnityEngine.Object.FindFirstObjectByType<UIManager>();
        if (ui != null)
            ui.ShowGameOver(score);
    }

    public void RestartGame()
    {
        if (spawnedParent != null)
            Destroy(spawnedParent);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("Game Quit");
    }
}