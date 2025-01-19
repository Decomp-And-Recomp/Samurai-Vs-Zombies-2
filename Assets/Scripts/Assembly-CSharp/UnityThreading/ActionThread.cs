using System;

namespace UnityThreading
{
	public class ActionThread : ThreadBase
	{
		private Action<ActionThread> action;

		public ActionThread(Action<ActionThread> action)
			: this(action, true)
		{
		}

		public ActionThread(Action<ActionThread> action, bool autoStartThread)
			: base(Dispatcher.Current, false)
		{
			this.action = action;
			if (autoStartThread)
			{
				Start();
			}
		}

		protected override void Do()
		{
			action(this);
		}
	}
}
