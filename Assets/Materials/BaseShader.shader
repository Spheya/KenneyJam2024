Shader "Custom/BaseShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _MainColor ("Color", Color) = (1,1,1,1)
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
                float3 positionVS : TEXCOORD3;
                float3 ball1VS : TEXCOORD4;
                float3 ball2VS : TEXCOORD5;
                float4 vertex : SV_POSITION;
            };
            
            float4 _Ball1Pos;
            float4 _Ball2Pos;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _MainColor;

            Varyings vert (Attributes attribs) {
                Varyings varyings;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(attribs.vertex.xyz);
                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(attribs.normal, attribs.tangent);
                
                varyings.vertex = vertexInput.positionCS;
                varyings.uv = attribs.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                varyings.normal = vertexNormalInput.normalWS;
                varyings.position = vertexInput.positionWS;
                varyings.positionVS = vertexInput.positionVS;
                varyings.ball1VS = mul(UNITY_MATRIX_V, float4(_Ball1Pos.xyz, 1.0));
                varyings.ball2VS = mul(UNITY_MATRIX_V, float4(_Ball2Pos.xyz, 1.0));
            
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

                if(
                    (length(varyings.ball1VS.xy - varyings.positionVS.xy) < _Ball1Pos.w && _Ball1Pos.y + 0.25 < varyings.position.y) ||
                    (length(varyings.ball2VS.xy - varyings.positionVS.xy) < _Ball2Pos.w && _Ball2Pos.y + 0.25 < varyings.position.y)
                ) {
                    discard;
                    col = float4(1.0, 0.0, 1.0, 1.0);
                }
            
                return col;
            }

            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };
            
            struct Varyings {
                float3 normal : TEXCOORD1;
                float3 position : TEXCOORD2;
                float3 positionVS : TEXCOORD3;
                float3 ball1VS : TEXCOORD4;
                float3 ball2VS : TEXCOORD5;
                float4 vertex : SV_POSITION;
            };
            
            float4 _Ball1Pos;
            float4 _Ball2Pos;

            Varyings vert (Attributes attribs) {
                Varyings varyings;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(attribs.vertex.xyz);
                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(attribs.normal, attribs.tangent);
                
                varyings.vertex = vertexInput.positionCS;
                varyings.normal = vertexNormalInput.normalWS;
                varyings.position = vertexInput.positionWS;
                varyings.positionVS = vertexInput.positionVS;
                varyings.ball1VS = mul(UNITY_MATRIX_V, float4(_Ball1Pos.xyz, 1.0));
                varyings.ball2VS = mul(UNITY_MATRIX_V, float4(_Ball2Pos.xyz, 1.0));
            
                return varyings;
            }
            
            float4 frag (Varyings varyings) : SV_Target {
                if(
                    (length(varyings.ball1VS.xy - varyings.positionVS.xy) < _Ball1Pos.w && _Ball1Pos.y + 0.25 < varyings.position.y) ||
                    (length(varyings.ball2VS.xy - varyings.positionVS.xy) < _Ball2Pos.w && _Ball2Pos.y + 0.25 < varyings.position.y)
                ) {
                    discard;
                }
            
                return float4(normalize(varyings.normal), 0.0);
            }

            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Lit"
}
