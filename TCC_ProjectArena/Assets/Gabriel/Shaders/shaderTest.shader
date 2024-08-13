Shader "Custom/EnemyZTestURP"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Texture("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        ZTest LEqual
        Cull Back

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            ZTest Greater
            ZWrite On
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Properties to access in the shader code
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                sampler2D _Texture;
            CBUFFER_END

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Combine texture with color
                half4 texColor = tex2D(_Texture, i.uv);
                return texColor * _Color;
            }
            ENDHLSL
        }
    }
}
