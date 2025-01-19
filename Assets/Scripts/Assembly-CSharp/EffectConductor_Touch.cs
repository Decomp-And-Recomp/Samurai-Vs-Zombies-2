using UnityEngine;

[AddComponentMenu("Effect Maestro/Effect Conductor - Touch")]
public class EffectConductor_Touch : EffectConductor, IInputContainer
{
	public void FilterInput(InputCrawl crawl, GameObject objectToFilter, out InputRouter.InputResponse response)
	{
		switch (crawl.inputEvent.EventType)
		{
		case InputEvent.EEventType.OnCursorDown:
			OnCursorDown(crawl);
			break;
		case InputEvent.EEventType.OnCursorMove:
			OnCursorMove(crawl);
			break;
		case InputEvent.EEventType.OnCursorUp:
			OnCursorUp(crawl);
			break;
		case InputEvent.EEventType.OnCursorExit:
			OnCursorExit(crawl);
			break;
		}
		response = InputRouter.InputResponse.Handled_Passive;
	}

	protected virtual void OnCursorDown(InputCrawl crawl)
	{
		effectContainer.EffectEnable();
	}

	protected virtual void OnCursorMove(InputCrawl crawl)
	{
		effectContainer.EffectEnable();
	}

	protected virtual void OnCursorUp(InputCrawl crawl)
	{
		effectContainer.EffectDisable();
	}

	protected virtual void OnCursorExit(InputCrawl crawl)
	{
		effectContainer.EffectDisable();
	}
}
