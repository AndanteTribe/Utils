Shader "UI/TapRippleEffect"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _RippleWidth ("Ripple Width", Range(0.0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _RippleWidth;
            float _Duration;

            StructuredBuffer<float3> _Records;
            int _Count;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 finalColor = float4(0, 0, 0, 0);
                float aspectRatio = _ScreenParams.x / _ScreenParams.y;

                for (int idx = 0; idx < _Count; idx++)
                {
                    float2 center = _Records[idx].xy;
                    float progress = saturate((_Time.y - _Records[idx].z) / _Duration);

                    float2 delta = i.uv - center;
                    delta.x *= aspectRatio;
                    float dist = length(delta);

                    float radius = progress * 0.1;
                    float ringMask = radius - _RippleWidth < dist && dist < radius ? 1.0 : 0.0;
                    float alpha = (1.0 - progress * 0.8) * ringMask;
                    alpha *= smoothstep(radius - _RippleWidth, radius, dist);

                    float4 rippleColor = float4(_Color.rgb, alpha * _Color.a);
                    finalColor = float4(
                        lerp(finalColor.rgb, rippleColor.rgb, rippleColor.a * (1 - finalColor.a)),
                        finalColor.a + rippleColor.a * (1 - finalColor.a)
                    );
                }

                return finalColor;
            }
            ENDCG
        }
    }
}