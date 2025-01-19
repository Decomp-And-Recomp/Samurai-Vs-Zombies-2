using System.Collections.Generic;
using UnityEngine;

public class GluiPotentialStates : MonoBehaviour
{
	protected List<GluiStateBase> states = new List<GluiStateBase>();

	public void ScanForStates()
	{
		if (states.Count <= 0)
		{
			Component[] componentsInChildren = GetComponentsInChildren(typeof(GluiStateBase));
			Component[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				GluiStateBase item = (GluiStateBase)array[i];
				states.Add(item);
			}
		}
	}

	public GluiStateBase StateData(string state)
	{
		foreach (GluiStateBase state2 in states)
		{
			if (state2.name == state)
			{
				return state2;
			}
		}
		return null;
	}

	public GluiStateBase StateForAction(string action)
	{
		ScanForStates();
		foreach (GluiStateBase state in states)
		{
			if (state.HandlesAction(action))
			{
				return state;
			}
		}
		return null;
	}
}
