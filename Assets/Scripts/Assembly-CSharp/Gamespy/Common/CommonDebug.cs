using System.Diagnostics;

namespace Gamespy.Common
{
	public static class CommonDebug
	{
		[Conditional("LOGGING_LEVEL_ERROR")]
		public static void Log(string logString)
		{
		}
	}
}
