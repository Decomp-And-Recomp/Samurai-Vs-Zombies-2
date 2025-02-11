Shader "GluiOpaqueColored"
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
					Lighting Off
					Cull Off
					ZWrite On
					SetTexture [_MainTex]
					{
						ConstantColor [_Color]
						combine texture * constant
	            	}
	            }
        	}
        }