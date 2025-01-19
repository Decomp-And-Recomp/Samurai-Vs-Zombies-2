using System;
using UnityEngine;

[AddComponentMenu("Glui State/State Empty")]
public class GluiState_Empty : GluiStateBase
{
	public override void InitState(Action<GameObject> whenDone)
	{
		whenDone(null);
	}

	public override void DestroyState()
	{
	}
}
