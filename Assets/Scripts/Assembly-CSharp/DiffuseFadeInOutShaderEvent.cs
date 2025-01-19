using System.Collections.Generic;
using UnityEngine;

public class DiffuseFadeInOutShaderEvent : ShaderEvent
{
	private enum FadeState
	{
		eFadeIn = 0,
		eHold = 1,
		eFadeOut = 2
	}

	private Color mTargetColor;

	private FadeInOutEvent mFadeEvent;

	private bool mAlphaOnly;

	public DiffuseFadeInOutShaderEvent(GameObject obj, Color targetColor, float fadeInTime, float holdTime, float fadeOutTime)
		: base(obj, null, null)
	{
		mTargetColor = targetColor;
		mFadeEvent = new FadeInOutEvent(fadeInTime, holdTime, fadeOutTime);
	}

	public DiffuseFadeInOutShaderEvent(GameObject obj, Color targetColor, float fadeInTime, float holdTime, float fadeOutTime, Dictionary<int, Color> startColors, List<GameObject> objToIgnore)
		: base(obj, objToIgnore, startColors)
	{
		mTargetColor = targetColor;
		mFadeEvent = new FadeInOutEvent(fadeInTime, holdTime, fadeOutTime);
	}

	public override void resetToBaseValues()
	{
		if (mAlphaOnly)
		{
			RevertAllMaterialsToAlpha(mFadeEvent.interpolant);
		}
		else
		{
			RevertAllMaterialsToColor(mFadeEvent.interpolant);
		}
	}

	public override void update()
	{
		base.update();
		mFadeEvent.update();
		if (base.shouldDie || mFadeEvent.isComplete)
		{
			base.shouldDie = true;
		}
		else if (mAlphaOnly)
		{
			BlendAllMaterialsToAlpha(mTargetColor.a);
		}
		else
		{
			BlendAllMaterialsToColor(mTargetColor, mFadeEvent.interpolant);
		}
	}

	private void BlendAllMaterialsToAlpha(float targetAlpha)
	{
		for (int i = 0; i < mMaterials.Count; i++)
		{
			if (!object.ReferenceEquals(mMaterials[i].ptr, null))
			{
				Color materialColor = GetMaterialColor(i);
				float a = Mathf.Lerp(materialColor.a, targetAlpha, mFadeEvent.interpolant);
				Color color = new Color(materialColor.r, materialColor.g, materialColor.b, a);
				mMaterials[i].ptr.SetColor("_Color", color);
				mMaterials[i].ptr.SetColor("_MainColor", color);
				mMaterials[i].ptr.SetColor("_RimColor", color);
			}
		}
	}
}
