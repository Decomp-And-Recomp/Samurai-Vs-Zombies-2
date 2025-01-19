namespace Gamespy.Matchmaking
{
	public enum SearchRequestState
	{
		Begin = 0,
		DnsPending = 1,
		InitializeSocket = 2,
		SocketConnectPending = 3,
		StartSendSearchRequest = 4,
		SearchRequestPending = 5,
		StartReceiveSearchResponse = 6,
		PollingInterval = 7,
		ReceiveSearchResponsePending = 8,
		UpdateReceived = 9,
		ShuttingDown = 10,
		CompleteWithError = 11,
		Complete = 12
	}
}
