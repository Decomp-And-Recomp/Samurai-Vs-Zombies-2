using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Glui/RenderTarget")]
public class GluiRenderTarget : GluiSprite
{
	[SerializeField]
	private Camera sourceCamera;

	[SerializeField]
	private int targetDepth = 24;

	[SerializeField]
	private RenderTextureFormat targetFormat;

	private RenderTexture renderTexture;

	public string actionOnRender;

	private bool bCreated;

	public Camera SourceCamera
	{
		get
		{
			return sourceCamera;
		}
		set
		{
			sourceCamera = value;
		}
	}

	public int TargetDepth
	{
		get
		{
			return targetDepth;
		}
		set
		{
			if (targetDepth != value)
			{
				targetDepth = value;
				CreateRenderTexture();
			}
		}
	}

	public RenderTextureFormat TargetFormat
	{
		get
		{
			return targetFormat;
		}
		set
		{
			if (targetFormat != value)
			{
				targetFormat = value;
				CreateRenderTexture();
			}
		}
	}

	public override Texture2D Texture
	{
		set
		{
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		CreateRenderTexture();
	}

	protected override void OnResize()
	{
		base.OnResize();
		CreateRenderTexture();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		DestroyRenderTexture();
	}

	protected override void UpdateMaterial(bool replace)
	{
		base.UpdateMaterial(replace);
		if (base.GetComponent<Renderer>() != null && base.GetComponent<Renderer>().sharedMaterial != null)
		{
			base.GetComponent<Renderer>().sharedMaterial.mainTexture = renderTexture;
			base.GetComponent<Renderer>().sharedMaterial.color = color;
		}
	}

	private void CreateRenderTexture()
	{
		if (!bCreated)
		{
			bCreated = true;
			DestroyRenderTexture();
			renderTexture = new RenderTexture((int)base.Size.x, (int)base.Size.y, targetDepth, targetFormat);
			if (!(renderTexture != null) || renderTexture.Create())
			{
			}
			if (base.GetComponent<Renderer>() != null && base.GetComponent<Renderer>().sharedMaterial != null && base.GetComponent<Renderer>().sharedMaterial.mainTexture != renderTexture)
			{
				base.GetComponent<Renderer>().sharedMaterial.mainTexture = renderTexture;
			}
		}
	}

	private void DestroyRenderTexture()
	{
		if (renderTexture != null)
		{
			if (renderTexture.IsCreated())
			{
				renderTexture.Release();
			}
			ObjectUtils.DestroyImmediate(renderTexture);
			renderTexture = null;
			if (sourceCamera != null)
			{
				sourceCamera.targetTexture = null;
			}
		}
	}

	private void OnWillRenderObject()
	{
		if (base.Visible && base.Enabled)
		{
			Render();
		}
	}

	public void Render()
	{
		if (!(renderTexture == null) && !(sourceCamera == null))
		{
			if (sourceCamera.targetTexture != null)
			{
				sourceCamera.targetTexture.Release();
				sourceCamera.targetTexture = null;
			}
			sourceCamera.targetTexture = renderTexture;
			sourceCamera.Render();
			sourceCamera.targetTexture.DiscardContents();
			sourceCamera.targetTexture = null;
			GluiActionSender.SendGluiAction(actionOnRender, base.gameObject, null);
		}
	}
}
