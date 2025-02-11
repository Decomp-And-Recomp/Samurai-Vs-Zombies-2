Shader "GluiAdditiveColored"
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
					Blend One One
					ZWrite On
					Lighting Off
					Cull Off
					SetTexture [_MainTex]
					{
						ConstantColor [_Color]
						combine texture * constant
	            	}
	            }
        	}
        }