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

        // Enable shadows
        ZWrite On
        ZTest LEqual
        Cull Front

        HLSLPROGRAM
        #pragma multi_compile_fog
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        // This struct will carry the data from the vertex shader to the fragment shader
        struct Attributes
        {
            float4 position : POSITION;
            float3 normal : NORMAL;
            float4 tangent : TANGENT;
            float2 uv : TEXCOORD0;
        };

    // The Varyings struct will hold the output of the vertex shader and the input to the fragment shader
    struct Varyings
    {
        float4 position : SV_POSITION;
        float4 shadowCoord : TEXCOORD0;
    };

    // Vertex shader
    Varyings vert(Attributes v)
    {
        Varyings o;
        // Transform the object space position into clip space
        o.position = TransformObjectToHClip(v.position.xyz);
        // Compute screen space position for shadow sampling
        o.shadowCoord = ComputeScreenPos(o.position);
        return o;
    }

    // Fragment shader
    half4 frag(Varyings i) : SV_Target
    {
        // Sample the shadow map using shadow coordinates
        half shadow = SAMPLE_SHADOW(i.shadowCoord);

    // If the shadow value is lower than 0.5, it is in shadow
    if (shadow < 0.5)
    {
        return _ShadowColor;
    }
    else
    {
        return _BaseColor;
    }
}
ENDHLSL
}
    }

        // Fallback if no suitable hardware is found
    FallBack "UniversalForward"
}
