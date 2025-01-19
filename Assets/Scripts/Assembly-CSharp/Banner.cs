using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Banner
{
	private enum State
	{
		Intro = 0,
		Running = 1,
		Outro = 2
	}

	private float mTimeLeft;

	private List<GluiTransition> transitions;

	private int runningTransitions;

	private State state;

	protected abstract string uiPrefabPath { get; }

	protected GameObject bannerInstance { get; private set; }

	public Banner(float timeBeforeFade)
	{
		mTimeLeft = timeBeforeFade;
	}

	public GameObject Init()
	{
		Destroy();
		SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(string.Format("Assets/Game/Resources/{0}.prefab", uiPrefabPath), 1);
		GameObject original = cachedResource.Resource as GameObject;
		bannerInstance = UnityEngine.Object.Instantiate(original) as GameObject;
		bannerInstance.transform.localPosition = Vector3.zero;
		GluiWidget[] componentsInChildren = bannerInstance.GetComponentsInChildren<GluiWidget>();
		foreach (GluiWidget gluiWidget in componentsInChildren)
		{
			gluiWidget.SendMessage("Start");
		}
		InitText();
		runningTransitions = 0;
		transitions = new List<GluiTransition>();
		transitions.AddRange(bannerInstance.GetComponentsInChildren<GluiTransition>(true));
		transitions.ForEach(delegate(GluiTransition thisSequence)
		{
			thisSequence.onDoneCallback = (Action<GluiTransition.Position>)Delegate.Combine(thisSequence.onDoneCallback, new Action<GluiTransition.Position>(OnTransitionDoneCallback));
			if (thisSequence.Transition_Forward())
			{
				runningTransitions++;
			}
		});
		if (runningTransitions > 0)
		{
			state = State.Intro;
		}
		else
		{
			state = State.Running;
		}
		return bannerInstance;
	}

	public void Destroy()
	{
		if (bannerInstance != null)
		{
			UnityEngine.Object.Destroy(bannerInstance);
			bannerInstance = null;
		}
	}

	public bool Update()
	{
		switch (state)
		{
		case State.Running:
			if (mTimeLeft <= 0f)
			{
				runningTransitions = 0;
				transitions.ForEach(delegate(GluiTransition thisSequence)
				{
					if (thisSequence.Transition_Back())
					{
						runningTransitions++;
					}
				});
				if (runningTransitions <= 0)
				{
					return false;
				}
				state = State.Outro;
			}
			else
			{
				mTimeLeft -= Time.deltaTime;
			}
			break;
		}
		return true;
	}

	protected abstract void InitText();

	private void OnTransitionDoneCallback(GluiTransition.Position position)
	{
		runningTransitions--;
		if (runningTransitions == 0)
		{
			if (state == State.Intro)
			{
				state = State.Running;
			}
			else if (state == State.Outro)
			{
				Destroy();
			}
		}
	}
}
