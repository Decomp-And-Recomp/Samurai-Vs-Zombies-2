using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityThreading
{
	public abstract class DispatcherBase : IDisposable
	{
		protected Queue<TaskBase> taskQueue = new Queue<TaskBase>();

		protected ManualResetEvent dataEvent = new ManualResetEvent(false);

		public int TaskCount
		{
			get
			{
				lock (taskQueue)
				{
					return taskQueue.Count;
				}
			}
		}

		public DispatcherBase()
		{
		}

		public Task<T> Dispatch<T>(Func<T> function)
		{
			CheckAccessLimitation();
			Task<T> task = new Task<T>(function);
			AddTask(task);
			return task;
		}

		public Task Dispatch(Action action)
		{
			CheckAccessLimitation();
			Task task = new Task(action);
			AddTask(task);
			return task;
		}

		internal void AddTask(TaskBase task)
		{
			lock (taskQueue)
			{
				taskQueue.Enqueue(task);
			}
			dataEvent.Set();
		}

		internal void AddTasks(IEnumerable<TaskBase> tasks)
		{
			lock (taskQueue)
			{
				foreach (TaskBase task in tasks)
				{
					taskQueue.Enqueue(task);
				}
			}
			dataEvent.Set();
		}

		internal IEnumerable<TaskBase> SplitTasks(int divisor)
		{
			if (divisor == 0)
			{
				divisor = 2;
			}
			int count = TaskCount / divisor;
			return IsolateTasks(count);
		}

		internal IEnumerable<TaskBase> IsolateTasks(int count)
		{
			List<TaskBase> list = new List<TaskBase>();
			if (count == 0)
			{
				count = taskQueue.Count;
			}
			lock (taskQueue)
			{
				for (int i = 0; i < count; i++)
				{
					if (taskQueue.Count == 0)
					{
						break;
					}
					list.Add(taskQueue.Dequeue());
				}
			}
			if (TaskCount == 0)
			{
				dataEvent.Reset();
			}
			return list;
		}

		protected abstract void CheckAccessLimitation();

		public virtual void Dispose()
		{
			while (true)
			{
				TaskBase taskBase;
				lock (taskQueue)
				{
					if (taskQueue.Count != 0)
					{
						taskBase = taskQueue.Dequeue();
						goto IL_003f;
					}
				}
				break;
				IL_003f:
				taskBase.Dispose();
			}
			dataEvent.Close();
			dataEvent = null;
		}
	}
}
