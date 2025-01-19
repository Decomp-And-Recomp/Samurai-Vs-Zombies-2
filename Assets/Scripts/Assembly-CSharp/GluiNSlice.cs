using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Glui/N-Slice")]
[ExecuteInEditMode]
public class GluiNSlice : GluiWidget
{
	[Serializable]
	public class Slice
	{
		public Texture2D texture;

		public GluiTexture gluiTexture = new GluiTexture();

		public byte textureIndex;

		public short quadCount = 1;

		public float xs;

		public float xe;

		public float ys;

		public float ye;

		public float us;

		public float ue;

		public float vs;

		public float ve;

		public float tileX;

		public float tileY;

		public bool linked = true;

		public bool tiled = true;

		public bool clampCapUVs;
	}

	[SerializeField]
	private int xSlices = 3;

	[SerializeField]
	private int ySlices = 3;

	[SerializeField]
	private float leftEdge = 32f;

	[SerializeField]
	private float rightEdge = 32f;

	[SerializeField]
	private float topEdge = 32f;

	[SerializeField]
	private float bottomEdge = 32f;

	private Slice[,] slices;

	[SerializeField]
	private Slice[] serializedSlices;

	private GluiMaterialTracker[] materialTrackers;

	protected bool ownMaterial;

	public float LeftEdge
	{
		get
		{
			return leftEdge;
		}
		set
		{
			leftEdge = value;
			UpdateFrameMesh();
		}
	}

	public float RightEdge
	{
		get
		{
			return rightEdge;
		}
		set
		{
			rightEdge = value;
			UpdateFrameMesh();
		}
	}

	public float TopEdge
	{
		get
		{
			return topEdge;
		}
		set
		{
			topEdge = value;
			UpdateFrameMesh();
		}
	}

	public float BottomEdge
	{
		get
		{
			return bottomEdge;
		}
		set
		{
			bottomEdge = value;
			UpdateFrameMesh();
		}
	}

	public int XSlices
	{
		get
		{
			return xSlices;
		}
	}

	public int YSlices
	{
		get
		{
			return ySlices;
		}
	}

	public Slice[,] Slices
	{
		get
		{
			return slices;
		}
		set
		{
			slices = value;
			UpdateFrameMesh();
		}
	}

	protected override void OnCreate()
	{
		UpdateFrameMesh();
	}

	protected override void OnResize()
	{
		UpdateFrameMesh();
	}

	protected override void OnTextureChanged()
	{
		UpdateFrameMesh();
	}

	protected override void UpdateQuadMesh(Vector2 size)
	{
		UpdateFrameMesh();
	}

	public override void OnDestroy()
	{
		if (Application.isPlaying)
		{
			DestroyMaterials(true);
		}
		base.OnDestroy();
	}

	public bool IsCorner(int sliceX, int sliceY)
	{
		if (sliceX == 0 && sliceY == 0)
		{
			return true;
		}
		if (sliceX == XSlices - 1 && sliceY == 0)
		{
			return true;
		}
		if (sliceX == 0 && sliceY == YSlices - 1)
		{
			return true;
		}
		if (sliceX == XSlices - 1 && sliceY == YSlices - 1)
		{
			return true;
		}
		return false;
	}

	public bool IsEdge(int sliceX, int sliceY)
	{
		if (IsCorner(sliceX, sliceY))
		{
			return false;
		}
		if (sliceX == 0)
		{
			return true;
		}
		if (sliceX == XSlices - 1)
		{
			return true;
		}
		if (sliceY == 0)
		{
			return true;
		}
		if (sliceY == YSlices - 1)
		{
			return true;
		}
		return false;
	}

	public bool IsCenter(int sliceX, int sliceY)
	{
		if (IsCorner(sliceX, sliceY))
		{
			return false;
		}
		if (IsEdge(sliceX, sliceY))
		{
			return false;
		}
		return true;
	}

