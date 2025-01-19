using System;
using UnityEngine;

public class HUD : UIHandler<HUD>, IGluiActionHandler
{
	private const double updateEverySecs = 0.33;

	private GameObject mUI;

	private DateTime lastUpdateTime;

	private GluiStateProcesses processes = new GluiStateProcesses();

	public bool abilitiesEnabled
	{
		get
		{
			HUDAbilities hUDAbilities = FindHUD<HUDAbilities>();
			if (hUDAbilities != null)
			{
				return hUDAbilities.enabled;
			}
			return false;
		}
		set
		{
			HUDAbilities hUDAbilities = FindHUD<HUDAbilities>();
			if (hUDAbilities != null)
			{
				hUDAbilities.enabled = value;
			}
		}
	}

	public bool alliesEnabled
	{
		get
		{
			HUDAllies hUDAllies = FindHUD<HUDAllies>();
			if (hUDAllies != null)
			{
				return hUDAllies.enabled;
			}
			return false;
		}
		set
		{
			HUDAllies hUDAllies = FindHUD<HUDAllies>();
			if (hUDAllies != null)
			{
				hUDAllies.enabled = value;
			}
			HUDLeadership hUDLeadership = FindHUD<HUDLeadership>();
			if (hUDLeadership != null)
			{
				hUDLeadership.enabled = value;
			}
		}
	}

	public void Init()
	{
		mUI = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("UI/Prefabs/HUD/HUD") as GameObject);
		mUI.transform.parent = base.gameObject.transform;
		mComponents.Add(new HUDHealthBar(mUI, WeakGlobalMonoBehavior<InGameImpl>.Instance.hero));
		mComponents.Add(new HUDLeadership(mUI));
		mComponents.Add(new HUDWaveInfo(mUI));
		mComponents.Add(new HUDAllies(mUI));
		mComponents.Add(new HUDAbilities(mUI));
		mComponents.Add(new HUDConsumables(mUI));
		mComponents.Add(new HUDCharm(mUI));
		processes.AddStateProcesses(mUI);
		processes.Start(GluiStatePhase.Running, string.Empty);
	}

	public override void Update()
	{
		if (!WeakGlobalMonoBehavior<InGameImpl>.Instance.gamePaused)
		{
			base.Update();
		}
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (!WeakGlobalMonoBehavior<InGameImpl>.Instance.gamePaused || action == "PAUSE")
		{
			return OnUIEvent(action);
		}
		return false;
	}

	public void ResetAbilities()
	{
		if (mComponents != null && mComponents.Count > 4)
		{
			HUDAbilities hUDAbilities = mComponents[4] as HUDAbilities;
			if (hUDAbilities != null)
			{
				hUDAbilities.Init();
			}
		}
	}

	public void ResetLeadershipCosts()
	{
		if (mComponents != null && mComponents.Count > 3)
		{
			HUDAllies hUDAllies = mComponents[3] as HUDAllies;
			if (hUDAllies != null)
			{
				hUDAllies.RefreshCosts();
			}
		}
	}

	public T FindHUD<T>() where T : class
	{
		if (mComponents != null)
		{
			foreach (UIHandlerComponent mComponent in mComponents)
			{
				if (mComponent is T)
				{
					return (T)mComponent;
				}
			}
		}
		return (T)null;
	}

	public void AddUIResponder(UIHandlerComponent c)
	{
		mComponents.Add(c);
	}

	public void RemoveUIResponder(UIHandlerComponent c)
	{
		mComponents.Remove(c);
	}

	public void HandleGamePause(bool paused)
	{
		foreach (UIHandlerComponent mComponent in mComponents)
		{
			mComponent.OnPause(paused);
		}
	}
}
