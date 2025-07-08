Shader "Custom/BlurByAlpha"
{
    Properties
    {
        _Color   ("Tint Color", Color)            = (1,1,1,1)
        _MainTex ("Main Tex (RGB × A = blur)", 2D)= "white" {}
        _Size    ("Max Blur Size", Range(0,20))   = 4

        // ↓ третий pass (опционален)
        _BumpMap ("Normal Map", 2D)               = "bump" {}
        _BumpAmt ("Distortion Strength", Range(0,128)) = 10
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull  Off
        ZWrite Off

    /*──────────────────────────── 1. Горизонтальное размытие ────────────────────────────*/
        GrabPass {}
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f
            {
                float4 pos    : SV_POSITION;
                float4 uvGrab : TEXCOORD0;
                float2 uvMain : TEXCOORD1;
            };

            float4 _MainTex_ST;
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

            #if UNITY_UV_STARTS_AT_TOP
                float s = -1.0;
            #else
                float s =  1.0;
            #endif
                o.uvGrab.xy = (float2(o.pos.x, o.pos.y * s) + o.pos.w) * 0.5;
                o.uvGrab.zw =  o.pos.zw;
                o.uvMain    = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            sampler2D _MainTex;
            sampler2D _GrabTexture;
            float4    _GrabTexture_TexelSize;
            float     _Size;

            half4 frag (v2f i) : SV_Target
            {
                // коэффициент размытия из альфы текстуры-маски
                half alpha = tex2D(_MainTex, i.uvMain).a;

                // «чистый» пиксель
                half4 colSharp = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvGrab));

                // смещение по X в texel-координатах
                float dx = _GrabTexture_TexelSize.x * _Size;

                half4 sum = 0;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x - 4.0*dx, i.uvGrab.y, i.uvGrab.z, i.uvGrab.w))) * 0.05;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x - 3.0*dx, i.uvGrab.y, i.uvGrab.z, i.uvGrab.w))) * 0.09;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x - 2.0*dx, i.uvGrab.y, i.uvGrab.z, i.uvGrab.w))) * 0.12;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x - 1.0*dx, i.uvGrab.y, i.uvGrab.z, i.uvGrab.w))) * 0.15;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x + 0.0*dx, i.uvGrab.y, i.uvGrab.z, i.uvGrab.w))) * 0.18;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x + 1.0*dx, i.uvGrab.y, i.uvGrab.z, i.uvGrab.w))) * 0.15;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x + 2.0*dx, i.uvGrab.y, i.uvGrab.z, i.uvGrab.w))) * 0.12;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x + 3.0*dx, i.uvGrab.y, i.uvGrab.z, i.uvGrab.w))) * 0.09;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x + 4.0*dx, i.uvGrab.y, i.uvGrab.z, i.uvGrab.w))) * 0.05;

                return lerp(colSharp, sum, alpha);
            }
            ENDCG
        }

    /*──────────────────────────── 2. Вертикальное размытие ──────────────────────────────*/
        GrabPass {}
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f
            {
                float4 pos    : SV_POSITION;
                float4 uvGrab : TEXCOORD0;
                float2 uvMain : TEXCOORD1;
            };

            float4 _MainTex_ST;
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

            #if UNITY_UV_STARTS_AT_TOP
                float s = -1.0;
            #else
                float s =  1.0;
            #endif
                o.uvGrab.xy = (float2(o.pos.x, o.pos.y * s) + o.pos.w) * 0.5;
                o.uvGrab.zw =  o.pos.zw;
                o.uvMain    = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            sampler2D _MainTex;
            sampler2D _GrabTexture;
            float4    _GrabTexture_TexelSize;
            float     _Size;

            half4 frag (v2f i) : SV_Target
            {
                half alpha = tex2D(_MainTex, i.uvMain).a;
                half4 colSharp = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvGrab));

                float dy = _GrabTexture_TexelSize.y * _Size;

                half4 sum = 0;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x, i.uvGrab.y - 4.0*dy, i.uvGrab.z, i.uvGrab.w))) * 0.05;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x, i.uvGrab.y - 3.0*dy, i.uvGrab.z, i.uvGrab.w))) * 0.09;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x, i.uvGrab.y - 2.0*dy, i.uvGrab.z, i.uvGrab.w))) * 0.12;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x, i.uvGrab.y - 1.0*dy, i.uvGrab.z, i.uvGrab.w))) * 0.15;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x, i.uvGrab.y + 0.0*dy, i.uvGrab.z, i.uvGrab.w))) * 0.18;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x, i.uvGrab.y + 1.0*dy, i.uvGrab.z, i.uvGrab.w))) * 0.15;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x, i.uvGrab.y + 2.0*dy, i.uvGrab.z, i.uvGrab.w))) * 0.12;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x, i.uvGrab.y + 3.0*dy, i.uvGrab.z, i.uvGrab.w))) * 0.09;
                sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvGrab.x, i.uvGrab.y + 4.0*dy, i.uvGrab.z, i.uvGrab.w))) * 0.05;

                return lerp(colSharp, sum, alpha);
            }
            ENDCG
        }

    /*──────────── 3. (по желанию) Pass: дисторсия по normal-map + цвет ────────────*/
        GrabPass {}
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f
            {
                float4 pos    : SV_POSITION;
                float4 uvGrab : TEXCOORD0;
                float2 uvBump : TEXCOORD1;
                float2 uvMain : TEXCOORD2;
            };

            float4 _BumpMap_ST, _MainTex_ST;
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

            #if UNITY_UV_STARTS_AT_TOP
                float s = -1.0;
            #else
                float s =  1.0;
            #endif
                o.uvGrab.xy = (float2(o.pos.x, o.pos.y * s) + o.pos.w) * 0.5;
                o.uvGrab.zw =  o.pos.zw;

                o.uvBump = TRANSFORM_TEX(v.uv, _BumpMap);
                o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            sampler2D _GrabTexture, _BumpMap, _MainTex;
            float4    _GrabTexture_TexelSize;
            float     _BumpAmt;
            fixed4    _Color;

            half4 frag (v2f i) : SV_Target
            {
                half2 n = UnpackNormal(tex2D(_BumpMap, i.uvBump)).rg;
                float2 ofs = n * _BumpAmt * _GrabTexture_TexelSize.xy;

                half4 col  = tex2Dproj(_GrabTexture,
                               UNITY_PROJ_COORD(float4(i.uvGrab.xy + ofs * i.uvGrab.z,
                                                       i.uvGrab.zw)));
                half4 tint = tex2D(_MainTex, i.uvMain) * _Color;
                return col * tint;
            }
            ENDCG
        }
    }

    Fallback Off
}
