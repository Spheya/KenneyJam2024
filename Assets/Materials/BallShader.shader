Shader "Custom/BallShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _MainColor ("Color", Color) = (1,1,1,1)
        _HasSquishMatrix("Has Squish Matrix", Float) = 0.0
    }
    SubShader
    {
        Tags { "LightMode" = "UniversalForwardOnly" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

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
            
            float4 _MainColor;

            float _HasSquishMatrix;
            float4x4 _SquishMatrix;
            
            Varyings vert (Attributes attribs) {
                Varyings varyings;
            
                float3 objectPos = attribs.vertex.xyz;
                if(_HasSquishMatrix > 0.5) {
                    objectPos = mul(_SquishMatrix, float4(objectPos, 1.0));
                }

                VertexPositionInputs vertexInput = GetVertexPositionInputs(objectPos);
                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(attribs.normal, attribs.tangent);
                
                varyings.vertex = vertexInput.positionCS;
                varyings.uv = attribs.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                varyings.normal = vertexNormalInput.normalWS;
                varyings.position = vertexInput.positionWS;
            
                return varyings;
            }
            
            float4 frag (Varyings varyings) : SV_Target {
                float4 col = tex2D(_MainTex, varyings.uv);
                col *= _MainColor;
                col.rgb *= SampleSH(normalize(varyings.normal));

                #ifdef _MAIN_LIGHT_SHADOWS

                Light mainLight = GetMainLight();
                float3 shadowTestPosWS = varyings.position + mainLight.direction * 0.005 + varyings.normal * float3(0.02, 0.0, 0.02);
                float4 shadowCoord = TransformWorldToShadowCoord(shadowTestPosWS);
                col *= MainLightRealtimeShadow(shadowCoord);
                
                #endif

                #ifdef _SCREEN_SPACE_OCCLUSION

                float2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(varyings.vertex);
                AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUV);
                col.rgb *= aoFactor.directAmbientOcclusion * aoFactor.indirectAmbientOcclusion;

                #endif
            
                return col;
            }

            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }

    Fallback "Universal Render Pipeline/Lit"
}
