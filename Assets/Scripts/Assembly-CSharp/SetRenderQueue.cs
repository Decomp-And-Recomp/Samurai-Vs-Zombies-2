using UnityEngine;

[RequireComponent(typeof(Renderer))]
[AddComponentMenu("Effects/SetRenderQueue")]
public class SetRenderQueue : MonoBehaviour
{
	public bool affectOnlyThisObject;

	public int queue = 1;

	public int[] queues;

	protected void Start()
	{
		if (!base.GetComponent<Renderer>() || !base.GetComponent<Renderer>().sharedMaterial || queues == null)
		{
			return;
		}
		if (affectOnlyThisObject)
		{
			base.GetComponent<Renderer>().material.renderQueue = queue;
		}
		else
		{
			base.GetComponent<Renderer>().sharedMaterial.renderQueue = queue;
		}
		for (int i = 0; i < queues.Length && i < base.GetComponent<Renderer>().sharedMaterials.Length; i++)
		{
			if (affectOnlyThisObject)
			{
				base.GetComponent<Renderer>().materials[i].renderQueue = queues[i];
			}
			else
			{
				base.GetComponent<Renderer>().sharedMaterials[i].renderQueue = queues[i];
			}
		}
	}
}
