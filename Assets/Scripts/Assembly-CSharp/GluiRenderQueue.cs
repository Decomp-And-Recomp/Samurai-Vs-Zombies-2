using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Glui/RenderQueue")]
public class GluiRenderQueue : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private int renderQueue = 3000;

	public int RenderQueue
	{
		get
		{
			return renderQueue;
		}
		set
		{
			renderQueue = value;
			GluiWidget component = GetComponent<GluiWidget>();
			component.Refresh();
		}
	}
}
