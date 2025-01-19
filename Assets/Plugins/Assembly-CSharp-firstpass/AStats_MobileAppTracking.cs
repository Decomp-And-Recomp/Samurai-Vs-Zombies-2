using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class AStats_MobileAppTracking : MonoBehaviour
{
	private struct Event
	{
		public float Time { get; set; }

		public string Id { get; set; }
	}

	private const float UpdateInterval = 60f;

	private Queue<Event> pendingEvents;

	private float cumulativeTime;

	private float prevTime;

	private float nextUpdateTime;

	private long previousStoredTime;

	private static AndroidJavaClass matClass;

	private static AndroidJavaClass MatClass
	{
		get
		{
			if (matClass == null)
			{
				JniHelper.PushLocalFrame();
				try
				{
					matClass = new AndroidJavaClass("com.glu.plugins.MobileAppTrackerGlu");
				}
				finally
				{
					JniHelper.PopLocalFrame();
				}
			}
			return matClass;
		}
	}

	private static IEnumerable<Event> GetAllEvents()
	{
		return new Event[3]
		{
			new Event
			{
				Time = 900f,
				Id = "event_15"
			},
			new Event
			{
				Time = 1800f,
				Id = "event_30"
			},
			new Event
			{
				Time = 3600f,
				Id = "event_60"
			}
		}.OrderBy((Event e) => e.Time);
	}

	private void Awake()
	{
		pendingEvents = new Queue<Event>(GetAllEvents());
	}

	private void Start()
	{
		previousStoredTime = GetCumulativeTime();
		cumulativeTime = previousStoredTime;
		prevTime = Time.realtimeSinceStartup;
		HandleTimeUpdate();
	}

	private void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = realtimeSinceStartup - prevTime;
		if (!(num <= 0f))
		{
			prevTime = realtimeSinceStartup;
			cumulativeTime += num;
			HandleTimeUpdate();
		}
	}

	private void OnApplicationPause(bool paused)
	{
		prevTime = Time.realtimeSinceStartup;
		if (paused)
		{
			SaveState();
		}
		else
		{
			TrackActionOpens();
		}
	}

	private void OnDestroy()
	{
		SaveState();
	}

	private void HandleTimeUpdate()
	{
		if (cumulativeTime < nextUpdateTime)
		{
			return;
		}
		while (pendingEvents.Any())
		{
			Event @event = pendingEvents.Peek();
			float time = @event.Time;
			if (cumulativeTime < time)
			{
				break;
			}
			if (!IsEventHandled(@event.Id))
			{
				AStats.MobileAppTracking.TrackAction(@event.Id);
				SetEventHandled(@event.Id, true);
			}
			pendingEvents.Dequeue();
		}
		SaveState();
		nextUpdateTime = cumulativeTime + 60f;
		if (pendingEvents.Any())
		{
			nextUpdateTime = Math.Min(pendingEvents.Peek().Time, nextUpdateTime);
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void SaveState()
	{
		long num = (long)cumulativeTime;
		if (previousStoredTime != num)
		{
			SetCumulativeTime(num);
			previousStoredTime = num;
		}
	}

	private static long GetCumulativeTime()
	{
		JniHelper.PushLocalFrame();
		try
		{
			return MatClass.CallStatic<long>("getCumulativeTime", new object[0]);
		}
		finally
		{
			JniHelper.PopLocalFrame();
		}
	}

	private static void SetCumulativeTime(long time)
	{
		JniHelper.PushLocalFrame();
		try
		{
			MatClass.CallStatic("setCumulativeTime", time);
		}
		finally
		{
			JniHelper.PopLocalFrame();
		}
	}

	private static bool IsEventHandled(string id)
	{
		JniHelper.PushLocalFrame();
		try
		{
			return MatClass.CallStatic<bool>("isEventHandled", new object[1] { id });
		}
		finally
		{
			JniHelper.PopLocalFrame();
		}
	}

	private static void SetEventHandled(string id, bool handled)
	{
		JniHelper.PushLocalFrame();
		try
		{
			MatClass.CallStatic("setEventHandled", id, handled);
		}
		finally
		{
			JniHelper.PopLocalFrame();
		}
	}

	private static void TrackActionOpens()
	{
		JniHelper.PushLocalFrame();
		try
		{
			MatClass.CallStatic("trackOpens");
		}
		finally
		{
			JniHelper.PopLocalFrame();
		}
	}
}
