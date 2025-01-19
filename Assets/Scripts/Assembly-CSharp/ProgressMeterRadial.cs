using System;
using UnityEngine;

[AddComponentMenu("Glui/Radial Meter")]
public class ProgressMeterRadial : GluiSprite
{
	public enum orientType
	{
		right = 0,
		up = 1,
		left = 2,
		down = 3
	}

	public delegate void ValueChangedAction(ref float valueToModify, float originalValue);

	private const string requiredShaderName = "Mobile/Particles/Alpha Blended";

	public float currentValue = 1f;

	public bool normalize = true;

	public orientType zeroOrient = orientType.up;

	public bool clockwise = true;

	public float startAngle;

	public float rangeAngle = 360f;

	public bool drawFull = true;

	public bool drawEmpty = true;

	public Vector2 scaleUV = new Vector2(1f, 1f);

	public Vector2 fullUVcenter = new Vector2(0.5f, 0.5f);

	public Vector2 emptyUVcenter = new Vector2(0.5f, 0.5f);

	public ValueChangedAction onValueChanged;

	private static Shader requiredShader;

	private Vector3[] defaultVerts = new Vector3[10]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3(-1f, -1f, 0f),
		new Vector3(1f, -1f, 0f),
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(0f, 1f, 0f)
	};

	private static int FullOrigin;

	private static int EmptyOrigin = 5;

	private static int FullStart = 6;

	private static int FullEnd = 7;

	private static int EmptyStart = 8;

	private static int EmptyEnd = 9;

	private Vector3[] meshVerts;

	private Vector2[] meshUVs;

	private Color[] meshColors;

	private int[] meshTris;

	private int lastTri;

	public float Value
	{
		get
		{
			return currentValue;
		}
		set
		{
			SetValue(value);
		}
	}

	protected override Shader RequiredShader
	{
		get
		{
			if (requiredShader == null)
			{
				requiredShader = Shader.Find("Mobile/Particles/Alpha Blended");
			}
			return requiredShader;
		}
	}

	protected override void OnInit()
	{
		RedrawValue();
	}

	private void SetValue(float newValue)
	{
		newValue = Mathf.Clamp(newValue, 0f, 1f);
		if (!GluiWidget.inspectorGUIUpdating && onValueChanged != null)
		{
			onValueChanged(ref newValue, newValue);
		}
		newValue = Mathf.Clamp(newValue, 0f, 1f);
		currentValue = newValue;
		RedrawValue();
	}

	protected virtual void PostRedrawValue()
	{
	}

	public void RedrawValue()
	{
		SanitizeStartAngle();
		int full = TriRange();
		ConstructVertPositions();
		ConstructVertColors(full);
		ConstructUVs();
		ConstructTris(full);
		ScaleMesh();
		UpdateMesh(meshVerts, meshUVs, meshTris, meshColors);
		PostRedrawValue();
	}

	private void ConstructVertPositions()
	{
		meshVerts = new Vector3[10];
		Array.Copy(defaultVerts, meshVerts, 10);
		meshVerts[FullStart] = (meshVerts[EmptyEnd] = AngleOnQuad(startAngle));
		meshVerts[FullEnd] = (meshVerts[EmptyStart] = AngleOnQuad(startAngle + rangeAngle * Value));
		if (startAngle + rangeAngle < 360f)
		{
			meshVerts[EmptyEnd] = AngleOnQuad(startAngle + rangeAngle);
			if (startAngle + rangeAngle > 315f)
			{
				lastTri = 6;
			}
			else if (startAngle + rangeAngle > 225f)
			{
				lastTri = 5;
			}
			else if (startAngle + rangeAngle > 135f)
			{
				lastTri = 4;
			}
			else if (startAngle + rangeAngle > 45f)
			{
				lastTri = 3;
			}
		}
		else
		{
			lastTri = 6;
		}
	}

	private void ConstructVertColors(int full)
	{
		meshColors = new Color[10];
		for (int i = 0; i < 4; i++)
		{
			if (i < full)
			{
				meshColors[NextCornerVert(i)] = Color;
			}
			else
			{
				meshColors[NextCornerVert(i)] = base.DisabledColor;
			}
		}
		meshColors[FullOrigin] = Color;
		meshColors[FullStart] = Color;
		meshColors[FullEnd] = Color;
		meshColors[EmptyOrigin] = base.DisabledColor;
		meshColors[EmptyStart] = base.DisabledColor;
		meshColors[EmptyEnd] = base.DisabledColor;
	}

	private void ConstructUVs()
	{
		meshUVs = new Vector2[10];
		for (int i = 1; i < 5; i++)
		{
			if (meshColors[i] == Color)
			{
				meshUVs[i] = ConstructBaseUV(i) + fullUVcenter;
			}
			else
			{
				meshUVs[i] = ConstructBaseUV(i) + emptyUVcenter;
			}
		}
		meshUVs[FullOrigin] = ConstructBaseUV(FullOrigin) + fullUVcenter;
		meshUVs[FullStart] = ConstructBaseUV(FullStart) + fullUVcenter;
		meshUVs[FullEnd] = ConstructBaseUV(FullEnd) + fullUVcenter;
		meshUVs[EmptyOrigin] = ConstructBaseUV(EmptyOrigin) + emptyUVcenter;
		meshUVs[EmptyStart] = ConstructBaseUV(EmptyStart) + emptyUVcenter;
		meshUVs[EmptyEnd] = ConstructBaseUV(EmptyEnd) + emptyUVcenter;
	}

	private Vector2 ConstructBaseUV(int fromIndex)
	{
		return new Vector2(meshVerts[fromIndex].x * scaleUV.x, meshVerts[fromIndex].y * scaleUV.y) / 2f;
	}

	private void ConstructTris(int full)
	{
		meshTris = new int[18];
		int num = FullStart;
		for (int i = 0; i < 6; i++)
		{
			if (i <= full)
			{
				meshTris[i * 3] = FullOrigin;
			}
			else
			{
				meshTris[i * 3] = EmptyOrigin;
			}
			if (((drawFull && i <= full) || (drawEmpty && i > full)) && i < lastTri)
			{
				meshTris[i * 3 + WindA()] = num;
			}
			if (i < full)
			{
				num = (meshTris[i * 3 + WindB()] = NextCornerVert(i));
				continue;
			}
			if (i > full)
			{
				num = (meshTris[i * 3 + WindB()] = NextCornerVert(i - 1));
				continue;
			}
			meshTris[i * 3 + WindB()] = FullEnd;
			num = EmptyStart;
		}
		meshTris[lastTri * 3 + WindB() - 3] = EmptyEnd;
	}

	private int WindA()
	{
		if (clockwise)
		{
			return 1;
		}
		return 2;
	}

	private int WindB()
	{
		if (clockwise)
		{
			return 2;
		}
		return 1;
	}

	private void ScaleMesh()
	{
		float x = base.Size.x;
		float y = base.Size.y;
		for (int i = 0; i < 10; i++)
		{
			meshVerts[i].x *= x * SmartWidth();
			meshVerts[i].y *= y * SmartHeight();
		}
	}

	private float SmartWidth()
	{
		if (normalize)
		{
			return 0.5f;
		}
		if (zeroOrient == orientType.up || zeroOrient == orientType.down)
		{
			if (Mathf.Abs(startAngle + rangeAngle) > 180f)
			{
				return 0.5f;
			}
			if (startAngle < 0f)
			{
				return 0.5f;
			}
		}
		if ((zeroOrient == orientType.left || zeroOrient == orientType.right) && Mathf.Abs(startAngle + rangeAngle) > 90f)
		{
			return 0.5f;
		}
		return 1f;
	}

	private float SmartHeight()
	{
		if (normalize)
		{
			return 0.5f;
		}
		if (zeroOrient == orientType.left || zeroOrient == orientType.right)
		{
			if (Mathf.Abs(startAngle + rangeAngle) > 180f)
			{
				return 0.5f;
			}
			if (startAngle < 0f)
			{
				return 0.5f;
			}
		}
		if ((zeroOrient == orientType.up || zeroOrient == orientType.down) && Mathf.Abs(startAngle + rangeAngle) > 90f)
		{
			return 0.5f;
		}
		return 1f;
	}

	private float ClockAngleEul(float inAngle)
	{
		if (clockwise)
		{
			return Mathf.Repeat(0f - inAngle + 90f * (float)zeroOrient, 360f);
		}
		return Mathf.Repeat(inAngle + 90f * (float)zeroOrient, 360f);
	}

	private float ClockAngleRad(float inAngle)
	{
		return ClockAngleEul(inAngle) * ((float)Math.PI / 180f);
	}

	private Vector3 AngleOnQuad(float inAngle)
	{
		return AngleOnQuadRad(ClockAngleRad(inAngle));
	}

	private Vector3 AngleOnQuadRad(float inAngle)
	{
		float num = Mathf.Sin(inAngle);
		float num2 = Mathf.Cos(inAngle);
		if (Mathf.Abs(num) > Mathf.Abs(num2))
		{
			return new Vector3(num2 * (1f / Mathf.Abs(num)), Mathf.Sign(num), 0f);
		}
		return new Vector3(Mathf.Sign(num2), num * (1f / Mathf.Abs(num2)), 0f);
	}

	private int RepeatInt(int source, int max)
	{
		return Mathf.RoundToInt(Mathf.Repeat(source, max));
	}

	private int TriRange()
	{
		float num = startAngle + rangeAngle * Value;
		if (num < 45f)
		{
			return 0;
		}
		if (num < 135f)
		{
			return 1;
		}
		if (num < 225f)
		{
			return 2;
		}
		if (num < 315f)
		{
			return 3;
		}
		return 4;
	}

	private int ClockSign()
	{
		if (clockwise)
		{
			return -1;
		}
		return 1;
	}

	private int NextCornerVert(int value)
	{
		if (clockwise)
		{
			return RepeatInt((int)(zeroOrient - value - 1), 4) + 1;
		}
		return RepeatInt((int)(zeroOrient + value), 4) + 1;
	}

	private void SanitizeStartAngle()
	{
		while (Mathf.Abs(startAngle) > 45f)
		{
			NextZeroOrient();
			startAngle -= 90f * Mathf.Sign(startAngle);
		}
	}

	private void NextZeroOrient()
	{
		if ((clockwise ^ (startAngle < 0f)) || (!clockwise ^ (startAngle > 0f)))
		{
			zeroOrient--;
			if (zeroOrient < orientType.right)
			{
				zeroOrient = orientType.down;
			}
		}
		else
		{
			zeroOrient++;
			if (zeroOrient > (orientType)4)
			{
				zeroOrient = orientType.up;
			}
		}
	}
}
