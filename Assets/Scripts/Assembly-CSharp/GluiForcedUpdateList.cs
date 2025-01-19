using System;
using UnityEngine;

[Serializable]
public class GluiForcedUpdateList
{
	public enum TargetTrigger
	{
		GluiElement_EnableByValue = 0,
		GluiElements_Any = 1
	}

	public TargetTrigger[] thingsToUpdate;

	public GameObject[] additionalObjectsToUpdate;

	public void TriggerOnTarget(GameObject thisGameObject, bool modifyChildren)
	{
		TargetTrigger[] array = thingsToUpdate;
		for (int i = 0; i < array.Length; i++)
		{
			switch (array[i])
			{
			case TargetTrigger.GluiElement_EnableByValue:
			{
				Component[] componentsInChildren2 = thisGameObject.GetComponentsInChildren(typeof(GluiElement_EnableByValue), modifyChildren);
				Component[] array3 = componentsInChildren2;
				for (int k = 0; k < array3.Length; k++)
				{
					GluiElement_EnableByValue gluiElement_EnableByValue = (GluiElement_EnableByValue)array3[k];
					if (gluiElement_EnableByValue.gameObject.activeInHierarchy)
					{
						gluiElement_EnableByValue.ForceUpdate();
					}
				}
				break;
			}
			case TargetTrigger.GluiElements_Any:
			{
				Component[] componentsInChildren = thisGameObject.GetComponentsInChildren(typeof(GluiElement_Base), modifyChildren);
				Component[] array2 = componentsInChildren;
				for (int j = 0; j < array2.Length; j++)
				{
					GluiElement_Base gluiElement_Base = (GluiElement_Base)array2[j];
					if (gluiElement_Base.gameObject.activeInHierarchy)
					{
						gluiElement_Base.ForceUpdate();
					}
				}
				break;
			}
			}
		}
	}

	public void TriggerAdditionalObjects()
	{
		GameObject[] array = additionalObjectsToUpdate;
		foreach (GameObject thisGameObject in array)
		{
			TriggerOnTarget(thisGameObject, false);
		}
	}
}
