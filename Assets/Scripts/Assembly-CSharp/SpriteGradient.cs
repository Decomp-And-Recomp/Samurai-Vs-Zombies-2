using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SpriteGradient : GluiWidget
{
	public enum trisSets
	{
		TopLeft_BottomRight = 0,
		TopRight_BottomLeft = 1
	}

	public Color topLeft = Color.red;

	public Color topRight = Color.green;

	public Color bottomLeft = Color.blue;

	public Color bottomRight = Color.yellow;

	public trisSets triangulation;

	public override IEnumerable<Color> Colors
	{
		get
		{
			return new Color[4] { topLeft, topRight, bottomLeft, bottomRight };
		}
		set
		{
			if (value == null)
			{
				return;
			}
			IEnumerator<Color> enumerator = value.GetEnumerator();
			if (enumerator != null)
			{
				if (enumerator.MoveNext())
				{
					topLeft = enumerator.Current;
				}
				if (enumerator.MoveNext())
				{
					topRight = enumerator.Current;
				}
				if (enumerator.MoveNext())
				{
					bottomLeft = enumerator.Current;
				}
				if (enumerator.MoveNext())
				{
					bottomRight = enumerator.Current;
				}
				UpdateMaterial(false);
			}
		}
	}

	protected override ColliderType DefaultColliderType
	{
		get
		{
			return ColliderType.Auto_None;
		}
	}

	protected override Shader RequiredShader
	{
		get
		{
			return null;
		}
	}

	protected override void UpdateQuadMesh(Vector2 size)
	{
		float x = size.x;
		float y = size.y;
		float num = x / 2f;
		float num2 = y / 2f;
		Vector3[] verts = new Vector3[4]
		{
			new Vector3(0f - num, num2, 0f),
			new Vector3(num, num2, 0f),
			new Vector3(0f - num, 0f - num2, 0f),
			new Vector3(num, 0f - num2, 0f)
		};
		Color[] cols = new Color[4] { topLeft, topRight, bottomLeft, bottomRight };
		Vector2[] uvs = new Vector2[4]
		{
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(0f, 0f),
			new Vector3(1f, 0f)
		};
		int[] tris = ((triangulation != trisSets.TopRight_BottomLeft) ? new int[6] { 3, 2, 0, 3, 0, 1 } : new int[6] { 0, 1, 2, 2, 1, 3 });
		UpdateMesh(verts, uvs, tris, cols);
	}

	protected override void OnResize()
	{
		UpdateQuadMesh(base.Size);
	}

	protected override void UpdateMaterial(bool replace)
	{
		base.UpdateMaterial(replace);
		UpdateQuadMesh(base.Size);
	}
}
