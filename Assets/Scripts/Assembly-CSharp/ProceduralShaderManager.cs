using System;
using System.Collections.Generic;

public class ProceduralShaderManager : WeakGlobalInstance<ProceduralShaderManager>
{
	private List<ShaderEvent> mEvents;

	public ProceduralShaderManager()
	{
		SetUniqueInstance(this);
		mEvents = new List<ShaderEvent>();
	}

	public void update()
	{
		int num = 0;
		while (num < mEvents.Count)
		{
			if (mEvents[num].shouldDie)
			{
				mEvents.RemoveAt(num);
				continue;
			}
			mEvents[num].resetToBaseValues();
			num++;
		}
		foreach (ShaderEvent mEvent in mEvents)
		{
			mEvent.update();
		}
	}

	public static void postShaderEvent(ShaderEvent shaderEvent)
	{
		if (WeakGlobalInstance<ProceduralShaderManager>.Instance != null)
		{
			WeakGlobalInstance<ProceduralShaderManager>.Instance.mEvents.Add(shaderEvent);
		}
	}

	public static void StopShaderEvents(Predicate<ShaderEvent> shouldStop)
	{
		if (WeakGlobalInstance<ProceduralShaderManager>.Instance == null || shouldStop == null)
		{
			return;
		}
		int num = 0;
		while (num < WeakGlobalInstance<ProceduralShaderManager>.Instance.mEvents.Count)
		{
			ShaderEvent shaderEvent = WeakGlobalInstance<ProceduralShaderManager>.Instance.mEvents[num];
			if (shouldStop(shaderEvent))
			{
				shaderEvent.resetToBaseValues();
				WeakGlobalInstance<ProceduralShaderManager>.Instance.mEvents.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}
}
