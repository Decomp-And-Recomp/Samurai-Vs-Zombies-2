using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[AddComponentMenu("Glui Action/Action Filter - Tutorial")]
public class GluiActionFilter_Tutorial : MonoBehaviour, IGluiActionHandler
{
	public class ActionSet
	{
		public List<string> allowedActions;

		public List<string> blockedActions;

		public bool blockAllUnhandledActions;

		public Action<string, GameObject, object> allowedCallback;

		public Action<string, GameObject, object> blockedCallback;

		public string allowedCallback_ReplyAction = string.Empty;

		public string blockedCallback_ReplyAction = string.Empty;

		public ActionSet(List<string> allowedActions)
		{
			this.allowedActions = allowedActions;
			blockedActions = new List<string>();
		}

		public ActionSet(List<string> allowedActions, List<string> blockedActions)
		{
			this.allowedActions = allowedActions;
			this.blockedActions = blockedActions;
		}

		public void OnAllowed(string action, GameObject sender, object data)
		{
			if (allowedCallback != null)
			{
				allowedCallback(action, sender, data);
			}
			if (allowedCallback_ReplyAction != string.Empty)
			{
				GluiActionSender.SendGluiAction(allowedCallback_ReplyAction, sender, data);
			}
		}

		public void OnBlocked(string action, GameObject sender, object data)
		{
			if (blockedCallback != null)
			{
				blockedCallback(action, sender, data);
			}
			if (blockedCallback_ReplyAction != string.Empty)
			{
				GluiActionSender.SendGluiAction(blockedCallback_ReplyAction, sender, data);
			}
		}

		public void Clear()
		{
			allowedActions.Clear();
			blockedActions.Clear();
		}
	}

	public delegate void FilterResponseHandler(string action, GameObject sender, object data);

	private Dictionary<string, ActionSet> actionSets = new Dictionary<string, ActionSet>();

	[HideInInspector]
	public bool allowSetsToBlockAllUnhandledActions = true;

	[method: MethodImpl(32)]
	public event FilterResponseHandler ActionAllowed;

	[method: MethodImpl(32)]
	public event FilterResponseHandler ActionBlocked;

	public bool HandleAction(string action, GameObject sender, object data)
	{
		List<ActionSet> setsAllowing = null;
		List<ActionSet> setsBlocking = null;
		bool blockAllUnhandledActions = false;
		actionSets.ForEachWithIndex(delegate(KeyValuePair<string, ActionSet> thisNode, int index)
		{
			ActionSet value = thisNode.Value;
			blockAllUnhandledActions = value.blockAllUnhandledActions || blockAllUnhandledActions;
			if (value.allowedActions.Contains(action))
			{
				if (setsAllowing == null)
				{
					setsAllowing = new List<ActionSet>();
				}
				setsAllowing.Add(value);
			}
			if (value.blockedActions.Contains(action))
			{
				if (setsBlocking == null)
				{
					setsBlocking = new List<ActionSet>();
				}
				setsBlocking.Add(value);
			}
		});
		if (setsAllowing == null || setsBlocking != null)
		{
		}
		if (setsAllowing != null)
		{
			OnActionAllowed(action, sender, data);
			setsAllowing.ForEach(delegate(ActionSet thisActionSet)
			{
				thisActionSet.OnAllowed(action, sender, data);
			});
			return false;
		}
		if (setsBlocking != null)
		{
			OnActionBlocked(action, sender, data);
			setsAllowing.ForEach(delegate(ActionSet thisActionSet)
			{
				thisActionSet.OnBlocked(action, sender, data);
			});
			return true;
		}
		if (blockAllUnhandledActions && allowSetsToBlockAllUnhandledActions)
		{
			return true;
		}
		return false;
	}

	public void Add(ActionSet newSet, string name)
	{
		if (actionSets.ContainsKey(name))
		{
			actionSets.Remove(name);
		}
		actionSets.Add(name, newSet);
	}

	public bool Remove(string name)
	{
		if (actionSets.ContainsKey(name))
		{
			return actionSets.Remove(name);
		}
		return false;
	}

	public void Clear()
	{
		actionSets.Clear();
		actionSets = null;
	}

	private string DebugActionSetList()
	{
		string text = "Sets:";
		actionSets.ForEachWithIndex(delegate(KeyValuePair<string, ActionSet> actionSet, int index)
		{
			text = text + " " + actionSet.Key;
		});
		return text;
	}

	protected void OnActionAllowed(string action, GameObject sender, object data)
	{
		if (this.ActionAllowed != null)
		{
			this.ActionAllowed(action, sender, data);
		}
	}

	protected void OnActionBlocked(string action, GameObject sender, object data)
	{
		if (this.ActionBlocked != null)
		{
			this.ActionBlocked(action, sender, data);
		}
	}
}
