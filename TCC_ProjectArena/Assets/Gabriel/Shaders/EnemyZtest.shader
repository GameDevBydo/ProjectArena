Shader "Custom/EnemyZtest"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Texture("Textura", 2D) = "white"{}
    }
    SubShader
    {
        Pass
        {
            ZTest LEqual
            Cull Back
            SetTexture[_Texture]
            {
                combine texture * previous
            }
        }

        Pass
        {
            ZTest Greater
            Cull Back
            Color [_Color]
        }

        
        
    }
}
