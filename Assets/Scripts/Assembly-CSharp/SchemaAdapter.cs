using UnityEngine;

public abstract class SchemaAdapter<T> : MonoBehaviour
{
	public DataBundleRecordKey record;

	public T Schema { get; protected set; }

	public abstract GameObject Deserialize(DataBundleRecordKey record);

	public abstract void Serialize(GameObject obj);

	protected virtual void Awake()
	{
		if (Application.isEditor && !Application.isPlaying)
		{
		}
	}

	protected virtual void Start()
	{
	}
}
