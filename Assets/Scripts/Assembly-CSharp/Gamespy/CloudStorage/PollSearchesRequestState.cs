namespace Gamespy.CloudStorage
{
	public enum PollSearchesRequestState
	{
		SearchRequestPending = 0,
		UpdateReceived = 1,
		PollingInterval = 2,
		ShuttingDown = 3,
		CompleteWithError = 4,
		Complete = 5
	}
}
