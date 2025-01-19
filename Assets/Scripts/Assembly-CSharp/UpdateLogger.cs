public class UpdateLogger : LoggerSingleton<UpdateLogger>
{
	public UpdateLogger()
	{
		LoggerSingleton<UpdateLogger>.SetLoggerName("Update");
	}
}
