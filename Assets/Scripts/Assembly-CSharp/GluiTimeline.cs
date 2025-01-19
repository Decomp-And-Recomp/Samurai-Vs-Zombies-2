using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Glui Agent/Agent Timeline")]
public class GluiTimeline : MonoBehaviour
{
	[Serializable]
	public class PersistentDataToSave
	{
		public string id;

		public string textToSave;
	}

	[Serializable]
	public class SequenceStep
	{
		public float delayBeforeSeconds;

		public PersistentDataToSave[] dataToSave;

		public string[] actions;

		public GluiOrderToSend[] agentOrders;
	}

	public enum RepeatType
	{
		None = 0,
		UseCycleCount = 1,
		Forever = 2
	}

	public SequenceStep[] steps;

	private int currentStep;

	private int cycle;

	public Action callbackWhenDone;

	public RepeatType repeat;

	public int repeatCycles;

	public bool StartTimeline(Action callbackWhenDone)
	{
		currentStep = 0;
		cycle = 0;
		return DoNext();
	}

	public void StopTimeline()
	{
		CancelInvoke("DelayedNext");
		currentStep = 0;
		cycle = 0;
	}

	public void DoCurrentStep()
	{
		SequenceStep sequenceStep = steps[currentStep];
		for (int i = 0; i < sequenceStep.dataToSave.Length; i++)
		{
			SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(sequenceStep.dataToSave[i].id, sequenceStep.dataToSave[i].textToSave);
		}
		for (int j = 0; j < sequenceStep.actions.Length; j++)
		{
			GluiActionSender.SendGluiAction(sequenceStep.actions[j], base.gameObject, null);
		}
		for (int k = 0; k < sequenceStep.agentOrders.Length; k++)
		{
			sequenceStep.agentOrders[k].Send(base.gameObject);
		}
		currentStep++;
	}

	public void DelayedNext()
	{
		DoCurrentStep();
		if (!DoNext() && callbackWhenDone != null)
		{
			callbackWhenDone();
		}
	}

	public bool DoNext()
	{
		while (true)
		{
			if (currentStep < steps.Length)
			{
				SequenceStep sequenceStep = steps[currentStep];
				if (sequenceStep.delayBeforeSeconds > 0f)
				{
					Invoke("DelayedNext", sequenceStep.delayBeforeSeconds);
					return true;
				}
				DoCurrentStep();
			}
			else if (!NextCycle())
			{
				break;
			}
		}
		return false;
	}

	public bool NextCycle()
	{
		currentStep = 0;
		switch (repeat)
		{
		case RepeatType.Forever:
			return true;
		case RepeatType.None:
			return false;
		case RepeatType.UseCycleCount:
			cycle++;
			if (cycle < repeatCycles)
			{
				return true;
			}
			return false;
		default:
			return false;
		}
	}
}
