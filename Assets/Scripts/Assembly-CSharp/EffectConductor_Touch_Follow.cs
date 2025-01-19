using UnityEngine;

[AddComponentMenu("Effect Maestro/Effect Conductor - Touch Follow")]
public class EffectConductor_Touch_Follow : EffectConductor_Touch
{
	protected override void OnCursorMove(InputCrawl crawl)
	{
		effectContainer.EffectEnable();
		if (effectContainer.EffectCamera != null)
		{
			effectContainer.EffectMove(crawl.inputEvent.Position);
		}
	}

	protected override void OnCursorUp(InputCrawl crawl)
	{
		effectContainer.EffectDisable();
	}

	protected override void OnCursorExit(InputCrawl crawl)
	{
		effectContainer.EffectDisable();
	}
}
