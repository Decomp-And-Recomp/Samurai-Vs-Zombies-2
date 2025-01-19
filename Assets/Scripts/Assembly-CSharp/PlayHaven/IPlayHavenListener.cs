namespace PlayHaven
{
	public interface IPlayHavenListener
	{
		event RequestCompletedHandler OnRequestCompleted;

		event BadgeUpdateHandler OnBadgeUpdate;

		event RewardTriggerHandler OnRewardGiven;

		event PurchasePresentedTriggerHandler OnPurchasePresented;

		event SimpleDismissHandler OnDismissCrossPromotionWidget;

		event DismissHandler OnDismissContent;

		event WillDisplayContentHandler OnWillDisplayContent;

		event DidDisplayContentHandler OnDidDisplayContent;

		event SuccessHandler OnSuccessOpenRequest;

		event SuccessHandler OnSuccessPreloadRequest;

		event ErrorHandler OnErrorOpenRequest;

		event ErrorHandler OnErrorCrossPromotionWidget;

		event ErrorHandler OnErrorContentRequest;

		event ErrorHandler OnErrorMetadataRequest;

		void NotifyRequestCompleted(int requestId);

		void NotifyOpenSuccess(int requestId);

		void NotifyPreloadSuccess(int requestId);

		void NotifyOpenError(int requestId, Error error);

		void NotifyWillDisplayContent(int requestId);

		void NotifyDidDisplayContent(int requestId);

		void NotifyBadgeUpdate(int requestId, string badge);

		void NotifyRewardGiven(int requestId, Reward reward);

		void NotifyPurchasePresented(int requestId, Purchase purchase);

		void NotifyCrossPromotionWidgetDismissed();

		void NotifyCrossPromotionWidgetError(int requestId, Error error);

		void NotifyContentDismissed(int requestId, DismissType dismissType);

		void NotifyContentError(int requestId, Error error);

		void NotifyMetaDataError(int requestId, Error error);
	}
}
