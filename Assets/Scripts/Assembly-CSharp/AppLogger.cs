public sealed class AppLogger : LoggerSingleton<AppLogger>
{
	public AppLogger()
	{
		LoggerSingleton<AppLogger>.SetLoggerName("App");
	}
}
