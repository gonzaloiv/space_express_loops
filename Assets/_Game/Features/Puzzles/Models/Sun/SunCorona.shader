Shader "DigitalLove/SpaceShaping/SunCorona"
{
    Properties
    {
        _CoronaColor ("Corona Color", Color) = (1, 0.82, 0.55, 1)
        _Intensity ("Intensity", Range(0, 8)) = 2
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.5
        _RimBias ("Rim Fill", Range(0, 1)) = 0.05
        [Header(Living corona)]
        _FlareScale ("Flare Scale", Range(0.5, 12)) = 4
        _FlareSpeed ("Flare Drift", Range(0, 2)) = 0.35
        _FlareDepth ("Flare Depth", Range(0, 1)) = 0.55
        _BreathSpeed ("Breath Speed", Range(0, 4)) = 0.8
        _BreathDepth ("Breath Depth", Range(0, 1)) = 0.25
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        Blend One One
        ZWrite Off
        Cull Back

        Pass
        {
            Name "SunCorona"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _CoronaColor;
                half _Intensity;
                half _RimPower;
                half _RimBias;
                half _FlareScale;
                half _FlareSpeed;
                half _FlareDepth;
                half _BreathSpeed;
                half _BreathDepth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                half rim : TEXCOORD0;
                float3 flareUV : TEXCOORD1;
                half breath : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            // Smooth value noise: the drifting flares over the shell.
            float Noise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(lerp(Hash(i), Hash(i + float3(1, 0, 0)), f.x),
                         lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                    lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x),
                         lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y),
                    f.z);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(positionWS);

                half3 normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                half viewDotNormal = saturate(dot(normalWS, GetWorldSpaceNormalizeViewDir(positionWS)));
                OUT.rim = pow(saturate(1.0h - viewDotNormal + _RimBias), _RimPower);

                // Each sun breathes on its own phase, taken from where it sits.
                float3 originWS = TransformObjectToWorld(float3(0, 0, 0));
                float phase = dot(originWS, float3(37.1, 17.3, 23.7));
                OUT.breath = 1.0h + _BreathDepth * sin(_Time.y * _BreathSpeed * 6.2831853 + phase);
                OUT.flareUV = IN.positionOS.xyz * _FlareScale + float3(0, _Time.y * _FlareSpeed, phase);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                half flare = lerp(1.0h - _FlareDepth, 1.0h + _FlareDepth, Noise(IN.flareUV));
                return half4(_CoronaColor.rgb * (_Intensity * IN.rim * flare * IN.breath), 0.0h);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
