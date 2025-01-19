using UnityEngine;

public class GluiScrollListSetRenderQueue : MonoBehaviour
{
	public static readonly int ScrollListRenderQueueValue = 3001;

	private void Start()
	{
		SetRenderQueue(base.gameObject);
	}

	private void Update()
	{
		SetRenderQueue(base.gameObject);
	}

	private void SetRenderQueue(GameObject obj)
	{
		GluiWidget[] components = obj.GetComponents<GluiWidget>();
		if (components != null && components.Length > 0)
		{
			GluiWidget[] array = components;
			foreach (GluiWidget gluiWidget in array)
			{
				gluiWidget.RenderQueue = ScrollListRenderQueueValue;
			}
		}
		else if (obj.GetComponent<Renderer>() != null && obj.GetComponent<Renderer>().material != null && obj.GetComponent<Renderer>().material.renderQueue != ScrollListRenderQueueValue)
		{
			obj.GetComponent<Renderer>().material.renderQueue = ScrollListRenderQueueValue;
		}
		for (int j = 0; j < obj.transform.childCount; j++)
		{
			SetRenderQueue(obj.transform.GetChild(j).gameObject);
		}
	}
}
