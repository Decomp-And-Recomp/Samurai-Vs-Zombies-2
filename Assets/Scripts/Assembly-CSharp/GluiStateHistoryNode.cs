public class GluiStateHistoryNode
{
	public GluiStateBase state;

	public object context;

	public int priority;

	public bool reverseTransition;

	public GluiStateHistoryNode(GluiStateBase state, int priority = 0, object context = null, bool reverseTransition = false)
	{
		this.context = context;
		this.state = state;
		this.priority = priority;
		this.reverseTransition = reverseTransition;
	}
}
