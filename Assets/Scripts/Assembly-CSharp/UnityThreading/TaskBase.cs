using System;
using System.Threading;

namespace UnityThreading
{
	public abstract class TaskBase
	{
		private ManualResetEvent abortEvent = new ManualResetEvent(false);

		private ManualResetEvent endedEvent = new ManualResetEvent(false);

		private bool hasStarted;

		public bool ShouldAbort
		{
			get
			{
				return abortEvent.WaitOne(0);
			}
		}

		public bool HasEnded
		{
			get
			{
				return endedEvent.WaitOne(0);
			}
		}

		public bool IsSucceeded
		{
			get
			{
				return endedEvent.WaitOne(0) && !abortEvent.WaitOne(0);
			}
		}

		public bool IsFailed
		{
			get
			{
				return endedEvent.WaitOne(0) && abortEvent.WaitOne(0);
			}
		}

		protected abstract void Do();

		public void Abort()
		{
			abortEvent.Set();
		}

		public void AbortWait()
		{
			Abort();
			Wait();
		}

		public void AbortWaitForSeconds(float seconds)
		{
			Abort();
			WaitForSeconds(seconds);
		}

		public void Wait()
		{
			endedEvent.WaitOne();
		}

		public void WaitForSeconds(float seconds)
		{
			endedEvent.WaitOne(TimeSpan.FromSeconds(seconds));
		}

		public abstract TResult Wait<TResult>();

		public abstract TResult WaitForSeconds<TResult>(float seconds);

		public abstract TResult WaitForSeconds<TResult>(float seconds, TResult defaultReturnValue);

		internal void DoInternal()
		{
			hasStarted = true;
			if (!ShouldAbort)
			{
				Do();
			}
			endedEvent.Set();
		}

		public void Dispose()
		{
			if (hasStarted)
			{
				Wait();
			}
			endedEvent.Close();
			abortEvent.Close();
		}
	}
}
