Shader "MMORPG/Map/DesertEnvironment"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.78, 0.58, 0.34, 1)
        _NoiseColor ("Noise Color", Color) = (0.95, 0.78, 0.48, 1)
        _NoiseScale ("Noise Scale", Float) = 0.18
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.18
        _TopLight ("Top Light", Range(0, 1)) = 0.18
        _GridColor ("Grid Color", Color) = (0.0, 0.85, 1.0, 1)
        _GridScale ("Grid Scale", Float) = 0.12
        _GridStrength ("Grid Strength", Range(0, 1)) = 0.2
        _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionStrength ("Emission Strength", Range(0, 4)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _NoiseColor;
                float _NoiseScale;
                half _NoiseStrength;
                half _TopLight;
                half4 _GridColor;
                float _GridScale;
                half _GridStrength;
                half4 _EmissionColor;
                half _EmissionStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
            };

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float noise = Hash(floor(input.positionWS.xz * _NoiseScale));
                half3 color = lerp(_BaseColor.rgb, _NoiseColor.rgb, noise * _NoiseStrength);
                half top = saturate(input.normalWS.y) * _TopLight;
                color += top;
                float2 gridUv = abs(frac(input.positionWS.xz * _GridScale) - 0.5);
                float gridLine = 1.0 - saturate(min(gridUv.x, gridUv.y) * 24.0);
                color = lerp(color, _GridColor.rgb, gridLine * _GridStrength);
                color += _EmissionColor.rgb * _EmissionStrength;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
