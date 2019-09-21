﻿Shader "Custom/WorldSea_CookTrrance"
{
    Properties
    {
        _SeaNorTex ("Normal Map1", 2D) = "bump" { }
        _SeaNorTex2 ("Normal Map2", 2D) = "bump" { }

        _WorldLightPos ("WorldLightPos", vector) = (0, 0, 0)
        _WorldLightColor ("WorldLightColor", Color) = (1, 1, 1, 1)

        _GTex ("海水环境光渐变贴图", 2D) = "white" {}
        _GRange ("海水环境光渐变_采样系数", Float) = 1
        _Ambi_k ("海水环境光渐变_强度系数", Float) = 1

        _DiffColor ("漫反射颜色", Color) = (1, 1, 1, 1)
        _Diff_k ("漫反射系数", Float) = 1

        _SpecularColor ("高光颜色", Color) = (1, 1, 1, 1)
        _Specular_k ("高光系数1", Range(0, 1)) = 1
        _Specular_f ("高光系数2", Range(0, 1)) = 0.5
        _Specular_m ("高光系数3", Range(0, 1)) = 0.1

        _WaveSpeed1_X ("Wave Speed1_X", Range(-1, 1)) = 1
        _WaveSpeed1_Y ("Wave Speed1_Y", Range(-1, 1)) = 1
        _WaveSpeed2_X ("Wave Speed2_X", Range(-1, 1)) = 1
        _WaveSpeed2_Y ("Wave Speed2_Y", Range(-1, 1)) = 1
        _WaveScale1 ("Wave Scale1", Range(0, 1)) = 1
        _WaveScale2 ("Wave Scale2", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
                float4 tangent: TANGENT;
                float2 texcoord: TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex: SV_POSITION;
                float4 uv: TEXCOORD0;
                float3 objSpaceLightDir: TEXCOORD1;
                float3 objSpaceViewDir: TEXCOORD2;
                float eyeDepth: TEXCOORD3;
            };

            uniform sampler2D _SeaNorTex;
            uniform float4 _SeaNorTex_ST;
            uniform sampler2D _SeaNorTex2;
            uniform float4 _SeaNorTex2_ST;

            uniform sampler2D _GTex;
            uniform float _GRange;
            uniform float _Ambi_k;

            uniform float4 _DiffColor;
            uniform float _Diff_k;

            uniform float4 _SpecularColor;
            uniform float _Specular_k;
            uniform float _Specular_f;
            uniform float _Specular_m;
            
            uniform float4 _WorldLightColor;
            uniform float3 _WorldLightPos;

            uniform float _WaveSpeed1_X;
            uniform float _WaveSpeed1_Y;
            uniform float _WaveSpeed2_X;
            uniform float _WaveSpeed2_Y;
            uniform float _WaveScale1;
            uniform float _WaveScale2;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.x = v.texcoord.x + _WaveSpeed1_X / _SeaNorTex_ST.x * _Time.x;
                o.uv.y = v.texcoord.y + _WaveSpeed1_Y / _SeaNorTex_ST.y * _Time.x;
                o.uv.z = v.texcoord.x + _WaveSpeed2_X / _SeaNorTex2_ST.x * _Time.x;
                o.uv.w = v.texcoord.y + _WaveSpeed2_Y / _SeaNorTex2_ST.y * _Time.x;
                TANGENT_SPACE_ROTATION;
                o.objSpaceLightDir = mul(rotation, mul(unity_WorldToObject, float4(_WorldLightPos,0)).xyz);
                o.objSpaceViewDir = mul(rotation, ObjSpaceViewDir(v.vertex));
                o.eyeDepth = -(UnityObjectToViewPos( v.vertex ).z * _ProjectionParams.w);//计算顶点摄像机空间的深度：距离裁剪平面的距离
                return o;
            }
            float3 BlendAngleCorrectedNormals(float3 baseNormal, float3 additionalNormal)
            {
                baseNormal.b += 1;
                additionalNormal *= float3(-1, -1, 1);
                float3 normal = dot(baseNormal, additionalNormal) * baseNormal - baseNormal.b * additionalNormal;
                return normalize(normal);
            }
            fixed4 frag(v2f i): SV_Target
            {
                float3 tangentLightDir = normalize(i.objSpaceLightDir);
                float3 tangentViewDir = normalize(i.objSpaceViewDir);

                float2 uv1 = i.uv.xy * _SeaNorTex_ST.xy + _SeaNorTex_ST.zw;
                float2 uv2 = i.uv.zw * _SeaNorTex2_ST.xy + _SeaNorTex2_ST.zw;
                float4 packedNormal1 = tex2D(_SeaNorTex, uv1);
                float4 packedNormal2 = tex2D(_SeaNorTex2, uv2);
                float3 tangentNormal1 = UnpackNormal(packedNormal1);
                float3 tangentNormal2 = UnpackNormal(packedNormal2);
                tangentNormal1.xy *= _WaveScale1;
                tangentNormal1.z = sqrt(1.0 - saturate(dot(tangentNormal1.xy, tangentNormal1.xy)));
                tangentNormal2.xy *= _WaveScale2;
                tangentNormal2.z = sqrt(1.0 - saturate(dot(tangentNormal2.xy, tangentNormal2.xy)));
                float3 tangentNormalMix = BlendAngleCorrectedNormals(tangentNormal1, tangentNormal2);

                float3 l = tangentLightDir;
                float3 v = tangentViewDir;
                float3 n = tangentNormalMix;
                float3 h = normalize(l + v);

                float vh = dot(v, h);
                float nh = dot(n, h);
                float nl = dot(n, l);
                float nv = dot(n, v);

                float4 waterColor = tex2D(_GTex, float2(1-(min(_GRange, i.eyeDepth)/_GRange),1));
                float4 ambi = waterColor * _Ambi_k;

                float4 diff = _DiffColor * _Diff_k * saturate(nl);

                if (nh <= 0 || nl <= 0)
                    return _WorldLightColor * ambi + diff;

                float F = _Specular_f + (1 - _Specular_f) * (1 - pow(vh, 5));
                float D = exp((nh * nh - 1) / (_Specular_m * _Specular_m * nh * nh)) / (_Specular_m * _Specular_m * pow(nh, 4));
                float G = min(1, min(2 * nh * nl / vh, 2 * nh * nv / vh));

                float R = F * D * G / (nv * nl);

                float4 spec = _SpecularColor * _Specular_k * R * nl;

                
                // return waterColor;
                return _WorldLightColor * ambi + diff + spec;
                // return _WorldLightColor * ambi + diff;
            }
            
            ENDCG
            
        }
    }
}