using System.Collections;
using UnityEngine;

public class GluiDelayedAction : MonoBehaviour
{
	private float delay;

	private string action;

	private GameObject sender;

	private bool enableInput;

	public static void Create(string action, float delay, GameObject sender, bool enableInput)
	{
		GameObject gameObject = new GameObject("DelayedAction");
		Object.DontDestroyOnLoad(gameObject);
		GluiDelayedAction gluiDelayedAction = gameObject.AddComponent<GluiDelayedAction>();
		gluiDelayedAction.action = action;
		gluiDelayedAction.delay = delay;
		gluiDelayedAction.sender = sender;
		gluiDelayedAction.enableInput = enableInput;
		gluiDelayedAction.StartCoroutine(gluiDelayedAction.Run());
	}

	private IEnumerator Run()
	{
		yield return new WaitForSeconds(delay);
		if (enableInput)
		{
			SingletonMonoBehaviour<InputManager>.Instance.InputEnabled = true;
		}
		GluiActionSender.SendGluiAction(action, sender, null);
		Object.Destroy(base.gameObject);
	}
}
