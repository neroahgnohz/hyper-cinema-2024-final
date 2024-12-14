using UnityEngine;

public class EmissiveTrailSystem : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private float trailHeight = 0.1f;
    [SerializeField] private float trailWidth = 0.5f;
    [SerializeField] private float trailDuration = 2f;
    [SerializeField] private Material trailMaterial;

    private TrailRenderer trail;
    private MeshRenderer meshRenderer;

    void Start()
    {
        // Add TrailRenderer component
        trail = gameObject.AddComponent<TrailRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();

        // Configure trail settings
        trail.material = trailMaterial;
        trail.time = trailDuration;
        trail.startWidth = trailWidth;
        trail.endWidth = trailWidth;
        trail.minVertexDistance = 0.1f;

        // Position the trail slightly above the floor
        Vector3 position = transform.position;
        position.y = trailHeight;
        trail.transform.position = position;

        // Make sure the trail follows the object
        trail.autodestruct = false;
    }

    void Update()
    {
        // Get the current emission color from the object
        Color currentEmissionColor = meshRenderer.material.GetColor("_EmissionColor");

        // Update trail color to match emission
        trail.startColor = new Color(currentEmissionColor.r, currentEmissionColor.g, currentEmissionColor.b, 1f);
        trail.endColor = new Color(currentEmissionColor.r, currentEmissionColor.g, currentEmissionColor.b, 0f);

        // Keep trail at constant height above floor
        Vector3 position = transform.position;
        position.y = trailHeight;
        trail.transform.position = position;
    }
}