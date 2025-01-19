using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class SetRenderQueueGroup : MonoBehaviour
{
	public int minQueueValue = 2001;

	public int maxQueueValue = 2999;

	public bool affectAllObjects;

	public GameObject[] orderedList;

	private void Start()
	{
		UpdateQueue();
	}

	private void Update()
	{
	}

	private void UpdateQueue()
	{
		if (orderedList == null || orderedList.Length <= 0)
		{
			return;
		}
		int num = minQueueValue;
		for (int i = 0; i < orderedList.Length; i++)
		{
			if (orderedList[i] != null && orderedList[i].GetComponent<Renderer>() != null && orderedList[i].GetComponent<Renderer>().sharedMaterial != null)
			{
				if (affectAllObjects)
				{
					orderedList[i].GetComponent<Renderer>().sharedMaterial.renderQueue = num++;
				}
				else
				{
					orderedList[i].GetComponent<Renderer>().material.renderQueue = num++;
				}
			}
		}
	}
}
