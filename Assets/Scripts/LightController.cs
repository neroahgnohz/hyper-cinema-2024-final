using UnityEngine;

public class SpotlightController : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light spotlight1;
    
    private Color originalColor;
    private Vector3 originalHsv;

    private void Start()
    {
        originalColor = spotlight1.color;
        Color.RGBToHSV(originalColor, out originalHsv.x, out originalHsv.y, out originalHsv.z);
    }

    private void Update()
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        
        // Normalize angles to 0-1 range
        float normalizedPitch = NormalizeAngle(rotation.x);
        float normalizedHeading = NormalizeAngle(rotation.y);
        float normalizedRoll = NormalizeAngle(rotation.z);

        if (spotlight1 != null)
        {
            // value needs to be greater than 0.3
            Color color1 = Color.HSVToRGB(normalizedHeading, originalHsv.y, AdjustValue(normalizedPitch, normalizedRoll));
            spotlight1.color = color1;
        }
    }

    private float NormalizeAngle(float angle)
    {
        return Mathf.Abs(angle) / 360;
    }

    private float AdjustValue(float pitch, float roll)
    {
        float avg = 0.5f * (pitch + roll);

        return 0.4f + (avg * (1f - 0.4f));
    }
}