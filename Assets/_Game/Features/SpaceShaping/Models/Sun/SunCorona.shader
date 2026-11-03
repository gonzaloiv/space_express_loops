Shader "DigitalLove/SpaceShaping/SunCorona"
{
    Properties
    {
        _CoronaColor ("Corona Color", Color) = (1, 0.82, 0.55, 1)
        _Intensity ("Intensity", Range(0, 8)) = 2
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.5
        _RimBias ("Rim Fill", Range(0, 1)) = 0.05
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
                UNITY_VERTEX_OUTPUT_STEREO
            };

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
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                return half4(_CoronaColor.rgb * (_Intensity * IN.rim), 0.0h);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
