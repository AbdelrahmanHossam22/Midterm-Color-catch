using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Prefab of the collectible object to spawn")]
    public GameObject collectiblePrefab;

    [Tooltip("How many coins per color to spawn (3 means 3 red, 3 blue, 3 green)")]
    public int coinsPerColor = 3;

    [Header("Spawn Area")]
    [Tooltip("Minimum X, Y, Z of the spawn area (Y controls height)")]
    public Vector3 areaMin = new Vector3(-8f, 0.5f, -4f);

    [Tooltip("Maximum X, Y, Z of the spawn area (Y controls height)")]
    public Vector3 areaMax = new Vector3(8f, 0.5f, 4f);

    // Keep track of all spawned collectibles for management or cleanup
    public List<Collectible> spawnedCollectibles = new List<Collectible>();

    void Start()
    {
        // Automatically spawn all coins when the scene starts
        SpawnAll();
    }

    /// <summary>
    /// Spawns all colors in equal numbers.
    /// </summary>
    public void SpawnAll()
    {
        spawnedCollectibles.Clear();

        SpawnColor(Collectible.ColorType.Red, coinsPerColor);
        SpawnColor(Collectible.ColorType.Blue, coinsPerColor);
        SpawnColor(Collectible.ColorType.Green, coinsPerColor);

        Debug.Log($"✅ Spawned {spawnedCollectibles.Count} total collectibles.");
    }

    /// <summary>
    /// Spawns collectibles of a given color within the area bounds.
    /// </summary>
    private void SpawnColor(Collectible.ColorType color, int count)
    {
        if (collectiblePrefab == null)
        {
            Debug.LogError("❌ No collectible prefab assigned to Spawner!");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // Random position within the area box
            Vector3 pos = new Vector3(
                Random.Range(areaMin.x, areaMax.x),
                Random.Range(areaMin.y, areaMax.y),
                Random.Range(areaMin.z, areaMax.z)
            );

            GameObject obj = Instantiate(collectiblePrefab, pos, Quaternion.identity);
            var col = obj.GetComponent<Collectible>();

            if (col != null)
            {
                col.colorType = color;
                col.SetColorVisual();
                spawnedCollectibles.Add(col);
            }
        }
    }

#if UNITY_EDITOR
    // Optional: draw the spawn area box in the Scene view for debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = (areaMin + areaMax) * 0.5f;
        Vector3 size = new Vector3(
            Mathf.Abs(areaMax.x - areaMin.x),
            Mathf.Abs(areaMax.y - areaMin.y),
            Mathf.Abs(areaMax.z - areaMin.z)
        );
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
