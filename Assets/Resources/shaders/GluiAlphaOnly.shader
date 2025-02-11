Shader "GluiAlphaOnly"
        {
			Properties
			{
        		_MainTex ("Texture", 2D) = "white" {}
        		_Color ("Color", Color) = (1, 1, 1, 1)
        	}
        	SubShader
        	{
        		Tags
        		{
        			"Queue" = "Transparent"
        		}
        		Pass
        		{
					Blend SrcAlpha OneMinusSrcAlpha
					ZWrite On
					ZTest LEqual 
					Lighting Off
					Cull Off
					Fog { Mode Off }
					SetTexture [_MainTex]
					{
						ConstantColor [_Color]
						combine constant, texture * constant
	            	}
	            }
        	}
        }