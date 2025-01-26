Shader "Custom/ShoreWaves"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {} 
        _DistanceMap("Distance Map", 2D) = "white" {}
        _Speed("Wave Speed", Float) = 1.0
        _WaveScale("Wave Scale", Float) = 1.0
        _LineSpacing("Line Spacing", Float) = 0.1
        _LineWidth("Line Width", Range(0.0, 0.1)) = 0.02
        _CoastWidth("Coast Width", Float) = 0.05

        _MinContourSpacing("Min Contour Spacing", Float) = 0.01
        _MaxContourSpacing("Max Contour Spacing", Float) = 0.1
        _ContourRange("Contour Range", Float) = 10.0
        _ContourLineWidth("Contour Line Width", Float) = 0.02

        // 如果你有需要的话，也可以加一个 “额外AA强度” 用来放大fwidth
        //_ExtraAA("Extra AA Scale", Range(1,4)) = 1.0
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
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            ////////////////////////////////////////////////////
            // Attributes and Uniforms
            ////////////////////////////////////////////////////
            sampler2D _MainTex;         
            sampler2D _DistanceMap;     

            float4 _MainTex_ST;   // Sprite UV 变换

            float _Speed;
            float _WaveScale;
            float _LineSpacing;
            float _LineWidth;
            float _CoastWidth;

            float _MinContourSpacing;
            float _MaxContourSpacing;
            float _ContourRange;
            float _ContourLineWidth;

            // 如果有需要，可声明：
            // float _ExtraAA; 

            ////////////////////////////////////////////////////
            // Structs
            ////////////////////////////////////////////////////
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

            ////////////////////////////////////////////////////
            // Vertex Shader
            ////////////////////////////////////////////////////
            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            ////////////////////////////////////////////////////
            // 一些辅助函数
            ////////////////////////////////////////////////////

            // 用于给 frac 做平移，让 0 和 1 的交界处也能平滑
            // 例：x -> frac(x + 0.5) - 0.5，然后取绝对值变成一个对称的区间 [0,0.5]
            float shiftedFrac(float x)
            {
                float f = frac(x + 0.5) - 0.5;
                return abs(f);
            }

            ////////////////////////////////////////////////////
            // Fragment Shader
            ////////////////////////////////////////////////////
            float4 frag (v2f i) : SV_Target
            {
                // ======================================================
                // 1. 采样纹理
                // ======================================================
                float4 spriteColor = tex2D(_MainTex, i.uv);         // RGBA
                float dist = tex2D(_DistanceMap, i.uv).r;           // 距离图中 R 通道

                // 判断是否陆地 (非常接近0)
                float isLand = step(dist, 0.0001);

                // ======================================================
                // 2. 动态波浪线（周期线）
                // ======================================================
                // 原理：waveVal = (dist * _WaveScale - t * _Speed) / _LineSpacing
                // 之后把 waveVal 转到 [0,1) 区间并形成周期线
                float waveVal = (dist * _WaveScale - _Time.y * _Speed) / _LineSpacing;

                // 平移+对称的 frac，避免 0/1 边界出现锯齿
                float waveFrac = shiftedFrac(waveVal);

                // 计算屏幕空间的梯度，用以做平滑
                // 如果需要额外放大AA范围，可 * 一个系数
                float waveFwidth = fwidth(waveVal) * 0.5; 
                // waveFwidth *= _ExtraAA; // 如果需要的话

                // 将线条定义为 waveFrac < _LineWidth * 0.5 (周期内一小段)
                // 利用 smoothstep 在边缘衰减
                float waveLine = 1.0 - smoothstep(_LineWidth * 0.5 - waveFwidth, 
                                                  _LineWidth * 0.5 + waveFwidth,
                                                  waveFrac);

                // ======================================================
                // 3. 海岸线 (单阈值)
                // ======================================================
                // dist < _CoastWidth -> 海岸区域
                // 同样利用 fwidth 做平滑
                float coastFwidth = fwidth(dist);
                // coastFwidth *= _ExtraAA; // 如果需要的话

                float coastLine = 1.0 - smoothstep(_CoastWidth - coastFwidth,
                                                  _CoastWidth + coastFwidth,
                                                  dist);

                // 陆地区域做一个淡出(或者直接乘 1.0-isLand 也行)
                // 这里让非常靠近 0 的地方平滑退出
                float landFade = smoothstep(0.0, 0.001, dist);
                float finalCoastLine = coastLine * landFade;

                // ======================================================
                // 4. 等高线（可选）
                // ======================================================
                // 需求：dist 越大，线距越大
                // localLineSpacing 从 _MinContourSpacing 到 _MaxContourSpacing
                float localLineSpacing = lerp(_MaxContourSpacing, 
                                              _MinContourSpacing,
                                              saturate(dist / _ContourRange));

                float contourVal = dist / localLineSpacing;
                float contourFrac = shiftedFrac(contourVal);
                float contourFwidth = fwidth(contourVal) * 0.5;
                // contourFwidth *= _ExtraAA; // 如果需要的话

                float staticLine = 1.0 - smoothstep(_ContourLineWidth * 0.5 - contourFwidth,
                                                   _ContourLineWidth * 0.5 + contourFwidth,
                                                   contourFrac);

                // ======================================================
                // 5. 组合线条
                //    a) 如果只要波浪+海岸线:  max(waveLine, coastLine)
                //    b) 如果还需要等高线:    max(waveLine, max(staticLine, coastLine))
                // ======================================================
                // float combinedLine = max(waveLine, max(staticLine, finalCoastLine));
                float combinedLine = max(waveLine, finalCoastLine);

                // 陆地区域去除
                float waterLine = combinedLine * (1.0 - isLand);

                // ======================================================
                // 6. 最终颜色
                //    这里直接把线条当成 alpha，即线条可逐渐衰减到 0
                // ======================================================
                float4 col;
                col.rgb = spriteColor.rgb;
                col.a   = spriteColor.a * waterLine;

                // 如果想在透明度极低时不绘制，提高效率，可保留 discard：
                if (col.a <= 0.0)
                    discard;

                return col;
            }
            ENDHLSL
        }
    }
}
