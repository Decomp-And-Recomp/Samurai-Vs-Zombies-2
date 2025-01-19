using System;
using System.Threading;

namespace UnityThreading
{
	public abstract class ThreadBase : IDisposable
	{
		protected Dispatcher targetDispatcher;

		protected Thread thread;

		protected ManualResetEvent exitEvent = new ManualResetEvent(false);

		[ThreadStatic]
		private static ThreadBase currentThread;

		public static ThreadBase CurrentThread
		{
			get
			{
				return currentThread;
			}
		}

		public bool IsAlive
		{
			get
			{
				return thread != null && thread.IsAlive;
			}
		}

		public bool ShouldStop
		{
			get
			{
				return exitEvent.WaitOne(0);
			}
		}

		public ThreadBase()
			: this(true)
		{
		}

		public ThreadBase(bool autoStartThread)
			: this(Dispatcher.Current, autoStartThread)
		{
		}

		public ThreadBase(Dispatcher targetDispatcher)
			: this(targetDispatcher, true)
		{
			this.targetDispatcher = targetDispatcher;
		}

		public ThreadBase(Dispatcher targetDispatcher, bool autoStartThread)
		{
			this.targetDispatcher = targetDispatcher;
			if (autoStartThread)
			{
				Start();
			}
		}

		public void Start()
		{
			if (thread != null)
			{
				Abort();
			}
			exitEvent.Reset();
			thread = new Thread(DoInternal);
			thread.Start();
		}

		public void Exit()
		{
			if (thread != null)
			{
				exitEvent.Set();
			}
		}

		public void Abort()
		{
			Exit();
			if (thread != null)
			{
				thread.Join();
			}
		}

		public void AbortWaitForSeconds(float seconds)
		{
			Exit();
			if (thread != null)
			{
				thread.Join((int)(seconds * 1000f));
				if (thread.IsAlive)
				{
					thread.Abort();
				}
			}
		}

		public Task<T> Dispatch<T>(Func<T> function)
		{
			return targetDispatcher.Dispatch(function);
		}

		public T DispatchAndWait<T>(Func<T> function)
		{
			Task<T> task = Dispatch(function);
			task.Wait();
			return task.Result;
		}

		public T DispatchAndWait<T>(Func<T> function, float timeOutSeconds)
		{
			Task<T> task = Dispatch(function);
			task.WaitForSeconds(timeOutSeconds);
			return task.Result;
		}

		public Task Dispatch(Action action)
		{
			return targetDispatcher.Dispatch(action);
		}

		public void DispatchAndWait(Action action)
		{
			Task task = Dispatch(action);
			task.Wait();
		}

		public void DispatchAndWait(Action action, float timeOutSeconds)
		{
			Task task = Dispatch(action);
			task.WaitForSeconds(timeOutSeconds);
		}

		protected void DoInternal()
		{
			currentThread = this;
			Do();
		}

		protected abstract void Do();

		public virtual void Dispose()
		{
			AbortWaitForSeconds(1f);
		}
	}
}
