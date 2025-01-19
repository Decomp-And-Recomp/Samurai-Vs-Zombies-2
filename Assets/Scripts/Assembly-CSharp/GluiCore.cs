using System.Collections.Generic;
using UnityEngine;

public class GluiCore
{
	public const int MaxCursors = 5;

	private static Camera camera;

	private static Vector2 screenScale;

	private static Vector2 anchorOffset;

	private static bool hasAnchorOffset;

	private static Vector2 LastValidCursorPosition;

	private static float inputLockTime;

	public static Material defaultMaterial;

	public static Material additiveColoredMaterial;

	public static Material opaqueColoredMaterial;

	public static Material alphaDesaturateMaterial;

	public static Material vertColorMaterial;

	public static Material textAlphaBlendVCOL4ChannelMateral;

	public static Material maskWriteMaterial;

	public static Material maskReadMaterial;

	public static Material alphaOnlyMaterial;

	public static Material alphaOnlyMaskMaterial;

	public static int defaultRenderQueue;

	public static Dictionary<Texture, List<GluiMaterialTracker>> sMaterialDictionary;

	public static List<GluiMaterialTracker> sNullTextureMaterials;

	public static Vector2 Scale
	{
		get
		{
			return screenScale;
		}
	}

	public static Camera Camera
	{
		get
		{
			return camera;
		}
		set
		{
			camera = value;
		}
	}

	public static string MISSING_STRING
	{
		get
		{
			return "<MISSING_STRING>";
		}
	}

	public static Vector2 AnchorOffset
	{
		get
		{
			if (!hasAnchorOffset)
			{
				FindScreen();
			}
			return anchorOffset;
		}
	}

	public static Shader DefaultShader
	{
		get
		{
			if (defaultMaterial == null)
			{
				CreateShaders();
			}
			return defaultMaterial.shader;
		}
	}

	public static Shader AdditiveColoredShader
	{
		get
		{
			if (additiveColoredMaterial == null)
			{
				CreateShaders();
			}
			return additiveColoredMaterial.shader;
		}
	}

	public static Shader OpaqueColoredShader
	{
		get
		{
			if (opaqueColoredMaterial == null)
			{
				CreateShaders();
			}
			return opaqueColoredMaterial.shader;
		}
	}

	public static Shader AlphaDesaturate
	{
		get
		{
			if (alphaDesaturateMaterial == null)
			{
				CreateShaders();
			}
			return alphaDesaturateMaterial.shader;
		}
	}

	public static Shader TextAlphaBlendVCOL4Channel
	{
		get
		{
			if (textAlphaBlendVCOL4ChannelMateral == null)
			{
				CreateShaders();
			}
			return textAlphaBlendVCOL4ChannelMateral.shader;
		}
	}

	public static Shader MaskReadShader
	{
		get
		{
			if (maskReadMaterial == null)
			{
				CreateShaders();
			}
			return maskReadMaterial.shader;
		}
	}

	public static Shader MaskWriteShader
	{
		get
		{
			if (maskWriteMaterial == null)
			{
				CreateShaders();
			}
			return maskWriteMaterial.shader;
		}
	}

	public static Shader AlphaOnlyShader
	{
		get
		{
			if (alphaOnlyMaterial == null)
			{
				CreateShaders();
			}
			return alphaOnlyMaterial.shader;
		}
	}

	public static Shader AlphaOnlyMaskShader
	{
		get
		{
			if (alphaOnlyMaskMaterial == null)
			{
				CreateShaders();
			}
			return alphaOnlyMaskMaterial.shader;
		}
	}

	public static Shader VertColorShader
	{
		get
		{
			if (vertColorMaterial == null)
			{
				CreateShaders();
			}
			return vertColorMaterial.shader;
		}
	}

	public static bool InputLocked
	{
		get
		{
			return inputLockTime > Time.realtimeSinceStartup;
		}
	}

