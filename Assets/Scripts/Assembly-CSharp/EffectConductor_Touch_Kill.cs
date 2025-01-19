using UnityEngine;

[AddComponentMenu("Effect Maestro/Effect Conductor - Touch Kill")]
public class EffectConductor_Touch_Kill : EffectConductor_Touch
{
	protected override void OnCursorDown(InputCrawl crawl)
	{
		effectContainer.EffectKill();
	}

	protected override void OnCursorMove(InputCrawl crawl)
	{
		effectContainer.EffectKill();
	}

	protected override void OnCursorUp(InputCrawl crawl)
	{
	}

	protected override void OnCursorExit(InputCrawl crawl)
	{
	}
}
