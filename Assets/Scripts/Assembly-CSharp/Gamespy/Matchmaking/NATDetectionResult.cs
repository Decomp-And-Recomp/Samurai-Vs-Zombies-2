namespace Gamespy.Matchmaking
{
	public enum NATDetectionResult
	{
		Success = 0,
		DetectionFailed = 1,
		ConstructorError = 2,
		DnsException = 3,
		ResolveLocalHostAddressException = 4,
		DetectNATSocketConnectException = 5,
		DetectNATSendRequestException = 6,
		DetectNATResponseException = 7,
		UnknownError = 8
	}
}
