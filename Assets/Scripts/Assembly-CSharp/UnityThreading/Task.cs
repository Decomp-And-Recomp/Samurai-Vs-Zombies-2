using System;

namespace UnityThreading
{
	public class Task : TaskBase
	{
		private Action action;

		public Task(Action action)
		{
			this.action = action;
		}

		protected override void Do()
		{
			action();
		}

		public override TResult Wait<TResult>()
		{
			throw new InvalidOperationException("This task type does not support return values.");
		}

		public override TResult WaitForSeconds<TResult>(float seconds)
		{
			throw new InvalidOperationException("This task type does not support return values.");
		}

		public override TResult WaitForSeconds<TResult>(float seconds, TResult defaultReturnValue)
		{
			throw new InvalidOperationException("This task type does not support return values.");
		}
	}
	public class Task<T> : TaskBase
	{
		private Func<T> function;

		private T result;

		public T Result
		{
			get
			{
				if (!base.HasEnded)
				{
					Wait();
				}
				return result;
			}
		}

		public Task(Func<T> function)
		{
			this.function = function;
		}

		protected override void Do()
		{
			result = function();
		}

		public override TResult Wait<TResult>()
		{
			return (TResult)(object)Result;
		}

		public override TResult WaitForSeconds<TResult>(float seconds)
		{
			return ((TaskBase)this).WaitForSeconds(seconds, default(TResult));
		}

		public override TResult WaitForSeconds<TResult>(float seconds, TResult defaultReturnValue)
		{
			if (!base.HasEnded)
			{
				WaitForSeconds(seconds);
			}
			if (base.IsSucceeded)
			{
				return (TResult)(object)result;
			}
			return defaultReturnValue;
		}
	}
}
