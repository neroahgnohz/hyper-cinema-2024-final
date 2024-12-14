Shader "Custom/ShadowCheckURP"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _ShadowColor("Shadow Color", Color) = (0, 0, 0, 1)
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "ShadowCheck"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        // This function determines if the pixel is in shadow
        float4 frag(v2f i) : SV_Target
        {
            // Sample the shadow map
            float shadow = UNITY_SAMPLE_SHADOW(i.shadowCoord);

    // If shadow > 0.5, it's in shadow
    if (shadow > 0.5)
    {
        return _ShadowColor; // Return shadow color
    }
    else
    {
        return _BaseColor; // Return base color if not in shadow
    }
}
ENDHLSL
}
    }

        FallBack "UniversalForward"
}