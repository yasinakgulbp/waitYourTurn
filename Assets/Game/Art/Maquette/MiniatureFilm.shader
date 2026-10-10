Shader "WaitYourTurn/MiniatureFilm"
{
    Properties { _MainTex("Source",2D)="white"{} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex, _BlurTex;
        float4 _MainTex_TexelSize, _Axis, _Focus;
        half _Vignette, _UseBlur;
        fixed4 blur(v2f_img i):SV_Target
        {
            float2 d=_Axis.xy;
            fixed4 c=tex2D(_MainTex,i.uv)*.227027;
            c+=(tex2D(_MainTex,i.uv+d*1.384615)+tex2D(_MainTex,i.uv-d*1.384615))*.316216;
            c+=(tex2D(_MainTex,i.uv+d*3.230769)+tex2D(_MainTex,i.uv-d*3.230769))*.070270;
            return c;
        }
        fixed4 film(v2f_img i):SV_Target
        {
            fixed4 c=tex2D(_MainTex,i.uv);
            half band=max(smoothstep(0,_Focus.z,_Focus.x-i.uv.y),smoothstep(0,_Focus.z,i.uv.y-_Focus.y));
            c.rgb=lerp(c.rgb,tex2D(_BlurTex,i.uv).rgb,band*_Focus.w*_UseBlur);
            // Subtle warm highlights / cool paper shadows. UI is drawn afterward.
            c.rgb=lerp(c.rgb*.98,c.rgb*fixed3(1.035,1.015,.99),saturate(dot(c.rgb,fixed3(.21,.72,.07))));
            float2 edge=(i.uv-.5)*2;
            half v=saturate(dot(edge,edge)*.65);
            c.rgb*=1-v*v*_Vignette;
            return c;
        }
        ENDCG
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment blur
            #pragma target 2.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment film
            #pragma target 2.0
            ENDCG
        }
    }
    Fallback Off
}
