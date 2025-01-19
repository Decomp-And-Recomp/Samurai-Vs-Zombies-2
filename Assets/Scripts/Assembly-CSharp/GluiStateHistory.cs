using System.Collections.Generic;

public class GluiStateHistory
{
	protected List<GluiStateHistoryNode> stateHistory = new List<GluiStateHistoryNode>();

	private object mContext;

	public List<GluiStateHistoryNode> States
	{
		get
		{
			return stateHistory;
		}
	}

	public GluiStateHistoryNode this[int i]
	{
		get
		{
			return stateHistory[i];
		}
		set
		{
			stateHistory[i] = value;
		}
	}

	public int Count
	{
		get
		{
			return stateHistory.Count;
		}
	}

	public GluiStateHistoryNode Current
	{
		get
		{
			if (stateHistory.Count == 0)
			{
				return null;
			}
			return stateHistory[stateHistory.Count - 1];
		}
	}

	public void PushCurrentContext(object context)
	{
		mContext = context;
	}

	public object PopCurrentContext()
	{
		object result = mContext;
		mContext = null;
		return result;
	}

	public void Pop()
	{
		if (stateHistory.Count > 0)
		{
			GluiStateHistoryNode gluiStateHistoryNode = stateHistory[stateHistory.Count - 1];
			gluiStateHistoryNode.state.DestroyState();
			stateHistory.Remove(gluiStateHistoryNode);
		}
	}

	public GluiStateHistoryNode Push(GluiStateHistoryNode newNode)
	{
		stateHistory.Add(newNode);
		return newNode;
	}

	public List<GluiStateHistoryNode> Remove(string action, bool removeAllInstances = false, bool keepCurrent = true)
	{
		List<GluiStateHistoryNode> list = new List<GluiStateHistoryNode>();
		if (removeAllInstances)
		{
			list = stateHistory.FindAll((GluiStateHistoryNode node) => node.state.HandlesAction(action));
		}
		else
		{
			GluiStateHistoryNode gluiStateHistoryNode = stateHistory.Find((GluiStateHistoryNode node) => node.state.HandlesAction(action));
			if (gluiStateHistoryNode != null)
			{
				list.Add(gluiStateHistoryNode);
			}
		}
		if (keepCurrent)
		{
			list.Remove(Current);
		}
		list.ForEach(delegate(GluiStateHistoryNode nodeToRemove)
		{
			stateHistory.Remove(nodeToRemove);
		});
		return list;
	}

	public GluiStateHistoryNode Find(string action)
	{
		return stateHistory.Find((GluiStateHistoryNode node) => node.state.HandlesAction(action));
	}

	public bool AttemptInsert(GluiStateHistoryNode newNode)
	{
		int num = stateHistory.FindIndex((GluiStateHistoryNode node) => node.priority > newNode.priority);
		if (num == -1)
		{
			return false;
		}
		stateHistory.Insert(num, newNode);
		return true;
	}

	public GluiStateHistoryNode FindHistoryNodeByState(GluiStateBase state)
	{
		for (int num = stateHistory.Count - 1; num >= 0; num--)
		{
			GluiStateHistoryNode gluiStateHistoryNode = stateHistory[num];
			if (gluiStateHistoryNode.state == state)
			{
				return gluiStateHistoryNode;
			}
		}
		return null;
	}

	public void AssignContextToState(GluiStateBase state, object context)
	{
		GluiStateHistoryNode gluiStateHistoryNode = FindHistoryNodeByState(state);
		if (gluiStateHistoryNode != null)
		{
			gluiStateHistoryNode.context = context;
		}
	}

	public object GetContextForState(GluiStateBase state)
	{
		GluiStateHistoryNode gluiStateHistoryNode = FindHistoryNodeByState(state);
		if (gluiStateHistoryNode != null)
		{
			return gluiStateHistoryNode.context;
		}
		return null;
	}

	public override string ToString()
	{
		string text = string.Format("[GluiStateHistory: Count={0}, Current={1}]", Count, Current);
		foreach (GluiStateHistoryNode item in stateHistory)
		{
			string text2 = text;
			text = text2 + " [" + item.state.name + "," + item.priority + "] ";
		}
		return text;
	}
}
