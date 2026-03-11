Shader "Custom/EauAnimee"
{
    Properties
    {
        _MainTex    ("Texture", 2D)         = "white" {}
        _CouleurEau ("Couleur eau",  Color) = (0.1, 0.5, 0.8, 1)
        _CouleurRef ("Reflets",      Color) = (0.6, 0.85, 1.0, 1)
        _VitesseX   ("Vitesse X",    Float) = 0.05
        _VitesseY   ("Vitesse Y",    Float) = 0.03
        _Echelle    ("Echelle onde", Float) = 3.0
        _Intensite  ("Intensite",    Float) = 0.15
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4    _CouleurEau;
            float4    _CouleurRef;
            float     _VitesseX;
            float     _VitesseY;
            float     _Echelle;
            float     _Intensite;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f    { float4 pos:SV_POSITION;  float2 uv:TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UV scrolling
                float2 uv = i.uv;
                uv.x += _Time.y * _VitesseX;
                uv.y += _Time.y * _VitesseY;

                // Ondulation
                float onde1 = sin(uv.x * _Echelle * 6.28 +
                                  _Time.y * 1.5) * _Intensite;
                float onde2 = sin(uv.y * _Echelle * 4.0  +
                                  _Time.y * 1.0) * _Intensite * 0.6;
                float onde  = saturate((onde1 + onde2 + 1.0) * 0.5);

                // Mélange eau + reflets
                return lerp(_CouleurEau, _CouleurRef, onde);
            }
            ENDCG
        }
    }
}