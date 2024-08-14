Shader "Custom/URP_ZTestShaderLEqual"
{
    Properties
    {
        _NormalColor("Normal Color", Color) = (1, 1, 1, 1)
        _Texture("Textura", 2D) = "white" {}
        _ZTestColor("ZTest Color", Color) = (1, 1, 1, 1)
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth("Outline width", Range(0.0, 1.0)) = 0.005
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // Normal pass
        Pass
        {
            Name "NormalPass"
            Tags { "LightMode"="UniversalForward" }
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Texture2D _Texture;
            SamplerState sampler_Texture;
            float4 _NormalColor;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv; // No transformation needed for UVs here
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Sample the texture
                half4 texColor = _Texture.Sample(sampler_Texture, i.uv);

                // Combine the texture color with the normal color
                return texColor * _NormalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
