using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class DecayTimer
{
	public delegate void TimerResponseHandler(DecayTimer timer, int numberOfTicks);

	private DecaySchema data;

	private float nextTick;

	public DecaySchema Data
	{
		get
		{
			return data;
		}
	}

	[method: MethodImpl(32)]
	public event TimerResponseHandler Event_TickReached;

	public void Init(DecaySchema data)
	{
		this.data = data;
		StartNextTick();
	}

	public void Update()
	{
		if (Time.time >= nextTick)
		{
			OnTickReached();
		}
	}

	private void OnTickReached()
	{
		StartNextTick();
		if (this.Event_TickReached != null)
		{
			this.Event_TickReached(this, 1);
		}
	}

	public uint FastForward(uint seconds)
	{
		if (data == null || !data.allowFastForward)
		{
			return 0u;
		}
		float num = (float)seconds / 60f;
		uint num2 = (uint)Mathf.Floor(num / data.minutesPerTick);
		float num3 = num - (float)num2 * data.minutesPerTick;
		nextTick -= num3 * 60f;
		return num2;
	}

	private void StartNextTick()
	{
		nextTick = Time.time + data.minutesPerTick * 60f;
	}

	public float TimeToNextTick()
	{
		float num = nextTick - Time.time;
		if (num < 0f)
		{
			num = 0f;
		}
		return num;
	}
}