	public void AllocateSlices(int width, int height)
	{
		xSlices = width;
		ySlices = height;
		float num = 1f / (float)XSlices;
		float num2 = 1f / (float)YSlices;
		int num3 = 0;
		slices = new Slice[XSlices, YSlices];
		serializedSlices = new Slice[XSlices * YSlices];
		for (int i = 0; i < XSlices; i++)
		{
			for (int j = 0; j < YSlices; j++)
			{
				Slice slice = new Slice();
				slice.us = (slice.xs = (float)i * num);
				slice.vs = (slice.ys = (float)j * num2);
				slice.ue = (slice.xe = (float)(i + 1) * num);
				slice.ve = (slice.ye = (float)(j + 1) * num2);
				slice.tileX = Mathf.Round((slice.xe - slice.xs) * base.Size.x);
				slice.tileY = Mathf.Round((slice.ye - slice.ys) * base.Size.y);
				if (i == 0)
				{
					leftEdge = slice.xe * base.Size.x;
				}
				if (j == 0)
				{
					bottomEdge = slice.ye * base.Size.y;
				}
				if (i == XSlices - 1)
				{
					rightEdge = base.Size.x - slice.xs * base.Size.x;
				}
				if (j == YSlices - 1)
				{
					topEdge = base.Size.y - slice.ys * base.Size.y;
				}
				slices[i, j] = slice;
				serializedSlices[num3] = slice;
				num3++;
			}
		}
	}

	private bool DeserializeSlices()
	{
		if (serializedSlices == null)
		{
			return false;
		}
		slices = new Slice[XSlices, YSlices];
		int num = 0;
		for (int i = 0; i < XSlices; i++)
		{
			for (int j = 0; j < YSlices; j++)
			{
				slices[i, j] = serializedSlices[num];
				num++;
			}
		}
		return true;
	}

