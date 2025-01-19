namespace Gamespy.MetricsAndRankings
{
	public enum AtlasRequestResult
	{
		Success = 0,
		ConstructorError = 1,
		InvalidQueryId = 2,
		ErrorCreatingRequest = 3,
		HttpError = 4,
		ResponseParseError = 5,
		ErrorSendingRequest = 6,
		Error = 7
	}
}
