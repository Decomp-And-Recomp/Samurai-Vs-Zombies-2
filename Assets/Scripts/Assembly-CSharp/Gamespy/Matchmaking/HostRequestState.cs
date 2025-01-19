namespace Gamespy.Matchmaking
{
	public enum HostRequestState
	{
		Begin = 0,
		DnsPending = 1,
		BeginLocalIPConnect = 2,
		BeginLocalIPSend = 3,
		LocalIPPending = 4,
		EndLocalIP = 5,
		InitializeSocket = 6,
		ListedResponsePending = 7,
		Listed = 8,
		ClientMessageReceived = 9,
		CompleteWithError = 10,
		Complete = 11
	}
}