	static GluiCore()
	{
		screenScale = new Vector2(1f, 1f);
		hasAnchorOffset = false;
		inputLockTime = 0f;
		defaultMaterial = null;
		additiveColoredMaterial = null;
		opaqueColoredMaterial = null;
		alphaDesaturateMaterial = null;
		vertColorMaterial = null;
		textAlphaBlendVCOL4ChannelMateral = null;
		maskWriteMaterial = null;
		maskReadMaterial = null;
		alphaOnlyMaterial = null;
		alphaOnlyMaskMaterial = null;
		sMaterialDictionary = new Dictionary<Texture, List<GluiMaterialTracker>>();
		sNullTextureMaterials = new List<GluiMaterialTracker>();
		CreateShaders();
	}

	public static void AddLocalizedString(string stringName, string text)
	{
		if (string.IsNullOrEmpty(stringName))
		{
			return;
		}
		foreach (GluiLocale value2 in GluiSettings.Locales.Values)
		{
			string value;
			if (value2.stringTable.strings.TryGetValue(stringName, out value))
			{
				value2.stringTable.strings[stringName] = text;
			}
			else
			{
				value2.stringTable.strings.Add(stringName, text);
			}
		}
		GluiSettings.SaveLocales();
	}

	public static string FindString(string stringName)
	{
		if (string.IsNullOrEmpty(stringName))
		{
			return MISSING_STRING;
		}
		GluiLocale value;
		string value2;
		if (GluiSettings.Locales.TryGetValue(GluiSettings.CurrentLocale, out value) && value.stringTable.strings.TryGetValue(stringName, out value2))
		{
			return value2;
		}
		return MISSING_STRING;
	}

	public static void FindScreen()
	{
		GluiScreen gluiScreen = Object.FindObjectOfType(typeof(GluiScreen)) as GluiScreen;
		if (gluiScreen != null)
		{
			SetScreen(gluiScreen);
		}
	}

	public static void SetScreen(GluiScreen screen)
	{
		if (screen.resizeConstant == GluiScreen.ResizeAxis.Width)
		{
			screenScale.x = (float)Screen.width / screen.nativeWidth;
		}
		else
		{
			screenScale.x = (float)Screen.height / screen.nativeHeight;
		}
		screenScale.y = screenScale.x;
		anchorOffset = new Vector2(0f, 0f);
		float num = screen.nativeWidth / screen.nativeHeight;
		float num2 = (float)Screen.width / (float)Screen.height;
		float num3 = num2 / num;
		if (screen.resizeConstant == GluiScreen.ResizeAxis.Height)
		{
			anchorOffset.x = screen.nativeWidth * num3 - screen.nativeWidth;
		}
		else if (screen.resizeConstant == GluiScreen.ResizeAxis.Width)
		{
			anchorOffset.y = screen.nativeHeight * num3 - screen.nativeHeight;
		}
		anchorOffset.Scale(new Vector2(0.5f, 0.5f));
		hasAnchorOffset = true;
	}

