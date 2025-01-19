using System.Collections.Generic;
using UnityEngine;

public class DiffusePermShaderEvent : ShaderEvent
{
	private Color mTargetColor;

	public DiffusePermShaderEvent(GameObject obj, Color targetColor, List<GameObject> objToIgnore, Dictionary<int, Color> originalColors)
		: base(obj, objToIgnore, originalColors)
	{
		mTargetColor = targetColor;
	}

	public override void resetToBaseValues()
	{
		RevertAllMaterialsToColor(1f);
	}

	public override void update()
	{
		base.update();
		if (!base.shouldDie)
		{
			SetAllMaterialsToColor(mTargetColor);
		}
	}
}
