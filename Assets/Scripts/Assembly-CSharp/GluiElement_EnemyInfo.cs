using UnityEngine;

public class GluiElement_EnemyInfo : GluiElement_DataAdaptor<DataAdaptor_EnemyInfo>, IGluiActionHandler
{
	public override void OnSafeEnable()
	{
		SingletonMonoBehaviour<TutorialMain>.Instance.TutorialDone();
		base.OnSafeEnable();
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		return false;
	}
}
