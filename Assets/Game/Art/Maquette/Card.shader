Shader "WaitYourTurn/MaquetteCard"
{
    Properties
    {
        _Color ("Paper tint", Color) = (1,1,1,1)
        _Ambient ("Studio fill", Color) = (.28,.31,.35,1)
        _Grain ("Fine card grain", 2D) = "white" {}
        _VertexTint ("Use baked vertex tint", Float) = 1
        _Atmosphere ("Darken distant model board", Float) = 0
        _Night ("Studio night treatment", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        CGINCLUDE
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; fixed4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f {
                float4 pos:SV_POSITION; half3 normal:TEXCOORD0; float2 grain:TEXCOORD1;
                fixed3 color:COLOR; half depth:TEXCOORD2; float3 world:TEXCOORD3;
                LIGHTING_COORDS(4,5)
                half3 lamps:TEXCOORD6;
            };
            fixed4 _Color, _Ambient; sampler2D _Grain; half _VertexTint, _Atmosphere, _Night;
            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o; o.pos=UnityObjectToClipPos(v.vertex);
                o.normal=UnityObjectToWorldNormal(v.normal);
                // Local coordinates: paper grain stays attached during travel.
                o.grain=(v.vertex.xz+v.vertex.y*.37)*8;
                o.color=lerp(fixed3(1,1,1),v.color.rgb,_VertexTint)*_Color.rgb;
                #ifdef UNITY_COLORSPACE_GAMMA
                if(_VertexTint>.5)o.color=LinearToGammaSpace(v.color.rgb)*_Color.rgb;
                #endif
                o.depth=-UnityObjectToViewPos(v.vertex).z;
                o.world=mul(unity_ObjectToWorld,v.vertex).xyz;
                o.lamps=0;
                #ifdef VERTEXLIGHT_ON
                o.lamps=Shade4PointLights(unity_4LightPosX0,unity_4LightPosY0,unity_4LightPosZ0,
                    unity_LightColor[0].rgb,unity_LightColor[1].rgb,unity_LightColor[2].rgb,
                    unity_LightColor[3].rgb,unity_4LightAtten0,o.world,normalize(o.normal));
                #endif
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                half3 n=normalize(i.normal);
                half light=saturate(dot(n,normalize(UnityWorldSpaceLightDir(i.world))));
                UNITY_LIGHT_ATTENUATION(atten,i,i.world);
                fixed3 fill=_Ambient.rgb*lerp(.62,1,saturate(n.y*.5+.5));
                half fiber=lerp(.975,1.025,tex2D(_Grain,i.grain).r);
                fixed4 result=fixed4(i.color*(fill+i.lamps+_LightColor0.rgb*light*atten)*fiber,1);
                // A cheap board-only depth gradient, not screen-space blur.
                // Train and actors retain full gameplay contrast.
                // Track-relative distance, not a camera distance that misses the visible board.
                // Both enemy approaches remain readable near the train.
                half fade=lerp(saturate((i.depth-17)/9),smoothstep(2.8,6.8,abs(i.world.z)),_Night)*_Atmosphere;
                result.rgb=lerp(result.rgb,fixed3(.018,.027,.041),fade);
                half stage=1-.22*smoothstep(1.1,2.5,abs(i.world.z));
                // Soft baked-style wall contact shade on the actual floor, no shadow map.
                half deck=saturate((.08-i.world.y)*25)*saturate(n.y);
                stage*=1-.23*deck*smoothstep(1.45,2.1,abs(i.world.z));
                result.rgb*=lerp(1,stage,_Night*(1-_Atmosphere));
                return result;
            }
            fixed4 fragAdd(v2f i):SV_Target
            {
                UNITY_LIGHT_ATTENUATION(atten,i,i.world);
                half diffuse=saturate(dot(normalize(i.normal),normalize(UnityWorldSpaceLightDir(i.world))));
                half fiber=lerp(.975,1.025,tex2D(_Grain,i.grain).r);
                return fixed4(i.color*_LightColor0.rgb*(diffuse*atten*fiber),0);
            }
        ENDCG
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            #pragma skip_variants LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON
            ENDCG
        }
        // Only the player flashlight uses an additional pixel-light pass.
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }
            Blend One One
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragAdd
            #pragma target 2.0
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_instancing
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    Fallback Off
}
