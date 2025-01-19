namespace Gamespy.Matchmaking
{
	public enum SendMessageRequestState
	{
		Begin = 0,
		DnsPending = 1,
		InitializeSocket = 2,
		SocketConnectPending = 3,
		StartSendMessageRequest = 4,
		SendMessageRequestPending = 5,
		ShuttingDown = 6,
		CompleteWithError = 7,
		Complete = 8
	}
}
