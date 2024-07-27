Shader "Custom/ArrowShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _MainColor ("Color", Color) = (1,1,1,1)
        _Shift ("Shift", Range(0.0, 1.0)) = 0.0
        _MaxShift ("MaxShift", Range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        LOD 100

        Pass
        {
            Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
            ZWrite On
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings {
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 position : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Shift;
            float _MaxShift;
            float _Fade;
            
            float4 _MainColor;

                        float3 hsvToRgb(float3 c) {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }
            
            float3 rgbToHsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
            
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            Varyings vert (Attributes attribs) {
                Varyings varyings;
            
                VertexPositionInputs vertexInput = GetVertexPositionInputs(attribs.vertex.xyz);
                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(attribs.normal, attribs.tangent);
                
                varyings.vertex = vertexInput.positionCS;
                varyings.uv = attribs.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                varyings.normal = vertexNormalInput.normalWS;
                varyings.position = vertexInput.positionWS;
            
                return varyings;
            }
            
            float4 frag (Varyings varyings) : SV_Target {
                float4 col = tex2D(_MainTex, varyings.uv);
                col.rgb = rgbToHsv(col.rgb);
                col.r -= _Shift * _MaxShift;
                col.rgb = hsvToRgb(col.rgb);

                col *= _MainColor;
                col.rgb *= SampleSH(normalize(varyings.normal));
            
                return col;
            }

            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Lit"
}