Shader "Unlit/Shader1" {
    Properties {
        _BaseColor ("Color", Color) = (1, 1,1,1)
    }
    SubShader {
        Tags {"RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline" = "UniversalPipeline"}
        Pass {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial) //Constant Buffer - otimização URP/HDRP
                float4 _BaseColor;
            CBUFFER_END

            v2f vert (appdata v) {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                return o;
            }
            float4 frag (v2f i) : SV_Target {
                return _BaseColor.rgba;
            }
            ENDHLSL
        }
    }
}