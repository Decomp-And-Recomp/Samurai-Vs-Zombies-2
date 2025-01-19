using System;
using UnityEngine;

[Serializable]
public class GluiTexture
{
	public Texture2D Texture;

	public Rect UVs = new Rect(0f, 0f, 1f, 1f);

	public string AtlasRecordKey;

	[SerializeField]
	private string sourceAssetPath;

	private GluiAtlasedTextureSchema atlasData;

	private DataBundleRecordHandle<GluiAtlasedTextureSchema> atlasDataHandle;

	public Vector2 TextureOriginalSize
	{
		get
		{
			if (!string.IsNullOrEmpty(AtlasRecordKey) && atlasData != null)
			{
				return new Vector2(atlasData.ActualSizeX, atlasData.ActualSizeY);
			}
			if (Texture != null)
			{
				return new Vector2(Texture.width, Texture.height);
			}
			return Vector2.zero;
		}
	}

	public string OriginalAssetPath
	{
		get
		{
			if (atlasData == null)
			{
				return sourceAssetPath;
			}
			return atlasData.SourceAssetPath;
		}
	}

	public void ApplyGluiAtlasedTexture(string atlasRecordKey, Action onComplete)
	{
		AtlasRecordKey = atlasRecordKey;
		RefreshAtlasData(onComplete);
	}

	private void RefreshAtlasData(Action onComplete)
	{
		if (!string.IsNullOrEmpty(AtlasRecordKey) && DataBundleRuntime.Instance != null)
		{
			if (atlasDataHandle != null)
			{
				atlasDataHandle.Dispose();
				atlasDataHandle = null;
			}
			atlasData = DataBundleRuntime.Instance.InitializeRecord<GluiAtlasedTextureSchema>(AtlasRecordKey);
			string value = DataBundleRuntime.Instance.GetValue<string>(typeof(GluiAtlasedTextureSchema), AtlasRecordKey, "3", true);
			atlasData.AtlasTexture = SharedResourceLoader.LoadAsset(value).Resource as Texture2D;
			ApplyGluiAtlasedTexture(atlasData, AtlasRecordKey, onComplete);
		}
	}

	public void FastRefreshAtlasRect()
	{
		if (!string.IsNullOrEmpty(AtlasRecordKey) && DataBundleRuntime.Instance != null)
		{
			atlasData = DataBundleRuntime.Instance.InitializeRecord<GluiAtlasedTextureSchema>(AtlasRecordKey);
			UVs = atlasData.AtlasRect;
			atlasData.AtlasTexture = Texture;
		}
	}

	public void ApplyGluiAtlasedTexture(GluiAtlasedTextureSchema data, string atlasRecordKey, Action onComplete)
	{
		AtlasRecordKey = atlasRecordKey;
		atlasData = data;
		if (atlasData == null)
		{
			Reset();
		}
		else
		{
			Texture = atlasData.AtlasTexture;
			UVs = atlasData.AtlasRect;
			sourceAssetPath = atlasData.SourceAssetPath;
		}
		if (onComplete != null)
		{
			onComplete();
		}
	}

	public void ApplyNormalTexture(Texture2D tex)
	{
		Reset();
		Texture = tex;
	}

	public void Reset()
	{
		Texture = null;
		UVs = new Rect(0f, 0f, 1f, 1f);
		sourceAssetPath = null;
		AtlasRecordKey = null;
		atlasData = null;
	}
}
