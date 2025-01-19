using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Timers/Decay System")]
public class DecaySystem : SingletonSpawningMonoBehaviour<DecaySystem>
{
	private List<DecayTimer> timers = new List<DecayTimer>();

	public string DecaySchemaTableName = "DecayTimers";

	private uint fastForwardSecondsToApplyToNewTimers;

	private DateTime? timeBeforePauseApp;

	public List<DecayTimer> Timers
	{
		get
		{
			return timers;
		}
	}

	public bool Initialized { get; set; }

	protected override void Awake()
	{
		base.Awake();
	}

	public void Initialize()
	{
		timers.Clear();
		LoadAllTimers(DecaySchemaTableName, OnTimersLoaded);
		Initialized = true;
	}

	private void Update()
	{
		foreach (DecayTimer timer in timers)
		{
			timer.Update();
		}
	}

	protected override void OnApplicationQuit()
	{
		OnApplicationPause(true);
		base.OnApplicationQuit();
	}

	private void OnApplicationPause(bool state)
	{
		if (Initialized)
		{
			if (!state && timeBeforePauseApp.HasValue)
			{
				uint seconds = (uint)(SntpTime.UniversalTime - timeBeforePauseApp.Value).TotalSeconds;
				FastForward(seconds, false);
				timeBeforePauseApp = null;
			}
			else
			{
				timeBeforePauseApp = SntpTime.UniversalTime;
			}
		}
		else
		{
			Singleton<Profile>.Instance.StartDecaySystem();
		}
	}

	public void LoadAllTimers(string decayTableName, Action onTimersLoaded)
	{
		if (DataBundleRuntime.Instance == null)
		{
			return;
		}
		DataBundleTableHandle<DecaySchema> dataBundleTableHandle = new DataBundleTableHandle<DecaySchema>(decayTableName);
		dataBundleTableHandle.Load(DataBundleResourceGroup.All, false, delegate(DecaySchema[] decayTimers)
		{
			foreach (DecaySchema data in decayTimers)
			{
				DecayTimer decayTimer = new DecayTimer();
				decayTimer.Init(data);
				decayTimer.Event_TickReached += HandleTimerEvent_TickReached;
				timers.Add(decayTimer);
			}
			onTimersLoaded();
		});
	}

	private void OnTimersLoaded()
	{
		FastForward(fastForwardSecondsToApplyToNewTimers, false);
	}

	public void FastForward(uint seconds, bool applyToNewTimers)
	{
		foreach (DecayTimer timer in timers)
		{
			FastForward(timer, seconds);
		}
		if (applyToNewTimers)
		{
			fastForwardSecondsToApplyToNewTimers += seconds;
		}
	}

	private void FastForward(DecayTimer timer, uint seconds)
	{
		uint num = timer.FastForward(seconds);
		if (num != 0)
		{
			HandleTimerEvent_TickReached(timer, (int)num);
		}
	}

	public DecayTimer Find(string name)
	{
		foreach (DecayTimer timer in timers)
		{
			if (timer.Data.name == name)
			{
				return timer;
			}
		}
		return null;
	}

	private void HandleTimerEvent_TickReached(DecayTimer timer, int numberOfTicks)
	{
		if (timer.Data.actionOnTick != string.Empty)
		{
			GluiActionSender.SendGluiAction(timer.Data.actionOnTick, base.gameObject, numberOfTicks);
		}
		OnTickReached(timer, numberOfTicks);
	}

	protected virtual void OnTickReached(DecayTimer timer, int numberOfTicks)
	{
	}
}
