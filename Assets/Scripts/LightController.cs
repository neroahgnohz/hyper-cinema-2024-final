using UnityEngine;

public class SpotlightController : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light spotlight1;
    [SerializeField] private Light spotlight2;
    [SerializeField] private Light spotlight3;

    [Header("Color Ranges")]
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 1f;
    
    private Transform targetBox;    // Reference to the box with Arduino control
    private float maxAngle = 90f;   // Maximum angle for color mapping

    private void Start()
    {
        // Find the box with the StageController
        targetBox = FindObjectOfType<StageController>().transform;
    }

    private void Update()
    {
        if (!targetBox) return;

        Vector3 rotation = targetBox.rotation.eulerAngles;
        
        // Normalize angles to 0-1 range
        float normalizedPitch = Mathf.Abs(NormalizeAngle(rotation.x)) / maxAngle;
        float normalizedHeading = Mathf.Abs(NormalizeAngle(rotation.y)) / maxAngle;
        float normalizedRoll = Mathf.Abs(NormalizeAngle(rotation.z)) / maxAngle;

        // Set different color ranges for each light
        if (spotlight1 != null)
        {
            // Light 1: Red to Yellow spectrum
            Color color1 = new Color(
                1f,  // Red always full
                normalizedPitch,  // Green varies
                0f   // No blue
            );
            spotlight1.color = color1;
            spotlight1.intensity = Mathf.Lerp(maxIntensity, minIntensity, normalizedPitch);
        }

        if (spotlight2 != null)
        {
            // Light 2: Green to Cyan spectrum
            Color color2 = new Color(
                0f,   // No red
                1f,   // Green always full
                normalizedHeading  // Blue varies
            );
            spotlight2.color = color2;
            spotlight2.intensity = Mathf.Lerp(maxIntensity, minIntensity, normalizedHeading);
        }

        if (spotlight3 != null)
        {
            // Light 3: Blue to Purple spectrum
            Color color3 = new Color(
                normalizedRoll,  // Red varies
                0f,   // No green
                1f    // Blue always full
            );
            spotlight3.color = color3;
            spotlight3.intensity = Mathf.Lerp(maxIntensity, minIntensity, normalizedRoll);
        }
    }

    private float NormalizeAngle(float angle)
    {
        // Convert angle to -180 to 180 range
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
}