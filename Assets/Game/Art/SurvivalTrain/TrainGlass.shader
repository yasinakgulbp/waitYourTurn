Shader "WaitYourTurn/TrainGlass"
{
    Properties { _Color ("Tint", Color) = (0.14,0.26,0.29,0.12) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos:SV_POSITION; half3 normal:TEXCOORD0; half3 view:TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };
            fixed4 _Color;
            v2f vert(appdata v)
            {
                v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos=UnityObjectToClipPos(v.vertex); o.normal=UnityObjectToWorldNormal(v.normal);
                o.view=WorldSpaceViewDir(v.vertex); return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                half rim=1-saturate(abs(dot(normalize(i.normal),normalize(i.view))));
                return fixed4(_Color.rgb + rim*rim*.12,_Color.a + rim*rim*.08);
            }
            ENDCG
        }
    }
    FallBack Off
}
