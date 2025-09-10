Shader "Unlit/VertexColorURP"
{
    // Shader for "PointCloudToQuads", works with URP and uses a stereo view for VR glasses.

    Properties 
    {
        _Tint("Tint", Color) = (0.5, 0.5, 0.5, 1)
        _Brightness("Brightness Multiplier", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #pragma multi_compile _ STEREO_INSTANCING_ON
            #pragma multi_compile_instancing

            // Helpers for colorign.
            inline float3 LinearToGamma(float3 linearColor)
            {
                return pow(linearColor, 1.0 / 2.2); // Approximate gamma 2.2
            }

            inline float3 GammaToLinear(float3 gammaColor)
            {
                return pow(gammaColor, 2.2);
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // For better colors.
            CBUFFER_START(UnityPerMaterial)
            half4 _Tint;
            float _Brightness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);

                float3 tint = _Tint.rgb;
            #if !defined(UNITY_COLORSPACE_GAMMA)
                tint = LinearToGamma(tint);
                tint = GammaToLinear(tint);
            #endif
            tint *= 2.0;

                output.color.rgb = input.color.rgb * tint * _Brightness;
                output.color.a = input.color.a * _Tint.a;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
    }
}