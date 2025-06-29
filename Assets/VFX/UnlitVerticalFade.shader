// File: UnlitVerticalFadeURP.shader
// This shader is compatible with the Universal Render Pipeline (URP).
// It makes an object transparent based on its local height.

Shader "Custom/UnlitVerticalFadeURP"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (0, 0.5, 1, 0.5)
        _FadeHeight ("Fade Height", Float) = 1.0
        // The Cull property controls which faces of the geometry are rendered.
        // Off: Renders both back and front faces (visible from inside and out).
        // Front: Hides front faces.
        // Back: Hides back faces (default for opaque objects).
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0 // Default to Off
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull [_Cull] // Use the property to control culling

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionOS   : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _FadeHeight;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionOS = IN.positionOS.xyz; 
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float fade = 1.0 - saturate(IN.positionOS.y / _FadeHeight);
                half4 col = _Color;
                col.a *= fade;
                return col;
            }
            ENDHLSL
        }
    }
}
    