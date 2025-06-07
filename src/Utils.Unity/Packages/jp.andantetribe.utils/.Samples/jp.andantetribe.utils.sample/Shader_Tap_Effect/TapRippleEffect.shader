Shader "UI/TapRippleEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _RippleWidth ("Ripple Width", Range(0.0, 0.5)) = 0.1
        _Progress ("Progress", Range(0.0, 1.0)) = 0.0
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _RippleWidth;
            float _Progress;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                
                float radius = _Progress * 0.5;
                float alpha = 0;
                
                if (dist < radius && dist > radius - _RippleWidth)
                {
                    // リング内部の場合、中心からの距離に応じて透明度を計算
                    alpha = 1.0 - (_Progress * 0.8); // プログレスに応じてフェードアウト
                    alpha *= smoothstep(radius - _RippleWidth, radius, dist);
                }
                
                return float4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}