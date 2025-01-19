using UnityEngine;

public class FlurryBehaviourScript : MonoBehaviour
{
	private void Awake()
	{
		base.enabled = false;
		base.useGUILayout = false;
	}

	private void OnDestroy()
	{
		CFlurry.StopSession();
	}
}
