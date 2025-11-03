using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]
public class Collectible : MonoBehaviour
{
    public enum ColorType { Red, Blue, Green }
    public ColorType colorType;

    private Renderer rend;
    private Collider col;
    private bool collected = false;
    private Material instanceMaterial;

    [Header("Spin Settings")]
    [Tooltip("How fast the collectible rotates (degrees per second)")]
    public float spinSpeed = 90f;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();

        // Ensure collider is a trigger for OnTriggerEnter
        if (col != null)
            col.isTrigger = true;

        // Create a unique material instance to prevent color-sharing issues
        if (rend != null)
            instanceMaterial = rend.material;
    }

    /// <summary>
    /// Called by Spawner or GameManager to set this collectible's color visually.
    /// </summary>
    public void SetColorVisual()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        if (instanceMaterial == null) instanceMaterial = rend.material;

        switch (colorType)
        {
            case ColorType.Red:
                instanceMaterial.color = Color.red;
                break;
            case ColorType.Blue:
                instanceMaterial.color = Color.blue;
                break;
            case ColorType.Green:
                instanceMaterial.color = Color.green;
                break;
        }

        rend.material = instanceMaterial;
    }

    void Update()
    {
        // 🔁 Rotate smoothly around the Y-axis (for visibility and motion)
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return; // Prevent multiple triggers

        if (other.CompareTag("Player"))
        {
            collected = true;

            // Inform GameManager about the collection event
            if (GameManager.Instance != null)
                GameManager.Instance.HandleCollectible(this);

            Destroy(gameObject); // Remove the coin
        }
    }
}
