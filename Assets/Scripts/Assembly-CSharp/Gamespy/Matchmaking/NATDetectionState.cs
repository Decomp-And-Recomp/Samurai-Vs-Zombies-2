namespace Gamespy.Matchmaking
{
	public enum NATDetectionState
	{
		Begin = 0,
		DnsPending = 1,
		InitializeSocket = 2,
		BeginLocalIPConnect = 3,
		BeginLocalIPSend = 4,
		LocalIPPending = 5,
		EndLocalIP = 6,
		StartSendingDetectionPackets = 7,
		DetectionResponsePending = 8,
		DetermineNATConfiguration = 9,
		CompleteWithError = 10,
		Complete = 11
	}
}
