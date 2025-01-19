using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
	public class QueuedData<TD>
	{
		public TD data;

		public int iPriority;
	}

	private int LowestPriority;

	private int HighestPriority;

	private List<QueuedData<T>> mList;

	public bool IsEmpty
	{
		get
		{
			if (mList != null)
			{
				return TotalItemsInQueue <= 0;
			}
			return true;
		}
	}

	public int TotalItemsInQueue
	{
		get
		{
			int num = 0;
			for (int i = 0; i < mList.Count; i++)
			{
				if (mList[i] != null)
				{
					num++;
				}
			}
			return num;
		}
	}

	public PriorityQueue()
	{
		mList = new List<QueuedData<T>>();
		mList.Clear();
		LowestPriority = 0;
		HighestPriority = -1;
	}

	public void Enqueue(T element)
	{
		Enqueue(element, LowestPriority + 2);
	}

	public void Enqueue(T element, PriorityQueueType pt)
	{
		int iPriority = 0;
		switch (pt)
		{
		case PriorityQueueType.Highest:
			iPriority = GetHighestPriority();
			break;
		case PriorityQueueType.Middle:
		{
			int highestPriority = GetHighestPriority();
			int num = LowestPriority - highestPriority;
			int num2 = highestPriority + num / 2;
			iPriority = num2;
			break;
		}
		case PriorityQueueType.Lowest:
			iPriority = LowestPriority + 2;
			break;
		}
		Enqueue(element, iPriority);
	}

	public void Enqueue(T element, int iPriority)
	{
		iPriority = Math.Max(0, iPriority);
		if (HighestPriority == -1)
		{
			HighestPriority = iPriority;
		}
		if (iPriority < HighestPriority)
		{
			HighestPriority = iPriority;
		}
		if (iPriority > mList.Count - 1)
		{
			LowestPriority = iPriority;
			int num = iPriority - (mList.Count - 1);
			for (int i = 0; i < num; i++)
			{
				mList.Add(null);
			}
		}
		QueuedData<T> queuedData = new QueuedData<T>();
		queuedData.data = element;
		queuedData.iPriority = iPriority;
		if (mList[iPriority] != null)
		{
			mList.Insert(iPriority, queuedData);
		}
		else
		{
			mList[iPriority] = queuedData;
		}
	}

	public T Dequeue()
	{
		int highestPriority = GetHighestPriority();
		QueuedData<T> queuedData = new QueuedData<T>();
		queuedData = mList[highestPriority];
		mList.RemoveAt(highestPriority);
		return queuedData.data;
	}

	public int GetHighestPriority()
	{
		int result = 0;
		for (int i = 0; i < mList.Count; i++)
		{
			if (mList[i] != null)
			{
				return i;
			}
		}
		return result;
	}

	public T GetValueByPriority(int iPriority)
	{
		return mList[iPriority].data;
	}
}
