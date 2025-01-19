using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
[AddComponentMenu("Glui/Frame")]
public class GluiFrame : GluiWidget
{
	[SerializeField]
	private float leftEdge = 32f;

	[SerializeField]
	private float rightEdge = 32f;

	[SerializeField]
	private float topEdge = 32f;

	[SerializeField]
	private float bottomEdge = 32f;

	[SerializeField]
	private bool hollow;

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

	public bool Hollow
	{
		get
		{
			return hollow;
		}
		set
		{
			hollow = value;
			UpdateFrameMesh();
		}
	}

	protected override void OnResize()
	{
		UpdateFrameMesh();
	}

	protected void UpdateFrameMesh()
	{
		Rect edges = default(Rect);
		edges.xMin = leftEdge;
		edges.xMax = rightEdge;
		edges.yMin = bottomEdge;
		edges.yMax = topEdge;
		Vector3[] verts;
		Vector2[] uvs;
		GetFrameVerts(edges, Texture, out verts, out uvs);
		int[] tris;
		GetFrameFaces(out tris);
		UpdateMesh(verts, uvs, tris);
	}

	protected void GetFrameFaces(out int[] tris)
	{
		if (hollow)
		{
			tris = new int[48];
		}
		else
		{
			tris = new int[54];
		}
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (!hollow || j != 1 || i != 1)
				{
					tris[num] = i * 4 + j;
					tris[num + 1] = (i + 1) * 4 + j;
					tris[num + 2] = i * 4 + (j + 1);
					tris[num + 3] = i * 4 + (j + 1);
					tris[num + 4] = (i + 1) * 4 + j;
					tris[num + 5] = (i + 1) * 4 + (j + 1);
					num += 6;
				}
			}
		}
	}

	protected void GetFrameVerts(Rect edges, Texture2D texture, out Vector3[] verts, out Vector2[] uvs)
	{
		float xMin = edges.xMin;
		float xMax = edges.xMax;
		float yMin = edges.yMin;
		float yMax = edges.yMax;
		float num = base.Size.x / 2f;
		float num2 = base.Size.y / 2f;
		verts = new Vector3[16];
		verts[0] = new Vector3(0f - num, 0f - num2, 0f);
		verts[1] = new Vector3(0f - num + xMin, 0f - num2, 0f);
		verts[2] = new Vector3(num - xMax, 0f - num2, 0f);
		verts[3] = new Vector3(num, 0f - num2, 0f);
		verts[4] = new Vector3(0f - num, 0f - num2 + yMin, 0f);
		verts[5] = new Vector3(0f - num + xMin, 0f - num2 + yMin, 0f);
		verts[6] = new Vector3(num - xMax, 0f - num2 + yMin, 0f);
		verts[7] = new Vector3(num, 0f - num2 + yMin, 0f);
		verts[8] = new Vector3(0f - num, num2 - yMax, 0f);
		verts[9] = new Vector3(0f - num + xMin, num2 - yMax, 0f);
		verts[10] = new Vector3(num - xMax, num2 - yMax, 0f);
		verts[11] = new Vector3(num, num2 - yMax, 0f);
		verts[12] = new Vector3(0f - num, num2, 0f);
		verts[13] = new Vector3(0f - num + xMin, num2, 0f);
		verts[14] = new Vector3(num - xMax, num2, 0f);
		verts[15] = new Vector3(num, num2, 0f);
		uvs = new Vector2[16];
		float x = 0f;
		float num3 = 1f;
		float y = 0f;
		float num4 = 1f;
		if (texture != null)
		{
			x = xMin / (float)texture.width;
			num3 = xMax / (float)texture.width;
			num4 = yMax / (float)texture.height;
			y = yMin / (float)texture.height;
		}
		uvs[0] = new Vector2(0f, 0f);
		uvs[1] = new Vector2(x, 0f);
		uvs[2] = new Vector2(1f - num3, 0f);
		uvs[3] = new Vector2(1f, 0f);
		uvs[4] = new Vector2(0f, y);
		uvs[5] = new Vector2(x, y);
		uvs[6] = new Vector2(1f - num3, y);
		uvs[7] = new Vector2(1f, y);
		uvs[8] = new Vector2(0f, 1f - num4);
		uvs[9] = new Vector2(x, 1f - num4);
		uvs[10] = new Vector2(1f - num3, 1f - num4);
		uvs[11] = new Vector2(1f, 1f - num4);
		uvs[12] = new Vector2(0f, 1f);
		uvs[13] = new Vector2(x, 1f);
		uvs[14] = new Vector2(1f - num3, 1f);
		uvs[15] = new Vector2(1f, 1f);
	}
}
