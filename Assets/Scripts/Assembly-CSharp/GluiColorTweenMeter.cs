using UnityEngine;

[AddComponentMenu("Glui/Meter Color Tween")]
[ExecuteInEditMode]
public class GluiColorTweenMeter : GluiMeter
{
	public Color MeterColorMin = Color.black;

	public Color MeterColorMax = Color.white;

	protected override void PostRedrawValue()
	{
		Color color = MeterColorMin + (MeterColorMax - MeterColorMin) * currentValue;
		Color = color;
		base.PostRedrawValue();
	}
}
