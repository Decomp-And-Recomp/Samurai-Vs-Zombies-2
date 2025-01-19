using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TriangleGradient : GluiWidget
{
	public enum anchorSet
	{
		Tip = 0,
		Center = 1,
		Base = 2
	}

	public Color tipColor = Color.white;

	public anchorSet gradientAnchor;

	public override IEnumerable<Color> Colors
	{
		get
		{
			return new Color[2] { Color, tipColor };
		}
		set
		{
			if (value == null)
			{
				return;
			}
			IEnumerator<Color> enumerator = value.GetEnumerator();
			if (enumerator != null && enumerator.MoveNext())
			{
				color = enumerator.Current;
				if (enumerator.MoveNext())
				{
					tipColor = enumerator.Current;
				}
				UpdateMaterial(false);
			}
		}
	}

	protected override Shader RequiredShader
	{
		get
		{
			return null;
		}
	}

	protected override ColliderType DefaultColliderType
	{
		get
		{
			return ColliderType.Auto_None;
		}
	}

	protected override void UpdateQuadMesh(Vector2 size)
	{
		float x = size.x;
		float y = size.y;
		float num = x / 2f;
		Vector3[] verts;
		if (gradientAnchor == anchorSet.Tip)
		{
			verts = new Vector3[3]
			{
				new Vector3(0f, 0f, 0f),
				new Vector3(0f - num, 0f - y, 0f),
				new Vector3(num, 0f - y, 0f)
			};
		}
		else if (gradientAnchor == anchorSet.Base)
		{
			verts = new Vector3[3]
			{
				new Vector3(0f, y, 0f),
				new Vector3(0f - num, 0f, 0f),
				new Vector3(num, 0f, 0f)
			};
		}
		else
		{
			float num2 = y / 2f;
			verts = new Vector3[3]
			{
				new Vector3(0f, num2, 0f),
				new Vector3(0f - num, 0f - num2, 0f),
				new Vector3(num, 0f - num2, 0f)
			};
		}
		Color[] cols = new Color[3] { tipColor, Color, Color };
		Vector2[] uvs = new Vector2[3]
		{
			new Vector2(0.5f, 1f),
			new Vector2(0f, 0f),
			new Vector3(1f, 0f)
		};
		int[] tris = new int[3] { 0, 2, 1 };
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
