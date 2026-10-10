Shader "WaitYourTurn/MaquetteCard"
{
    Properties
    {
        _Color ("Paper tint", Color) = (1,1,1,1)
        _Ambient ("Studio fill", Color) = (.28,.31,.35,1)
        _Grain ("Fine card grain", 2D) = "white" {}
        _VertexTint ("Use baked vertex tint", Float) = 1
        _Atmosphere ("Darken distant model board", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; fixed4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos:SV_POSITION; half3 normal:TEXCOORD0; float2 grain:TEXCOORD1; fixed3 color:COLOR; half depth:TEXCOORD2; };
            fixed4 _Color, _Ambient; sampler2D _Grain; half _VertexTint, _Atmosphere;
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
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                half3 n=normalize(i.normal);
                half light=saturate(dot(n,_WorldSpaceLightPos0.xyz));
                fixed3 fill=_Ambient.rgb*lerp(.62,1,saturate(n.y*.5+.5));
                half fiber=lerp(.975,1.025,tex2D(_Grain,i.grain).r);
                fixed4 result=fixed4(i.color*(fill+_LightColor0.rgb*light)*fiber,1);
                // A cheap board-only depth gradient, not screen-space blur.
                // Train and actors retain full gameplay contrast.
                half fade=saturate((i.depth-17)/9)*_Atmosphere;
                result.rgb=lerp(result.rgb,fixed3(.018,.027,.041),fade);
                return result;
            }
            ENDCG
        }
    }
    Fallback Off
}
