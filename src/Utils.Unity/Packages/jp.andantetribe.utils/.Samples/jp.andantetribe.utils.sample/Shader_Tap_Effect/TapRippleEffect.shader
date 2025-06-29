Shader "UI/TapRippleEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _RippleWidth ("Ripple Width", Range(0.0, 0.5)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Color;
            float _RippleWidth;

            #define MAX_TAPS 10
            float4 _TapPositions[MAX_TAPS];
            float _TapProgresses[MAX_TAPS];
            int _TapCount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 finalColor = float4(0, 0, 0, 0);
                float aspectRatio = _ScreenParams.x / _ScreenParams.y;

                for (int idx = 0; idx < _TapCount; idx++)
                {
                    float2 tapCenter = _TapPositions[idx].xy;
                    float progress = _TapProgresses[idx];
                    
                    float2 delta = i.uv - tapCenter;
                    
                    float2 aspectCorrectedDelta = delta;
                    aspectCorrectedDelta.x *= aspectRatio;
                    float dist = length(aspectCorrectedDelta);
                    
                    float radius = progress * 0.1;
                    float alpha = 0;
                    
                    if (dist < radius && dist > radius - _RippleWidth)
                    {
                        alpha = 1.0 - (progress * 0.8);
                        alpha *= smoothstep(radius - _RippleWidth, radius, dist);
                    
                        float4 rippleColor = float4(_Color.rgb, alpha * _Color.a);
                        finalColor = float4(
                            lerp(finalColor.rgb, rippleColor.rgb, rippleColor.a * (1 - finalColor.a)),
                            finalColor.a + rippleColor.a * (1 - finalColor.a)
                        );
                    }
                }

                return finalColor;
            }
            ENDCG
        }
    }
}