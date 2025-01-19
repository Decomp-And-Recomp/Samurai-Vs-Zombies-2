using System;
using System.Threading;

namespace UnityThreading
{
	public class TaskDistributor : DispatcherBase
	{
		private TaskWorker[] workerThreads;

		private static TaskDistributor mainTaskDistributor;

		internal WaitHandle NewDataWaitHandle
		{
			get
			{
				return dataEvent;
			}
		}

		public static TaskDistributor Main
		{
			get
			{
				if (mainTaskDistributor == null)
				{
					throw new InvalidOperationException("No default TaskDistributor found, please create a new TaskDistributor instance before calling this property.");
				}
				return mainTaskDistributor;
			}
		}

		public TaskDistributor()
			: this(0)
		{
		}

		public TaskDistributor(int workerThreadCount)
			: this(workerThreadCount, true)
		{
		}

		public TaskDistributor(int workerThreadCount, bool autoStart)
		{
			if (workerThreadCount <= 0)
			{
				workerThreadCount = 1;
			}
			workerThreads = new TaskWorker[workerThreadCount];
			lock (workerThreads)
			{
				for (int i = 0; i < workerThreadCount; i++)
				{
					workerThreads[i] = new TaskWorker(this);
				}
			}
			if (mainTaskDistributor == null)
			{
				mainTaskDistributor = this;
			}
			if (autoStart)
			{
				Start();
			}
		}

		public void Start()
		{
			lock (workerThreads)
			{
				for (int i = 0; i < workerThreads.Length; i++)
				{
					if (!workerThreads[i].IsAlive)
					{
						workerThreads[i].Dispatcher.AddTasks(SplitTasks(workerThreads.Length));
						workerThreads[i].Start();
					}
				}
			}
		}

		internal void FillTasks(Dispatcher target)
		{
			target.AddTasks(IsolateTasks(1));
		}

		protected override void CheckAccessLimitation()
		{
			if (ThreadBase.CurrentThread != null && ThreadBase.CurrentThread is TaskWorker && ((TaskWorker)ThreadBase.CurrentThread).TaskDistributor == this)
			{
				throw new InvalidOperationException("Access to TaskDistributor prohibited when called from inside a TaskDistributor thread. Dont dispatch new Tasks through the same TaskDistributor. If you want to distribute new tasks create a new TaskDistributor and use the new created instance. Remember to dispose the new instance to prevent thread spamming.");
			}
		}

		public override void Dispose()
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
			lock (workerThreads)
			{
				for (int i = 0; i < workerThreads.Length; i++)
				{
					workerThreads[i].Dispose();
				}
				workerThreads = new TaskWorker[0];
			}
			dataEvent.Close();
			dataEvent = null;
			if (mainTaskDistributor == this)
			{
				mainTaskDistributor = null;
			}
		}
	}
}
