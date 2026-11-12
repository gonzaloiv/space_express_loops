Shader "DigitalLove/Cube/Frame"
{
    // The cube's edge bars drawn as corner brackets: each edge is bright near its corners and faint
    // between them. _Bracket is how far from a corner the bright part reaches, as a share of the
    // edge; 0.5 or more lights the whole frame.
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _Bracket ("Bracket", Range(0, 0.6)) = 0.18
        _EdgeAlpha ("Edge Alpha", Range(0, 1)) = 0.2
        _HalfSize ("Half Size", Float) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Pass
        {
            Name "CubeFrame"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Bracket;
                half _EdgeAlpha;
                float _HalfSize;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                // On an edge bar two coordinates sit at the half size; the third runs along the edge,
                // so the smallest one says how far this point is from the nearest corner.
                float3 a = abs(IN.positionOS) / _HalfSize;
                float along = min(a.x, min(a.y, a.z));
                // 0 at a corner, 1 at the edge's middle: half the edge.
                float fromCorner = 1.0 - along;
                half bracket = 1.0h - smoothstep(_Bracket * 2.0 - 0.04, _Bracket * 2.0, fromCorner);
                half alpha = lerp(_EdgeAlpha, 1.0h, bracket);
                return half4(_BaseColor.rgb, _BaseColor.a * alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
