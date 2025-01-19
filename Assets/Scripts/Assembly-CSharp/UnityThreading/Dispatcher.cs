using System;
using System.Threading;

namespace UnityThreading
{
	public class Dispatcher : DispatcherBase
	{
		[ThreadStatic]
		private static TaskBase currentTask;

		[ThreadStatic]
		internal static Dispatcher currentDispatcher;

		protected static Dispatcher mainDispatcher;

		public static TaskBase CurrentTask
		{
			get
			{
				if (currentTask == null)
				{
					throw new InvalidOperationException("No task is currently running.");
				}
				return currentTask;
			}
		}

		public static Dispatcher Current
		{
			get
			{
				if (currentDispatcher == null)
				{
					throw new InvalidOperationException("No Dispatcher found for the current thread, please create a new Dispatcher instance before calling this property.");
				}
				return currentDispatcher;
			}
			set
			{
				if (currentDispatcher != null)
				{
					currentDispatcher.Dispose();
				}
				currentDispatcher = value;
			}
		}

		public static Dispatcher Main
		{
			get
			{
				if (mainDispatcher == null)
				{
					throw new InvalidOperationException("No Dispatcher found for the main thread, please create a new Dispatcher instance before calling this property.");
				}
				return mainDispatcher;
			}
		}

		public static bool IsMainThread
		{
			get
			{
				return mainDispatcher == currentDispatcher;
			}
		}

		public Dispatcher()
			: this(true)
		{
		}

		internal Dispatcher(bool setThreadDefaults)
		{
			if (setThreadDefaults)
			{
				if (currentDispatcher != null)
				{
					throw new InvalidOperationException("Only one Dispatcher instance allowed per thread.");
				}
				currentDispatcher = this;
				if (mainDispatcher == null)
				{
					mainDispatcher = this;
				}
			}
		}

		public static Func<T> CreateSafeFunction<T>(Func<T> function)
		{
			return delegate
			{
				try
				{
					return function();
				}
				catch
				{
					CurrentTask.Abort();
					return default(T);
				}
			};
		}

		public static Action CreateSafeAction<T>(Action action)
		{
			return delegate
			{
				try
				{
					action();
				}
				catch
				{
					CurrentTask.Abort();
				}
			};
		}

		public void ProcessTasks()
		{
			if (dataEvent.WaitOne(0))
			{
				ProcessTasksInternal();
			}
		}

		public bool ProcessTasks(WaitHandle exitHandle)
		{
			if (WaitHandle.WaitAny(new WaitHandle[2] { exitHandle, dataEvent }) == 0)
			{
				return false;
			}
			ProcessTasksInternal();
			return true;
		}

		public bool ProcessNextTask()
		{
			lock (taskQueue)
			{
				if (taskQueue.Count == 0)
				{
					return false;
				}
				ProcessTask();
			}
			if (base.TaskCount == 0)
			{
				dataEvent.Reset();
			}
			return true;
		}

		public bool ProcessNextTask(WaitHandle exitHandle)
		{
			if (WaitHandle.WaitAny(new WaitHandle[2] { exitHandle, dataEvent }) == 0)
			{
				return false;
			}
			lock (taskQueue)
			{
				ProcessTask();
			}
			if (base.TaskCount == 0)
			{
				dataEvent.Reset();
			}
			return true;
		}

		private void ProcessTasksInternal()
		{
			lock (taskQueue)
			{
				while (taskQueue.Count != 0)
				{
					ProcessTask();
				}
			}
			if (base.TaskCount == 0)
			{
				dataEvent.Reset();
			}
		}

		private void ProcessTask()
		{
			if (taskQueue.Count != 0)
			{
				RunTask(taskQueue.Dequeue());
			}
		}

		internal void RunTask(TaskBase task)
		{
			TaskBase taskBase = currentTask;
			currentTask = task;
			currentTask.DoInternal();
			currentTask = taskBase;
		}

		protected override void CheckAccessLimitation()
		{
			if (currentDispatcher == this)
			{
				throw new InvalidOperationException("Dispatching a Task with the Dispatcher associated to the current thread is prohibited. You can run these Tasks without the need of a Dispatcher.");
			}
		}

		public override void Dispose()
		{
			while (true)
			{
				lock (taskQueue)
				{
					if (taskQueue.Count != 0)
					{
						currentTask = taskQueue.Dequeue();
						goto IL_0043;
					}
				}
				break;
				IL_0043:
				currentTask.Dispose();
			}
			dataEvent.Close();
			dataEvent = null;
			if (currentDispatcher == this)
			{
				currentDispatcher = null;
			}
			if (mainDispatcher == this)
			{
				mainDispatcher = null;
			}
		}
	}
}
