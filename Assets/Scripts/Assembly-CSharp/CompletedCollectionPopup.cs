using UnityEngine;

public class CompletedCollectionPopup : MonoBehaviour, IGluiActionHandler
{
	public GluiStandardButtonContainer FacebookButton;

	private void Start()
	{
		/*if (FacebookButton != null)
		{
			FacebookButton.gameObject.SetActive(true);
		}*/
		ApplicationUtilities.MakePlayHavenContentRequest("collection_complete");
	}

	public bool HandleAction(string action, GameObject sender, object data)
	{
		if (action == "FACEBOOK_COLLECTION_COMPLETE")
		{
			CollectionItemSchema collectionItem_InConflict = Singleton<Profile>.Instance.MultiplayerData.MultiplayerGameSessionData.collectionItem_InConflict;
			CollectionSchema collectionSet;
			Singleton<Profile>.Instance.MultiplayerData.GetCollectionItemData(collectionItem_InConflict.CollectionID, out collectionSet);
			if (collectionSet != null)
			{
				string description = string.Format(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookCollectionMessage"), StringUtils.GetStringFromStringRef(collectionSet.displayName));
				SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.AndroidFacebookFeed(StringUtils.GetStringFromStringRef("LocalizedStrings", "FacebookCollectionTitle"), description, FacebookButton.gameObject, string.Empty, string.Empty);
			}
			else
			{
				FacebookButton.gameObject.SetActive(false);
			}
			return true;
		}
		return false;
	}

	private void onFeedPost(string postId)
	{
		if (!string.IsNullOrEmpty(postId))
		{
			FacebookButton.gameObject.SetActive(false);
		}
	}
}
