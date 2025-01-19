using UnityEngine;

[ExecuteInEditMode]
public class GluiBase : MonoBehaviour
{
	[SerializeField]
	private bool isEnabled = true;

	private bool isCreated;

	public bool Enabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			if (value != isEnabled)
			{
				isEnabled = value;
				OnEnableChanged();
			}
		}
	}

	protected virtual void OnCreate()
	{
	}

	protected virtual void OnReset()
	{
	}

	protected virtual void OnEnableChanged()
	{
	}

	public virtual void Refresh()
	{
	}

	protected virtual void Awake()
	{
	}

	public virtual void Start()
	{
		if (!isCreated)
		{
			isCreated = true;
			OnCreate();
		}
	}

	protected virtual void Reset()
	{
		OnReset();
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}
}
