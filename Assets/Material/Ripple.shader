Shader "Unlit/Ripple"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveSpeed("wave speed", Float) = 1
        _WaveFrequency("wave frequency", Float) = 1
        _NoiseStrength("noise strength", Float) = 1
        _LineColor ("line color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            /*
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            */
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            //TEXTURE2D(_MainTex);
            //SAMPLER(sampler_MainTex);
            float _WaveSpeed;
            float _WaveFrequency;
            float _NoiseStrength;
            float4 _LineColor;

            float _TimeY;

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

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = UnityObjectToClipPos(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float random(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float perlinNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 baseColor = tex2D(_MainTex, i.uv);

                float wallMask = step(0.9, dot(baseColor.rgb, float3(0.333,.333,.333)));

                float2 uv = i.uv;
                float distanceFromCenter = length(uv - float2(0.5, 0.5));
                float ripple = sin(distanceFromCenter * _WaveFrequency - _Time.y * _WaveSpeed);

                float noise = perlinNoise(uv * 10.0) * _NoiseStrength;

                ripple += noise;

                float rippleMask = smoothstep(0.4, 0.5, abs(ripple));

                float finalEffect = wallMask * rippleMask;

                return lerp(baseColor, _LineColor, finalEffect);
            }
            ENDCG
        }
    }
}
