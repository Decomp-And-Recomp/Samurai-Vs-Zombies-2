using System.Collections.Generic;
using UnityEngine;

public class MultiplayerCollectionStatusQueryPool
{
	public const float QueryRetainSeconds = 180f;

	public const int QueryRetainMax = 5;

	private List<MultiplayerCollectionStatusQueryResponse> responses = new List<MultiplayerCollectionStatusQueryResponse>();

	public MultiplayerCollectionStatusQueryResponse Query(string queryType, int queryID, out bool isNewQuery)
	{
		Clean();
		MultiplayerCollectionStatusQueryResponse multiplayerCollectionStatusQueryResponse = responses.Find((MultiplayerCollectionStatusQueryResponse response) => response.queryType == queryType && response.queryID == queryID);
		isNewQuery = multiplayerCollectionStatusQueryResponse == null || multiplayerCollectionStatusQueryResponse.records == null;
		if (isNewQuery)
		{
			multiplayerCollectionStatusQueryResponse = new MultiplayerCollectionStatusQueryResponse(queryType, queryID);
			responses.Add(multiplayerCollectionStatusQueryResponse);
		}
		return multiplayerCollectionStatusQueryResponse;
	}

	public void Clear()
	{
		responses.Clear();
	}

	private void Clean()
	{
		responses.RemoveAll((MultiplayerCollectionStatusQueryResponse response) => Time.time - response.createdTime > 180f);
		while (responses.Count > 5)
		{
			responses.RemoveAt(0);
		}
	}
}
