Shader "Custom/ShoreWaves"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "black" {}
        _DistanceMap("Distance Map", 2D) = "white" {}
        _Speed("Wave Speed", Float) = 0.2
        _WaveScale("Wave Scale", Float) = 1.0
        _LineSpacing("Line Spacing", Float) = 0.05
        _LineWidth("Line Width", Range(0.0, 0.1)) = 0.1
        _CoastWidth("Coast Width", Float) = 0.0025
        _MinContourSpacing("Min Contour Spacing", Float) = 0.01
        _MaxContourSpacing("Max Contour Spacing", Float) = 0.1
        _ContourRange("Contour Range", Float) = 10.0
        _ContourLineWidth("Contour Line Width", Float) = 0.02
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;         // Sprite texture (required for Sprite Renderer)
            sampler2D _DistanceMap;     // Distance map (R channel for distance to land)
            float4 _MainTex_ST;         // Tiling and offset for the sprite texture

            float _Speed;
            float _WaveScale;
            float _LineSpacing;
            float _LineWidth;

            float _CoastWidth;

            float _MinContourSpacing;
            float _MaxContourSpacing;
            float _ContourRange;
            float _ContourLineWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv       : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // Apply tiling and offset to UVs
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 spriteColor = tex2D(_MainTex, i.uv);

                float dist = tex2D(_DistanceMap, i.uv).r;

                float isLand = step(dist, 0.0001);

                float waveDistance = dist * _WaveScale - _Time.y * _Speed;
                float waveFrac     = frac(waveDistance / _LineSpacing);
                float waveLine     = step(waveFrac, _LineWidth);

                float coastLine = step(dist, _CoastWidth);

                float localLineSpacing = lerp(_MaxContourSpacing, _MinContourSpacing, saturate(dist / _ContourRange));
                float contourFrac = frac(dist / localLineSpacing);
                float staticLine  = step(contourFrac, _ContourLineWidth);

                float combinedLine = max(waveLine, coastLine);

                float waterLine = combinedLine * (1 - isLand);

                if (waterLine < 0.01)
                    discard;

                float4 col;
                col.rgb = waterLine.xxx * spriteColor.rgb;
                col.a = waterLine;
                return col;
            }
            ENDHLSL
        }
    }
}
