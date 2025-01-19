public interface IUdamanLogger
{
	void LogMessage(string message);

	void LogMessage(LoggerWarningLevel warningLevel, string message);
}
