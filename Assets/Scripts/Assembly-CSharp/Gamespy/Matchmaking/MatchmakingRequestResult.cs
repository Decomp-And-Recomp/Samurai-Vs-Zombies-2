namespace Gamespy.Matchmaking
{
	public enum MatchmakingRequestResult
	{
		Success = 0,
		ConstructorError = 1,
		SearchCountTooHigh = 2,
		DnsException = 3,
		ResolveLocalHostAddressException = 4,
		SearchSocketConnectException = 5,
		SearchSendRequestException = 6,
		SendMessageSocketConnectException = 7,
		SendMessageSendRequestException = 8,
		SendMessageRequestTooLong = 9,
		SearchResponseException = 10,
		SubscribeResponseException = 11,
		DecryptSearchResponseException = 12,
		MethodNotAvailable = 13,
		UnknownError = 14
	}
}
