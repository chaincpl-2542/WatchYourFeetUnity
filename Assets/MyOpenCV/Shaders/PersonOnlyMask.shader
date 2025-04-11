Shader "Custom/PersonOnlyMask"
{
    Properties
    {
        _MainTex ("Main Texture (Video)", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "black" {}
        _Cutoff ("Mask Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float _Cutoff;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ตัวอย่างดึงสีจาก video texture
                fixed4 color = tex2D(_MainTex, i.uv);
                // ดึงค่า mask จาก _MaskTex (ใช้เฉพาะ channel สีแดง)
                float mask = tex2D(_MaskTex, i.uv).r;
                // หาก mask มีค่าสูงกว่า _Cutoff ให้ alpha เท่ากับ 1, มิฉะนั้น alpha เป็น 0
                color.a = step(_Cutoff, mask);
                return color;
            }
            ENDCG
        }
    }
}