	private static void CreateShaders()
	{
		string contents = "Shader \"GluiDefault\"\n        {\n\t\t\tProperties\n\t\t\t{\n        \t\t_MainTex (\"Texture\", 2D) = \"white\" {}\n        \t\t_Color (\"Color\", Color) = (1, 1, 1, 1)\n        \t}\n        \tSubShader\n        \t{\n        \t\tTags\n        \t\t{\n        \t\t\t\"Queue\" = \"Transparent\"\n        \t\t}\n        \t\tPass\n        \t\t{\n\t\t\t\t\tBlend SrcAlpha OneMinusSrcAlpha\n\t\t\t\t\tZWrite On\n\t\t\t\t\tZTest LEqual\n\t\t\t\t\tLighting Off\n\t\t\t\t\tCull Off\n\t\t\t\t\tSetTexture [_MainTex]\n\t\t\t\t\t{\n\t\t\t\t\t\tConstantColor [_Color]\n\t\t\t\t\t\tcombine texture * constant\n\t            \t}\n\t            }\n        \t}\n        }";
		string contents2 = "Shader \"GluiAlphaOnly\"\n        {\n\t\t\tProperties\n\t\t\t{\n        \t\t_MainTex (\"Texture\", 2D) = \"white\" {}\n        \t\t_Color (\"Color\", Color) = (1, 1, 1, 1)\n        \t}\n        \tSubShader\n        \t{\n        \t\tTags\n        \t\t{\n        \t\t\t\"Queue\" = \"Transparent\"\n        \t\t}\n        \t\tPass\n        \t\t{\n\t\t\t\t\tBlend SrcAlpha OneMinusSrcAlpha\n\t\t\t\t\tZWrite On\n\t\t\t\t\tZTest LEqual \n\t\t\t\t\tLighting Off\n\t\t\t\t\tCull Off\n\t\t\t\t\tFog { Mode Off }\n\t\t\t\t\tSetTexture [_MainTex]\n\t\t\t\t\t{\n\t\t\t\t\t\tConstantColor [_Color]\n\t\t\t\t\t\tcombine constant, texture * constant\n\t            \t}\n\t            }\n        \t}\n        }";
		string contents3 = "Shader \"GluiAdditiveColored\"\n        {\n\t\t\tProperties\n\t\t\t{\n        \t\t_MainTex (\"Texture\", 2D) = \"white\" {}\n        \t\t_Color (\"Color\", Color) = (1, 1, 1, 1)\n        \t}\n        \tSubShader\n        \t{\n        \t\tTags\n        \t\t{\n        \t\t\t\"Queue\" = \"Transparent\"\n        \t\t}\n        \t\tPass\n        \t\t{\n\t\t\t\t\tBlend One One\n\t\t\t\t\tZWrite On\n\t\t\t\t\tLighting Off\n\t\t\t\t\tCull Off\n\t\t\t\t\tSetTexture [_MainTex]\n\t\t\t\t\t{\n\t\t\t\t\t\tConstantColor [_Color]\n\t\t\t\t\t\tcombine texture * constant\n\t            \t}\n\t            }\n        \t}\n        }";
		string contents4 = "Shader \"GluiOpaqueColored\"\n        {\n\t\t\tProperties\n\t\t\t{\n        \t\t_MainTex (\"Texture\", 2D) = \"white\" {}\n        \t\t_Color (\"Color\", Color) = (1, 1, 1, 1)\n        \t}\n        \tSubShader\n        \t{\n        \t\tTags\n        \t\t{\n        \t\t\t\"Queue\" = \"Transparent\"\n        \t\t}\n        \t\tPass\n        \t\t{\n\t\t\t\t\tLighting Off\n\t\t\t\t\tCull Off\n\t\t\t\t\tZWrite On\n\t\t\t\t\tSetTexture [_MainTex]\n\t\t\t\t\t{\n\t\t\t\t\t\tConstantColor [_Color]\n\t\t\t\t\t\tcombine texture * constant\n\t            \t}\n\t            }\n        \t}\n        }";
		defaultMaterial = new Material(contents);
		alphaOnlyMaterial = new Material(contents2);
		additiveColoredMaterial = new Material(contents3);
		opaqueColoredMaterial = new Material(contents4);
		alphaDesaturateMaterial = new Material(Shader.Find("Glui/Texture/ABLEND/Unlit_Color-Desaturate"));
		defaultMaterial = new Material(contents);
		vertColorMaterial = new Material(Shader.Find("Glui/Texture/ABLEND/Unlit/VCOL"));
		textAlphaBlendVCOL4ChannelMateral = new Material(Shader.Find("Glui/Texture4x8bit/ABLEND/Unlit/VCOL"));
		defaultRenderQueue = defaultMaterial.renderQueue;
	}

	public static bool GetCursor(out Vector2 pos, out GluiCursorState state)
	{
		return GetCursor(0, out pos, out state);
	}

