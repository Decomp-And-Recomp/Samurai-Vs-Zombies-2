using System;
using UnityEngine;

[Serializable]
public abstract class GluiButtonAction : MonoBehaviour
{
	public static readonly int LabelWidth = 60;

	[SerializeField]
	public string State;

	public abstract string GetActionName();

	public abstract void OnEnterState();

	public abstract void OnLeaveState();
}