	public void UpdateFrameMesh()
	{
		if (slices == null && !DeserializeSlices())
		{
			AllocateSlices(XSlices, YSlices);
		}
		MeshFilter component = GetComponent<MeshFilter>();
		Vector3[] verts;
		Vector2[] uvs;
		int[][] faces;
		if (component == null || !GetFrameMesh(Texture, out verts, out uvs, out faces))
		{
			return;
		}
		if (mesh != null)
		{
			if (GluiWidget.cloneResources && Application.isEditor)
			{
				if (ownMesh)
				{
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(mesh);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(mesh);
					}
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
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.subMeshCount = faces.Length;
		for (int i = 0; i < faces.Length; i++)
		{
			mesh.SetTriangles(faces[i], i);
		}
		component.sharedMesh = mesh;
		UpdateFrameMaterials(true);
	}

	public bool ClampSize()
	{
		Vector2 vector = base.Size;
		if (vector.x < rightEdge + leftEdge)
		{
			vector.x = rightEdge + leftEdge;
		}
		if (vector.y < topEdge + bottomEdge)
		{
			vector.y = topEdge + bottomEdge;
		}
		if (vector != base.Size)
		{
			base.Size = vector;
			return true;
		}
		return false;
	}

	protected bool GetFrameMesh(Texture2D texture, out Vector3[] verts, out Vector2[] uvs, out int[][] faces)
	{
		verts = null;
		uvs = null;
		faces = null;
		if (slices == null)
		{
			return false;
		}
		List<Texture2D> list = FindUniqueTextures();
		if (list.Count == 0)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < XSlices; i++)
		{
			for (int j = 0; j < YSlices; j++)
			{
				Slice slice = slices[i, j];
				if (slice.tiled)
				{
					float sx;
					float sy;
					float ex;
					float ey;
					GetSliceCorners(slice, i, j, out sx, out sy, out ex, out ey);
					float num2 = Mathf.Abs(ex - sx);
					float num3 = Mathf.Abs(ey - sy);
					int num4 = Mathf.CeilToInt(num2 / slice.tileX);
					int num5 = Mathf.CeilToInt(num3 / slice.tileY);
					slice.quadCount = (short)(num4 * num5);
					num += slice.quadCount;
				}
				else
				{
					num++;
				}
			}
		}
		int num6 = num * 4;
		verts = new Vector3[num6];
		uvs = new Vector2[num6];
		faces = new int[list.Count][];
		for (int k = 0; k < list.Count; k++)
		{
			int num7 = 0;
			for (int l = 0; l < XSlices; l++)
			{
				for (int m = 0; m < YSlices; m++)
				{
					if (slices[l, m].gluiTexture.Texture == null)
					{
						slices[l, m].textureIndex = 0;
						num7 += slices[l, m].quadCount;
					}
					else if (slices[l, m].gluiTexture.Texture == list[k])
					{
						slices[l, m].textureIndex = (byte)k;
						num7 += slices[l, m].quadCount;
					}
				}
			}
			faces[k] = new int[num7 * 6];
		}
		int num8 = 0;
		int[] array = new int[list.Count];
		for (int n = 0; n < XSlices; n++)
		{
			for (int num9 = 0; num9 < YSlices; num9++)
			{
				Slice slice2 = slices[n, num9];
				Rect uVs = base.GluiTexture.UVs;
				if (slice2.gluiTexture.Texture != null)
				{
					uVs = slice2.gluiTexture.UVs;
				}
				float sx2;
				float sy2;
				float ex2;
				float ey2;
				GetSliceCorners(slice2, n, num9, out sx2, out sy2, out ex2, out ey2);
				float num10 = sy2;
				bool flag = false;
				if (slice2.tiled && !IsCorner(n, num9))
				{
					flag = true;
				}
				float num11 = ex2 - sx2;
				float num12 = ey2 - sy2;
				float num13 = num11;
				float num14 = num12;
				if (flag)
				{
					if (n > 0 && n < XSlices - 1)
					{
						num13 = slice2.tileX;
					}
					if (num9 > 0 && num9 < YSlices - 1)
					{
						num14 = slice2.tileY;
					}
				}
				float num15 = num13;
				float num16 = num14;
				if (!slice2.clampCapUVs)
				{
					int num17 = Mathf.CeilToInt((ex2 - sx2) / slice2.tileX);
					int num18 = Mathf.CeilToInt((ey2 - sy2) / slice2.tileY);
					num15 = num11 / (float)num17;
					num16 = num12 / (float)num18;
				}
				float num19 = 0f;
				for (float num20 = 0f; num20 < num11; num20 += num13)
				{
					bool flag2 = sx2 + num13 >= ex2;
					float num21 = slice2.ue;
					if (flag2 && flag && slice2.clampCapUVs && n > 0 && n <= XSlices - 1)
					{
						float num22 = (ex2 - (sx2 + num13)) / num13;
						num22 -= (float)Mathf.FloorToInt(num22);
						num21 = slice2.us + (slice2.ue - slice2.us) * num22;
					}
					sy2 = num10;
					float num23 = 0f;
					for (float num24 = 0f; num24 < num12; num24 += num14)
					{
						bool flag3 = sy2 + num14 >= ey2;
						float num25 = slice2.ve;
						if (flag3 && flag && slice2.clampCapUVs && num9 > 0 && num9 <= YSlices - 1)
						{
							float num26 = (ey2 - (sy2 + num14)) / num14;
							num26 -= (float)Mathf.FloorToInt(num26);
							num25 = slice2.vs + (slice2.ve - slice2.vs) * num26;
						}
						int textureIndex = slice2.textureIndex;
						faces[textureIndex][array[textureIndex]] = num8;
						faces[textureIndex][array[textureIndex] + 1] = num8 + 2;
						faces[textureIndex][array[textureIndex] + 2] = num8 + 1;
						faces[textureIndex][array[textureIndex] + 3] = num8;
						faces[textureIndex][array[textureIndex] + 4] = num8 + 3;
						faces[textureIndex][array[textureIndex] + 5] = num8 + 2;
						array[textureIndex] += 6;
						verts[num8].x = sx2;
						verts[num8].y = sy2;
						verts[num8].z = 0f;
						uvs[num8].x = slice2.us * uVs.width + uVs.x;
						uvs[num8].y = slice2.vs * uVs.height + uVs.y;
						num8++;
						verts[num8].x = ((!flag2) ? (sx2 + num15) : ex2);
						verts[num8].y = sy2;
						verts[num8].z = 0f;
						uvs[num8].x = num21 * uVs.width + uVs.x;
						uvs[num8].y = slice2.vs * uVs.height + uVs.y;
						num8++;
						verts[num8].x = ((!flag2) ? (sx2 + num15) : ex2);
						verts[num8].y = ((!flag3) ? (sy2 + num16) : ey2);
						verts[num8].z = 0f;
						uvs[num8].x = num21 * uVs.width + uVs.x;
						uvs[num8].y = num25 * uVs.height + uVs.y;
						num8++;
						verts[num8].x = sx2;
						verts[num8].y = ((!flag3) ? (sy2 + num16) : ey2);
						verts[num8].z = 0f;
						uvs[num8].x = slice2.us * uVs.width + uVs.x;
						uvs[num8].y = num25 * uVs.height + uVs.y;
						num8++;
						sy2 += num16;
						num23 += 1f;
					}
					sx2 += num15;
					num19 += 1f;
				}
			}
		}
		return true;
	}

	private void GetSliceCorners(Slice s, int sliceX, int sliceY, out float sx, out float sy, out float ex, out float ey)
	{
		float x = base.Size.x;
		float y = base.Size.y;
		int num = XSlices - 1;
		int num2 = YSlices - 1;
		sx = 0f;
		ex = x;
		sy = 0f;
		ey = y;
		if (YSlices == 2)
		{
			if (sliceX == 0)
			{
				sx = 0f;
			}
			else
			{
				sx = 0.5f * x;
			}
			if (sliceX == 0)
			{
				ex = 0.5f * x;
			}
			else
			{
				ex = x;
			}
		}
		else if (XSlices > 2)
		{
			if (sliceX == 1)
			{
				sx = leftEdge;
			}
			else if (sliceX == num)
			{
				sx = x - rightEdge;
			}
			else
			{
				sx = s.xs * x;
			}
			if (sliceX == 0)
			{
				ex = leftEdge;
			}
			else if (sliceX == num - 1)
			{
				ex = x - rightEdge;
			}
			else
			{
				ex = s.xe * x;
			}
		}
		if (YSlices == 2)
		{
			if (sliceY == 0)
			{
				sy = 0f;
			}
			else
			{
				sy = 0.5f * y;
			}
			if (sliceY == 0)
			{
				ey = 0.5f * y;
			}
			else
			{
				ey = y;
			}
		}
		else if (YSlices > 2)
		{
			if (sliceY == 1)
			{
				sy = bottomEdge;
			}
			else if (sliceY == num2)
			{
				sy = y - topEdge;
			}
			else
			{
				sy = s.ys * y;
			}
			if (sliceY == 0)
			{
				ey = bottomEdge;
			}
			else if (sliceY == num2 - 1)
			{
				ey = y - topEdge;
			}
			else
			{
				ey = s.ye * y;
			}
		}
		sx -= x / 2f;
		sy -= y / 2f;
		ex -= x / 2f;
		ey -= y / 2f;
	}

	protected override void UpdateMaterial(bool replace)
	{
		UpdateFrameMaterials(true);
	}

	private void UpdateFrameMaterials(bool replace)
	{
		bool flag = materialTrackers == null;
		if (!flag)
		{
			GluiMaterialTracker[] array = materialTrackers;
			foreach (GluiMaterialTracker gluiMaterialTracker in array)
			{
				if (gluiMaterialTracker == null)
				{
					flag = true;
					break;
				}
			}
		}
		bool flag2 = false;
		if (replace || flag)
		{
			flag2 = true;
			if (!flag && GluiWidget.cloneResources && Application.isEditor)
			{
				if (ownMaterial)
				{
					if (Application.isPlaying)
					{
						DestroyMaterials(false);
					}
					else
					{
						DestroyMaterials(true);
					}
				}
				ownMaterial = true;
			}
		}
		if (flag2)
		{
			CreateMaterials();
		}
		if (materialTrackers != null && base.GetComponent<Renderer>() != null)
		{
			Material[] array2 = new Material[materialTrackers.Length];
			for (int j = 0; j < materialTrackers.Length; j++)
			{
				array2[j] = materialTrackers[j].material;
			}
			if (!MaterialArraysEqual(base.GetComponent<Renderer>().sharedMaterials, array2))
			{
				base.GetComponent<Renderer>().sharedMaterials = array2;
			}
		}
	}

	private bool MaterialArraysEqual(Material[] array1, Material[] array2)
	{
		if (array1.Length != array2.Length)
		{
			return false;
		}
		for (int i = 0; i < array1.Length; i++)
		{
			if (array1[i] != array2[i])
			{
				return false;
			}
		}
		return true;
	}

	private List<Texture2D> FindUniqueTextures()
	{
		if (slices == null)
		{
			UpdateFrameMesh();
		}
		if (slices == null)
		{
			return new List<Texture2D>();
		}
		List<Texture2D> list = new List<Texture2D>();
		if (Texture != null)
		{
			list.Add(Texture);
		}
		for (int i = 0; i < XSlices; i++)
		{
			for (int j = 0; j < YSlices; j++)
			{
				Slice slice = slices[i, j];
				if (slice.gluiTexture.Texture != null && !list.Contains(slice.gluiTexture.Texture))
				{
					list.Add(slice.gluiTexture.Texture);
				}
			}
		}
		return list;
	}

	private void CreateMaterials()
	{
		if (slices == null)
		{
			UpdateFrameMesh();
			return;
		}
		List<Texture2D> list = FindUniqueTextures();
		if (list != null && list.Count > 0)
		{
			DestroyMaterials(true);
			materialTrackers = new GluiMaterialTracker[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				materialTrackers[i] = GluiCore.GetSharedMaterial(list[i], color, GluiCore.DefaultShader, base.RenderQueue);
			}
		}
	}

	private void DestroyMaterials(bool immediate)
	{
		if (materialTrackers == null)
		{
			return;
		}
		for (int i = 0; i < materialTrackers.Length; i++)
		{
			if (materialTrackers[i] != null)
			{
				GluiCore.ReleaseSharedMaterial(materialTrackers[i]);
			}
		}
		materialTrackers = null;
	}

	protected override void Awake()
	{
		base.Awake();
		RemoveOldTextureVariable();
	}

	public override void Start()
	{
		base.Start();
	}

	protected override void Reset()
	{
		base.Reset();
	}

	private void RemoveOldTextureVariable()
	{
		if (serializedSlices == null)
		{
			return;
		}
		for (int i = 0; i < serializedSlices.Length; i++)
		{
			if (serializedSlices[i].texture != null)
			{
				if (serializedSlices[i].gluiTexture == null)
				{
					serializedSlices[i].gluiTexture = new GluiTexture();
				}
				serializedSlices[i].gluiTexture.Texture = serializedSlices[i].texture;
				serializedSlices[i].texture = null;
			}
		}
	}

	public override List<GluiTexture> GetGluiTextures()
	{
		List<GluiTexture> list = new List<GluiTexture>();
		if (gluiTexture != null)
		{
			list.Add(gluiTexture);
		}
		if (serializedSlices != null)
		{
			for (int i = 0; i < serializedSlices.Length; i++)
			{
				if (!list.Contains(serializedSlices[i].gluiTexture))
				{
					list.Add(serializedSlices[i].gluiTexture);
				}
			}
		}
		if (slices != null)
		{
			for (int j = 0; j < XSlices; j++)
			{
				for (int k = 0; k < YSlices; k++)
				{
					Slice slice = slices[j, k];
					if (!list.Contains(slice.gluiTexture))
					{
						list.Add(slice.gluiTexture);
					}
				}
			}
		}
		return list;
	}
}
