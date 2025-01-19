using UnityEngine;

[AddComponentMenu("Glui Agent/Agent Base")]
public abstract class GluiAgentBase : MonoBehaviour
{
	public enum Order
	{
		Enable = 0,
		Disable = 1,
		Kill = 2,
		Enable_Reverse = 3,
		Redraw = 4,
		None = 5
	}

	public string Agency;

	public string ID;

	private string dataKey;

	public virtual void Awake()
	{
		SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.Add(this);
	}

	public virtual void OnDestroy()
	{
		if (SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance != null)
		{
			SingletonSpawningMonoBehaviour<GluiAgent_CentralDispatch>.Instance.Remove(this);
		}
	}

	public abstract bool HandleOrder(GluiOrderPacket orderPacket);
}
