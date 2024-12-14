using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(AudioSource))]
public class LightAbsorptionController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip absorptionSound;
    [SerializeField] private float minVolume = 0.5f;
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private int textureResolution = 512;
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private float intensityIncreaseRate = 0.5f;
    [SerializeField] private float intensityDecreaseRate = 0.5f;
    [SerializeField] private float minEmissionIntensity = -5;
    [SerializeField] private float maxEmissionIntensity = 5;
    [SerializeField] private float smooth = 2f;
    [SerializeField] private float requiredTimeInLight = 2f;

    public Light lightSource;
    
    public float changeRate = 1f;
    private float timer = 0f;
    private MeshRenderer meshRenderer;
    private Material materialInstance;
    private Texture2D modifiedTexture;
    private Mesh mesh;
    private int lightenedIndex = 0;
    private int lightendCount = 0;
    private List<int> unlightenedIndexList = new List<int>();
    private Texture2D textureA;
    private RenderTexture textureB;
    private bool useTextureA = true;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly string EmissionKeyword = "_EMISSION";

    private Color originalEmissionColor = Color.white;
    private Color emissionColor;
    private float currentIntensity = 0;
    private float targetIntensity = -5;
    private Material material;

    private AudioSource audioSource;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = GetComponent<Renderer>().material;
        material.EnableKeyword(EmissionKeyword);
        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        emissionColor = originalEmissionColor;
        mesh = GetComponent<MeshFilter>().mesh;

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = absorptionSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.Play();
    }

    void FixedUpdate()
    {
        if (isHitByLights())
        {
            //Debug.Log("hit by light");
            timer += Time.deltaTime;
            if (timer >= requiredTimeInLight)
            {
                emissionColor.r = Mathf.Lerp(emissionColor.r, lightSource.color.r, Time.deltaTime * smooth);
                emissionColor.g = Mathf.Lerp(emissionColor.g, lightSource.color.g, Time.deltaTime * smooth);
                emissionColor.b = Mathf.Lerp(emissionColor.b, lightSource.color.b, Time.deltaTime * smooth);
                emissionColor = AdjustSaturation(emissionColor, 1.03f);
                IncreaseEmissionColorIntensity();
            }
        }
        else
        {
            timer = 0f;
            DecreaseEmissionColorIntensity();
            emissionColor.r = Mathf.Lerp(emissionColor.r, originalEmissionColor.r, Time.deltaTime * smooth);
            emissionColor.g = Mathf.Lerp(emissionColor.g, originalEmissionColor.g, Time.deltaTime * smooth);
            emissionColor.b = Mathf.Lerp(emissionColor.b, originalEmissionColor.b, Time.deltaTime * smooth);
        }

        targetIntensity = Mathf.Clamp(targetIntensity, minEmissionIntensity, maxEmissionIntensity);
        //Debug.Log("target intensity" + targetIntensity);
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * smooth);
        material.SetColor(EmissionColor, emissionColor * currentIntensity);

        UpdateAudio();
    }

    void IncreaseEmissionColorIntensity()
    {
        targetIntensity = targetIntensity + intensityIncreaseRate;
    }

    void DecreaseEmissionColorIntensity()
    {
        targetIntensity = targetIntensity - intensityDecreaseRate;
    }

    private void UpdateAudio()
    {
        float normalizedIntensity = Mathf.InverseLerp(minEmissionIntensity, maxEmissionIntensity, currentIntensity);
        audioSource.volume = Mathf.Lerp(minVolume, maxVolume, normalizedIntensity);
    }

    private Color AdjustSaturation(Color color, float saturation)
    {
        // Convert RGB to HSV
        Color.RGBToHSV(color, out float h, out float s, out float v);

        // Adjust saturation
        s *= saturation;
        s = Mathf.Clamp01(s); // Keep saturation between 0 and 1

        // Convert back to RGB
        return Color.HSVToRGB(h, s, v);
    }

    //void UpdateAbsorptionRandomlyWithShader()
    //{
    //    int kernel = computeShader.FindKernel("ChangeTextureColor");

    //    computeShader.SetTexture(kernel, "InputTexture", textureA);
    //    computeShader.SetTexture(kernel, "OutputTexture", textureB);
    //    computeShader.SetFloats("LightColor", lightSource.color.r, lightSource.color.g, lightSource.color.b);
    //    computeShader.SetFloat("ChangeRate", changeRate);
    //    computeShader.SetInt("Seed", Random.Range(0, int.MaxValue));

    //    // Dispatch the compute shader
    //    int threadGroupsX = Mathf.CeilToInt(textureA.width / 8.0f);
    //    int threadGroupsY = Mathf.CeilToInt(textureA.height / 8.0f);
    //    computeShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

    //    // Create a temporary texture with matching settings
    //    Texture2D temp = new Texture2D(textureA.width, textureA.height,
    //        textureA.format,
    //        false, // no mipmap
    //        false); // linear

    //    RenderTexture.active = textureB;
    //    temp.ReadPixels(new Rect(0, 0, textureB.width, textureB.height), 0, 0);
    //    temp.Apply();

    //    // Copy the pixels directly
    //    Color[] pixels = temp.GetPixels();
    //    textureA.SetPixels(pixels);
    //    textureA.Apply();

    //    Destroy(temp);
    //}

    private Texture2D CreateTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        InitializeTextureBlack(texture);
        return texture;
    }

    private void InitializeTextureBlack(Texture2D texture)
    {
        var colors = new Color32[texture.width * texture.height];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
        }


        texture.SetPixels32(colors);
        texture.Apply();
    }

    // still too slow
    void UpdateAbsorptionRandomly()
    {
        float lightNeedToBeUpdted = 256;
        Color lightColor = lightSource.color;

        while (lightNeedToBeUpdted > 0 && unlightenedIndexList.Count > 0)
        {
            int randomIndex = Random.Range(0, unlightenedIndexList.Count);
            int unlightendPixelIndex = unlightenedIndexList[randomIndex];
            unlightenedIndexList[randomIndex] = unlightenedIndexList[unlightenedIndexList.Count - 1];
            unlightenedIndexList.Remove(randomIndex);

            modifiedTexture.SetPixel(unlightendPixelIndex % modifiedTexture.height, unlightendPixelIndex / modifiedTexture.height, lightColor);
            lightNeedToBeUpdted--;
        }
        modifiedTexture.Apply();
    }

    //// change the color of the ball with order
    //void UpdateAbsorption()
    //{

    //    if (lightenedIndex >= modifiedTexture.height * modifiedTexture.width)
    //    {
    //        return;
    //    }

    //    Color lightColor = lightSource.color;

    //    for (int i = 0; i < modifiedTexture.width; i ++)
    //    {
    //        modifiedTexture.SetPixel(lightenedIndex % modifiedTexture.height, lightenedIndex / modifiedTexture.height, lightColor);
    //        lightenedIndex++;
    //    }

    //    modifiedTexture.Apply();
    //}

    // percise apprach to change the color only hit by light, but it is too slow
    //void UpdateAbsorption()
    //{
    //    Vector2[] uvCoords = mesh.uv;
    //    Vector3[] vertices = mesh.vertices;
    //    Transform meshTransform = transform;
    //    int[] triangles = mesh.triangles;

    //    Bounds lightBounds = new Bounds(lightSource.transform.position, Vector3.one * lightSource.range * 2);
    //    for (int i = 0; i < triangles.Length; i += 3)
    //    {
    //        Vector3 v1 = mesh.vertices[triangles[i]];
    //        Vector3 v2 = mesh.vertices[triangles[i + 1]];
    //        Vector3 v3 = mesh.vertices[triangles[i + 2]];

    //        Bounds triangleBounds = new Bounds(v1, Vector3.zero);
    //        triangleBounds.Encapsulate(v2);
    //        triangleBounds.Encapsulate(v3);

    //        if (!lightBounds.Intersects(triangleBounds))
    //        {
    //            continue;
    //        }

    //        Vector2 uv1 = uvCoords[triangles[i]];
    //        Vector2 uv2 = uvCoords[triangles[i + 1]];
    //        Vector2 uv3 = uvCoords[triangles[i + 2]];

    //        Vector3 worldPos1 = meshTransform.TransformPoint(vertices[triangles[i]]);
    //        Vector3 worldPos2 = meshTransform.TransformPoint(vertices[triangles[i + 1]]);
    //        Vector3 worldPos3 = meshTransform.TransformPoint(vertices[triangles[i + 2]]);

    //        float minX = Mathf.Min(uv1.x, Mathf.Min(uv2.x, uv3.x));
    //        float maxX = Mathf.Max(uv1.x, Mathf.Max(uv2.x, uv3.x));
    //        float minY = Mathf.Min(uv1.y, Mathf.Min(uv2.y, uv3.y));
    //        float maxY = Mathf.Max(uv1.y, Mathf.Max(uv2.y, uv3.y));

    //        int startX = Mathf.Max(0, Mathf.FloorToInt(minX * modifiedTexture.width));
    //        int endX = Mathf.Min(modifiedTexture.width - 1, Mathf.CeilToInt(maxX * modifiedTexture.width));
    //        int startY = Mathf.Max(0, Mathf.FloorToInt(minY * modifiedTexture.height));
    //        int endY = Mathf.Min(modifiedTexture.height - 1, Mathf.CeilToInt(maxY * modifiedTexture.height));

    //        for (int x = startX; x <= endX; x++)
    //        {
    //            for (int y = startY; y <= endY; y++)
    //            {
    //                Vector2 pixelUV = new Vector2((float)x / modifiedTexture.width, (float)y / modifiedTexture.height);

    //                if (IsPointInTriangle(pixelUV, uv1, uv2, uv3) && !IsLightenedAlready(x, y))
    //                {
    //                    Vector3 worldPos = InterpolateWorldPosition(
    //                        pixelUV, uv1, uv2, uv3,
    //                        worldPos1, worldPos2, worldPos3
    //                    );

    //                    if (IsPointHitByLight(worldPos))
    //                    {
    //                        Color lightColor = lightSource.color;
    //                        modifiedTexture.SetPixel(x, y, lightColor);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    modifiedTexture.Apply();
    //}

    //private bool IsLightenedAlready(int x, int y)
    //{
    //    return modifiedTexture.GetPixel(x, y) != Color.black;
    //}

    //private bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    //{
    //    float d1 = Sign(p, a, b);
    //    float d2 = Sign(p, b, c);
    //    float d3 = Sign(p, c, a);

    //    bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
    //    bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

    //    return !(hasNeg && hasPos);
    //}

    //private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    //{
    //    return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    //}

    //private Vector3 InterpolateWorldPosition(Vector2 p, Vector2 a, Vector2 b, Vector2 c,
    //                                   Vector3 worldA, Vector3 worldB, Vector3 worldC)
    //{
    //    float denominator = ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y));
    //    float u = ((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) / denominator;
    //    float v = ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) / denominator;
    //    float w = 1 - u - v;

    //    return worldA * u + worldB * v + worldC * w;
    //}

    bool isHitByLights()
    {
        if (lightSource == null || lightSource.type != LightType.Spot)
        {
            Debug.LogWarning("Please assign a valid spotlight.");
        }

        Bounds bounds = meshRenderer.bounds;
        Vector3 topPoint = bounds.center + new Vector3(0, bounds.extents.y, 0);
        Vector3 bottomPoint = bounds.center - new Vector3(0, bounds.extents.y, 0);
        Vector3 leftPoint = bounds.center - new Vector3(bounds.extents.x, 0, 0);
        Vector3 rightPoint = bounds.center + new Vector3(bounds.extents.x, 0, 0);

        if (IsPointHitByLight(topPoint) || IsPointHitByLight(bottomPoint) ||
            IsPointHitByLight(leftPoint) || IsPointHitByLight(rightPoint) ||
            IsPointHitByLight(transform.position))
        {
            return true;
        }

        return false;
    }

    private bool IsPointHitByLight(Vector3 point)
    {
        if (isInLightRange(point))
        {
            RaycastHit hit;
            Vector3 directionToTarget = point - lightSource.transform.position;
            if (Physics.Raycast(lightSource.transform.position, directionToTarget, out hit, lightSource.range))
            {
                return hit.collider.gameObject.name == gameObject.name;
            }
        }
        return false;
    }

    private bool isInLightRange(Vector3 point)
    {
        Vector3 directionToTarget = point - lightSource.transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget > lightSource.range)
        {
            return false;
        }

        directionToTarget.Normalize();

        float angleToTarget = Vector3.Angle(lightSource.transform.forward, directionToTarget);

        return angleToTarget <= lightSource.spotAngle / 2;
    }
}