	public static bool GetCursor(int index, out Vector2 pos, out GluiCursorState state)
	{
		state = GluiCursorState.None;
		pos = new Vector2(0f, 0f);
		if (InputLocked)
		{
			return false;
		}
		if (index < Input.touchCount)
		{
			Touch touch = Input.touches[index];
			pos = touch.position;
			LastValidCursorPosition = pos;
			if (touch.phase == TouchPhase.Began)
			{
				state = GluiCursorState.Pressed;
			}
			else if (touch.phase == TouchPhase.Ended)
			{
				state = GluiCursorState.Released;
			}
			else if (touch.phase == TouchPhase.Canceled)
			{
				state = GluiCursorState.Released;
			}
			else
			{
				state = GluiCursorState.Held;
			}
			return true;
		}
		if (Input.touchCount > 0)
		{
			pos = LastValidCursorPosition;
			return false;
		}
		if (Input.GetMouseButton(0))
		{
			pos = Input.mousePosition;
			LastValidCursorPosition = pos;
			if (Input.GetMouseButtonDown(0))
			{
				state = GluiCursorState.Pressed;
			}
			else
			{
				state = GluiCursorState.Held;
			}
			return true;
		}
		if (Input.GetMouseButtonUp(0))
		{
			pos = Input.mousePosition;
			LastValidCursorPosition = pos;
			state = GluiCursorState.Released;
			return true;
		}
		pos = LastValidCursorPosition;
		return false;
	}

	public static bool GetCursor(out Vector2 pos)
	{
		GluiCursorState state;
		return GetCursor(out pos, out state);
	}

	public static bool GetCursor(int index, out Vector2 pos)
	{
		GluiCursorState state;
		return GetCursor(index, out pos, out state);
	}

	public static void LockInput(float lockTime)
	{
		inputLockTime = Time.realtimeSinceStartup + lockTime;
	}

	public static GluiMaterialTracker GetSharedMaterial(Texture texture, Color color, Shader shader, int renderQueue = -1)
	{
		if (shader == null)
		{
			return null;
		}
		List<GluiMaterialTracker> value = null;
		if (texture == null)
		{
			value = sNullTextureMaterials;
		}
		GluiMaterialTracker gluiMaterialTracker = null;
		if (texture != null && !sMaterialDictionary.TryGetValue(texture, out value))
		{
			value = new List<GluiMaterialTracker>();
			sMaterialDictionary[texture] = value;
		}
		else
		{
			foreach (GluiMaterialTracker item in value)
			{
				if (item.material != null && (!item.material.HasProperty("_Color") || item.material.color == color) && item.material.shader == shader && ((renderQueue == -1 && item.material.renderQueue == defaultRenderQueue) || item.material.renderQueue == renderQueue))
				{
					gluiMaterialTracker = item;
					break;
				}
			}
		}
		if (gluiMaterialTracker == null)
		{
			gluiMaterialTracker = new GluiMaterialTracker();
			gluiMaterialTracker.material = new Material(shader);
			gluiMaterialTracker.material.mainTexture = texture;
			if (gluiMaterialTracker.material.HasProperty("_Color"))
			{
				gluiMaterialTracker.material.color = color;
			}
			if (renderQueue != -1)
			{
				gluiMaterialTracker.material.renderQueue = renderQueue;
			}
			value.Add(gluiMaterialTracker);
		}
		gluiMaterialTracker.refCount++;
		return gluiMaterialTracker;
	}

	public static void ReleaseSharedMaterial(GluiMaterialTracker tracker)
	{
		tracker.refCount--;
		GluiMaterialTracker gluiMaterialTracker = null;
		List<GluiMaterialTracker> value = null;
		Texture mainTexture = tracker.material.mainTexture;
		if (tracker.refCount == 0)
		{
			gluiMaterialTracker = tracker;
		}
		if (mainTexture == null)
		{
			value = sNullTextureMaterials;
		}
		else
		{
			sMaterialDictionary.TryGetValue(mainTexture, out value);
		}
		if (value != null && gluiMaterialTracker != null)
		{
			value.Remove(gluiMaterialTracker);
			if (Application.isPlaying)
			{
				Object.Destroy(gluiMaterialTracker.material);
			}
			else
			{
				Object.DestroyImmediate(gluiMaterialTracker.material);
			}
			if (mainTexture != null && value.Count == 0)
			{
				sMaterialDictionary.Remove(mainTexture);
			}
		}
	}
}
