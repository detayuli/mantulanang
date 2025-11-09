Shader "Custom/2D_GlassProceduralHDR"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (A controls transparency)", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _GlassWarp ("Glass Warp Intensity", Range(0, 1)) = 0.1

        // HDR Color + Emission
        [HDR]_Color ("Tint (HDR)", Color) = (1,1,1,1)
        [HDR]_EmissionColor ("Emission (HDR)", Color) = (0,0,0,0)
        _EmissionStrength ("Emission Strength", Range(0,5)) = 1
        _WarpSpeed ("Warp Speed", Range(0,10)) = 2
        _WarpScale ("Warp Scale", Range(0,10)) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        GrabPass { "_GrabTexture" }

        // Sprite Mask Support
        Stencil
        {
            Ref 1
            Comp [_MaskInteraction]
            Pass Keep
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MASK_INTERACTION_VISIBLE_INSIDE _MASK_INTERACTION_VISIBLE_OUTSIDE
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _MaskTex;
            sampler2D _NormalMap;
            sampler2D _GrabTexture;

            float4 _Color;
            float4 _EmissionColor;
            float _GlassWarp;
            float _EmissionStrength;
            float _WarpSpeed;
            float _WarpScale;

            struct appdata_custom
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f_custom
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 grabPos : TEXCOORD1;
            };

            v2f_custom vert(appdata_custom v)
            {
                v2f_custom o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f_custom i) : SV_Target
            {
                // --- Base sprite color ---
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // --- Mask alpha ---
                fixed4 mask = tex2D(_MaskTex, i.uv);

                // --- Normal-based distortion ---
                float3 normal = UnpackNormal(tex2D(_NormalMap, i.uv));
                float2 offset = normal.xy * _GlassWarp;

                // --- Procedural Warp (tanpa texture) ---
                float2 waveUV = i.uv * _WarpScale;
                float wave = sin(waveUV.x * 10 + _Time.y * _WarpSpeed) * cos(waveUV.y * 10 + _Time.y * _WarpSpeed);
                offset += wave * 0.03;

                // --- Distort background ---
                float2 grabUV = (i.grabPos.xy / i.grabPos.w) + offset;
                fixed4 bg = tex2D(_GrabTexture, grabUV);

                // --- Combine glass color with background ---
                fixed4 finalColor = lerp(bg, col, 0.3);

                // --- HDR Emission ---
                fixed3 emission = _EmissionColor.rgb * _EmissionStrength;
                finalColor.rgb += emission; // Additive glow

                // --- Final alpha ---
                finalColor.a = mask.r * _Color.a;

                return finalColor;
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
