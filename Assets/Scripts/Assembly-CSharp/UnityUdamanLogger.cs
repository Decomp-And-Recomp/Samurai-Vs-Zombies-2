public class UnityUdamanLogger : IUdamanLogger
{
	public void LogMessage(string message)
	{
	}

	public void LogMessage(LoggerWarningLevel warningLevel, string message)
	{
		switch (warningLevel)
		{
		case LoggerWarningLevel.Error:
			break;
		case LoggerWarningLevel.Message:
			break;
		case LoggerWarningLevel.Warning:
			break;
		}
	}
}
