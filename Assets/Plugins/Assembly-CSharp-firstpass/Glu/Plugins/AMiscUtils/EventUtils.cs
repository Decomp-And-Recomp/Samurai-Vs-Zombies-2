using System;

namespace Glu.Plugins.AMiscUtils
{
	public static class EventUtils
	{
		public static void Raise<TEventArgs>(this EventHandler<TEventArgs> eventToTrigger, object sender, TEventArgs eventArgs) where TEventArgs : EventArgs
		{
			if (eventToTrigger != null)
			{
				eventToTrigger(sender, eventArgs);
			}
		}
	}
}
