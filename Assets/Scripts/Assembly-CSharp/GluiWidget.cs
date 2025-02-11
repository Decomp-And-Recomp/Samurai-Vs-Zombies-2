using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Glui/Widget")]
public class GluiWidget : GluiBase, IInputHandler
{
	public enum AnchorType
	{
		None = 0,
		Positive = 1,
		Negative = 2,
		Left = 2,
		Right = 1,
		Bottom = 2,
		Top = 1
	}

	public enum ColliderType
	{
		Manual = 0,
		Auto_None = 1,
		Auto_Box = 2,
		Auto_Sphere = 3,
		Auto_Capsule = 4
	}

	public enum ShaderType
	{
		Manual = 0,
		Auto_GluiAlphaBlend_Default = 1,
		Auto_GluiOpaque = 2,
		Auto_GluiAdditive = 3,
		Auto_GluiAlphaBlend_Desaturate = 4,
		Auto_GluiVertexLit = 5,
		Auto_GluiText_AlphaBlend_VertexColor_4x8bitChannel = 6
	}

	[SerializeField]
	private int layer = -1;

	[SerializeField]
	private bool visible = true;

	[SerializeField]
	private bool autoSize;

	[SerializeField]
	private ColliderType autoCollider;

	[SerializeField]
	private ShaderType autoShader = ShaderType.Auto_GluiAlphaBlend_Default;

	private bool allowInput = true;

	[SerializeField]
	private Vector2 size = new Vector2(100f, 100f);

	[SerializeField]
	private AnchorType anchor;

	[SerializeField]
	private GameObject handler;

	[SerializeField]
	protected GluiTexture gluiTexture = new GluiTexture();

	[SerializeField]
	protected Color color = Color.white;

	[SerializeField]
	protected Color disabledColor = Color.grey;

	[SerializeField]
	protected Material material;

	[SerializeField]
	protected int renderQueue = -1;

	[SerializeField]
	protected Mesh mesh;

	private Mesh meshB;

	private bool useMeshB;

	protected bool isInitialized;

	protected bool ownMesh;

	protected bool usePressedColor;

	protected bool mUsingSharedMaterial;

	public static bool cloneResources = true;

	public static bool inspectorGUIUpdating;

	private GluiMaterialTracker mMaterialTracker;

	public GluiMaterialTracker MaterialTracker
	{
		get
		{
			return mMaterialTracker;
		}
	}

