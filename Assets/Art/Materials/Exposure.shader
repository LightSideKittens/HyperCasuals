Shader "Custom/ExposureShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Exposure ("Exposure", Range(0,5)) = 1.0
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color    : COLOR;
            };
            
            struct v2f
            {
                fixed4 color    : COLOR;
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float _Exposure;
            fixed4 _Color;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }
            
            half4 frag (v2f i) : SV_Target
            {
                float4 col = i.color * tex2D(_MainTex, i.uv);
                col.rgb *= _Exposure; // Применяем экспозицию к цветам
                // col.a оставляем без изменений
                return col; // возвращает RGBA, включая альфа-канал исходной текстуры
            }
            ENDCG
        }
    }
}
