namespace Gamespy.CloudStorage
{
	public enum SakeRequestResult
	{
		Success = 0,
		SecretKeyInvalid = 1,
		ServiceDisabled = 2,
		DatabaseUnavailable = 3,
		LoginTicketInvalid = 4,
		LoginTicketExpired = 5,
		TableNotFound = 6,
		RecordNotFound = 7,
		FieldNotFound = 8,
		FieldTypeInvalid = 9,
		NoPermission = 10,
		RecordLimitReached = 11,
		NotRateable = 12,
		NotOwned = 13,
		FilterInvalid = 14,
		SortInvalid = 15,
		TargetFilterInvalid = 16,
		CertificateInvalid = 17,
		AlreadyReported = 18,
		RecordLocked = 19,
		ConstructorError = 20,
		ErrorCreatingRequest = 21,
		ErrorSendingRequest = 22,
		HttpError = 23,
		ResponseParseError = 24,
		NoFileDataError = 25,
		Error = 26
	}
}