	public bool Usable
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			base.Enabled = value;
		}
	}

	public bool AllowInput
	{
		get
		{
			return base.Enabled && allowInput;
		}
		set
		{
			allowInput = value;
		}
	}

	public Vector2 Size
	{
		get
		{
			return size;
		}
		set
		{
			if (size != value)
			{
				size = value;
				UpdateSize();
				OnResize();
			}
		}
	}

	public ColliderType AutoCollider
	{
		get
		{
			return autoCollider;
		}
		set
		{
			if (autoCollider != value)
			{
				autoCollider = value;
				UpdateSize();
				OnResize();
			}
		}
	}

	public ShaderType AutoShader
	{
		get
		{
			return autoShader;
		}
		set
		{
			if (autoShader != value)
			{
				autoShader = value;
				UpdateMaterial(true);
			}
		}
	}

	public Rect AtlasRect
	{
		get
		{
			return gluiTexture.UVs;
		}
		set
		{
			if (gluiTexture.UVs != value)
			{
				gluiTexture.UVs = value;
				UpdateQuadMesh(size);
			}
		}
	}

	public int Layer
	{
		get
		{
			return layer;
		}
		set
		{
			layer = value;
			int currentValue = value;
			if (currentValue == -1)
			{
				currentValue = GluiSettings.MainLayer;
			}
			base.gameObject.layer = currentValue;
			RecurseOnChildren(delegate(GluiWidget w)
			{
				w.Layer = currentValue;
			});
		}
	}

	public virtual Texture2D Texture
	{
		get
		{
			return gluiTexture.Texture;
		}
		set
		{
			if (gluiTexture.Texture != value)
			{
				gluiTexture.Texture = value;
				UpdateMaterial(false);
				OnTextureChanged();
			}
		}
	}

	public GluiTexture GluiTexture
	{
		get
		{
			return gluiTexture;
		}
		set
		{
			if (gluiTexture != value)
			{
				gluiTexture = value;
				UpdateQuadMesh(size);
				UpdateMaterial(false);
				OnTextureChanged();
			}
		}
	}

	public bool Visible
	{
		get
		{
			InitWidget();
			if (base.GetComponent<Renderer>() != null)
			{
				return base.GetComponent<Renderer>().enabled;
			}
			return false;
		}
		set
		{
			bool flag = visible;
			visible = value;
			UpdateVisible();
			if (visible && !flag)
			{
				OnShow();
			}
			else if (!visible && flag)
			{
				OnHide();
			}
		}
	}

	public bool Autosize
	{
		get
		{
			return autoSize;
		}
		set
		{
			autoSize = value;
		}
	}

	public AnchorType Anchor
	{
		get
		{
			return anchor;
		}
		set
		{
			anchor = value;
		}
	}

	public virtual Color Color
	{
		get
		{
			return color;
		}
		set
		{
			if (color != value)
			{
				color = value;
				UpdateMaterial(false);
			}
		}
	}

	public virtual IEnumerable<Color> Colors
	{
		get
		{
			return new Color[1] { Color };
		}
		set
		{
			if (value != null)
			{
				IEnumerator<Color> enumerator = value.GetEnumerator();
				if (enumerator != null && enumerator.MoveNext())
				{
					Color = enumerator.Current;
				}
			}
		}
	}

	public Color DisabledColor
	{
		get
		{
			return disabledColor;
		}
		set
		{
			if (disabledColor != value)
			{
				disabledColor = value;
				UpdateMaterial(false);
			}
		}
	}

	public GameObject Handler
	{
		get
		{
			return handler;
		}
		set
		{
			handler = value;
		}
	}

	public virtual Vector3 EffectAttachPoint
	{
		get
		{
			return base.transform.position;
		}
	}

	public int RenderQueue
	{
		get
		{
			return renderQueue;
		}
		set
		{
			if (renderQueue != value)
			{
				renderQueue = value;
				UpdateMaterial(false);
			}
		}
	}

	protected Shader DefaultShader
	{
		get
		{
			if (Texture != null && Texture.format == TextureFormat.Alpha8)
			{
				return GluiCore.AlphaOnlyShader;
			}
			switch (autoShader)
			{
			case ShaderType.Auto_GluiAdditive:
				return GluiCore.AdditiveColoredShader;
			case ShaderType.Auto_GluiAlphaBlend_Default:
				return GluiCore.DefaultShader;
			case ShaderType.Auto_GluiOpaque:
				return GluiCore.OpaqueColoredShader;
			case ShaderType.Auto_GluiAlphaBlend_Desaturate:
				return GluiCore.AlphaDesaturate;
			case ShaderType.Auto_GluiVertexLit:
				return GluiCore.VertColorShader;
			case ShaderType.Auto_GluiText_AlphaBlend_VertexColor_4x8bitChannel:
				return GluiCore.TextAlphaBlendVCOL4Channel;
			default:
				return null;
			}
		}
	}

	protected virtual Shader RequiredShader
	{
		get
		{
			return DefaultShader;
		}
	}

	protected virtual ColliderType DefaultColliderType
	{
		get
		{
			return ColliderType.Auto_None;
		}
	}

	public GluiWidget()
	{
		autoCollider = DefaultColliderType;
	}

	public virtual void OnDestroy()
	{
		if (Application.isPlaying && !ApplicationUtilities.HasShutdown)
		{
			if (mMaterialTracker != null && mUsingSharedMaterial)
			{
				GluiCore.ReleaseSharedMaterial(mMaterialTracker);
			}
			if (mesh != null)
			{
				mesh = null;
			}
			if (meshB != null)
			{
				meshB = null;
			}
			material = null;
		}
	}

	protected virtual void OnInit()
	{
	}

	protected virtual void OnResize()
	{
	}

	protected virtual void OnTextureChanged()
	{
	}

	protected virtual void OnDrawDebug()
	{
		DrawDebugRegion();
	}

	protected virtual void OnShow()
	{
	}

	protected virtual void OnHide()
	{
	}

	protected override void OnEnableChanged()
	{
	}

	public void UpdateFlags()
	{
		HideFlags hideFlags = (HideFlags)0;
		if (GluiSettings.HideDetails)
		{
			hideFlags = HideFlags.HideInInspector;
		}
		Component[] components = GetComponents(typeof(Component));
		Component[] array = components;
		foreach (Component component in array)
		{
			if (!(component != null))
			{
				continue;
			}
			component.hideFlags = (HideFlags)0;
			if (component is GluiBase || component is Transform)
			{
				continue;
			}
			if (component is MeshRenderer)
			{
				component.hideFlags = hideFlags;
				MeshRenderer meshRenderer = component as MeshRenderer;
				if (meshRenderer.sharedMaterial != null)
				{
					meshRenderer.sharedMaterial.hideFlags = hideFlags;
				}
			}
			else if (component is MeshFilter)
			{
				component.hideFlags = hideFlags;
			}
			else if (component is Collider)
			{
				component.hideFlags = hideFlags;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		OnDrawDebug();
	}

	private void DrawDebugRegion()
	{
		GluiDebugRenderSupport.DrawDebugRegion(base.GetComponent<Renderer>(), base.transform.localToWorldMatrix, Color.green, size);
	}

	public void RecalculateMesh()
	{
		UpdateQuadMesh(Size);
	}

	protected virtual void UpdateQuadMesh(Vector2 size)
	{
		int[] tris = new int[6] { 0, 2, 1, 0, 3, 2 };
		float num = size.x / 2f;
		float num2 = size.y / 2f;
		UpdateMesh(new Vector3[4]
		{
			new Vector3(0f - num, 0f - num2, 0f),
			new Vector3(num, 0f - num2, 0f),
			new Vector3(num, num2, 0f),
			new Vector3(0f - num, num2, 0f)
		}, new Vector2[4]
		{
			new Vector2(gluiTexture.UVs.x, gluiTexture.UVs.y),
			new Vector2(gluiTexture.UVs.xMax, gluiTexture.UVs.y),
			new Vector2(gluiTexture.UVs.xMax, gluiTexture.UVs.yMax),
			new Vector2(gluiTexture.UVs.x, gluiTexture.UVs.yMax)
		}, tris);
	}

	protected void ClearMesh()
	{
		UpdateMesh(null, null, null, null);
	}

	protected void UpdateMesh(Vector3[] verts, Vector2[] uvs, int[] tris)
	{
		UpdateMesh(verts, uvs, tris, null);
	}

	protected void UpdateMesh(Vector3[] verts, Vector2[] uvs, int[] tris, Color[] cols)
	{
		MeshFilter component = GetComponent<MeshFilter>();
		if (component == null)
		{
			return;
		}
		Mesh mesh = ((!Application.isPlaying || !useMeshB) ? this.mesh : meshB);
		if (mesh != null)
		{
			if (cloneResources && Application.isEditor)
			{
				if (ownMesh)
				{
					ObjectUtils.DestroyImmediate(mesh);
				}
				mesh = new Mesh();
				ownMesh = true;
			}
		}
		else
		{
			mesh = new Mesh();
		}
		mesh.Clear();
		if (verts != null)
		{
			mesh.vertices = verts;
		}
		if (cols != null)
		{
			mesh.colors = cols;
		}
		if (uvs != null)
		{
			mesh.uv = uvs;
		}
		if (tris != null)
		{
			mesh.triangles = tris;
		}
		component.sharedMesh = mesh;
		if (Application.isPlaying)
		{
			if (useMeshB)
			{
				meshB = mesh;
				useMeshB = false;
			}
			else
			{
				this.mesh = mesh;
				useMeshB = true;
			}
		}
		else
		{
			this.mesh = mesh;
		}
	}

	protected virtual void UpdateMaterial(bool replace)
	{
		Color color = ((!base.Enabled) ? disabledColor : this.color);
		int num = ((mMaterialTracker == null) ? 1 : mMaterialTracker.refCount);
		bool flag = material != null && material.HasProperty("_Color");
		if (!replace && material != null && material.mainTexture == Texture && num == 1)
		{
			if (flag)
			{
				material.color = color;
			}
			if (RenderQueue != -1)
			{
				material.renderQueue = RenderQueue;
			}
		}
		else
		{
			if (!replace && !(material == null) && !(material.mainTexture != Texture) && (!flag || !(material.color != color)) && (RenderQueue == -1 || material.renderQueue == RenderQueue))
			{
				return;
			}
			bool flag2 = true;
			if (material != null)
			{
				flag2 = false;
				if (Texture != material.mainTexture || (flag && material.color != color) || RequiredShader != material.shader || (RenderQueue != -1 && material.renderQueue != RenderQueue) || (base.GetComponent<Renderer>() != null && base.GetComponent<Renderer>().sharedMaterial != material))
				{
					if (mMaterialTracker != null)
					{
						GluiCore.ReleaseSharedMaterial(mMaterialTracker);
					}
					mUsingSharedMaterial = false;
					flag2 = true;
				}
			}
			if (!flag2)
			{
				return;
			}
			mMaterialTracker = GluiCore.GetSharedMaterial(Texture, color, RequiredShader, RenderQueue);
			if (mMaterialTracker != null)
			{
				mUsingSharedMaterial = true;
				material = mMaterialTracker.material;
				if (base.GetComponent<Renderer>() != null)
				{
					base.GetComponent<Renderer>().material = mMaterialTracker.material;
				}
			}
		}
	}

	public override void Refresh()
	{
		UpdateMaterial(false);
		UpdateSize();
	}

	public void Refresh_Full()
	{
		UpdateMaterial(true);
		UpdateSize();
	}

	protected void InitWidget()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			if (layer != -1)
			{
				base.gameObject.layer = layer;
			}
			else
			{
				base.gameObject.layer = GluiSettings.MainLayer;
			}
			AdjustAnchoring();
			if (Texture == null && base.GetComponent<Renderer>() != null && base.GetComponent<Renderer>().sharedMaterial != null && base.GetComponent<Renderer>().sharedMaterial.HasProperty("_MainTex"))
			{
				Texture = base.GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D;
			}
			if (base.GetComponent<Renderer>() != null)
			{
				base.GetComponent<Renderer>().castShadows = false;
				base.GetComponent<Renderer>().receiveShadows = false;
			}
			UpdateMaterial(true);
			UpdateSize();
			UpdateVisible();
			UpdateFlags();
			OnInit();
			MeshErrorTest();
		}
	}

	protected virtual void MeshErrorTest()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		if (component != null && component.sharedMesh == null)
		{
			OnResize();
		}
	}

	protected virtual void UpdateSize()
	{
		switch (AutoCollider)
		{
		case ColliderType.Auto_None:
		{
			Component component = GetComponent(typeof(Collider));
			if (component != null)
			{
				UnityEngine.Object.DestroyImmediate(component);
			}
			break;
		}
		case ColliderType.Auto_Box:
		{
			BoxCollider boxCollider = ObjectUtils.ForceComponentExists_AndDestroyOthers<BoxCollider, Collider>(base.gameObject);
			boxCollider.isTrigger = true;
			boxCollider.center = Vector3.zero;
			boxCollider.extents = new Vector3(Size.x, Size.y, 1f) / 2f;
			break;
		}
		case ColliderType.Auto_Sphere:
		{
			SphereCollider sphereCollider = ObjectUtils.ForceComponentExists_AndDestroyOthers<SphereCollider, Collider>(base.gameObject);
			sphereCollider.isTrigger = true;
			sphereCollider.center = Vector3.zero;
			sphereCollider.radius = ((!(Size.x < Size.y)) ? Size.y : Size.x) / 2f;
			break;
		}
		case ColliderType.Auto_Capsule:
		{
			CapsuleCollider capsuleCollider = ObjectUtils.ForceComponentExists_AndDestroyOthers<CapsuleCollider, Collider>(base.gameObject);
			capsuleCollider.isTrigger = true;
			capsuleCollider.center = Vector3.zero;
			if (Size.y > Size.x)
			{
				capsuleCollider.direction = 1;
				capsuleCollider.height = Size.y;
				capsuleCollider.radius = Size.x / 2f;
			}
			else
			{
				capsuleCollider.direction = 0;
				capsuleCollider.height = Size.x;
				capsuleCollider.radius = Size.y / 2f;
			}
			break;
		}
		}
		MeshFilter component2 = GetComponent<MeshFilter>();
		if (component2 != null && component2.sharedMesh != null)
		{
			Bounds bounds = default(Bounds);
			bounds.center = new Vector3(0f, 0f, 0f);
			bounds.extents = new Vector3(Size.x, Size.y, 1f) / 2f;
			component2.sharedMesh.bounds = bounds;
		}
	}

	private void UpdateVisible()
	{
		if (base.GetComponent<Renderer>() != null)
		{
			base.GetComponent<Renderer>().enabled = visible;
		}
	}

	private void RecurseEnabled(bool enabled)
	{
		if (Application.isPlaying)
		{
			RecurseOnChildren(delegate(GluiWidget w)
			{
				w.Enabled = enabled;
			});
		}
	}

	private void RecurseOnChildren(Action<GluiWidget> a)
	{
		GluiWidget[] componentsInChildren = base.gameObject.GetComponentsInChildren<GluiWidget>();
		GluiWidget[] array = componentsInChildren;
		foreach (GluiWidget gluiWidget in array)
		{
			if (gluiWidget != this)
			{
				a(gluiWidget);
			}
		}
	}

	private void AdjustAnchoring()
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			return;
		}
		if (anchor != 0)
		{
			Vector3 vector = GluiCore.AnchorOffset;
			if (anchor == AnchorType.Positive)
			{
				base.transform.position -= vector;
			}
			else if (anchor == AnchorType.Negative)
			{
				base.transform.position += vector;
			}
		}
		if (Autosize)
		{
			Vector2 vector2 = Size;
			vector2.x += GluiCore.AnchorOffset.x * 2f;
			vector2.y += GluiCore.AnchorOffset.y * 2f;
			Size = vector2;
		}
	}

	public virtual List<GluiTexture> GetGluiTextures()
	{
		List<GluiTexture> list = new List<GluiTexture>();
		list.Add(gluiTexture);
		return list;
	}

	private IEnumerator ApplyTextureWhenInitialized(string dataKey)
	{
		while (DataBundleRuntime.Instance == null)
		{
			yield return null;
		}
		ApplyGluiAtlasedTexture(dataKey);
	}

	public void ApplyGluiAtlasedTexture(string atlasKey)
	{
		if (string.IsNullOrEmpty(atlasKey))
		{
			ApplyGluiAtlasedTexture(null, null);
			return;
		}
		if (DataBundleRuntime.Instance == null)
		{
			StartCoroutine(ApplyTextureWhenInitialized(atlasKey));
			return;
		}
		gluiTexture.ApplyGluiAtlasedTexture(atlasKey, delegate
		{
			UpdateQuadMesh(size);
			Refresh();
		});
	}

	public void ApplyGluiAtlasedTexture(GluiAtlasedTextureSchema atlasedTexture, string atlasKey)
	{
		gluiTexture.ApplyGluiAtlasedTexture(atlasedTexture, atlasKey, delegate
		{
			UpdateQuadMesh(size);
			Refresh();
		});
	}

	public override void Start()
	{
		InitWidget();
		base.Start();
	}

	public virtual void HandleInput(InputCrawl inputCrawl, out InputRouter.InputResponse response)
	{
		response = InputRouter.InputResponse.Passthrough;
	}
}
