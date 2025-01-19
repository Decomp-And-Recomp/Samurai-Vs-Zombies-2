public abstract class GluiButtonContainerBase : GluiWidget
{
	public delegate void OnButtonStateChanged(string buttonState);

	private bool selected;

	public OnButtonStateChanged onButtonStateChanged;

	public virtual bool Selected
	{
		get
		{
			return selected;
		}
		set
		{
			if (selected != value)
			{
				selected = value;
				OnSelectedChanged();
			}
		}
	}

	protected override ColliderType DefaultColliderType
	{
		get
		{
			return ColliderType.Auto_Box;
		}
	}

	public GluiButtonContainerBase()
	{
		base.Usable = true;
	}

	public override void Start()
	{
		base.Start();
		base.Visible = false;
	}

	public abstract string[] GetStates();

	public abstract string GetCurrentState();

	protected virtual void OnSelectedChanged()
	{
	}
}
