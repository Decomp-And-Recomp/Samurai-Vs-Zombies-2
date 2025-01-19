using System.Runtime.InteropServices;

namespace Glu.Flurry
{
	public class FlurryAppCircle
	{
		private const string importName = "flurryplugin";

		public static void SetEnabled(bool enabled)
		{
		}

		public static void SetReengagementEnabled(bool enabled)
		{
		}

		[DllImport("flurryplugin")]
		private static extern void Glu_Flurry_SetAppCircleEnabled(bool enabled);

		[DllImport("flurryplugin")]
		private static extern void Glu_Flurry_SetReengagementEnabled(bool enabled);
	}
}
