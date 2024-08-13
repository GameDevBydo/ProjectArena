Shader "Custom/URP_ZTestShader2"
{
    Properties
    {
        //_NormalColor("Normal Color", Color) = (1, 1, 1, 1)
        //_Texture("Textura", 2D) = "white" {}
        _ZTestColor("ZTest Color", Color) = (1, 1, 1, 1)
        //_OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        //_OutlineWidth ("Outline width", Range (0.0, 1.0)) = 0.005
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        
        Pass
        {
            Name "ZTestPass_Color"
            Tags { "LightMode"="UniversalForward" }
            ZWrite On
            ZTest Greater

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float4 _ZTestColor;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return _ZTestColor;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
