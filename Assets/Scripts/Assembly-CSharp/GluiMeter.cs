using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Glui/Meter")]
public class GluiMeter : GluiSprite
{
	public enum MeterShapeType
	{
		Horizontal_ToRight = 0,
		Vertical_ToTop = 1,
		Horizontal_ToLeft = 2,
		Vertical_ToBottom = 3
	}

	public delegate void ValueChangedAction(ref float valueToModify, float originalValue);

	public float currentValue = 1f;

	public ValueChangedAction onValueChanged;

	[SerializeField]
	private MeterShapeType meterShape;

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

	public MeterShapeType MeterShape
	{
		get
		{
			return meterShape;
		}
		set
		{
			meterShape = value;
			RedrawValue();
		}
	}

	public override Vector3 EffectAttachPoint
	{
		get
		{
			float num = base.Size.x * currentValue;
			float num2 = base.Size.x / 2f;
			Vector3 result = new Vector3(base.transform.position.x - num2 + num, base.transform.position.y, base.transform.position.z);
			return result;
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
		if (currentValue != newValue)
		{
			newValue = Mathf.Clamp(newValue, 0f, 1f);
			currentValue = newValue;
			RedrawValue();
		}
	}

	private void RedrawValue()
	{
		InitWidget();
		int[] tris = new int[6] { 0, 2, 1, 0, 3, 2 };
		Rect shape;
		Rect atlas;
		BuildShape(out shape, out atlas);
		Vector3[] verts = new Vector3[4]
		{
			new Vector3(shape.xMin, shape.yMin, 0f),
			new Vector3(shape.xMax, shape.yMin, 0f),
			new Vector3(shape.xMax, shape.yMax, 0f),
			new Vector3(shape.xMin, shape.yMax, 0f)
		};
		Rect atlasRect = base.AtlasRect;
		float x = atlasRect.xMin + atlasRect.width * currentValue;
		UpdateMesh(verts, new Vector2[4]
		{
			new Vector2(atlasRect.xMin, atlasRect.yMin),
			new Vector2(x, atlasRect.yMin),
			new Vector2(x, atlasRect.yMax),
			new Vector2(atlasRect.xMin, atlasRect.yMax)
		}, tris);
		PostRedrawValue();
	}

	private void BuildShape(out Rect shape, out Rect atlas)
	{
		float num = base.Size.x / 2f;
		float num2 = base.Size.y / 2f;
		Rect atlasRect = base.AtlasRect;
		shape = Rect.MinMaxRect(0f - num, 0f - num2, num, num2);
		atlas = Rect.MinMaxRect(atlasRect.xMin, atlasRect.yMin, atlasRect.xMax, atlasRect.yMax);
		switch (meterShape)
		{
		case MeterShapeType.Horizontal_ToLeft:
		{
			float num6 = base.Size.x * currentValue;
			shape.xMin = num - num6;
			atlas.xMin = atlasRect.xMax - atlasRect.width * currentValue;
			break;
		}
		case MeterShapeType.Horizontal_ToRight:
		{
			float num5 = base.Size.x * currentValue;
			shape.xMax = 0f - num + num5;
			atlas.xMax = atlasRect.xMin + atlasRect.width * currentValue;
			break;
		}
		case MeterShapeType.Vertical_ToTop:
		{
			float num4 = base.Size.y * currentValue;
			shape.yMax = 0f - num2 + num4;
			atlas.yMax = atlasRect.yMin + atlasRect.height * currentValue;
			break;
		}
		case MeterShapeType.Vertical_ToBottom:
		{
			float num3 = base.Size.y * currentValue;
			shape.yMin = num2 - num3;
			atlas.yMin = atlasRect.yMax - atlasRect.height * currentValue;
			break;
		}
		}
	}

	protected virtual void PostRedrawValue()
	{
	}
}
