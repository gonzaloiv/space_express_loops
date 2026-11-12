Shader "DigitalLove/Spacetime/Lattice"
{
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _StrainColor ("Strain Color", Color) = (1, 1, 1, 1)
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
            Name "Lattice"
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
                half4 _StrainColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Set by FieldGlitch for a moment at the field's beats (0 = steady).
            float _WrapGlitch;

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                half alpha : TEXCOORD0;
                float band : TEXCOORD1;
                // How squeezed this part of the lattice is (vertex red): 0 = base colour, 1 = strain colour.
                half strain : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float GlitchHash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                // During a glitch, horizontal bands of the lattice tear sideways.
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float tick = floor(_Time.y * 30.0);
                float tear = GlitchHash(float2(floor(positionWS.y * 25.0), tick + 7.0)) - 0.5;
                positionWS.x += tear * 0.05 * _WrapGlitch;

                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.alpha = IN.color.a;
                OUT.strain = IN.color.r;
                OUT.band = positionWS.y;
                return OUT;
            }

            // During a glitch, horizontal bands of the lattice drop out and the rest flares.
            half4 frag(Varyings IN) : SV_Target
            {
                float tick = floor(_Time.y * 30.0);
                float band = floor(IN.band * 40.0);
                half drop = step(0.45, GlitchHash(float2(band, tick))) * _WrapGlitch;
                half4 color = lerp(_BaseColor, _StrainColor, IN.strain);
                half3 rgb = color.rgb * (1.0 + _WrapGlitch * 1.2);
                return half4(rgb, color.a * IN.alpha * (1.0 - drop));
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
