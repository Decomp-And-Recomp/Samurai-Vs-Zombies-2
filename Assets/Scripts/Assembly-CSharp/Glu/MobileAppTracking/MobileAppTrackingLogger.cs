namespace Glu.MobileAppTracking
{
	public class MobileAppTrackingLogger : LoggerSingleton<MobileAppTrackingLogger>
	{
		public MobileAppTrackingLogger()
		{
			LoggerSingleton<MobileAppTrackingLogger>.SetLoggerName("Package.MobileAppTracking");
		}
	}
}
