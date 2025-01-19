using UnityEngine;

[AddComponentMenu("Samurai Data/MultiplayerOpponentsData")]
public class MultiplayerOpponentsData : SafeEnable_Monobehaviour, IGluiDataSource
{
	public bool ShowingFriends;

	public void Get_GluiData(string dataFilterKey, string dataFilterKeySecondary, GluiDataScan_AdditionalParameters additionalParameters, out object[] records)
	{
		switch (dataFilterKey)
		{
		case "OpponentsForSelectedCollectibleCard":
		{
			MultiplayerCollectionStatusQueryResponse multiplayerCollectionStatusQueryResponse = SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData("MULTIPLAYER_OPPONENT_RECORDS") as MultiplayerCollectionStatusQueryResponse;
			if (multiplayerCollectionStatusQueryResponse != null && multiplayerCollectionStatusQueryResponse.records != null)
			{
				if (ShowingFriends)
				{
					records = multiplayerCollectionStatusQueryResponse.friendRecords.ToArray();
				}
				else
				{
					records = multiplayerCollectionStatusQueryResponse.nonFriendRecords.ToArray();
				}
			}
			else
			{
				records = new object[0];
			}
			break;
		}
		default:
			records = new object[0];
			break;
		}
	}
}
