
Shader "Point Cloud/Point (Blobby)"
{
    Properties
    {
        _Tint("Tint", Color) = (0.5, 0.5, 0.5, 1)
        _PointSize("Point Size", Float) = 0.05
        [Toggle] _Distance("Apply Distance", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" } // Use transparent queue
        Pass
        {
            // Enable soft overlap
            Blend One One
            ZWrite Off
            Cull Off

            CGPROGRAM

            #pragma target 3.0
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma multi_compile _ _DISTANCE_ON
            #pragma multi_compile _ _COMPUTE_BUFFER
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "Common.cginc"

            struct Attributes
            {
                float4 position : POSITION;
                half3 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_Position;
                half3 color : COLOR;
                half psize : PSIZE;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half4 _Tint;
            float4x4 _Transform;
            half _PointSize;

            #if _COMPUTE_BUFFER
            StructuredBuffer<float4> _PointBuffer;
            #endif

            #if _COMPUTE_BUFFER
            Varyings Vertex(uint vid : SV_VertexID)
            #else
            Varyings Vertex(Attributes input)
            #endif
            {
                Varyings o;

                #if _COMPUTE_BUFFER
                float4 pt = _PointBuffer[vid];
                float4 pos = mul(_Transform, float4(pt.xyz, 1));
                half3 col = PcxDecodeColor(asuint(pt.w));
                #else
                UNITY_SETUP_INSTANCE_ID(input);
                float4 pos = input.position;
                half3 col = input.color;
                #endif

                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                #ifdef UNITY_COLORSPACE_GAMMA
                col *= _Tint.rgb * 2;
                #else
                col *= LinearToGammaSpace(_Tint.rgb) * 2;
                col = GammaToLinearSpace(col);
                #endif

                o.position = UnityObjectToClipPos(pos);

                #ifdef _DISTANCE_ON
                o.psize = _PointSize / o.position.w * _ScreenParams.y;
                #else
                o.psize = _PointSize;
                #endif

                o.color = col;
                return o;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Soft circular alpha (simulate gl_PointCoord fallback)
                float2 uv = frac(input.position.xy) * 2 - 1; // approx fallback
                float d = length(uv);
                float alpha = saturate(1.0 - d * 2.0); // fade out near edges

                return half4(input.color * alpha, alpha * _Tint.a);
            }

            ENDCG
        }
    }

    CustomEditor "Pcx.PointMaterialInspector"
}
